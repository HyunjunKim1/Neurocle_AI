using ControlzEx.Behaviors;
using HDSInspector_AI.Class.GlobalFunctions;
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

namespace HDSInspector_AI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GLB.ImgProc.ImageMerge("D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_R.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_G.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_B.bmp");
            //GLB.ImgProc.ExtractUnits("D:\\ftp\\Images\\Detection_AI\\SourceImages\\Merged.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\Unit.bmp");
        }
    }
}
