using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

namespace HDSInspector_AI.Class.GlobalFunction
{
    #region define enums
    /// <summary>
    /// Severity Levels 3 Tier
    /// 
    /// SEV1 = Critical
    /// SEV2 = Moderate
    /// SEV3 = Information
    /// 
    /// </summary>
    public enum E_SERVERITY_LEVELS
    {
        Critical,
        Moderate,
        Information
    }
    public enum E_GRAB_STATUS
    {
        GrabReady,
        FrameDone,
        FrameComplete,
        Error
    }

    #endregion

    public class GlobalFunction : IDisposable
    {
        private static readonly Lazy<GlobalFunction> _instance = new Lazy<GlobalFunction>();
        public static GlobalFunction GLB => _instance.Value;

        CustomLog _clsCustomLog = new CustomLog();

        public GlobalFunction()
        {

        }

        public void Dispose()
        {
            _clsCustomLog.Dispose();
        }

        #region Global Functions
        public void AddLog(E_SERVERITY_LEVELS level, string log)
        {
            switch(level)
            {
                case E_SERVERITY_LEVELS.Critical:
                    log = $"[C][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
                case E_SERVERITY_LEVELS.Moderate:
                    log = $"[M][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
                case E_SERVERITY_LEVELS.Information:
                    log = $"[I][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
            }
            _clsCustomLog?.AddLog(log);
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
