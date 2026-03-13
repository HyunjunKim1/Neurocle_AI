using HDSInspector_AI.Class.GlobalFunction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Communication
{
    /// <summary>
    /// 실제로 AI S/W에서 사용
    /// </summary>
    public class CommTCPIP_Server : IDisposable
    {
        CustomThread _threadListen;
        CustomThread _threadProcessMessageSend;

        Socket _server;
        Socket _client;

        bool _bIsDisposed;

        public delegate void CallBack_Logging(string text);
        CallBack_Logging _callback_Logging = null;

        public bool UseLog { get; set; } = false;

        ConcurrentQueue<string> _recvCommand = new ConcurrentQueue<string>();

        bool _bclientConnected = false;

        public CommTCPIP_Server()
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _threadListen = new CustomThread(200, ThreadServer);
            _threadProcessMessageSend = new CustomThread(10, ThreadProcess);
        }

        public void SetParameter_IP(int Port)
        {
            _server.Bind(new IPEndPoint(IPAddress.Any, Port));
            _server.Listen(10); // 소켓 접속 대기 버퍼 10개까지로 설정
        }

        public void SetParameter_Log(CallBack_Logging callBack_Logging)
        {
            _callback_Logging = callBack_Logging;
        }
        
        private void Logging(string text)
        {
            if (UseLog) _callback_Logging?.Invoke(text);
        }
        ~CommTCPIP_Server()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (_bIsDisposed)
                return;

            // 쓰레드 종료 대기
            _threadListen.Stop();
            _threadProcessMessageSend.Stop();
            _threadListen.WaitStopThread();
            _threadProcessMessageSend.WaitStopThread();

            StopServer();

            _bIsDisposed = true;
        }
        public void StartServer()
        {
            _threadProcessMessageSend.Start();
            _threadListen.Start();
        }

        public void StopServer()
        {
            _threadListen.Pause();
            _threadProcessMessageSend.Pause();
            CloseClientConnection();
        }
        private void CloseClientConnection()
        {
            try
            {
                Logging($"[SequencerServer] Client Disconnected!!!");

                _bclientConnected = false;
                _client?.Close();
                _client = null;

                ClearCommand();
            }
            catch (Exception ex)
            {
                Logging($"[SequencerServer] CloseClientConnection(): {ex.Message}");
            }
        }
        private void ClearCommand()
        {
            while (_recvCommand.TryDequeue(out string command)) { }
        }

        private void ThreadServer()
        {
            try
            {
                if(_client == null)
                {
                    _client = _server.Accept();
                    Logging($@"[Inference Server] Client Connected!!");
                    Logging($@"[Inference Server] IP: {(_client.RemoteEndPoint as IPEndPoint).Address}, Port: {(_client.RemoteEndPoint as IPEndPoint).Port}");
                    _bclientConnected = true;
                }

                byte[] buff = new byte[1024];
                _client.Receive(buff);

                string receivedData = Encoding.ASCII.GetString(buff);
                receivedData = receivedData.Trim('\0');

                if (receivedData == null || receivedData == string.Empty)
                    CloseClientConnection();
                else
                    AddRecvCommand(receivedData);
            }
            catch(Exception ex)
            {
                Logging($@"[Inference Server] ThreadServer() : {ex.Message}");
                CloseClientConnection();
            }
        }

        private void ThreadProcess()
        {
            if (_bclientConnected == false)
                return;

            try
            {
                ReceiveAndSendCommand();
            }
            catch(Exception ex)
            {
                Logging($@"[Inference Server] ThreadProcess() : {ex.Message}");
            }
        }

        private void AddRecvCommand(string command)
        {
            try
            {
                Logging($@"[Inference Server] Recv : {command}");

                _recvCommand.Enqueue(command);
            }
            catch(Exception ex)
            {
                Logging($@"[Inference Server] AddRecvCommand() : {ex.Message}");
            }
        }

        private void ReceiveAndSendCommand()
        {
            if (_recvCommand.TryDequeue(out string command) == false)
                return;

            string[] splitMessages = command.Trim().Split(',');

            bool isInvalidMessage = false;
            string sendCommand = string.Empty;

            switch(splitMessages[0])
            {
                case "Group":
                    break;
                case "ModelName":
                    break;
                case "Location":
                    break;
                default:
                    Logging($"[Inference Server] Undefined command : {command}");
                    break;
            }
            if (isInvalidMessage == true)
            {
                Logging($"[Inference Server] It doesn't match the format : {command}");
                return;
            }
            
            if(sendCommand != string.Empty)
            {
                _client.Send(Encoding.ASCII.GetBytes(sendCommand + "\r\n"));
                Logging($"[Inference Server] Send : {sendCommand}");
            }
        }
        public void SendCommand(string command)
        {
            _client.Send(Encoding.ASCII.GetBytes(command + "\r\n"));
            Logging($"[Inference Server] Send: {command}");
        }
    }
}
