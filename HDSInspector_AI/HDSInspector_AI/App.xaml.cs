using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HDSInspector_AI
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public string Version = "1.0.0";

        Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "LF_AVI_AI";

            try
            {
                _mutex = new Mutex(false, mutexName);
                if(!_mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Program already started", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown();
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n" + "Application Exiting...", "Exception");
                Shutdown();
                return;
            }

            base.OnStartup(e);

            var splashScreen = new SplashWindow();
            splashScreen.Show();
        }
    }
}
