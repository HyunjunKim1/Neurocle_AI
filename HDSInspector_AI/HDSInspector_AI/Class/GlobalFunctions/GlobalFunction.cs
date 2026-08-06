using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using Common;
using System.Windows.Threading;
using System.IO;
using HDSInspector_AI.GUI.Windows.Popup;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.Manager;
using HDSInspector_AI.Class.Models;
using System.Diagnostics;

namespace HDSInspector_AI.Class.GlobalFunctions
{
    #region define enums
    
    public enum E_GRAB_STATUS
    {
        GRAB_READY,
        FRAME_DONE,
        FRAME_COMPLATE,

        ERROR
    }

    public enum E_IMAGE_STATUS
    {
        NONE,

        EROSION,
        DILATION,
        CANNY_EDGE,
        CONTRAST,
        CLAHE,
        SOBEL_EDGE,
        GAUSSIAN_FILTER,
        MEDIAN_FILTER,
        EXTRACT
    }
    #endregion

    public class GlobalFunction : IDisposable
    {
        private static readonly Lazy<GlobalFunction> _instance = new Lazy<GlobalFunction>();
        public static GlobalFunction GLB => _instance.Value;

        #region Events
        // 실시간 GUI 로그 전달 및 저장 이벤트
        public event Action<LogDisplayItem> LogAdded;
        #endregion

        #region Global Member variables

        public string StartupPath = Directory.GetCurrentDirectory();
        public bool IsRunning = false;

        DateTime m_StartTime;
        TimeSpan m_RunTime;
        DateTime m_NowTime;
        DateTime m_EndTime;

        public DateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        public TimeSpan RunTime
        {
            get { return m_RunTime; }
            set { m_RunTime = value; }
        }

        public DateTime NowTime
        {
            get { return m_NowTime; }
            set { m_NowTime = value; }
        }

        public DateTime EndTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

        #endregion

        #region Global Classes
        /// <summary>
        /// GlobalFunction Common.dll 응집도 높이기 위한 Singleton Pattern
        /// Global로 사용하기 위한 Class 정의
        /// </summary>
        
        public Logger           Logger      = Logger.GetLogger();
        public Setting          Setting     = new Setting(Directory.GetCurrentDirectory() + $@"\..\Config");
        public ImageProcessing  ImgProc     = new ImageProcessing();
        public devServerMain       Server   = new devServerMain();

        // Class Manager
        public SequenceManager          Sequence    = new SequenceManager();
        public HardwareMonitorManager   Hardware    = new HardwareMonitorManager(driveName:"E:\\", gpuIndex:0);
        public WindowManager            Windows     = new WindowManager();
        //public CameraManager    Cameras     = new CameraManager();

        #endregion

        public void Dispose()
        {
            Hardware?.Dispose();
            Sequence?.Dispose();
        }

        #region Global Functions

        public void AddLog(string system, string message, SeverityLevel level)
        {
            LogDisplayItem logItem = new LogDisplayItem
            {
                Time = DateTime.Now,
                System = system,
                Level = level,
                Message = message
            };

            try
            {
                Logger.Log(system, level, message);
            }
            catch (Exception ex) { Debug.WriteLine($"{ex.Message}"); }

            // Observer Pattern // 구독 해놓은 Control 들에게 전달
            try
            {
                LogAdded?.Invoke(logItem);
            }
            catch(Exception ex) { Debug.WriteLine($"{ex.Message}"); }
        }
        public void CleanLog()
        {
            int nDeleteLogCount = Logger.CleanLog(Setting.General.LogKeepDate);
            if (nDeleteLogCount > 0)
            {
                string amsg = $"최근에 기록된 {nDeleteLogCount}개의 로그 파일을 정리하였습니다.";
                MessageBox.Show(String.Format(amsg, nDeleteLogCount), "Information");
            }
        }

        public bool WarningMessage(string Message, string Title, Window window)
        {
            bool bCheck = false;
            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                WarningMessageBox msgBox = new WarningMessageBox(Message, Title);
                msgBox.Owner = window;

                if (msgBox.ShowDialog() == true) { bCheck = true; }
                else { bCheck = false; }
            }));

            return bCheck;
        }

        #region Window Animation

        public void ApplyFadeAndZoomAnimation(Window window, FrameworkElement zoomTarget, double fromScale = 0.8, double durationMs = 1000)
        {
            EventHandler handler = null;

            handler = (s, e) =>
            {
                CompositionTarget.Rendering -= handler;

                // Fade-in 애니메이션
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                window.BeginAnimation(Window.OpacityProperty, fadeIn);

                // Zoom 대상 Transform이 없으면 생성
                if (!(zoomTarget.RenderTransform is ScaleTransform))
                {
                    zoomTarget.RenderTransform = new ScaleTransform(fromScale, fromScale);
                    zoomTarget.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                var scaleTransform = zoomTarget.RenderTransform as ScaleTransform;
                if (scaleTransform != null)
                {
                    var zoomAnim = new DoubleAnimation
                    {
                        From = fromScale,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(durationMs),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomAnim);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomAnim);
                }
            };

            CompositionTarget.Rendering += handler;
        }

        #endregion

        #endregion
    }
}
