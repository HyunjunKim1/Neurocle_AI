using HDSInspector_AI.Class.Interface;
using HDSInspector_AI.GUI.UserControls.Main.GridLeft;
using HDSInspector_AI.GUI.UserControls.Main.GridMiddle;
using HDSInspector_AI.GUI.UserControls.Main.GridRight;
using HDSInspector_AI.GUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HDSInspector_AI.Class.Manager
{
    public class WindowManager : IWindowService
    {
        private readonly List<WeakReference<Window>> _openedWindows = new List<WeakReference<Window>>();
        private readonly Dictionary<Type, object> _windowInstances = new Dictionary<Type, object>();

        // 기능 동작하는 Windows
        public ImageReviewWindow Review;

        // Main UserControl
        // Grid Left uc
        public Uc_HW            HW;
        public Uc_Log           Log;
        public Uc_Status        Status;   
        
        // Grid Middle uc
        public Uc_Vision        Vision;

        // Grid Right uc
        public Uc_Control       Control;
        public Uc_DefectCount   DefectCount;
        public Uc_DefectMap     DefectMap;

        public T CreateWindows<T>() where T : Window, new()
        {
            return new T();
        }

        public T CreateUserControl<T>() where T : UserControl, new()
        {
            return new T();
        }

        public void ShowWindows<T>(bool asDialog = false) where T : Window, new()
        {
            // 기존에 열려 있는 윈도우 중 동일한 타입이 있는지 확인
            Window window = FindWindow<T>();

            if (window != null)
            {
                // 이미 열려 있으면 활성화
                window.Activate();
                return;
            }

            // 새로 생성
            window = new T();
            AddWindowReference(window);

            window.Closed += (s, e) =>
            {
                RemoveWindowReference((Window)s);
            };

            if (asDialog)
                window.ShowDialog();
            else
                window.Show();
        }

        public void CloseWindows<T>() where T : Window
        {
            Window window = FindWindow<T>();
            window?.Close();
        }

        public void CloseAllWindows()
        {
            // 열려 있는 모든 창 닫기 (WeakReference 처리)
            var toClose = _openedWindows
                .Select(wr => wr.TryGetTarget(out var w) ? w : null)
                .Where(w => w != null)
                .ToList();

            foreach (var window in toClose)
            {
                window?.Close();
            }
        }

        private Window FindWindow<T>() where T : Window
        {
            foreach (var wr in _openedWindows)
            {
                if (wr.TryGetTarget(out var window) && window is T)
                {
                    return window;
                }
            }
            return null;
        }

        private void AddWindowReference(Window window)
        {
            _openedWindows.Add(new WeakReference<Window>(window));
        }

        private void RemoveWindowReference(Window window)
        {
            _openedWindows.RemoveAll(wr => wr.TryGetTarget(out var w) && w == window);
        }
    }
}
