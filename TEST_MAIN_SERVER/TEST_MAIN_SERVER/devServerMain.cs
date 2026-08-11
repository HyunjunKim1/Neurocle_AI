using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TEST_MAIN_SERVER
{
    internal class devServerMain
    {
        private WhileThread _threadListen;
        private WhileThread _threadProcessMessageAndSend;

        private Socket _server;
        private Socket _client = null;
        private bool _disposed = false;

        public delegate void CallBack_Logging(string text);
        private CallBack_Logging _callback_Logging = null;

        private ConcurrentQueue<string> _q_RecvCommand = new ConcurrentQueue<string>();
        private bool _clientConnected = false;

        public bool UseLog { get; set; }
        public string FailReason { get; set; }

        // Ansi 설정해서 char = 1byte로
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

        public devServerMain()
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _threadListen = new WhileThread(200, ThreadServer);
            _threadProcessMessageAndSend = new WhileThread(100, ThreadProcess);
        }

        public void SetParameter_IP(int Port)
        {
            _server.Bind(new IPEndPoint(IPAddress.Any, Port));
            _server.Listen(10); // 소켓 접속 대기 개수임. 10개까지 설정해놓고 테스트
        }

        public void SetParameter_Log(CallBack_Logging callBack_Logging)
        {
            _callback_Logging = callBack_Logging;
        }

        private void Logging(string text)
        {
            if(UseLog)
                _callback_Logging?.Invoke(text);
        }

        ~devServerMain()
        {
            Dispose();
        }
        public void Dispose()
        {
            // 이미 Dispose했으면 패스.
            if (_disposed)
                return;

            StopServer();

            // 쓰레드가 종료되길 기다린다.
            _threadProcessMessageAndSend.Stop();
            _threadListen.Stop();

            _disposed = true;
        }

        private void ThreadServer()
        {
            try
            {
                if (_client == null)
                {
                    _client = _server.Accept();                    
                    Logging($"[Main] Client Connected!!!");
                    Logging($"[Main] IP: {(_client.RemoteEndPoint as IPEndPoint).Address}, Port: {(_client.RemoteEndPoint as IPEndPoint).Port}");
                    _clientConnected = true;
                }

                byte[] buffer = new byte[1024];
                _client.Receive(buffer);

                string receivedData = Encoding.ASCII.GetString(buffer);
                receivedData = receivedData.Trim('\0');

                if(receivedData == null || receivedData == string.Empty)
                    CloseClientConnection();
                else
                    AddRecvCommand(receivedData);
            }
            catch (Exception ex)
            {
                Logging($"[Main] ThreadServer(): {ex.Message}");
                CloseClientConnection();
            }
        }

        private void ThreadProcess()
        {
            if (_clientConnected == false)
                return;

            try
            {
                ReceiveAndSendCommand();
            }
            catch (Exception ex)
            {
                Logging($"[Main] ThreadProcess(): {ex.Message}");
            }
        }
        public void StartServer()
        {
            _threadProcessMessageAndSend.Start();
            _threadListen.Start();
        }

        public void StopServer()
        {
            _threadListen.Pause();
            _threadProcessMessageAndSend.Pause();
            CloseClientConnection();
        }
        public bool IsClientConnedted()
        {
            return _clientConnected;
        }

        private void CloseClientConnection()
        {
            try
            {
                Logging($"[Main] Client Disconnected!!!");

                _clientConnected = false;
                _client?.Close();
                _client = null;

                ClearCommand();
            }
            catch (Exception ex)
            {
                Logging($"[Main] CloseClientConnection(): {ex.Message}");
            }
        }

        private void AddRecvCommand(string command)
        {
            try
            {
                Logging($"[Main] Recv: {command}");

                _q_RecvCommand.Enqueue(command);
            }
            catch (Exception ex)
            {
                Logging($"[Main] AddRecvCommand(): {ex.Message}");
            }
        }

        private void ClearCommand()
        {
            while (_q_RecvCommand.TryDequeue(out string command))
            {
            }
        }

        int _stripNumber = 0;
        bool _isInspectionDone = false;
        bool _isInferenceDone = false;

        private void ReceiveAndSendCommand()
        {
            // 메세지 받을 수 있는 상태일 경우에만 처리하도록 추가해야함.

            if (_q_RecvCommand.TryDequeue(out string command) == false)
                return;


            try
            {
                string[] splitMessages = command.Trim().Split(',');

                bool isInvalidMessage = false;

                switch (splitMessages[0])
                {
                    case "INSPECTION_INFO":
                        if (splitMessages.Length != 1) { isInvalidMessage = true; break; }

                        Product_Info pInfo = new Product_Info();
                        pInfo.ProductName = "EAV44";
                        pInfo.DeviceName = "(AS)48QFN(4.9X4.9) 3A694R01 9X37X1 R10";
                        pInfo.OrderNumber = "105421727J01";

                        _client.Send(pInfo.Serialize());
                        break;

                    case "STRIP_NUMBER":
                        if (splitMessages.Length != 4) { isInvalidMessage = true; break; }

                        if (int.TryParse(splitMessages[1], out _stripNumber) == false)
                        {
                            isInvalidMessage = true;
                            break;
                        }

                        SendCommand($"STRIP_NUMBER,{_stripNumber:0.0}");
                        break;

                    case "INSPECTION_DONE":
                        if (splitMessages.Length != 2) { isInvalidMessage = true; break; }

                        _isInspectionDone = splitMessages[1] == "DONE" ? true : false;

                        SendCommand($"INSPECTION_DONE,SUCC,{_isInspectionDone}");
                        break;

                    case "INFERENCE_DONE":
                        if (splitMessages.Length != 3) { isInvalidMessage = true; break; }

                        _isInferenceDone = splitMessages[1] == "DONE" ? true : false;

                        SendCommand($"R_INSPECTION_DONE,{_isInspectionDone}");
                        break;

                    default:
                        Logging($"[Main Server] Undefined command: {command}");
                        return;
                }

                if (isInvalidMessage == true)
                {
                    Logging($"[Main Server] It doesn't match the format: {command}");
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void SendCommand(string command)
        {
            _client.Send(Encoding.ASCII.GetBytes(command + "\r\n"));
            Logging($"[Main Test Server] Send: {command}");
        }
    }
}
