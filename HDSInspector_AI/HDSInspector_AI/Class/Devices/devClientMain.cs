using Common;
using ControlzEx.Standard;
using HDSInspector_AI.Class.GlobalFunctions;
using SharpDX.Direct3D11;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Devices
{
    //=============== Receive/Send Command Enum =================================
    #region Receive/Send Command Enum
    // AI에서 Main으로 송신하는 Command List
    public enum E_SEND_CMD_MAIN
    {
        NONE = 0,

        R_INSPECTION_INFO,
        R_STRIP_NUMBER,
        R_INSPECTION_DONE,

        INFERENCE_DONE
    };
    //---------------------------------------------------------------------------

    // Main에서 AI로 수신되는 Command List
    public enum E_RECV_CMD_MAIN
    {
        NONE = 0,

        INSPECTION_INFO,
        STRIP_NUMBER,
        INSPECTION_DONE,

        R_INFERENCE_DONE,
    };

    #endregion
    //===========================================================================
    #region Receive/Send Data Struct
    public struct S_Send_Data
    {
        public bool Inference_Done;
    };
    //---------------------------------------------------------------------------

    public struct S_Recv_Data
    {
        public Product_Info Get_Product_Info;
        public int Strip_Number;
        public bool Inspection_Done;
    };

    #endregion Receive/Send Data Struct
    //===========================================================================

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Product_Info
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string ProductName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string OrderNumber;

        public byte[] Serialize()
        {
            var buffer = new byte[Marshal.SizeOf(typeof(Product_Info))];

            var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gch.AddrOfPinnedObject();

            Marshal.StructureToPtr(this, pBuffer, false);
            gch.Free();

            return buffer;
        }
    }

    /// <summary>
    /// 실제로 AI S/W에서 사용
    /// </summary>
    public class devClientMain
    {
        private TcpClient _tcpClient       = null;
        private StreamReader _reader       = null;
        private StreamWriter _writer       = null;
        private NetworkStream _ntsStream   = null;

        private CustomThread _taskReceive;
        private CustomThread _taskProcess;

        private bool _connected = false;
        private List<string> _recvMsgList;
        private List<E_SEND_CMD_MAIN> _sendMsgList;

        public S_Recv_Data _recvData;
        public S_Send_Data _sendData;

        private object _lockObj_Recv;
        private object _lockObj_Send;

        public devClientMain()
        {
            _recvMsgList = new List<string>();
            _sendMsgList = new List<E_SEND_CMD_MAIN>();

            _recvData = new S_Recv_Data();
            _sendData = new S_Send_Data();

            _lockObj_Recv = new object();
            _lockObj_Send = new object();

            _taskProcess = new CustomThread(50, Thread_Process);
            _taskReceive = new CustomThread(50, Thread_Receive);
        }

        #region Connect / Disconnect

        public void Connect()
        {
            string IP = GLB.Setting.General.MachineIP;
            int Port = GLB.Setting.General.MachinePort;

            try
            {
                if (_reader != null) { _reader.Close(); _reader = null; }
                if (_writer != null) { _writer.Close(); _writer = null; }
                if (_ntsStream != null) { _ntsStream.Close(); _ntsStream = null; }
                if (_tcpClient != null) { _tcpClient.Close(); _tcpClient = null; }

                _tcpClient = new TcpClient();
                _tcpClient.Connect(IP, Port);

                _ntsStream = _tcpClient.GetStream();

                _reader = new StreamReader(_ntsStream);
                _writer = new StreamWriter(_ntsStream);

                _connected = true;

                GLB.AddLog("COMMUNICATION","Connected to Server - Main S/W", SeverityLevel.INFO);
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Connect() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
        }

        public bool Connected()
        {
            return _connected;
        }

        public bool IsConnected()
        {
            try
            {
                if (_tcpClient == null)
                    return false;

                bool one = !(_tcpClient.Client.Poll(1, SelectMode.SelectRead) && _tcpClient.Client.Available == 0);
                bool two = _tcpClient.Client.Send(new byte[0]) == 0;

                return one && two;
            }
            catch (SocketException)
            {
                return false;
            }

        }
        public void Disconnect()
        {
            try
            {
                _connected = false;
                if (_reader != null) { _reader.Close(); _reader = null; }
                if (_writer != null) { _writer.Close(); _writer = null; }
                if (_ntsStream != null) { _ntsStream.Close(); _ntsStream = null; }
                if (_tcpClient != null) { _tcpClient.Close(); _tcpClient = null; }

                GLB.AddLog("COMMUNICATION", "Disconnected to Server - Main S/W", SeverityLevel.INFO);
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Disconnect() : " + e.Message, SeverityLevel.ERROR);
            }
        }

        #endregion

        #region Thread Sequence.

        private void Thread_Receive()
        {
            if(_connected == false) { Thread.Sleep(1); return; }

            try
            {
                // 송신받은 Messgae를 읽어서 Message List에 하나씩 넣어줌
                Process_Read();
            }
            catch(Exception ex)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Thread_Receive::Process_Read() : " + ex.Message, SeverityLevel.ERROR);
                Disconnect();
            }
        }

        private void Thread_Process()
        {
            if (_connected == false) { Thread.Sleep(1); return; }

            try
            {
                // Message List에서 하나씩 읽어들여서 처리함
                Process_Receive();
            }
            catch (Exception ex)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Thread_Receive::Process_Receive() : " + ex.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            try
            {
                // 송신해야할 Message 처리
                Process_Send();
            }
            catch (Exception ex)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Thread_Receive::Process_Send() : " + ex.Message, SeverityLevel.ERROR);
                Disconnect();
            }
        }

        private void Send_Message(string sSend_Msg)
        {
            try
            {
                if (_connected == false) return;

                _writer.WriteLine(sSend_Msg);
                _writer.Flush();
                GLB.AddLog("COMMUNICATION", "[AI->Main] " + sSend_Msg, SeverityLevel.INFO);
                
            }
            catch (Exception ex)
            {
                GLB.AddLog("COMMUNICATION", "Exception - class_Client_Prober::Send_Message() : " + ex.Message, SeverityLevel.ERROR);
                Disconnect();
            }
        }

        public void Add_Receive_Msg(string sMsg)
        {
            Monitor.Enter(_lockObj_Recv);
            try
            {
                if (_ntsStream.CanRead)
                    _recvMsgList.Add(sMsg);
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Add_Receive_Msg() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Recv);
            }
        }

        private void Process_Read() // TCP/IP 로 부터 송신된 Message를 읽어 Message List에 추가
        {
            string sRead_Str = "";

            if (_connected == false) { return; }

            try
            {
                if (_ntsStream.CanRead)
                {
                    // 제품 Info 구조체 크기만큼 버퍼를 만든다.
                    int nSize = Marshal.SizeOf(typeof(Product_Info));
                    byte[] byteArray = new byte[nSize];

                    // TCP/IP통신으로 받은 메시지 읽기. nMessageLength: 읽은 바이트 수.
                    int nMessageLength = _ntsStream.Read(byteArray, 0, byteArray.Length);

                    if (nMessageLength == 0) // Main S/W와 연결이 끊겼을 경우.
                    {
                        Disconnect();
                        return;
                    }
                    else if (nMessageLength < 200) // 일반 메시지
                    {
                        sRead_Str = Encoding.Default.GetString(byteArray);
                        string sTrimStr = sRead_Str.Substring(0, nMessageLength);
                        Add_Receive_Msg(sTrimStr);
                        GLB.AddLog("COMMUNICATION", "[Main->AI] " + sTrimStr, SeverityLevel.INFO);
                        return;
                    }
                    else // 구조체 메시지
                    {
                        // 형변환을 위해 IntPtr 버퍼 생성.
                        IntPtr buffer = Marshal.AllocHGlobal(nSize);
                        // 카피. byteArray -> buffer ( Byte[] -> IntPtr )
                        Marshal.Copy(byteArray, 0, buffer, nSize);
                        // 버퍼를 S_TestInfo형식으로 변환.
                        object obj = Marshal.PtrToStructure(buffer, typeof(Product_Info));
                        // 버퍼 메모리 초기화.
                        Marshal.FreeHGlobal(buffer);

                        Product_Info pInfo = (Product_Info)obj;
                        GLB.DefectImage.SetInfo(pInfo);

                        // 임시 로그 찍기.
                        Add_Receive_Msg("R_INSPECTION_INFO, Inspection Info View...");
                    }
                }
                else
                {
                    Add_Receive_Msg("R_INSPECTION_INFO, Inspection Info View...");
                }
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Process_Read_Msg() : " + e.Message, SeverityLevel.ERROR);

                Disconnect();
            }
        }

        private string Get_Recevie_Message()
        {
            int count = 0;
            string Message = string.Empty;

            Monitor.Enter(_lockObj_Recv);
            try
            {
                if (_connected == false) return Message;

                count = _recvMsgList.Count;
                if (count > 0)
                {
                    Message = _recvMsgList[0];
                    _recvMsgList.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Get_Recevie_Message() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Recv);
            }

            return Message;
        }

        private int Get_Count_Receive_Message()
        {
            int count = 0;

            Monitor.Enter(_lockObj_Recv);
            try
            {
                if (_connected == false) return count;

                count = _recvMsgList.Count;
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Get_Count_Receive_Message() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Recv);
            }

            return count;
        }
        public void Add_Send_Cmd(E_SEND_CMD_MAIN e_SendCmd)
        {
            Monitor.Enter(_lockObj_Send);
            try
            {
                if (_connected == false) return;

                _sendMsgList.Add(e_SendCmd);
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Add_Send_Cmd() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Send);
            }
        }

        private E_SEND_CMD_MAIN Get_Send_Cmd()
        {
            int nCnt = 0;
            E_SEND_CMD_MAIN e_Cmd = E_SEND_CMD_MAIN.NONE;

            Monitor.Enter(_lockObj_Send);
            try
            {
                nCnt = _sendMsgList.Count;

                if (nCnt > 0)
                {
                    e_Cmd = _sendMsgList[0];
                    _sendMsgList.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Get_Send_Cmd() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Send);
            }

            return e_Cmd;
        }

        private int Get_Count_Send_Cmd()
        {
            int nCnt = 0;

            Monitor.Enter(_lockObj_Send);
            try
            {
                if (_connected == false) return nCnt;

                nCnt = _sendMsgList.Count;
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - devClientMain::Get_Count_Send_Cmd() : " + e.Message, SeverityLevel.ERROR);
                Disconnect();
            }
            finally
            {
                Monitor.Exit(_lockObj_Send);
            }

            return nCnt;
        }
        #endregion

        #region Process Receive / Send

        private void Process_Send() // 송신해야할 Message의 처리
        {
            int Count = 0;
            string Send = string.Empty;

            try
            {
                E_SEND_CMD_MAIN Send_CMD = E_SEND_CMD_MAIN.NONE;

                Count = Get_Count_Send_Cmd();
                if (Count < 1) { return; }

                Send_CMD = Get_Send_Cmd();

                if (Send_CMD == E_SEND_CMD_MAIN.NONE) { return; }

                switch (Send_CMD)
                {
                    case E_SEND_CMD_MAIN.R_INSPECTION_INFO:
                        Send = "R_INSPECTION_INFO";
                        break;

                    case E_SEND_CMD_MAIN.R_STRIP_NUMBER:
                        Send = "R_STRIP_INFO," + _recvData.Strip_Number;
                        break;

                    case E_SEND_CMD_MAIN.R_INSPECTION_DONE:
                        Send = "R_INSPECTION_DONE," + _recvData.Inspection_Done;
                        break;
                    case E_SEND_CMD_MAIN.INFERENCE_DONE:
                        Send = "INFERENCE_DONE," + _sendData.Inference_Done;
                        break;
                }

                Send_Message(Send);
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - class_Client_Prober::Process_Send_Msg() : " + e.Message, SeverityLevel.ERROR);
            }
        }

        private void Process_Receive() // Message List에서 Message를 하나씩 읽어들여 처리
        {
            int Count_Message = 0;
            string Origin_Message = string.Empty;
            string Header = string.Empty;
            E_RECV_CMD_MAIN e_RCV_CMD = E_RECV_CMD_MAIN.NONE;

            int Parse_Cnt = 0;
            string[] slParse_Str = null;

            int nInt = 0;
            double dDouble = 0.0;
            bool bBool = false;
            string sStr = string.Empty;

            if (_connected == false) { return; }

            try
            {
                Count_Message = Get_Count_Receive_Message();
                if (Count_Message < 1) { return; }
                Origin_Message = Get_Recevie_Message(); // m_RcvMsgList로 부터 Message를 추출
                Origin_Message = Origin_Message.Trim();
                slParse_Str = Origin_Message.Split(','); // Message를 ','를 기준으로 분리
                Parse_Cnt = slParse_Str.Length;

                if (Parse_Cnt < 1)
                {
                    GLB.AddLog("COMMUNICATION", "Undefined Command: " + Origin_Message, SeverityLevel.ERROR);
                    return;
                }

                Header = slParse_Str[0];
                if (Header == "INSPECTION_INFO")        { e_RCV_CMD = E_RECV_CMD_MAIN.INSPECTION_INFO; }
                else if (Header == "STRIP_NUMBER")      { e_RCV_CMD = E_RECV_CMD_MAIN.STRIP_NUMBER; }
                else if (Header == "INSPECTION_DONE")   { e_RCV_CMD = E_RECV_CMD_MAIN.INSPECTION_DONE; }
                else if (Header == "R_INFERENCE_DONE")  { e_RCV_CMD = E_RECV_CMD_MAIN.R_INFERENCE_DONE; }
                else
                {
                    GLB.AddLog("COMMUNICATION", "Undefined Command: " + Origin_Message, SeverityLevel.ERROR);
                    return;
                }

                switch (e_RCV_CMD)
                {
                    case E_RECV_CMD_MAIN.INSPECTION_INFO:
                        break;

                    case E_RECV_CMD_MAIN.STRIP_NUMBER:
                        if (Parse_Cnt != 2) { GLB.AddLog("COMMUNICATION", "Undefined Command: " + Origin_Message, SeverityLevel.ERROR); break; }

                        sStr = slParse_Str[1];
                        nInt = Convert.ToInt32(sStr);
                        _recvData.Strip_Number = nInt; 

                        Add_Send_Cmd(E_SEND_CMD_MAIN.R_STRIP_NUMBER);
                        break;

                    case E_RECV_CMD_MAIN.INSPECTION_DONE:
                        if (Parse_Cnt != 2) { GLB.AddLog("COMMUNICATION", "Undefined Command: " + Origin_Message, SeverityLevel.ERROR); break; }

                        sStr = slParse_Str[1];
                        bBool = Convert.ToBoolean(sStr);
                        _recvData.Inspection_Done = bBool;

                        Add_Send_Cmd(E_SEND_CMD_MAIN.R_INSPECTION_DONE);
                        break;

                    case E_RECV_CMD_MAIN.R_INFERENCE_DONE:
                        if (Parse_Cnt != 2) { GLB.AddLog("COMMUNICATION", "Undefined Command: " + Origin_Message, SeverityLevel.ERROR); break; }

                        break;
                }
            }
            catch (Exception e)
            {
                GLB.AddLog("COMMUNICATION", "Exception - class_Client_Prober::Process_Receive() : " + e.Message, SeverityLevel.ERROR);
            }
        }

        #endregion
    }
}
