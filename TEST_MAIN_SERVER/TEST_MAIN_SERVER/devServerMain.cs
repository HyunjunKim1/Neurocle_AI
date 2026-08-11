using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TEST_MAIN_SERVER
{
    internal class devServerMain : IDisposable
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _networkStream;

        private Thread _listenThread;
        private Thread _receiveThread;
        private Thread _sendThread;

        private readonly ConcurrentQueue<MainPacket> _sendQueue;
        private readonly AutoResetEvent _sendSignal;
        private readonly object _connectionLock;
        private volatile bool _running;
        private volatile bool _clientConnected;

        private bool _disposed;

        public delegate void CallBack_Logging(string text);
        private CallBack_Logging _callback_Logging = null;

        public bool UseLog { get; set; }
        public bool ClientConnected { get { return _clientConnected; } }

        public devServerMain()
        {
            _sendQueue = new ConcurrentQueue<MainPacket>();
            _sendSignal = new AutoResetEvent(false);
            _connectionLock = new object();
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

        public void StartServer(int port)
        {
            if (_running) return;
            _running = true;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listenThread = new Thread(ListenThread);
            _listenThread.IsBackground = true;
            _listenThread.Start();

            _sendThread = new Thread(SendThread);
            _sendThread.IsBackground = true;
            _sendThread.Start();

            Logging($"Main Server listen - {port}");
        }

        private void ListenThread()
        {
            while(_running)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    // 어짜피 한 Main에 하나의 AI Server만 붙음
                    CloseClientConnection();

                    lock (_connectionLock)
                    {
                        _client = client;
                        _networkStream = client.GetStream();
                        _clientConnected = true;
                    }

                    Logging("AI Client Connected");

                    StartReceiveThread();
                }
                catch (Exception ex)
                {
                    Logging($"Listen error -{ex.Message}");
                }
            }
        }

        private void StartReceiveThread()
        {
            _receiveThread = new Thread(ReceiveThread);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void ReceiveThread()
        {
            try
            {
                while(_running && _clientConnected)
                {
                    NetworkStream stream;
                    lock ((_connectionLock))
                    {
                        stream = _networkStream;
                    }

                    if (stream == null) break;

                    MainPacket packet = PacketStreamHelper.ReadPacket(stream);
                    ProcessPacket(packet);
                }
            }
            catch(Exception ex)
            {
                if(_running)
                {
                    Logging($"Receive Error : {ex.Message}");
                }
            }
            finally
            {
                CloseClientConnection();
            }
        }

        private void ProcessPacket(MainPacket packet)
        {
            Logging($"[AI → Main] {packet.Command}");

            switch(packet.Command)
            {
                case MainCommand.R_INSPECTION_INFO:
                    Logging($"[Main → AI] Product Info ACK");
                    break;

                case MainCommand.R_STRIP_NUMBER:
                    {
                        int stripNumber = IntPayload.Deserialize(packet.Payload);
                        Logging($"[Main → AI] Strip ACK : {stripNumber}");
                    }
                    break;

                case MainCommand.R_INSPECTION_DONE:
                    {
                        int stripNumber = IntPayload.Deserialize(packet.Payload);
                        Logging($"[Main → AI] Inspection Done ACK : {stripNumber}");
                    }
                    break;

                case MainCommand.INFERENCE_DONE:
                    {
                        int stripNumber = IntPayload.Deserialize(packet.Payload);
                        Logging($"[Main → AI] AI Inference Done : {stripNumber}");
                    }
                    break;

                case MainCommand.PING:
                    Send(MainCommand.PONG);
                    break;

                case MainCommand.PONG:
                    break;
            }
        }

        public void Send(MainCommand command, byte[] payload = null)
        {
            if (!_running) return;

            _sendQueue.Enqueue(new MainPacket(command, payload));
            _sendSignal.Set();
        }

        private void SendThread()
        {
            while(_running)
            {
                _sendSignal.WaitOne(100);

                if (!_running) break;
                if (!_clientConnected) continue;

                while(_sendQueue.TryDequeue(out MainPacket packet))
                {
                    try
                    {
                        NetworkStream stream;

                        lock(_connectionLock)
                        {
                            stream = _networkStream;
                        }

                        if (stream == null) break;

                        PacketStreamHelper.WritePacket(stream, packet);

                        Logging($"[Main → AI] {packet.Command}");
                    }
                    catch(Exception ex)
                    {
                        Logging($"Main Send Error {ex.Message}");
                        CloseClientConnection();

                        break;
                    }
                }
            }
        }

        #region Test Send

        public void SendProductInfo(ProductInfo pInfo)
        {
            if (pInfo == null) return;
            Send(MainCommand.INSPECTION_INFO, pInfo.Serialize());
        }

        public void SendStripNumber(int stripNumber)
        {
            Send(MainCommand.STRIP_NUMBER, IntPayload.Serialize(stripNumber));
        }

        public void SendInspectionDone(int stripNumber)
        {
            Send(MainCommand.INSPECTION_DONE, IntPayload.Serialize(stripNumber));
        }

        #endregion

        private void CloseClientConnection()
        {
            bool wasConnected = _clientConnected;
            lock (_connectionLock)
            {
                _clientConnected = false;
                try
                {
                    _networkStream?.Close();
                }
                catch { }
                try
                {
                    _client?.Close();
                }
                catch (Exception ex)
                {
                    Logging($"[Main] CloseClientConnection(): {ex.Message}");
                }

                _networkStream = null;
                _client = null;
            }

            if(wasConnected) Logging($"[Main] CloseClientConnection()");
        }

        public void StopServer()
        {
            _running = false;
            _sendSignal.Set();

            CloseClientConnection();

            try
            {
                _listener?.Stop();
            }
            catch { }

            JoinThread(_receiveThread);
            JoinThread(_sendThread);
            JoinThread(_listenThread);

            _listener = null;
        }

        private static void JoinThread(Thread thread)
        {
            try
            {
                if (thread != null && thread.IsAlive) thread.Join(1000);
            }
            catch { }
        }
        public void Dispose()
        {
            if (_disposed) return;

            StopServer();
            _sendSignal.Dispose();
            _disposed = true;
        }
    }
}
