using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.GUI.UserControls.Main.GridLeft
{
    /// <summary>
    /// Uc_Log.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Log : UserControl
    {
        // Log 최대 개수
        private const int MaxDisplayLogCount = 1000;

        public ObservableCollection<LogDisplayItem> LogItems { get; }

        public Uc_Log()
        {
            InitializeComponent();

            LogItems = new ObservableCollection<LogDisplayItem>();

            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 혹시나 중복 구독이 되면 중복 기입이 되니 해제 후 재 구독
            GLB.LogAdded -= Global_LogAdded;
            GLB.LogAdded += Global_LogAdded;

            UpdateLogCount();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.LogAdded -= Global_LogAdded;
        }

        private void Global_LogAdded(LogDisplayItem logItem)
        {
            if (logItem == null) return;

            // AddLog가 검사 또는 통신 쓰레드에서 호출될수 있으니까 
            // 이 Uc의 UI 디스패쳐 사용하자. C#에서 form InvokeRequirement 같은거임
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => AddLogItem(logItem)));

                return;
            }

            AddLogItem(logItem);
        }

        private void AddLogItem(LogDisplayItem logItem)
        {
            LogItems.Add(logItem);

            // 메모리 증가 방지, 첫 번째 놈을 계속 지우기
            while (LogItems.Count > MaxDisplayLogCount) { LogItems.RemoveAt(0); }

            UpdateLogCount();

            if (cBoxAutoScroll.IsChecked == true) { ScrollToLastestLog(); }
        }

        private void ScrollToLastestLog()
        {
            if(LogItems.Count == 0 ) return;

            object lastItem = LogItems[LogItems.Count - 1];
            lvLogs.ScrollIntoView(lastItem);
        }

        private void UpdateLogCount()
        {
            if (tbkLogCount == null) return;

            tbkLogCount.Text = LogItems.Count.ToString("N0");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // 화면 표시 로그만 제거함
            LogItems.Clear();
            UpdateLogCount();
        }
    }
}
