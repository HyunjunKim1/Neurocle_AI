using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.GlobalFunction;
using HDSInspector_AI.Class.Interface;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{
    /// <summary>
    /// 여러 카메라를 사용할 수 있는 확장성을 고려하여, Interface화 시킴
    /// </summary>
    partial class CameraManager
    {
        CustomThread _threadAutoConnect;

        // Ini 파일 또는 전처리기로 어떤 카메라 종류인지 받아와서 쓰면될듯
        public devSapera Camera;

        public CameraManager()
        {
            Camera = new devSapera();

            _threadAutoConnect = new CustomThread(3000, Thread_AutoConnect);
        }

        private void Thread_AutoConnect()
        {
            if(Camera.IsConnected == false)
            {
                Camera.Logging($"[Sapera] Trying to camera auto connect...");
                if (Camera.Open())
                    Camera.StartAcquistion();
            }
            else if(Camera.HeartBeat++ > 3)
            {
                Camera.Logging($"[Sapera] Over heartbeat...");
                UnplugCallBack(null, null);
            }
        }
        

        private void UnplugCallBack(Object sender, EventArgs e)
        {
            Camera.Close();

            Camera.Logging($"[Sapera] UnplugCallBack(): Camera is disconnected.");
        }
    }
}
