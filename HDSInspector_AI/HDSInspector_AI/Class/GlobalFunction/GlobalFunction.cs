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

namespace HDSInspector_AI.Class.GlobalFunction
{
    #region define enums
    
    public enum E_GRAB_STATUS
    {
        GrabReady,
        FrameDone,
        FrameComplete,
        Error
    }

    #endregion

    public class GlobalFunction
    {
        private static readonly Lazy<GlobalFunction> _instance = new Lazy<GlobalFunction>();
        public static GlobalFunction GLB => _instance.Value;

        #region Global Member variables

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
        /// 
        public Logger   Logger      { get; set; }
        public Setting  Setting     { get; set; }

        #endregion

        public GlobalFunction()
        {
            Setting = new Setting(Directory.GetCurrentDirectory() + $@"\..\Config");
            Logger = Logger.GetLogger();
        }

        #region Global Functions
        public void AddLog(string system, string Msg, SeverityLevel lvl, bool IsDirectLog = false)
        {
            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                Msg = $"[{DateTime.Now:HH:mm:ss:fff}] {Msg}"; // [19:23:34:212] Blah, blah, blah.
                Logger.Log(system, lvl, Msg, IsDirectLog);
            }));
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
