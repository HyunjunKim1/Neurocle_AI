using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDSInspector_AI.GUI.UserControls.Main.GridRight
{
    /// <summary>
    /// SubUc_InferenceCameraPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubUc_InferenceCameraPanel : UserControl
    {
        public static readonly DependencyProperty OKItemsProperty = DependencyProperty.Register(nameof(OKItems), typeof(IEnumerable), typeof(SubUc_InferenceCameraPanel));
        public static readonly DependencyProperty NGItemsProperty = DependencyProperty.Register(nameof(NGItems), typeof(IEnumerable), typeof(SubUc_InferenceCameraPanel));
        public static readonly DependencyProperty UnknownItemsProperty = DependencyProperty.Register(nameof(UnknownItems), typeof(IEnumerable), typeof(SubUc_InferenceCameraPanel));

        private const double MouseWheelScrollStep = 50.0;

        public IEnumerable OKItems
        {
            get => (IEnumerable)GetValue(OKItemsProperty);
            set => SetValue(OKItemsProperty, value);
        }

        public IEnumerable NGItems
        {
            get => (IEnumerable)GetValue(NGItemsProperty);
            set => SetValue(NGItemsProperty, value);
        }

        public IEnumerable UnknownItems
        {
            get => (IEnumerable)GetValue(UnknownItemsProperty);
            set => SetValue(UnknownItemsProperty, value);
        }

        public SubUc_InferenceCameraPanel()
        {
            InitializeComponent();
            ScrollToStart();
        }

        private void HorizontalScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;

            if (scrollViewer == null) return;

            double direction = e.Delta > 0 ? -1.0 : 1.0;
            double newOffset = scrollViewer.HorizontalOffset + direction * MouseWheelScrollStep;

            newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableWidth));

            scrollViewer.ScrollToHorizontalOffset(newOffset);

            e.Handled = true;
        }

        public void ScrollToStart()
        {
            svOK.ScrollToLeftEnd();
            svNG.ScrollToLeftEnd();
            svUnknown.ScrollToLeftEnd();
        }
    }
}
