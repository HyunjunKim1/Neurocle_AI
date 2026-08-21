using DALSA.SaperaLT.SapClassBasic;
using HDSInspector_AI.Class.Interface;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HDSInspector_AI.Class.Devices
{
    /// <summary>
    /// DALSA Sapera LT Wrapper
    /// IS C++ 코드와 유사하나, Interface화, 9.11 ver 신규 옵션 Flag 추가
    /// </summary>
    public class devSapera : IDisposable, ICameraController
    {
        #region Sapera 멤버 변수
        private SapAcquisition      _acq = null;        // Frame 획득 객체
        private SapAcqDevice        _acqDevice = null;  // 카메라 Param 제어용
        private SapBuffer           _bufferRGB = null;  // Interleaved RGB8 버퍼
        private SapBuffer           _bufR, _bufG, _bufB;// 8bit 이미지 3개
        private SapAcqToBuf         _xfer = null;       // Acq -> Buffer 전송 파이프라인

        // 상태 플래그
        private bool _bAcquisitionCreated = false;
        private bool _bIsDisposed = false;
        private const int _bufferCount = 4;
        #endregion

        #region 콜백, 로그, UI 스레드 등
        private CallBack_Grabbed_Color          _callback_Grabbed           = null;
        private CallBack_Grabbed_SplitChannels  _callback_Grabbed_Channels  = null;
        private CallBack_Logging                _callback_Logging           = null;
        private CallBack_Status                 _callback_Status            = null;

        #endregion

        #region ICamera Interface 선언

        public bool IsConnected { get; set; }
        public int HeartBeat { get; set; }

        public int ImageHeight => _imageHeight;
        public int ImageWidth => _imageWidth;

        private int _imageHeight, _imageWidth;

        public void SetGrabbedCallBackFunction_Color(CallBack_Grabbed_Color func) => _callback_Grabbed = func;

        public void SetGrabbedCallBackFunction_SplitChannels(CallBack_Grabbed_SplitChannels func) => _callback_Grabbed_Channels = func;

        public void SetLoggingCallBackFunction(CallBack_Logging func) => _callback_Logging = func;

        #endregion

        public devSapera() { }
        // Camera 기타 필요 함수 작성
        ~devSapera()
        {
            Dispose();
        }

        public void Logging(string text)
        {
            _callback_Logging?.Invoke(text);
        }

        #region IDisposable
        public void Dispose()
        {
            if (_bIsDisposed) return;
            _bIsDisposed = true;

            StopAcquisition();
            Close();

            // SDK 객체 역순 파괴
            SafeDestroy(ref _xfer);
            SafeDestroy(ref _bufferRGB);
            SafeDestroy(ref _bufR);
            SafeDestroy(ref _bufG);
            SafeDestroy(ref _bufB);
            SafeDestroy(ref _acq);
            SafeDestroy(ref _acqDevice);

            Logging("[Sapera] Disposed");
            GC.SuppressFinalize(this);
        }

        private void SafeDestroy<T>(ref T obj) where T : IDisposable
        {
            if (obj == null) return;
            try
            {
                switch (obj)
                {
                    case SapAcquisition a when /*a.Create()*/ a != null : a.Destroy(); break; // 이거 실제로 장비 오면 테스트 해야함.
                    case SapAcqDevice d when d != null: d.Destroy(); break;
                    case SapAcqToBuf t when t != null: t.Destroy(); break;
                    case SapBuffer b when b != null: b.Destroy(); break;
                }
            }
            catch { /* ignore */ }
            finally
            {
                obj = default;
            }
        }

        #endregion

        #region Camera Control

        public bool Open()
        {
            try
            {
                const string deviceName = "Xtium2-CLHS_PX8"; // 이거 읽어오는거 뭔가 있을텐데..? 찾아보자
                const string camFile = ""; // 파일경로

                var loc = new SapLocation(deviceName, 1);
                _acq = new SapAcquisition(loc, camFile);

                if (!_acq.Create()) {  Logging("Create SapAcquisition failed"); return false; }
                
                _acqDevice = new SapAcqDevice(new SapLocation(deviceName, 0));
                if(!_acqDevice.Create()) { Logging("Create SapAcqDevice failed"); return false; }

                _acq.GetParameter(SapAcquisition.Prm.SCALE_HORZ, out int w);
                _acq.GetParameter(SapAcquisition.Prm.SCALE_VERT, out int h);
                _imageWidth = w;
                _imageHeight = h;

                // 기본 트리거랑 게인 설정
                _acqDevice.SetFeatureValue("TriggerMode", "External");  // 임시설정임. 수정필요
                _acqDevice.SetFeatureValue("AnalogGain", "One");        // 임시설정임. 수정필요

                IsConnected = true;
                Logging("[Sapera] Open succeeded");
                return true;
            }
            catch(Exception ex)
            {
                Logging($@"[Sapera] Open() exception: {ex.Message}");
                IsConnected = false;
                return false;
            }
        }

        public void Close()
        {
            if (!IsConnected) return;

            try
            {
                IsConnected = false;
                Dispose();
                Logging($@"[Sapera] Closed");
            }
            catch(Exception ex)
            {
                Logging($@"[Sapera] Close() exception: {ex.Message}");
            }
        }

        public void StartAcquistion()
        {
            if (!IsConnected) return;

            if(!_bAcquisitionCreated)
            {
                // 처음에만 버퍼, 파이프라인을 만듦.
                if (!AllocateBuffers()) return;
                if (!CreateTransfer()) return;
                _bAcquisitionCreated = true;
            }

            // 옵션들을 최신상태로 반영해놓자.
            //ApplyOptionsToDevice();

            // Image Grab 시작 (Snap은 지정한 수 만큼 프레임을 요청한다고함)
            _xfer.Snap(_bufferRGB.Count);

            Logging($@"[Sapera] Acquisition started.");
        }

        public void StopAcquisition()
        {
            if (!_bAcquisitionCreated) return;

            try
            {
                // snap 중지
                _xfer?.Abort();
                Logging($@"[Sapera] Acqusition stoopped.");
            }
            catch(Exception ex)
            {
                Logging($@"[Sapera] StopAcquisition exception {ex.Message}");
            }
        }

        /// <summary>
        ///   Enable 플래그에 따라 카메라 파라미터를 설정하자.
        ///   자동 노출·Flat‑Field·HDF5·GPU 는 여기서 토글허자
        /// </summary>
        internal void ApplyOptionsToDevice()
        {
            // ---- 자동 노출 ----
            //if (EnableAutoExposure)
            //{
            //    // 자동 노출을 직접 구현하고 싶다면 여기서 스레드/타이머를 시작해야한다는데 대충 선언 ㄱㄱ
            //    // 간단히 ExposureTime 를 고정값(예: 5 ms) 으로 두어도 무방함
            //    _acqDevice?.SetFeatureValue("ExposureMode", "Timed");
            //    _acqDevice?.SetFeatureValue("ExposureTime", 0.005); // 5 ms 기본값
            //}

            // ---- Flat‑Field ----
            // 실제 보정은 FrameCallBack 에서 적용하도록 플래그만 저장하자
            // (보정 테이블은 별도 메서드에서 로드·적용 가능)

            // ---- HDF5 저장 ----
            // 옵션을 켜면 FrameCallBack 에서 Recorder 로 바로 전달하도록 하자
            // 여기서는 별도 동작이 필요 없으며, FrameCallBack 에서 체크하자.

            // ---- GPU Zero‑Copy ----
            // 옵션을 켜면 FrameCallBack 에서 GPURenderer 로 렌더링함
            // 여기서는 설정만 저장하고 실제 렌더링은 FrameCallBack 에서 수행
        }

        #endregion

        #region 버퍼 생성 & 전송
        private bool AllocateBuffers()
        {
            _bufR = new SapBuffer(_bufferCount, _imageWidth, _imageHeight, SapFormat.Mono8, SapBuffer.MemoryType.ScatterGather);
            _bufG = new SapBuffer(_bufferCount, _imageWidth, _imageHeight, SapFormat.Mono8, SapBuffer.MemoryType.ScatterGather);
            _bufB = new SapBuffer(_bufferCount, _imageWidth, _imageHeight, SapFormat.Mono8, SapBuffer.MemoryType.ScatterGather);

            if (!(_bufR.Create() && _bufG.Create() && _bufB.Create()))
            {
                Logging("Failed to create R/G/B buffers");
                return false;
            }

            _bufferRGB = new SapBuffer(_bufferCount, _imageWidth, _imageHeight, SapFormat.RGBP8, SapBuffer.MemoryType.ScatterGather);
            if (!_bufferRGB.Create())
            {
                Logging("Failed to create interleaved RGB buffer");
                return false;
            }

            Logging($"[Sapera] Buffers allocated: {_bufferCount} x {_imageWidth}x{_imageHeight}");
            return true;
        }

        private bool CreateTransfer()
        {
            _xfer = new SapAcqToBuf(_acq, _bufferRGB);
            var pair = _xfer.Pairs.First();
            pair.FramesOnBoard = 2;
            pair.FramesPerCallback = 1;

            int oldTimeout = SapManager.CommandTimeout;
            SapManager.CommandTimeout = Math.Max(SapManager.CommandTimeout, 100 * _bufferCount);

            if (!_xfer.Create())
            {
                Logging("Failed to create transfer pipeline");
                SapManager.CommandTimeout = oldTimeout;
                return false;
            }

            SapManager.CommandTimeout = oldTimeout;
            _xfer.XferNotify += XferNotifyHandler;

            Logging($@"[Sapera] Transfer created.");
            return true;
        }
        #endregion

        #region Callback 처리 

        // Frame Lost 관련 Callback 처리 해야할것 같은데, SapAcqCallback이 Internal로 설정되어있음.
        // 방법을 찾아보자....
        /*
        private void AcqCallback(SapAcqCallbackInfo info)
        {

        }
        private void XferCallback(SapXferCallbackInfo info)
        {

        }
        */

        private void XferNotifyHandler(object sender, SapXferNotifyEventArgs e)
        {
            int bufferIdx = e.GenericParamValue0;
            OnXferCallback(bufferIdx);
        }

        private void OnXferCallback(int bufferIdx)
        {
            // Split Components
            _bufferRGB.SplitComponents(_bufR, _bufG, _bufB);

            // Get pointers
            IntPtr pR = GetPlanePtr(_bufR, bufferIdx);
            IntPtr pG = GetPlanePtr(_bufG, bufferIdx);
            IntPtr pB = GetPlanePtr(_bufB, bufferIdx);

            // Create Mats (no ownership)
            Mat matR = Mat.FromPixelData(_imageHeight, _imageWidth, MatType.CV_8UC1, pR);
            Mat matG = Mat.FromPixelData(_imageHeight, _imageWidth, MatType.CV_8UC1, pG);
            Mat matB = Mat.FromPixelData(_imageHeight, _imageWidth, MatType.CV_8UC1, pB);

            // Merge to BGR
            Mat bgr = new Mat();
            Cv2.Merge(new[] { matB, matG, matR }, bgr);

            // 두 개의 콜백 전달
            _callback_Grabbed?.Invoke(bgr.Clone());
            _callback_Grabbed_Channels?.Invoke(new[] { matB.Clone(), matG.Clone(), matR.Clone() });

            HeartBeat = 0;

            // Cleanup
            bgr.Dispose();
            matR.Dispose();
            matG.Dispose();
            matB.Dispose();
        }

        private static IntPtr GetPlanePtr(SapBuffer buf, int idx)
        {
            buf.GetAddress(idx, out IntPtr ptr);
            return ptr;
        }
        #endregion
    }
}
