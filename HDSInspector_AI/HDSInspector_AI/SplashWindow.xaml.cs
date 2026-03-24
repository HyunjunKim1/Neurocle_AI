using HDSInspector_AI.Class.GlobalFunction;
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
using System.Windows.Shapes;

namespace HDSInspector_AI
{
    /// <summary>
    /// SplashScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplashWindow : Window
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;
        CustomThread _threadSplash;
        int _step = 0;

        public SplashWindow()
        {
            InitializeComponent();


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;

            GLB.ApplyFadeAndZoomAnimation(this, GridSplash, durationMs:500);
        }
    }
}
