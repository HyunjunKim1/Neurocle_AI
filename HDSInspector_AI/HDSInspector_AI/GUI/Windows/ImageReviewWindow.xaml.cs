using Common;
using Common.Drawing;
using HDSInspector_AI.Class.GlobalFunctions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;

namespace HDSInspector_AI.GUI.Windows
{
    public delegate void ToolTypeChangeEventHandler(ToolType newToolType);
    /// <summary>
    /// ImageReivewWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageReviewWindow : Window
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;

        public static event ToolTypeChangeEventHandler ToolTypeChangeEvent;
        // 멤버변수
        private double _zoomToFitScale = 1.0;
        private System.Windows.Point? _ptLastDragPoint;
        private System.Windows.Point? _ptLastContentMousePosition;
        private System.Windows.Point? _ptLastCenterOfViewport;

        private Point _tmpPoint;

        private Algo _algo = new Algo();

        #region Properties

        // index 0 : origin, 1 : 25% resize
        private int NowImageIndex 
        {
            get
            {
                if (chkResize.IsChecked == true)
                    return 1;
                else return 0;
            }
        }
        // index 0 : origin, 1 : 25% resize
        public double ViewerHeight { get; set; }
        public double ViewerWidth { get; set; }
        public DrawingCanvas BasedCanvas { get; set; }  // Drawing Canvas
        public AntiAliasedImage BasedImage { get; set; } // Image Control
        public BitmapSource BaseImageSource{ get; set; } // Origin Image


        private int SourceHeight
        {
            get
            {
                if (BaseImageSource != null)
                    return BaseImageSource.PixelHeight;
                else
                    return -1;
            }
        }

        private int SourceWidth
        {
            get
            {
                if (BaseImageSource != null)
                    return BaseImageSource.PixelWidth;
                else
                    return -1;
            }
        }

        private double ZoomValue
        {
            get { return sldrScale.Value; }
            set
            {
                sldrScale.Value = value;
                UpdateScale();
            }
        }

        #endregion

        public ImageReviewWindow()
        {
            InitializeComponent();
            InitializeEvents();
            InitializeDialogs();
        }

        private void InitializeDialogs()
        {
            BasedImage = new AntiAliasedImage()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 0,
                Height = 0
            };
            BasedCanvas = new DrawingCanvas(true, false)
            {
                MaxGraphicsCount = 64, // 전체영상에서는 ROI(Section)을 64개까지 그릴 수 있다.
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 0,
                Height = 0
            };

            ToolChange(ToolType.Pointer);

            pnlMultiBinarization.Visibility = Visibility.Hidden; 
            radSingleThreshold.IsChecked = true;
            chkBinarization.IsChecked = false;

            sldrLowerThreshold.Value = 100;
            sldrUpperThreshold.Value = 200;
        }

        private void InitializeEvents()
        {
            // Zoom events.
            this.btnZoomIn.Click += zoomBtn_Click;
            this.btnZoomOut.Click += zoomBtn_Click;
            this.btnZoomToFit.Click += zoomBtn_Click;
            this.sldrScale.ValueChanged += sldrScale_ValueChanged;

            this.cvsCross.MouseEnter += CrossCanvas_MouseEnter;
            this.cvsCross.MouseLeave += CrossCanvas_MouseLeave;
        }

        public void ToolChange(ToolType newTool)
        {
            if (BasedCanvas == null) return;
            this._ptLastDragPoint = null;

            BasedCanvas.UnselectAll();
            BasedCanvas.Tool = newTool;

            ToolTypeChangeEventHandler eventRunner = ToolTypeChangeEvent;
            if (eventRunner != null)
            {
                eventRunner(newTool);
            }
        }

        #region Mouse Events

        #endregion

        #region Cross Canvas Events

        private static Line _horizontalLine = new Line() { Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 1.0 };
        private static Line _verticalLine = new Line() { Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 1.0 };

        private void InitializeIndicator()
        {
            _horizontalLine.X1 = 0;
            _horizontalLine.Y1 = -1;
            _horizontalLine.X2 = cvsCross.ActualWidth;
            _horizontalLine.Y2 = -1;

            _verticalLine.X1 = -1;
            _verticalLine.Y1 = 0;
            _verticalLine.X2 = -1;
            _verticalLine.Y2 = cvsCross.ActualHeight;
        }

        private void UpdateIndicator()
        {
            System.Windows.Point ptCrossCanvas = Mouse.GetPosition(cvsCross);

            _verticalLine.X1 = ptCrossCanvas.X;
            _verticalLine.X2 = ptCrossCanvas.X;

            _horizontalLine.Y1 = ptCrossCanvas.Y;
            _horizontalLine.Y2 = ptCrossCanvas.Y;

            _horizontalLine.X2 = cvsCross.ActualWidth;
            _verticalLine.Y2 = cvsCross.ActualHeight;
        }

        private void CrossCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            InitializeIndicator();

            cvsCross.Children.Add(_horizontalLine);
            cvsCross.Children.Add(_verticalLine);

            if (BasedCanvas != null)
            {
                if (BasedCanvas.Tool == ToolType.Move)
                {
                    this.Cursor = System.Windows.Input.Cursors.ScrollAll;
                    ToolChange(ToolType.Move);
                }
                else
                {
                    this.Cursor = BasedCanvas.Cursor;
                }
            }
        }

        private void CrossCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            cvsCross.Children.Remove(_horizontalLine);
            cvsCross.Children.Remove(_verticalLine);

            this.Cursor = Cursors.Arrow;
        }
        #endregion


        #region Zooming func
        private void UpdateScale()
        {
            DrawingCanvas drawingCanvas = BasedCanvas;
            if (drawingCanvas != null)
                drawingCanvas.ActualScale = ZoomValue;
        }

        private void sldrScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomValue = sldrScale.Value;
        }

        private void zoomBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender as Button == null)
            {
                return;
            }

            string strTag = (sender as Button).Tag.ToString();

            if (!string.IsNullOrEmpty(strTag))
            {
                if (strTag == "IN")
                {
                    Zoom(100);
                }
                else if (strTag == "OUT")
                {
                    Zoom(-100);
                }
                else// ZOOM_TO_FIT
                {
                    Zoom(0);
                }
            }
        }
        public void CalculateZoomToFitScale()
        {
            double fnumerator = 1.0;
            double fdenominator = 1.0;

            if (SourceHeight / ViewerHeight > SourceWidth / ViewerWidth)
            {
                fnumerator = ViewerHeight;
                fdenominator = SourceHeight;
            }
            else
            {
                fnumerator = ViewerWidth;
                fdenominator = SourceWidth;
            }
            _zoomToFitScale = fnumerator / fdenominator * 0.975;

            ZoomValue = _zoomToFitScale;
            sldrScale.Minimum = (_zoomToFitScale > 0) ? _zoomToFitScale : 0.1;
        }

        public void SetZoomToFit()
        {
            ZoomValue = _zoomToFitScale;
        }

        private System.Windows.Point GetContentMousePosition()
        {
            if (BasedImage == null)
            {
                return new System.Windows.Point(0, 0);
            }
            else
            {
                System.Windows.Point ptContentMousePosition = Mouse.GetPosition(BasedImage);

                return ptContentMousePosition;
            }
        }

        private System.Windows.Point GetCenterOfViewport()
        {
            if (BasedImage == null)
            {
                return new System.Windows.Point(0, 0);
            }
            else
            {
                System.Windows.Point ptCenterOfViewport = new System.Windows.Point(ViewerWidth / 2, ViewerHeight / 2);
                System.Windows.Point ptTranslatedCenterOfViewport = svTeaching.TranslatePoint(ptCenterOfViewport, BasedImage);

                return ptTranslatedCenterOfViewport;
            }
        }

        private void Zoom(int deltaValue)
        {
            // ROI가 많을 경우 속도 저하 발생하므로, Zoom Scale Frequency를 임의로 키운다.
            if (deltaValue > 0)
            {
                if (BasedCanvas != null && BasedCanvas.GraphicsList.Count > 2000)
                    ZoomValue *= 2.0;
                else
                    ZoomValue *= 1.1;
            }
            else if (deltaValue == 0)
            {
                ZoomValue = _zoomToFitScale;
            }
            else
            {
                if (BasedCanvas != null && BasedCanvas.GraphicsList.Count > 2000)
                    ZoomValue = (ZoomValue / 2.0 < _zoomToFitScale) ? _zoomToFitScale : ZoomValue / 2.0;
                else
                    ZoomValue = (ZoomValue / 1.1 < _zoomToFitScale) ? _zoomToFitScale : ZoomValue / 1.1;
            }
        }
        #endregion

        #region Other func
        public void UpdateViewerSource(BitmapSource aBitmapSource)
        {
            if (aBitmapSource != null)
            {
                BasedCanvas.Width = BasedImage.Width = aBitmapSource.PixelWidth;
                BasedCanvas.Height = BasedImage.Height = aBitmapSource.PixelHeight;
                BasedImage.Source = aBitmapSource;
                CalculateZoomToFitScale();
                LineProfileCtrl.SetLineProfileSource(BaseImageSource); 
                LineProfileCtrl.Refresh();
            }
            else
            {
                
                BasedCanvas.Width = BasedImage.Width = 0;
                BasedCanvas.Height = BasedImage.Height = 0;
                BasedImage.Source = null;
            }

            BasedCanvas.GraphicsList.Clear();
            BasedCanvas.SelectedGraphic = null;
            SetScrollViewerToHome();

            pnlInner.Children.Clear();
            pnlInner.Children.Add(BasedImage);
            pnlInner.Children.Add(BasedCanvas);

            ToolChange(ToolType.Pointer);
        }
        public void SetScrollViewerToHome()
        {
            svTeaching.ScrollToHorizontalOffset(0.0);
            svTeaching.ScrollToVerticalOffset(0.0);
        }

        public void Binarization()
        {
            int nLowerThreshold, nUpperThreshold, nErosionIter, nDilationIter;

            if ((bool)radSingleThreshold.IsChecked)
            {
                nLowerThreshold = (int)sldrThreshold.Value;
                nUpperThreshold = 255;
            }
            else
            {
                nLowerThreshold = (int)sldrLowerThreshold.Value;
                nUpperThreshold = (int)sldrUpperThreshold.Value;
            }

            nErosionIter = (int)sldrErosionIter.Value;
            nDilationIter = (int)sldrDilationIter.Value;

            try
            {
                // CHEKCK : 전체 영상일 경우 처리 시간이 이미지 로딩에 오래걸림
                //          개선 방향은 Algo 클래스 구조에서 이중 버퍼 형태를 취해야 하며
                //          원본을 처리하여 결과 이미지 버퍼에 쓰는 구조로 바꿔야 함
                //          위와 같은 구조에서 매번 이미지 셋팅이 하는 것이 아니라 원 이미지
                //          변경이 필요할 경우에만 셋팅해야 함으로 로딩 시간 단축 가능함
                //Binarization(BaseImageSource, nLowerThreshold, nUpperThreshold, nErosionIter, nDilationIter);
            }
            catch
            {
                Debug.WriteLine("Exception occured in Binarization(TeachingViewerCtrl.xaml.cs)");
            }
        }
        #endregion

        #region Display Image, Load & Save Image
        /// <summary>   Load & Save images. </summary>
        /// <remarks>   hjkim, 2026-04-27. </remarks>

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            ViewerWidth = cvsCross.ActualWidth;
            ViewerHeight = cvsCross.ActualHeight;

            LoadImage();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        public void LoadImage()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Bitmap Images (.bmp) | *.bmp";

            // Save Initial directory.
            string strOldInitialDirectory = dlg.InitialDirectory;

            string strParentPath = DirectoryManager.GetParentPath(GLB.StartupPath);
            dlg.InitialDirectory = DirectoryManager.GetCombinedPathName(strParentPath, @"\Temp\BasedImage\");

            if ((bool)dlg.ShowDialog())
            {
                DisplayImage(dlg.FileName);
            }

            // Restore Initial directory.
            dlg.InitialDirectory = strOldInitialDirectory;
        }
        private void DisplayImage(string aszFileName)
        {
            ToolChange(ToolType.Pointer);

            try
            {
                BitmapSource bitmapSource = BitmapImageLoader.LoadCachedBitmapImage(new Uri(aszFileName)) as BitmapSource;
                if (bitmapSource != null)
                {
                    BaseImageSource = bitmapSource;
                    UpdateViewerSource(BaseImageSource);
                }
            }
            catch
            {
                MessageBox.Show(ResourceStringHelper.GetErrorMessage("I001", false), "Error");
            }
        }

        public void SaveImage()
        {
            string fileName = string.Empty;

            if (BaseImageSource != null)
            {
                ImageSave(BaseImageSource, fileName);
            }
        }

        private void ImageSave(BitmapSource source, string fileName)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Bitmap Images (.bmp) | *.bmp";
            dlg.FileName = fileName;

            // Save Initial directory.
            string strOldInitialDirectory = dlg.InitialDirectory;
            string strParentPath = DirectoryManager.GetParentPath(GLB.StartupPath);

            dlg.InitialDirectory = DirectoryManager.GetCombinedPathName(strParentPath, @"\Temp\BasedImage\");

            if ((bool)dlg.ShowDialog())
                _algo.SaveBS(dlg.FileName, source);

            dlg.InitialDirectory = strOldInitialDirectory;
        }
        #endregion
    }
}
