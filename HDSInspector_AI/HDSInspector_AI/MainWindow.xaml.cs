using Common;
using ControlzEx.Behaviors;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.Class.Models;
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
        private int _currentStripNumber;
        public MainWindow()
        {
            InitializeComponent();

            RegisterUserControls();
            RegisterMainCommunication();
        }

        private void RegisterUserControls()
        {
            GLB.Windows.Main = this;

            // Left
            GLB.Windows.Status = ucStatus;
            GLB.Windows.HW = ucHW;
            GLB.Windows.Log = ucLog;

            // Right
            GLB.Windows.DefectImage = ucDefectImage;
            GLB.Windows.InferenceImage = ucInferenceImage;
            GLB.Windows.Result = ucResult;

        }
        private void RegisterMainCommunication()
        {
            GLB.Client.ProductInfoReceived -= Client_ProductInfoReceived;
            GLB.Client.ProductInfoReceived += Client_ProductInfoReceived;

            GLB.Client.StripNumberReceived -= Client_StripNumberReceived;
            GLB.Client.StripNumberReceived += Client_StripNumberReceived;

            GLB.Client.ConnectionChanged -= Client_ConnectionChanged;
            GLB.Client.ConnectionChanged += Client_ConnectionChanged;

            GLB.Client.InspectionStateChanged -= Client_InspectionStateChanged;
            GLB.Client.InspectionStateChanged += Client_InspectionStateChanged;

            GLB.DefectImage.InspectionImageReady -= DefectImage_InspectionImageReady;
            GLB.DefectImage.InspectionImageReady += DefectImage_InspectionImageReady;

            Client_ConnectionChanged(GLB.Client.Connected);
        }


        private async void DefectImage_InspectionImageReady(DefectImageFileSet fileSet)
        {
            try
            {
                await GLB.Inference.ProcessFileSetAsync(fileSet);
            }
            catch (Exception ex)
            {
                GLB.AddLog("INFERENCE", $"ProcessFileSetAsync Error : {ex.Message}", SeverityLevel.ERROR);
            }
        }

        private void Client_InspectionStateChanged(bool isRunning)
        {
            GLB.AddLog("MAIN", isRunning ? "Inspection Start" : "Inspection Stop", SeverityLevel.INFO);

            GLB.Windows.Status?.SetInspectionRunning(isRunning);
            GLB.Inference.SetInspectionState(isRunning);
        }

        private void Client_ProductInfoReceived(ProductInfo pInfo)
        {
            if (pInfo == null) return;

            InspectionInfo info = new InspectionInfo
            {
                DeviceName = pInfo.DeviceName,
                ProductName = pInfo.ProductName,
                OrderNumber = pInfo.OrderNumber
            };

            bool success = GLB.DefectImage.SetInfo(info);
            if (!success) GLB.AddLog("MAIN", $"{GLB.DefectImage.LastError}", SeverityLevel.ERROR);

            GLB.InferenceStatistics.SetInspectionInfo(info);
        }

        private void Client_StripNumberReceived(int stripNumber)
        {
            _currentStripNumber = stripNumber;

            GLB.AddLog("MAIN", $"Current Strip : {stripNumber}", SeverityLevel.INFO);

            /*
             * Main S/W는 이미지 저장까지 완료한 뒤
             * Strip Number를 송신해야만함.
             * 
             * 그래서 StripNumber 자체가 AI 검사 Trigger로 써도 될듯~?
             */
            bool success = GLB.DefectImage.ProcessInspectionComplete(stripNumber);
            if (!success)
                GLB.AddLog("MAIN", $"Strip [{stripNumber:D6}] 처리 실패 : {GLB.DefectImage.LastError}", SeverityLevel.ERROR);


        }

        private void Client_ConnectionChanged(bool connected)
        {
            if (GLB.Windows.Status == null)
                return;
            GLB.Windows.Status.SetMainSWConnected(connected);
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

                    /*
                     * 여기서 다 종료 해뿌자 메모리 누수 없도록
                     */
                    GLB.Dispose();
                    GLB.Logger.Close();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    //Environment.Exit(0);
                }
            }
        }
    }
}
