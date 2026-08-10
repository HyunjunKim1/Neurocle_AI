using System;
using System.Collections;
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

namespace HDSInspector_AI.GUI.UserControls.Main.GridRight
{
    /// <summary>
    /// SubUc_DefectPairPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubUc_DefectPairPanel : UserControl
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(SubUc_DefectPairPanel), new PropertyMetadata(null));

        private bool _isSynchronizingScroll;

        public IEnumerable Items
        {
            get
            {
                return (IEnumerable)GetValue(ItemsProperty);
            }
            set
            {
                SetValue(ItemsProperty, value);
            }
        }
        public SubUc_DefectPairPanel()
        {
            InitializeComponent();
        }

        private void svReference_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            SynchronizeScrollViewer(svReference, svDefect, e);
        }

        private void svDefect_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            SynchronizeScrollViewer(svDefect, svReference, e);
        }

        private void SynchronizeScrollViewer(ScrollViewer source,  ScrollViewer target, ScrollChangedEventArgs e)
        {
            if (_isSynchronizingScroll) return;

            if (e.HorizontalChange == 0.0) return;
            try
            {
                _isSynchronizingScroll = true;
                target.ScrollToHorizontalOffset(source.HorizontalOffset);
            }
            finally
            {
                _isSynchronizingScroll = false;
            }
        }

        public void ScrollToStart()
        {
            svReference.ScrollToLeftEnd();
            svDefect.ScrollToLeftEnd();
        }
    }
}
