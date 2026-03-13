using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Interface
{
    public delegate void CallBack_Grabbed(Mat image);
    public delegate void CallBack_Logging(string text);

    interface ICameraController
    {
        int ImageHeight { get; }
        int ImageWidth { get; }

        void SetGrabbedCallBackFunction(CallBack_Grabbed func);
        void SetLoggingCallBackFunction(CallBack_Logging func);
        bool Open();
        void Close();
        void StartAcquistion();
        void StopAcquisition();
    }
}
