using HDSInspector_AI.Class.GlobalFunctions;
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
    /// Uc_Control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Control : UserControl
    {
        public Uc_Control()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //GLB.ImgProc.ImageMerge("D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_R.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_G.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\L03K023R01\\GrabImage_00_B.bmp");
            GLB.ImgProc.ExtractUnits("D:\\ftp\\Images\\Detection_AI\\SourceImages\\Merged.bmp", "D:\\ftp\\Images\\Detection_AI\\SourceImages\\Unit.bmp");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GLB.Windows.Review.Dispatcher.Invoke(new Action(() =>
            {
                if (GLB.Windows.Review.Visibility == Visibility.Visible)
                    GLB.Windows.Review.Visibility = Visibility.Hidden;
                else
                {
                    GLB.Windows.Review.Topmost = true;
                    GLB.Windows.Review.ShowInTaskbar = true;
                    GLB.Windows.Review.Visibility = Visibility.Visible;
                    GLB.Windows.Review.WindowState = WindowState.Normal;

                    GLB.Windows.Review.Topmost = false;
                }
            }));
        }
    }
}
