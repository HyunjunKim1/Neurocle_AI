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
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.GUI.UserControls.Main.GridLeft
{
    /// <summary>
    /// Uc_HW.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_HW : UserControl
    {
        private HardwareMonitorManager _hwMonitor;

        public Uc_HW()
        {
            InitializeComponent();

            //Loaded += Uc_HW_Loaded;
            //Unloaded += Uc_HW_Unloaded;
        }

        //private async void Uc_HW_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_hwMonitor != null) return;
        //
        //    _hwMonitor = new HardwareMonitorManager(driveName: "E:\\", gpuIndex:0);
        //    bool initialized = _hwMonitor.Initialize();
        //
        //    if(!initialized)
        //    {
        //        GLB.WarningMessage(_hwMonitor.LastError, "Class 초기화 실패", GLB.Windows.Main);
        //
        //        return;
        //    }
        //
        //    // CPU PerformanceCounter의 첫 유효 샘플을 위해 잠시 기다린 후 조회
        //    await Task.Delay(1000);
        //
        //    HardwareStatus status = await _hwMonitor.ReadStatusAsync();
        //    StringBuilder msg = new StringBuilder();
        //
        //    msg.AppendLine($"CPU 사용률 : {status.CpuUsagePercent}");
        //
        //    if(status.GpuMemoryUsagePercent > 0)
        //    {
        //        msg.AppendLine($"GPU 사용률 : {status.GpuMemoryUsagePercent}");
        //        msg.AppendLine($"GPU VRAM : {status.GpuMemoryUsedBytes} / {status.GpuMemoryTotalBytes}");
        //    }
        //    else
        //    {
        //        msg.AppendLine($"GPU 사용률 : 0");
        //    }
        //
        //    if(status.isDriveReady)
        //    {
        //        msg.AppendLine($"E 드라이브 사용률 : {status.DriveUsagePercent}");
        //        msg.AppendLine($"E 드라이브 사용량 : {status.DriveUsedBytes} / {status.DriveTotalBytes}");
        //        msg.AppendLine($"E 드라이브 남은 용량 : {FormatBytes(status.DriveFreeBytes)}");
        //    }
        //    else
        //    {
        //        msg.AppendLine("E 드라이브 사용 불가");
        //    }
        //
        //    if (!string.IsNullOrWhiteSpace(status.ErrorMessage))
        //    {
        //        msg.AppendLine();
        //        msg.AppendLine("오류정보");
        //        msg.AppendLine(status.ErrorMessage);
        //    }
        //
        //    MessageBox.Show(msg.ToString(), "HW 통합테스트", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
        //private void Uc_HW_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    _hwMonitor?.Dispose();
        //    _hwMonitor = null;
        //}
        //
        //private static string FormatBytes(ulong bytes)
        //{
        //    const double gb = 1024.0 * 1024.0 * 1024.0;
        //    const double tb = gb * 1024.0;
        //
        //    if (bytes >= tb) return $"{bytes / tb:F2} TB";
        //    
        //    return $"{bytes / gb:F1} GB";
        //}
        //private static string FormatBytes(long bytes)
        //{
        //    if (bytes < 0) return "0 GB";
        //    
        //    return FormatBytes((ulong)bytes);
        //}
    }
}
