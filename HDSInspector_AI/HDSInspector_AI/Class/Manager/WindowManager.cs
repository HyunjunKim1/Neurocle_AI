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
        private readonly List<Window> _openedWindows = new List<Window>();
        
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

        public void ShowWindows(Window window, bool asDialog = false)
        {
            if (!_openedWindows.Contains(window))
            {
                _openedWindows.Add(window);
                window.Closed += (s, e) => _openedWindows.Remove(window);

                if (asDialog)
                    window.ShowDialog();
                else
                    window.Show();
            }
            else
            {
                window.Activate();
            }
        }

        public void CloseWindows(Window window)
        {
            if (_openedWindows.Contains(window))
            {
                window.Close();
                _openedWindows.Remove(window);
            }
        }
    }
}
