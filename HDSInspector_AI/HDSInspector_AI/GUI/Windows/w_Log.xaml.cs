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

namespace HDSInspector_AI.GUI.Windows
{
    /// <summary>
    /// w_Log.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class w_Log : Window
    {
        public w_Log()
        {
            InitializeComponent();
        }

        public void AddLog(string text)
        {
            this.Dispatcher.BeginInvoke(new Action(() => { AddLog(text); }));
        }
    }
}
