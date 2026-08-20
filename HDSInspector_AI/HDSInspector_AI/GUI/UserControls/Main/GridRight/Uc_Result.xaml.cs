using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.Class.Models;
using HDSInspector_AI.GUI.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace HDSInspector_AI.GUI.UserControls.Main.GridRight
{
    /// <summary>
    /// Uc_Result.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Result : UserControl
    {
        public Uc_Result()
        {
            InitializeComponent();

            Loaded += Uc_Result_Loaded;
            Unloaded += Uc_Result_Unloaded;
        }

        private void Uc_Result_Loaded(object sender, RoutedEventArgs e)
        {
            GLB.InferenceStatistics.StatisticsChanged -= Statistics_Changed;
            GLB.InferenceStatistics.StatisticsChanged += Statistics_Changed;
        }

        private void Uc_Result_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.InferenceStatistics.StatisticsChanged -= Statistics_Changed;
        }


        private void Statistics_Changed(InferenceStatistics statistics)
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => Statistics_Changed(statistics)));

                return;
            }

            if (statistics == null) return;

            tbkProductOrder.Text = $"{statistics.ProductName} / {statistics.OrderNumber}";
            tbkStripNumber.Text = statistics.CurrentStripNumber > 0 ? $"{statistics.CurrentStripNumber:D6}" : "[------]";

            // Current Strip
            tbkStripOK.Text = statistics.StripOKCount.ToString();
            tbkStripNG.Text = statistics.StripNGCount.ToString();
            tbkStripUnknown.Text = statistics.StripUnknownCount.ToString();

            // Product / Order Total
            tbkTotalOK.Text = statistics.TotalOKCount.ToString();
            tbkTotalNG.Text = statistics.TotalNGCount.ToString();
            tbkTotalUnknown.Text = statistics.TotalUnknownCount.ToString();
        }
    }
}
