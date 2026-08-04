using HDSInspector_AI.Class.Devices.NVML;
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

namespace HDSInspector_AI.GUI.UserControls.Main.GridLeft
{
    /// <summary>
    /// Uc_HW.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_HW : UserControl
    {
        private NvmlMonitor _nvmlMonitor;
        public Uc_HW()
        {
            InitializeComponent();

            //Loaded += Uc_HW_Loaded;
            //Unloaded += Uc_HW_Unloaded;
        }

        //private void Uc_HW_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_nvmlMonitor != null)
        //        return;
        //
        //    _nvmlMonitor = new NvmlMonitor();
        //
        //    bool initialized = _nvmlMonitor.Initialize(gpuIndex:0);
        //
        //    if(!initialized)
        //    {
        //        MessageBox.Show(_nvmlMonitor.LastError, "NVML 이니셜 실패", MessageBoxButton.OK, MessageBoxImage.Error);
        //
        //        return;
        //    }
        //
        //    bool success = _nvmlMonitor.TryGetGpuUtilization(out uint gpuUsage);
        //
        //    if (!success) 
        //    {
        //        MessageBox.Show(_nvmlMonitor.LastError, "GPU 정보 조회 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
        //
        //        return;
        //    }
        //
        //    MessageBox.Show("성공\n" + $"GPU 개수 : {_nvmlMonitor.DeviceCount}\n" + $"사용 GPU Index : {_nvmlMonitor.GpuIndex}" + $"GPU 이용률 : {gpuUsage}", "TEST", MessageBoxButton.OK, MessageBoxImage.Information);
        //
        //}
        //private void Uc_HW_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    _nvmlMonitor?.Dispose();
        //    _nvmlMonitor = null;
        //}

    }
}
