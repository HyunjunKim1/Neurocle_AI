using ControlzEx.Behaviors;
using HDSInspector_AI.Class.Devices.NVML;
using HDSInspector_AI.Class.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
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
using System.Windows.Threading;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.GUI.UserControls.Main.GridLeft
{
    /// <summary>
    /// CPU, GPU, Drive Status 표시 Control
    /// </summary>
    public partial class Uc_HW : UserControl
    {
        private readonly DispatcherTimer _updateTimer;
        private bool _isUpdating;

        public Uc_HW()
        {
            InitializeComponent();

            _updateTimer = new DispatcherTimer(DispatcherPriority.Background);
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
        }


        #region Window Functions
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Global HW Manager는 한번만 초기화됨. Initialized 내부에서 중복 초기화 방지함
            bool initialized = GLB.Hardware.Initialize();

            if(!initialized)
            {
                SetHardwareUnavailable(GLB.Hardware.LastError);

                return;
            }

            // CPU PerformanceCounter의 첫 샘플 확보하는데 시간이 좀 걸림. 최초 한번만 1초정도 기다리자
            await Task.Delay(1000);

            await UpdateHardwareStatusAsync();

            _updateTimer.Start();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Timer만 Stop함. dispose는 나중에 Global에서 됨
            _updateTimer.Stop();
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateHardwareStatusAsync();
        }

        #endregion

        #region Update Status
        private async Task UpdateHardwareStatusAsync()
        {
            // 이전 조회가 끝나지 않았을 경우 이번 Tick은 건너뛰자 ~ 
            // 어짜피 실시간으로 계속 확인할 필요없고 단순 Status이기때문에 걍 쓰자. 작업 중첩도 방지할겸

            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                HardwareStatus status = await GLB.Hardware.ReadStatusAsync();

                UpdateCpuStatus(status);
                UpdateGpuStatus(status);
                UpdateDriveStatus(status);
                UpdateOverallStatus(status);
            }
            catch (ObjectDisposedException)
            {
                SetHardwareUnavailable("Hardware Manager가 종료되었습니다.");
            }
            catch (Exception ex)
            {
                SetHardwareUnavailable(ex.Message);

                GLB.AddLog("HARDWARE", $"하드웨어 상태 조회 실패 : {ex.Message}", Common.SeverityLevel.ERROR);
            }
            finally { _isUpdating = false; }
        }

        private void UpdateCpuStatus(HardwareStatus status)
        {
            double usage = ClampPercent(status.CpuUsagePercent);

            pBarCpuUsage.Value = usage;
            pBarCpuUsage.Foreground = GetUsageBrush(usage);

            tbkCpuUsage.Text = $"{usage:F1} %";
            tbkCpuUsage.Foreground = GetUsageBrush(usage);
        }

        private void UpdateGpuStatus(HardwareStatus status)
        {
            if(!status.IsGpuAvailable)
            {
                pBarGpuUsage.Value = 0;
                pBarGpuUsage.Foreground = Brushes.Gray;
                tbkGpuUsage.Text = "N/A";
                tbkGpuUsage.Foreground = Brushes.Gray;
                tbkGpuMemory.Text = "VRAM 정보를 알 수 없습니다.";

                return;
            }

            double usage = ClampPercent(status.GpuUsagePercent);
            pBarGpuUsage.Value = usage;
            pBarGpuUsage.Foreground = GetUsageBrush(usage);
            tbkGpuUsage.Text = $"{usage:F1} %";

            tbkGpuMemory.Text = $"VRAM: {FormatBytes(status.GpuMemoryUsedBytes)} / {FormatBytes(status.GpuMemoryTotalBytes)} ({status.GpuMemoryUsagePercent:F1} %)";
        }

        private void UpdateDriveStatus(HardwareStatus status)
        {
            if(!status.IsDriveReady)
            {
                pBarDriveUsage.Value = 0;
                pBarDriveUsage.Foreground = Brushes.Gray;
                tbkDriveUsage.Text = "N/A";
                tbkDriveUsage.Foreground = Brushes.Gray;
                tbkDriveCapacity.Text = "드라이브 정보를 알 수 없습니다.";

                return;
            }

            double usage = ClampPercent(status.DriveUsagePercent);

            Brush driveBrush = GetDriveBrush(usage, status.DriveFreeBytes);

            pBarDriveUsage.Value = usage;
            pBarDriveUsage.Foreground = driveBrush;
            tbkDriveUsage.Text = $"{usage:F1} %";
            tbkDriveCapacity.Text = $"Free {FormatBytes(status.DriveFreeBytes)} / Total {FormatBytes(status.DriveTotalBytes)}";
        }

        private void UpdateOverallStatus(HardwareStatus status)
        {
            bool driveCritical = status.IsDriveReady && status.DriveFreeBytes <= 30UL * 1024UL * 1024UL * 1024UL; // 30GB 이하일때 Critical 표시

            bool hasError = !string.IsNullOrWhiteSpace(status.ErrorMessage);

            if(driveCritical)
            {
                ellStatus.Fill = Brushes.OrangeRed;
                tbkStatus.Text = "Disk Low";
                tbkStatus.Foreground = Brushes.OrangeRed;

                return;
            }

            if (hasError)
            {
                ellStatus.Fill = Brushes.Orange;
                tbkStatus.Text = "Warning";
                tbkStatus.Foreground = Brushes.Orange;
                return;
            }

            ellStatus.Fill = Brushes.LimeGreen;
            tbkStatus.Text = "Normal";
            tbkStatus.Foreground = Brushes.LimeGreen;
        }

        private void SetHardwareUnavailable(string errorMessage)
        {
            _updateTimer.Stop();

            ellStatus.Fill = Brushes.OrangeRed;
            tbkStatus.Text = "Error";
            tbkStatus.Foreground = Brushes.OrangeRed;

            pBarCpuUsage.Foreground = Brushes.Gray;
            tbkCpuUsage.Text = "N/A";
            tbkGpuUsage.Text = "N/A";
            tbkDriveUsage.Text = "N/A";

            pBarCpuUsage.Value = 0;
            pBarGpuUsage.Value = 0;
            pBarDriveUsage.Value = 0;

            tbkCpuUsage.Foreground = Brushes.Gray;
            pBarGpuUsage.Foreground = Brushes.Gray;
            tbkGpuUsage.Foreground = Brushes.Gray;

            tbkGpuMemory.Text = "GPU 정보를 알 수 없습니다.";

            if (!string.IsNullOrWhiteSpace(errorMessage)) ToolTip = errorMessage;

            GLB.AddLog("HARDWARE", $"하드웨어 상태 조회 실패 : {errorMessage}", Common.SeverityLevel.ERROR);
        }

        private static Brush GetUsageBrush(double usage)
        {
            if (usage >= 90.0) return Brushes.OrangeRed;
            if(usage >= 75.0) return Brushes.Orange;

            return Brushes.SkyBlue;
        }

        private static Brush GetDriveBrush(double usage, ulong freeBytes)
        {
            ulong thirtyGB = 30UL * 1024UL * 1024UL * 1024UL;
            ulong oneHnitGB = 100UL * 1024UL * 1024UL * 1024UL;

            if(usage >= 95.0 || freeBytes <= thirtyGB) return Brushes.OrangeRed;
            if (usage >= 85.0 || freeBytes <= oneHnitGB) return Brushes.Orange;
            
            return Brushes.SkyBlue;
        }

        private static double ClampPercent(double value)
        {
            if (value < 0.0) return 0.0;
            if (value > 100.0) return 100.0;
            return value;
        }

        private static string FormatBytes(ulong bytes)
        {
            const double KB = 1024.0;
            const double MB = KB * 1024.0;
            const double GB = MB * 1024.0;
            const double TB = GB * 1024.0;

            if (bytes >= TB) return $"{bytes / TB:F2} TB";

            return $"{bytes / GB:F1} GB";
        }
        #endregion

    }
}
