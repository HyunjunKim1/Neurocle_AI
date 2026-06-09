using Common;
using Common.Drawing;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.GUI.UserControls.ImageReivew;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;
using Window = System.Windows.Window;


namespace HDSInspector_AI.GUI.Windows
{
    public delegate void ToolTypeChangeEventHandler(ToolType newToolType);
    /// <summary>
    /// ImageReivewWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageReviewWindow : System.Windows.Window //애매한 참조 오류로 Window->System.Windows.Window로 수정
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;

        public static event ToolTypeChangeEventHandler ToolTypeChangeEvent;
        // 멤버변수
        private double _zoomToFitScale = 1.0;
        private System.Windows.Point? _ptLastDragPoint;
        private System.Windows.Point? _ptLastContentMousePosition;
        private System.Windows.Point? _ptLastCenterOfViewport;

        private System.Windows.Point _tmpPoint;
        private Algo _algo = new Algo();
        private double zoomFactor = 1.1; //확대, 축소 비율

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
        public BitmapSource BaseImageSource { get; set; } // Origin Image


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

            this.PreviewMouseWheel += MainWindow_PreviewMouseWheel;

            Debug.WriteLine(GLB.DxRender.ImageSource.PixelHeight);
            Debug.WriteLine(GLB.DxRender.ImageSource.PixelWidth);
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

            BasedImage.Source = GLB.DxRender.ImageSource;

            ToolChange(ToolType.Pointer);

            pnlMultiBinarization.Visibility = Visibility.Hidden;
            radSingleThreshold.IsChecked = true;
            chkBinarization.IsChecked = false;

            sldrLowerThreshold.Value = 100;
            sldrUpperThreshold.Value = 200;

            HistogramCtrl.Refresh();
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

            #region About Binariztation.
            this.sldrLowerThreshold.ValueChanged += sldrLowerThreshold_ValueChanged;
            this.sldrUpperThreshold.ValueChanged += sldrUpperThreshold_ValueChanged;
            this.sldrThreshold.ValueChanged += sldrThreshold_ValueChanged;
            this.sldrErosionIter.ValueChanged += sldrErosionIter_ValueChanged;
            this.sldrDilationIter.ValueChanged += sldrDilationIter_ValueChanged;

            this.sldrLowerThreshold.PreviewMouseUp += sldrProcessing_MouseUp;
            this.sldrUpperThreshold.PreviewMouseUp += sldrProcessing_MouseUp;
            this.sldrThreshold.PreviewMouseUp += sldrProcessing_MouseUp;
            this.sldrErosionIter.PreviewMouseUp += sldrProcessing_MouseUp;
            this.sldrDilationIter.PreviewMouseUp += sldrProcessing_MouseUp;

            this.txtLowerThreshold.LostFocus += txtLowerThreshold_LostFocus;
            this.txtUpperThreshold.LostFocus += txtUpperThreshold_LostFocus;
            this.txtErosionIter.LostFocus += txtErosionIter_LostFocus;
            this.txtDialtionIter.LostFocus += txtDialtionIter_LostFocus;
            this.radMultiThreshold.Checked += radThreshold_Checked;
            this.radSingleThreshold.Checked += radThreshold_Checked;
            #endregion

            this.chkResize.Checked += chkResize_Checked;

            this.Closed += ImageReviewWindow_Closed;
        }

        private void ImageReviewWindow_Closed(object sender, EventArgs e)
        {
            this.Close();
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

        #region Binarization-Controller Event Handler.

        private void radThreshold_Checked(object sender, RoutedEventArgs e)
        {
            if (this.radSingleThreshold.IsChecked == true)
            {
                this.pnlMultiBinarization.Visibility = Visibility.Hidden;
                this.pnlSingleBinarization.Visibility = Visibility.Visible;
            }
            else
            {
                this.pnlMultiBinarization.Visibility = Visibility.Visible;
                this.pnlSingleBinarization.Visibility = Visibility.Hidden;
            }
            GLB.Windows.Review.HistogramCtrl.HideThresholdGuideLine();
            chkBinarization_Click(null, null);
        }

        private void txtUpperThreshold_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtUpperThreshold.Text == "")
                txtUpperThreshold.Text = "0";
        }

        private void txtLowerThreshold_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtLowerThreshold.Text == "")
                txtLowerThreshold.Text = "0";
        }

        private void chkBinarization_Click(object sender, RoutedEventArgs e)
        {
            if (chkBinarization.IsChecked == true)
            {
                //단일 스레시
                if (radSingleThreshold.IsChecked == true)
                {
                    try
                    {
                        int threshold = Convert.ToInt32(txtThreshold.Text);
                        if (threshold >= 0 && threshold <= 255)
                        {
                            this.HistogramCtrl.EnableBinarization(threshold, threshold, IsSingleMode: true, isReference: false, isColor: true, ChannelType.Color);
                            Binarization();
                        }
                    }
                    catch
                    {
                    }
                }
                else //멀티 스레시
                {
                    try
                    {
                        int lowerThreshold = Convert.ToInt32(txtLowerThreshold.Text);
                        int upperThreshold = Convert.ToInt32(txtUpperThreshold.Text);

                        if (lowerThreshold <= upperThreshold &&
                            lowerThreshold >= 0 &&
                            upperThreshold <= 255)
                        {
                            this.HistogramCtrl.EnableBinarization(lowerThreshold, upperThreshold, IsSingleMode: false, isReference: false, isColor: true, ChannelType.Color);
                            Binarization();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else this.HistogramCtrl.HideThresholdGuideLine();
        }
        private void sldrLowerThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldrLowerThreshold.Value >= 0 && sldrLowerThreshold.Value <= sldrUpperThreshold.Value)
            {
                this.HistogramCtrl.EnableBinarization((int)sldrLowerThreshold.Value, (int)sldrUpperThreshold.Value, IsSingleMode: false, isReference: false, isColor: true, ChannelType.Color);

                if (Math.Abs(e.OldValue - e.NewValue) == 1.0)
                    Binarization();
                else
                {
                    // 영상 크기가 1500 * 1500 이하인 경우 UI를 즉각 반영하도록 한다.
                    BitmapSource source = BaseImageSource;
                    if (source != null && source.PixelWidth * source.PixelHeight < 1500 * 1500)
                        Binarization();
                }
            }
        }

        private void sldrUpperThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldrUpperThreshold.Value >= sldrLowerThreshold.Value && sldrUpperThreshold.Value <= 255)
            {
                this.HistogramCtrl.EnableBinarization((int)sldrLowerThreshold.Value, (int)sldrUpperThreshold.Value, IsSingleMode: false, isReference: false, isColor: true, ChannelType.Color);

                if (Math.Abs(e.OldValue - e.NewValue) == 1.0)
                    Binarization();
                else
                {
                    // 영상 크기가 1500 * 1500 이하인 경우 UI를 즉각 반영하도록 한다.
                    BitmapSource source = BaseImageSource;
                    if (source != null && source.PixelWidth * source.PixelHeight < 1500 * 1500)
                        Binarization();
                }
            }
        }

        private void sldrThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldrThreshold.Value >= 0 && sldrThreshold.Value <= 255)
            {
                this.HistogramCtrl.EnableBinarization((int)sldrThreshold.Value, (int)sldrThreshold.Value, true, isReference: false, isColor: true, ChannelType.Color);

                if (Math.Abs(e.OldValue - e.NewValue) == 1.0)
                    Binarization();
                else
                {
                    // 영상 크기가 1500 * 1500 이하인 경우 UI를 즉각 반영하도록 한다.
                    BitmapSource source = BaseImageSource;
                    if (source != null && source.PixelWidth * source.PixelHeight < 1500 * 1500)
                        Binarization();
                }
            }
        }
        private void sldrDilationIter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(e.OldValue - e.NewValue) == 1.0)
                Binarization();
            else
            {
                // 영상 크기가 1500 * 1500 이하인 경우 UI를 즉각 반영하도록 한다.
                BitmapSource source = BaseImageSource;
                if (source != null && source.PixelWidth * source.PixelHeight < 1500 * 1500)
                    Binarization();
            }
        }

        private void sldrErosionIter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(e.OldValue - e.NewValue) == 1.0)
                Binarization();
            else
            {
                // 영상 크기가 1500 * 1500 이하인 경우 UI를 즉각 반영하도록 한다.
                BitmapSource source = BaseImageSource;
                if (source != null && source.PixelWidth * source.PixelHeight < 1500 * 1500)
                    Binarization();
            }
        }

        private void sldrProcessing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Binarization();
        }

        private void txtDialtionIter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtDialtionIter.Text == "")
                txtDialtionIter.Text = "0";
        }

        private void txtErosionIter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtErosionIter.Text == "")
                txtErosionIter.Text = "0";
        }

        #endregion

        #region Other func
        public void UpdateDxRendererSource(BitmapSource aBitmapSource)
        {
            Mat orgMat = aBitmapSource.ToMat();

            if (orgMat != null)
            {
                BasedCanvas.Width = BasedImage.Width = orgMat.Width;
                BasedCanvas.Height = BasedImage.Height = orgMat.Height;
                GLB.DxRender.Load(orgMat);
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
            dlg.Filter = "Bitmap Images (.bmp; .png; .jpg; .jpeg; .gif; .tiff; .tif) | *.bmp; *.png; *.jpg; *.jpeg; *.gif; *.tiff; .tif";


            // Save Initial directory.
            string strOldInitialDirectory = dlg.InitialDirectory;

            string strParentPath = DirectoryManager.GetParentPath(GLB.StartupPath);
            //dlg.InitialDirectory = DirectoryManager.GetCombinedPathName(strParentPath, @"\Temp\BasedImage\");
            dlg.InitialDirectory = @"E:\Pilot 학습 파일(ag)\top_final\CT1-com(ag)"; //초기 경로 수정

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
                    //UpdateViewerSource(BaseImageSource);
                    UpdateDxRendererSource(BaseImageSource);
                }
            }
            catch
            {
                System.Windows.MessageBox.Show(ResourceStringHelper.GetErrorMessage("I001", false), "Error"); //애매한 참조 오류로 messagebox->System.Windows.messagebox로 수정
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

        #region Image processing
        private void chkResize_Checked(object sender, RoutedEventArgs e)
        {

        }



        private void chkErosion_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }

        private void chkDilation_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }

        private void chkCanny_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }

        private void chkContrast_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }

        private void chkClahe_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }

        private void chkResize_Changed(object sender, RoutedEventArgs e)
        {
            ApplyPreprocessing();
        }


        private void ApplyPreprocessing()
        {

            if (BaseImageSource == null)
                return;

            BitmapSource result = BaseImageSource;


            if (chkErosion.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyErosion(result);

            }

            if (chkDilation.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyDilation(result);
            }

            if (chkCanny.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyCanny(result);

            }

            if (chkContrast.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyContrast(result);
            }

            if (chkClahe.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyClahe(result);
            }

            if (chkResize.IsChecked == true)
            {
                result = Class.GlobalFunctions.ImageProcessing.ApplyResize(result);
            }

            UpdateViewerSource(result);

        }
        #endregion

        #region MouseEvent


        private void cvsCross_MouseEnter(object sender, MouseEventArgs e)
        {
            VerticalLine.Visibility = Visibility.Visible;
            HorizontalLine.Visibility = Visibility.Visible;
        }

        private void cvsCross_MouseLeave(object sender, MouseEventArgs e)
        {
            VerticalLine.Visibility = Visibility.Collapsed;
            HorizontalLine.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 휠의 회전 방향에 따라 배율 설정
            double zoom = e.Delta > 0 ? zoomFactor : 1.0 / zoomFactor;

            // 새로운 스케일 계산
            double newScaleX = imageScale.ScaleX * zoom;
            double newScaleY = imageScale.ScaleY * zoom;

            // 너무 작아지거나 커지지 않도록 제한 (최소 25%, 최대 1000%)
            if (newScaleX < 1.0 || newScaleX > 15.0)
                return;

            // 마우스 포인트 기준 확대/축소
            Point mousePosition = e.GetPosition(cvsCross);

            imageScale.ScaleX = newScaleX;
            imageScale.ScaleY = newScaleY;

            // ScrollViewer의 스크롤 위치 조정
            svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset + (mousePosition.X * (zoom - 1)));
            svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset + (mousePosition.Y * (zoom - 1)));

            e.Handled = true; // 이벤트가 다른 곳으로 전파되는 것을 막음
        }

        bool mousing = false; //현재 마우스가 클릭 상태인지에 대한 변수
        int startX = 0; //클릭 시 마우스 x좌표 위치 저장 변수
        int startY = 0; //클릭 시 마우스 y좌표 위치 저장 변수

        private void cvsCross_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mousing = true; //마우스 눌린 상태

            string position = e.GetPosition(svTeaching).ToString(); //현재 클릭된 이미지 위치 가져옴

            string[] split = position.Split(','); // 이미지 위치 문자열인 position을 ',' 단위로 split
            startX = Int32.Parse(split[0]); //x좌표 저장
            startY = Int32.Parse(split[1]); //y좌표 저장
        }

        private void cvsCross_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mousing = false; //마우스 뗀 상태 저장
        }


        private void cvsCross_MouseMove(object sender, MouseEventArgs e)
        {
            // Canvas 기준 현재 마우스 포지션 가져오기
            Point mousePos = e.GetPosition(cvsCross);

            // 수직선 위치 조정 (X좌표 고정)
            VerticalLine.X1 = mousePos.X;
            VerticalLine.X2 = mousePos.X;
            VerticalLine.Y1 = 0;
            VerticalLine.Y2 = cvsCross.ActualHeight;

            // 수평선 위치 조정 (Y좌표 고정)
            HorizontalLine.X1 = 0;
            HorizontalLine.X2 = cvsCross.ActualWidth;
            HorizontalLine.Y1 = mousePos.Y;
            HorizontalLine.Y2 = mousePos.Y;


            if (mousing) //마우스 클릭 시
            {
                string position = e.GetPosition(svTeaching).ToString(); //현재 마우스 위치 가져옴

                string[] split = position.Split(',');
                int changeX = Int32.Parse(split[0]) - startX;
                int changeY = Int32.Parse(split[1]) - startY; //변경된 좌표값 저장

                startX = Int32.Parse(split[0]);
                startY = Int32.Parse(split[1]); //처음 시작 좌표를 현재 좌표로 교체

                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset - changeX);
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset - changeY);
            }
        }


        #endregion

        private void sldrScale_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void chkBinarization_Checked(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
