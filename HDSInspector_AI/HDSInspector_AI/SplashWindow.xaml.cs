using Common;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.GUI.UserControls.Main.GridLeft;
using HDSInspector_AI.GUI.UserControls.Main.GridRight;
using HDSInspector_AI.GUI.Windows;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI
{
    /// <summary>
    /// SplashScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplashWindow : Window
    {
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
            SetImage(iBox_Func,         new Uri("pack://application:,,,/Resources/LED_RED.png"));
            SetImage(iBox_HW,           new Uri("pack://application:,,,/Resources/LED_RED.png"));

            GLB.ApplyFadeAndZoomAnimation(this, GridSplash, durationMs:500);

            _threadSplash.Start();
        }

        /// <summary>
        /// change led images
        /// </summary>
        /// <param name="iBox"> iBox : icon box     </param>
        /// <param name="img">  Resource images     </param>
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
                switch(_step)
                {
                    case 0:
                        Logging($"Loading... {App.Version}", SeverityLevel.INFO);
                        Logging($"######## Program Start !! ########", SeverityLevel.INFO);
                        break;

                    case 10:
                        Logging("Read Ini data", SeverityLevel.INFO);

                        GLB.Setting.Load();

                        Logging("Read Ini data - Success", SeverityLevel.INFO);

                        SetImage(iBox_Init, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));

                        // Initialize Global Data
                        // Log 기간 설정 및 삭제
                        int nDeleteLogCount = Logger.CleanLog(GLB.Setting.General.LogKeepDate);
                        if (nDeleteLogCount > 0)
                        {
                            string msg = $"최근에 기록된 {nDeleteLogCount}개의 로그 파일을 정리하였습니다.";
                            Logging($"{msg}", SeverityLevel.INFO);
                        }
                        break;

                    case 20:
                        if (GLB.Setting.General.Simulation == true)
                        {
                            Logging("Simulation skip - Load Alarm Data", SeverityLevel.INFO);

                            // Load Alarm Data
                            SetImage(iBox_Alarmlist, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        else
                        {
                            Logging("Load Alarm Data", SeverityLevel.INFO);

                            // Load Alarm Data
                            SetImage(iBox_Alarmlist, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        break;

                    case 30:
                        if (GLB.Setting.General.Simulation == true)
                        {
                            Logging("Simulation skip - Server open.. !", SeverityLevel.INFO);
                            SetImage(iBox_Server, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        else
                        {
                            Logging("Server open.. !", SeverityLevel.INFO);

                            try
                            {
                                GLB.Server.SetParameter_IP("500");
                                GLB.Server.SetParameter_Log(GLB.AddLog);
                                GLB.Server.StartServer();

                                SetImage(iBox_Server, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                            }

                            catch (Exception ex)
                            {
                                Logging($@"Server open failed - {ex.Message}", SeverityLevel.ERROR);
                                return;
                            }
                        }
                        break;

                    case 40:
                        if (GLB.Setting.General.Simulation == true)
                        {
                            Logging("Simulation skip - Try connect with Frame grabber.. !", SeverityLevel.INFO);
                            SetImage(iBox_Camera, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        else
                        {
                            Logging("Try connect with Frame grabber.. !", SeverityLevel.INFO);

                            try
                            {


                                SetImage(iBox_Camera, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                            }
                            catch (Exception ex)
                            {
                                Logging($@"Failed framegrabber connection. - {ex.Message}", SeverityLevel.ERROR);
                                return;
                            }
                        }
                        break;

                    case 60:
                        if (GLB.Setting.General.Simulation == true)
                        {
                            Logging("Simulation skip - Initialize necessary functions.. ", SeverityLevel.INFO);
                            SetImage(iBox_Func, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        else
                        {
                            Logging("Initialize necessary functions.. ", SeverityLevel.INFO);

                            try
                            {
                                SetImage(iBox_Func, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                            }
                            catch (Exception ex)
                            {
                                Logging($@"Fail initialized necessary functions.. - {ex.Message}", SeverityLevel.ERROR);
                                return;
                            }
                        }
                        break;

                    case 80:
                        if (GLB.Setting.General.Simulation == true)
                        {
                            Logging("Simulation skip - Try connect with hardwares.. ", SeverityLevel.INFO);
                            SetImage(iBox_HW, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));
                        }
                        else
                        {
                            Logging("Try connect with hardwares.. ", SeverityLevel.INFO);

                            try
                            {
                                SetImage(iBox_HW, new Uri("pack://application:,,,/Resources/LED_GREEN.png"));

                            }
                            catch (Exception ex)
                            {
                                Logging($@"Failed hardware connections.. - {ex.Message}", SeverityLevel.ERROR);
                            }
                        }
                        break;

                    case 90:
                        // Initialize Windows & UserControls
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            GLB.Windows.CreateWindows(Class.Manager.WindowManager.WINDOW_NAME.REVIEW);

                            // Grid Left uc
                            GLB.Windows.HW      = GLB.Windows.CreateUserControl<Uc_HW>();
                            GLB.Windows.Log     = GLB.Windows.CreateUserControl<Uc_Log>();
                            GLB.Windows.Status  = GLB.Windows.CreateUserControl<Uc_Status>();

                            // Grid Right uc
                            GLB.Windows.DefectImage = GLB.Windows.CreateUserControl<Uc_DefectImage>();
                            GLB.Windows.InferenceImage = GLB.Windows.CreateUserControl<Uc_InferenceImage>();
                            GLB.Windows.Result = GLB.Windows.CreateUserControl<Uc_Result>();

                        });
                        break;

                    case 100:
                        Logging("############### Complete load of programs ###############", SeverityLevel.INFO);

                        _threadSplash.Stop();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.Close();
                        }));

                        Dispatcher.Invoke(new Action(() =>
                        {
                            var mainWindow = new MainWindow();
                            mainWindow.Show();
                        }));
                        return;
                }
                Thread.Sleep(1);
                _step++;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    gBox_Status.Header = $"Program Loading.... {_step}%";
                }));
            }
            catch (Exception ex)
            {
                Logging($"Exception Error Occur! Please check the log.", SeverityLevel.ERROR);
                Logging($"Seq No : {_step} - {ex.Message}", SeverityLevel.ERROR);
            }
        }
    }
}
