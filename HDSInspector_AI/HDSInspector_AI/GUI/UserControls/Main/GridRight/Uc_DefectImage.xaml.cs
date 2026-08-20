using Common;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using Path = System.IO.Path;

namespace HDSInspector_AI.GUI.UserControls.Main.GridRight
{/// <summary>
 /// 불량 이미지 상태 표시 UserControl
 /// </summary>
    public partial class Uc_DefectImage :UserControl
    {
        // Simulation 용
        private int _simultationSequence = 1;

        public ObservableCollection<DefectImagePairItem> TopDefectPairs { get; }
        public ObservableCollection<DefectImagePairItem> BottomDefectPairs { get; }
        public ObservableCollection<DefectImagePairItem> TransDefectPairs { get; }


        public Uc_DefectImage()
        {
            InitializeComponent();

            TopDefectPairs      = new ObservableCollection<DefectImagePairItem>();
            BottomDefectPairs   = new ObservableCollection<DefectImagePairItem>();
            TransDefectPairs    = new ObservableCollection<DefectImagePairItem>();

            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GLB.Inference.DefectDataReady -= Inference_DefectDataReady;
            GLB.Inference.DefectDataReady += Inference_DefectDataReady;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.Inference.DefectDataReady -= Inference_DefectDataReady;
        }

        private void Inference_DefectDataReady(StripDefectData data)
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => Inference_DefectDataReady(data)));

                return;
            }

            if (data == null)
                return;
            ClearImages();

            tbkSequence.Text = $"[{data.StripNumber:D6}]";

            foreach(DefectImagePairItem item in data.TopPairs)
                TopDefectPairs.Add(item);
            foreach (DefectImagePairItem item in data.BottomPairs)
                BottomDefectPairs.Add(item);
            foreach (DefectImagePairItem item in data.TransPairs)
                TransDefectPairs.Add(item);

            pnlTop.ScrollToStart();
            pnlBottom.ScrollToStart();
            pnlTrans.ScrollToStart();
        }

        public void ClearImages()
        {
            TopDefectPairs.Clear();
            BottomDefectPairs.Clear();
            TransDefectPairs.Clear();

            tbkSequence.Text = "[------]";
        }

        #region Simulation 

        private void btnSimulationTrigger_Click(object sender, RoutedEventArgs e)
        {
            InspectionInfo info = new InspectionInfo
            {
                DeviceName = "EAV44",
                ProductName = "(AS)48QFN(4.9X4.9) 3A694R01 9X37X1 R10",
                OrderNumber = "105421727J01"
            };

            GLB.DefectImage.SetInfo(info);

            int currentSequence = _simultationSequence;
            bool succ = GLB.DefectImage.ProcessInspectionComplete(currentSequence);
            if(!succ)
                GLB.AddLog("DEFECT", $"Simulation Trigger 실패 : [{currentSequence}] / {GLB.DefectImage.LastError}", SeverityLevel.ERROR);

            GLB.AddLog("DEFECT", $"Simulation Trigger : Strip [{currentSequence:D6}]", SeverityLevel.INFO);

            _simultationSequence++;
        }

        #endregion
       
    }       
}
