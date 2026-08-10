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
        private readonly DefectImageCutter _imageCutter;
        private readonly DefectTextParser  _textParser;

        // Simulation 용
        private int _simultationSequence = 1;

        public ObservableCollection<DefectImagePairItem> TopDefectPairs { get; }
        public ObservableCollection<DefectImagePairItem> BottomDefectPairs { get; }
        public ObservableCollection<DefectImagePairItem> TransDefectPairs { get; }


        public Uc_DefectImage()
        {
            InitializeComponent();

            _imageCutter = new DefectImageCutter();
            _textParser  = new DefectTextParser();

            TopDefectPairs      = new ObservableCollection<DefectImagePairItem>();
            BottomDefectPairs   = new ObservableCollection<DefectImagePairItem>();
            TransDefectPairs    = new ObservableCollection<DefectImagePairItem>();

            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GLB.DefectImage.InspectionImageReady -= DefectImages_InspectionImageReady;
            GLB.DefectImage.InspectionImageReady += DefectImages_InspectionImageReady;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GLB.DefectImage.InspectionImageReady -= DefectImages_InspectionImageReady;
        }

        private void DefectImages_InspectionImageReady(DefectImageFileSet fileSet)
        {
            // Main 통신 Thread에서 Event 올 수 있으므로, UI Dispatcher 처리하자
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => DefectImages_InspectionImageReady(fileSet)));

                return;
            }

            LoadFileSet(fileSet);
        }

        public void LoadFileSet(DefectImageFileSet fileSet)
        {
            ClearImages();

            if (fileSet == null) return;

            tbkSequence.Text = $"[{fileSet.SequenceNumber:D6}]";

            if(!fileSet.HasAnyImage)
            {
                GLB.AddLog("DEFECT", $"[{fileSet.SequenceNumber:D6}] 불량 이미지 없음.", SeverityLevel.INFO);

                return;
            }

            if (fileSet.HasTopImage)
                LoadCameraImage(fileSet.TopImagePath, TopDefectPairs, "TOP");

            if (fileSet.HasBottomImage)
                LoadCameraImage(fileSet.TopImagePath, BottomDefectPairs, "BOTTOM");

            if (fileSet.HasTransImage)
                LoadCameraImage(fileSet.TopImagePath, TransDefectPairs, "TRANS");

            pnlTop.ScrollToStart();
            pnlBottom.ScrollToStart();
            pnlTrans.ScrollToStart();
        }

        public void LoadCameraImage(string imagePath, ObservableCollection<DefectImagePairItem> targetCollection, string camName)
        {
            try
            {
                // txt에서 실제 불량 개수 확인
                int defectCount;
                bool textSucc = _textParser.TryGetDefectCount(imagePath.Substring(0, imagePath.Length - 4) + ".txt", out defectCount);
                if (!textSucc) 
                {
                    GLB.AddLog("DEFECT", $"{camName} txt 읽기 실패 : {imagePath}", SeverityLevel.ERROR);

                    return;
                }
                if (defectCount <= 0) return;

                BitmapSource mergedImage = LoadBitmapWithoutLock(imagePath);
                List<DefectImagePairItem> pairItems;
                bool cuttingSucc = _imageCutter.CuttingImage(mergedImage, defectCount, out pairItems);

                if (!cuttingSucc)
                {
                    GLB.AddLog("DEFECT", $"{camName} 이미지 Cutting 실패.", SeverityLevel.ERROR);

                    return;
                }

                foreach(DefectImagePairItem item in pairItems)
                    targetCollection.Add(item);

                GLB.AddLog("DEFECT", $"{camName} : {defectCount}개 Pair Load 완료", SeverityLevel.INFO);
            }
            catch(Exception ex)
            {
                GLB.AddLog("DEFECT", $"{camName} 이미지 처리 오류 : {ex.Message}", SeverityLevel.ERROR);
            }
        }

        private static BitmapSource LoadBitmapWithoutLock(string imagePath)
        {
            using(FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = stream;
                bmp.EndInit();

                bmp.Freeze();

                return bmp;
            }
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
            GLB.Windows.Status.SetInspectionInfo("EAV44", "(AS)48QFN(4.9X4.9) 3A694R01 9X37X1 R10", "105421727J01");

            int currentSequence = _simultationSequence;
            bool succ = GLB.DefectImage.ProcessInspectionComplete(currentSequence);
            if(!succ)
                GLB.AddLog("DEFECT", $"Simulation Trigger 실패 : [{currentSequence}] / {GLB.DefectImage.LastError}", SeverityLevel.ERROR);

            GLB.AddLog("DEFECT", $"Simulation Trigger : Strip [{currentSequence:D6}]", SeverityLevel.ERROR);

            _simultationSequence++;
        }

        #endregion
        private List<int> CreateAllIndexList(BitmapSource mergedImage)
        {
            const int columnCount = 5;

            int tileSize = mergedImage.PixelWidth / columnCount;
            int pairRowCount = mergedImage.PixelHeight / (tileSize * 2);
            int totalCount = columnCount * pairRowCount;

            List<int> indexes = new List<int>();

            for (int i = 0; i < totalCount; i++)
                indexes.Add(i);

            return indexes;
        }

        private BitmapSource LoadBitmap(string imagePath)
        {
            BitmapImage bmp = new BitmapImage();
            
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(imagePath, UriKind.Absolute);
            bmp.EndInit();

            bmp.Freeze();

            return bmp;
        }


    }       
}
