using Common;
using HDSInspector_AI.Class.GlobalFunction;
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
using System.Windows.Shapes;

namespace HDSInspector_AI
{
    /// <summary>
    /// SplashScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplashWindow : Window
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;
        CustomThread _threadSplash;
        int _step = 0;

        public SplashWindow()
        {
            InitializeComponent();

            tbl_Version.Text = App.Version;

            _threadSplash = new CustomThread(10, SplashLoading);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;

            SetImage(iBox_Init,         new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_Alarmlist,    new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_Server,       new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_Camera,       new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_Type,         new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_HW,           new Uri("pack://application:,,,/Resources/LED_RED.png"));

            GLB.ApplyFadeAndZoomAnimation(this, GridSplash, durationMs:500);

            _threadSplash.Start();
        }

        private void SetImage(Image iBox, Uri img)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                iBox.Source = new BitmapImage(img);
            }));
        }
        private void Logging(string Msg, SeverityLevel lvl)
        {
            string NowProcess = "Splash";

            GLB.AddLog(NowProcess, Msg, lvl);
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            _threadSplash.Stop();
            this.Dispatcher.BeginInvoke(new Action(() => {
                this.Close();
            }));
        }

        private void SplashLoading()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
    }
}
