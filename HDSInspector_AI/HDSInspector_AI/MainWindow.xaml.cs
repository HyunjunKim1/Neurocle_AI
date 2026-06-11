using Common;
using ControlzEx.Behaviors;
using HDSInspector_AI.Class.GlobalFunctions;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!GLB.IsRunning)
            {
                string msg = "프로그램을 종료하시겠습니까?";
                if (MessageBox.Show(msg, "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    GLB.Setting.Save();

                    GLB.AddLog("MAIN", "프로그램을 종료합니다.", SeverityLevel.INFO);
                    GLB.Logger.Close();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    //Environment.Exit(0);
                }
            }
        }
    }
}
