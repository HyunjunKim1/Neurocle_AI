using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TEST_MAIN_SERVER
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        devServerMain _server;
        public MainWindow()
        {
            InitializeComponent();

            _server = new devServerMain();
            _server.UseLog = true;
            _server.SetParameter_Log(AddLog);
        }

        private void AddLog(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => AddLog(message)));

                return;
            }

            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");

            if (lstLog.Items.Count > 0)
            {
                lstLog.ScrollIntoView(lstLog.Items[lstLog.Items.Count - 1]);
            }

            tbkConnection.Text = _server.ClientConnected ? "Connected" : "Disconnected";
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tBoxPort.Text, out int port)) 
            {
                MessageBox.Show("Port 확인필요~");

                return;
            }

            _server.StartServer(port);

            btnListen.IsEnabled = false;

        }

        private void btnSendInfo_click(object sender, RoutedEventArgs e)
        {
            ProductInfo info = CreateProductInfo();
            _server.SendProductInfo(info);
        }

        private void btnSendStrip_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetStripNumber(out int stripNumber)) return;

            _server.SendStripNumber(stripNumber);
        }

        private void btnInspectionDone_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetStripNumber(out int stripNumber)) return;
            _server.SendInspectionDone(stripNumber);
        }

        private ProductInfo CreateProductInfo()
        {
            return new ProductInfo
            {
                DeviceName = tBoxEquipment.Text.Trim(),
                ProductName = tBoxProduct.Text.Trim(),
                OrderNumber = tBoxOrder.Text.Trim(),
            };
        }

        private bool TryGetStripNumber(out int stripNumber)
        {
            if (!int.TryParse(tbStripNumber.Text, out stripNumber) || stripNumber <= 0)
            {
                MessageBox.Show("Strip 번호 확인 필요~");
                return false;
            }
            return true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _server.Dispose();
        }

        private void btnFullSequence_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetStripNumber(out int stripNumber)) return;

            // 제품 정보
            _server.SendProductInfo(CreateProductInfo());
            Thread.Sleep(100);

            // Strip 번호
            _server.SendStripNumber(stripNumber);
            Thread.Sleep(100);

            // 검사 파일 저장 완료
            _server.SendInspectionDone(stripNumber);
        }
    }
}
