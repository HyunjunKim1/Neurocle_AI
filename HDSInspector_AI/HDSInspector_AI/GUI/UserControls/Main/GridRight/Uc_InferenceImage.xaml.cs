using ControlzEx.Behaviors;
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
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.GUI.UserControls.Main.GridRight
{
    /// <summary>
    /// Uc_InferenceImage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_InferenceImage : UserControl
    {
        // 9개 컬렉션 생성하자. 각기 다른걸 한번에 관리할 무언가를 찾질 못하겠음. 걍 9개 고정이니, 다 적자
        // 상부
        public ObservableCollection<InferenceImageDisplayItem> TopOKItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> TopNGItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> TopUnknownItems { get; }

        // 하부
        public ObservableCollection<InferenceImageDisplayItem> BottomOKItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> BottomNGItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> BottomUnknownItems { get; }

        // 투과
        public ObservableCollection<InferenceImageDisplayItem> TransOKItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> TransNGItems { get; }
        public ObservableCollection<InferenceImageDisplayItem> TransUnknownItems { get; }

        private int _currentStripNumber;

        public Uc_InferenceImage()
        {
            InitializeComponent();

            TopOKItems = new ObservableCollection<InferenceImageDisplayItem>();
            TopNGItems = new ObservableCollection<InferenceImageDisplayItem>();
            TopUnknownItems = new ObservableCollection<InferenceImageDisplayItem>();

            BottomOKItems = new ObservableCollection<InferenceImageDisplayItem>();
            BottomNGItems = new ObservableCollection<InferenceImageDisplayItem>();
            BottomUnknownItems = new ObservableCollection<InferenceImageDisplayItem>();

            TransOKItems = new ObservableCollection<InferenceImageDisplayItem>();
            TransNGItems = new ObservableCollection<InferenceImageDisplayItem>();
            TransUnknownItems = new ObservableCollection<InferenceImageDisplayItem>();

            DataContext = this;

            Loaded += Uc_InferenceImage_Loaded;
            Unloaded += Uc_InferenceImage_Unloaded;
        }

        private void Uc_InferenceImage_Loaded(object sender, RoutedEventArgs e)
        {
            GLB.Inference.InferenceImageReady -= Inference_InferenceImageReady;
            GLB.Inference.InferenceImageReady += Inference_InferenceImageReady;
        }


        private void Uc_InferenceImage_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.Inference.InferenceImageReady -= Inference_InferenceImageReady;
        }

        private void Inference_InferenceImageReady(InferenceImageDisplayItem item)
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => Inference_InferenceImageReady(item)));

                return;
            }

            // Strip이 변경되면 화면은 현재 Strip만 보여주자
            if(_currentStripNumber != item.StripNumber)
            {
                _currentStripNumber = item.StripNumber;

                ClearItems();

                tbkStripNumber.Text = $"[{item.StripNumber:D6}]";
            }

            ObservableCollection<InferenceImageDisplayItem> target = GetTargetCollection(item.CameraType, item.Judgement);

            target?.Add(item);
        }

        private ObservableCollection<InferenceImageDisplayItem> GetTargetCollection(InspectionCameraType cameraType, AIJudgement judgement)
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    switch (judgement)
                    {
                        case AIJudgement.OK:
                            return TopOKItems;
                        case AIJudgement.NG:
                            return TopNGItems;
                        case AIJudgement.Unknown:
                            return TopUnknownItems;
                        default:
                            return TopUnknownItems;
                    }

                case InspectionCameraType.Bottom:
                    switch (judgement)
                    {
                        case AIJudgement.OK:
                            return BottomOKItems;
                        case AIJudgement.NG:
                            return BottomNGItems;
                        case AIJudgement.Unknown:
                            return BottomUnknownItems;
                        default:
                            return BottomUnknownItems;
                    }

                case InspectionCameraType.Trans:
                    switch (judgement)
                    {
                        case AIJudgement.OK:
                            return TransOKItems;
                        case AIJudgement.NG:
                            return TransNGItems;
                        case AIJudgement.Unknown:
                            return TransUnknownItems;
                        default:
                            return TransUnknownItems;
                    }
            }

            return null;
        }

        private void ClearItems()
        {
            TopOKItems.Clear();
            TopNGItems.Clear() ;
            TopUnknownItems.Clear();

            BottomOKItems.Clear();
            BottomNGItems.Clear();
            BottomUnknownItems.Clear();

            TransOKItems.Clear();
            TransNGItems.Clear();
            TransUnknownItems.Clear();
        }
    }
}
