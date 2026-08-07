using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.Class.Models;
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
    /// Uc_Result.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Result : UserControl
    {
        public Uc_Result()
        {
            InitializeComponent();
        }

        private void SetCurrentInspectionInfo(string eid, string pName, string oNum)
        {
            InspectionInfo info = new InspectionInfo
            {
                EquipmentID = eid,
                ProductName = pName,
                OrderNumber = oNum
            };

            bool succ = GLB.DefectImage.SetInfo(info);

            if (!succ)
            {
                GLB.AddLog("DEFECT", GLB.DefectImage.LastError, Common.SeverityLevel.ERROR);
            }
            else
                GLB.AddLog("DEFECT", $"SUCC - {GLB.DefectImage.CurrentSystemDirectory}", Common.SeverityLevel.INFO);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetCurrentInspectionInfo("EAV44", "(AS)48QFN(4.9X4.9) 3A694R01 9X37X1 R10", "105421727J01");
            for (int i = 0; i < 100; i++)
                GLB.AddLog("test", "TEST", Common.SeverityLevel.INFO);
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
