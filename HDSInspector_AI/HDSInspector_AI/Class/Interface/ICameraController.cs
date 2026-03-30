using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Interface
{
    public delegate void CallBack_Grabbed_Color(Mat image);
    public delegate void CallBack_Grabbed_SplitChannels(Mat[] channels);
    public delegate void CallBack_Logging(string text);
    public delegate void CallBack_Status(int status, int param);

    interface ICameraController
    {
        int ImageHeight { get; }
        int ImageWidth { get; }

        void SetGrabbedCallBackFunction_Color(CallBack_Grabbed_Color func);
        void SetGrabbedCallBackFunction_SplitChannels(CallBack_Grabbed_SplitChannels func);
        void SetLoggingCallBackFunction(CallBack_Logging func);
        bool Open();
        void Close();
        void StartAcquistion();
        void StopAcquisition();
    }
}
