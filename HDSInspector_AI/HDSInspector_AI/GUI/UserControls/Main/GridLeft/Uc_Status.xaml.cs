using HDSInspector_AI.Class.Models;
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

namespace HDSInspector_AI.GUI.UserControls.Main.GridLeft
{
    /// <summary>
    /// Uc_Status.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Status : UserControl
    {
        public Uc_Status()
        {
            InitializeComponent();

            Loaded      += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GLB.DefectImage.InspectionInfoChanged -= DefectImage_InspectionInfoChanged;
            GLB.DefectImage.InspectionInfoChanged += DefectImage_InspectionInfoChanged;

            if (GLB.DefectImage.CurrentInfo != null)
                SetInspectionInfo(GLB.DefectImage.CurrentInfo);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.DefectImage.InspectionInfoChanged -= DefectImage_InspectionInfoChanged;
        }

        private void DefectImage_InspectionInfoChanged(InspectionInfo info)
        {
            SetInspectionInfo(info);
        }

        public void SetInspectionInfo(InspectionInfo info)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => SetInspectionInfo(info)));

                return;
            }

            tbkEquipmentNum.Text = string.IsNullOrWhiteSpace(info.DeviceName) ? "-" : info.DeviceName;
            tbkProductName.Text  = string.IsNullOrWhiteSpace(info.ProductName) ? "-" : info.ProductName;
            tbkOrderNumber.Text  = string.IsNullOrWhiteSpace(info.OrderNumber) ? "-" : info.OrderNumber;

        }

        public void ClearInspectionInfo()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(ClearInspectionInfo));

                return;
            }
            tbkEquipmentNum.Text = "-";
            tbkProductName.Text  = "-";
            tbkOrderNumber.Text  = "-";
        }

        public void SetMainSWConnected(bool isConnected)
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => SetMainSWConnected(isConnected)));

                return;
            }

            string imgPath = isConnected ? "/HDSInspector_AI;component/Resources/CHECK_ON.png" : "/HDSInspector_AI;component/Resources/CHECK_OFF.png";
            iBox_MainSW.Source = new BitmapImage(new Uri(imgPath, UriKind.Relative));

            btnMainSwConnect.Content = isConnected ? "Connected" : "Connect";
            btnMainSwConnect.Background = isConnected ? System.Windows.Media.Brushes.SeaGreen : Brushes.Firebrick;
        }

    }
}
