using ControlzEx.Behaviors;
using HDSInspector_AI.Class.Interface;
using HDSInspector_AI.GUI.UserControls.Main.GridLeft;
using HDSInspector_AI.GUI.UserControls.Main.GridRight;
using HDSInspector_AI.GUI.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{
    /// <summary>
    /// Singleton Pattern 기반, 각 GUI 객체를 별도 할당 하는것이 아님
    /// Popup GUI Window의 경우, Splash에서 미리 객체 생성을 한 후, Visible로 제어하며 Singleton Pattern 활용도를 높임
    /// </summary>
    public class WindowManager : IWindowService
    {
        public enum WINDOW_NAME
        {
            // 추가적인 Form들 여기에 추가 후 제어
            MAIN,
            REVIEW,
        }

        private readonly List<WeakReference<Window>> _openedWindows = new List<WeakReference<Window>>();
        private readonly Dictionary<Type, object> _windowInstances = new Dictionary<Type, object>();

        // Windows
        public MainWindow        Main; 
        public ImageReviewWindow Review;

        // Main UserControl
        // Grid Left uc
        public Uc_HW            HW;
        public Uc_Log           Log;
        public Uc_Status        Status;   
        
        // Grid Right uc
        public Uc_DefectImage       DefectImage;
        public Uc_InferenceImage    InferenceImage;
        public Uc_Result            Result;

        public void ThreadSorting()
        {
            Process currentProcess = Process.GetCurrentProcess();

            foreach (ProcessThread processThread in currentProcess.Threads)
            {
                processThread.ProcessorAffinity = currentProcess.ProcessorAffinity;
            }
        }
        
        /// <summary>
        /// Window 객체 생성 후, Visible 변경을 함.
        /// Memory 할당을 하며, 객체 생성 / 해제에 대한 리소스 낭비 방지
        /// </summary>
        /// <param name="name">생성 하고자 하는 Window Enum 관리</param>
        public void CreateWindows(WINDOW_NAME name)
        {
            try
            {
                Window win = null;
                Dispatcher dispatcher = null;

                Thread thrd = new Thread(() =>
                {
                    switch (name)
                    {
                        case WINDOW_NAME.REVIEW:
                            win = new ImageReviewWindow();
                            break;
                    }
                    win.Tag = name;
                    win.Visibility = Visibility.Hidden;

                    //Dispatcher 저장
                    dispatcher = Dispatcher.CurrentDispatcher;

                    win.Show();
                    Dispatcher.Run();
                });
                thrd.SetApartmentState(ApartmentState.STA);
                thrd.IsBackground = true;
                thrd.Start();

                while (win == null || dispatcher == null)
                    Thread.Sleep(10);

                dispatcher.Invoke(() =>
                {
                    win.Topmost = true;
                    win.Hide();
                    win.Topmost = false;
                });

                ThreadSorting();

                switch (name)
                {
                    case WINDOW_NAME.REVIEW:
                        Review = (ImageReviewWindow)win;
                        break;
                }
                win.Closing += Window_Closing;
                //win.IsVisibleChanged += Window_VisibleChanged;
            }
            catch (Exception ex) {
                GLB.AddLog("[WindowManager]", $@"{ex.Message}", Common.SeverityLevel.ERROR);
            }
        }

        private void Window_VisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Window win = (sender as Window);
            WINDOW_NAME winName = (WINDOW_NAME)Enum.Parse(typeof(WINDOW_NAME), win.Tag.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Window window = (sender as Window);
            window.WindowState = WindowState.Normal;
            window.Hide();
            window.ShowInTaskbar = false;
        }

        public T CreateUserControl<T>() where T : UserControl, new()
        {
            return new T();
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
