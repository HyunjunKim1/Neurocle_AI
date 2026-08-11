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

namespace TEST_MAIN_SERVER
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        devServerMain devServer;
        public MainWindow()
        {
            InitializeComponent();

            devServer = new devServerMain();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(int.TryParse(tBoxPort.Text, out int port) == false) { return; }

            devServer.SetParameter_IP(port);
            devServer.StartServer();

            btnListen.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            devServer.SendCommand($"STRIP_NUMBER,1");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            devServer.SendCommand($"INSPECTION_DONE,SUCC");
        }
    }
}
