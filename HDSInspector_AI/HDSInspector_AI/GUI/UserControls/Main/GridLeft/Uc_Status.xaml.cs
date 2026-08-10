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
        }

        public void SetInspectionInfo(string equipmentNumber, string productName, string orderNumber)
        {
            GLB.Windows.Status.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbkEquipmentNum.Text = equipmentNumber;
                tbkProductName.Text = string.IsNullOrWhiteSpace(productName) ? "-" : productName.Trim();
                tbkOrderNumber.Text = string.IsNullOrWhiteSpace(orderNumber) ? "-" : orderNumber.Trim();
            }));

            InspectionInfo info = new InspectionInfo();
            info.EquipmentID = equipmentNumber;
            info.ProductName = productName;
            info.OrderNumber = orderNumber;

            GLB.DefectImage.SetInfo(info);
        }

        public void ClearInspectionInfo()
        {
            SetInspectionInfo("-", "-", "-");
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
