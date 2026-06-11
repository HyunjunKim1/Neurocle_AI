using Common;
using Common.Drawing;
using ControlzEx.Standard;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.GUI.UserControls.ImageReivew;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;


namespace HDSInspector_AI.GUI.Windows
{
    public delegate void ToolTypeChangeEventHandler(ToolType newToolType);
    /// <summary>
    /// ImageReivewWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageReviewWindow : System.Windows.Window //애매한 참조 오류로 Window->System.Windows.Window로 수정
    {
        public static event ToolTypeChangeEventHandler ToolTypeChangeEvent;
        private devImageRendering _dxRender = new devImageRendering();
        // 멤버변수
        private double _zoomToFitScale = 1.0;
        private System.Windows.Point? _ptLastDragPoint;
        private System.Windows.Point? _ptLastContentMousePosition;
        private System.Windows.Point? _ptLastCenterOfViewport;

        private System.Windows.Point _tmpPoint;
        private Algo _algo = new Algo();
        private bool _isRGB = true;
        private double zoomFactor = 1.1; //확대, 축소 비율

        #region Properties

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
        }

        private void ReviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

            BasedImage.Source = _dxRender.ImageSource;

            ToolChange(ToolType.Pointer);

            pnlMultiBinarization.Visibility = Visibility.Hidden;
            radSingleThreshold.IsChecked = true;
            chkBinarization.IsChecked = false;

            sldrLowerThreshold.Value = 100;
            sldrUpperThreshold.Value = 200;

            HistogramCtrl.Refresh();
            LineProfileCtrl.Refresh();
        }

        private void InitializeEvents()
        {
            // Zoom events.
            this.btnZoomIn.Click += zoomBtn_Click;
            this.btnZoomOut.Click += zoomBtn_Click;
            this.btnZoomToFit.Click += zoomBtn_Click;
            this.sldrScale.ValueChanged += sldrScale_ValueChanged;

            this.pnlOuter.MouseDown += pnlOuter_MouseDown;
            this.pnlOuter.MouseLeftButtonUp += pnlOuter_MouseLeftUp;
            this.pnlOuter.MouseWheel += pnlOuter_MouseWheel;
            this.pnlOuter.MouseMove += pnlOuter_MouseMove;

            this.cvsCross.MouseEnter += cvsCross_MouseEnter;
            this.cvsCross.MouseLeave += cvsCross_MouseLeave;

            #region About Binariztation.
            this.chkBinarization.Click += chkBinarization_Click;
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
                    SetZoomToFit();
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

        private void chkBinarization_Click(object sender, RoutedEventArgs e)
        {
            if (chkBinarization.IsChecked == true)
            {
                if (radSingleThreshold.IsChecked == true)
                {
                    try
                    {
                        int threshold = Convert.ToInt32(txtThreshold.Text);
                        if (threshold >= 0 && threshold <= 255)
                        {
                            this.HistogramCtrl.EnableBinarization(threshold, threshold, true, false, _isRGB, ChannelType.Color);
                            Binarization();
                        }
                    }
                    catch (Exception ex) { GLB.AddLog($@"[Review]", $@"{ex.Message}", SeverityLevel.ERROR); }
                }
                else
                {
                    try
                    {
                        int lowerThreshold = Convert.ToInt32(txtLowerThreshold.Text);
                        int upperthreshold = Convert.ToInt32(txtUpperThreshold.Text);

                        if (lowerThreshold <= upperthreshold && lowerThreshold >= 0 && upperthreshold <= 255)
                        {
                            this.HistogramCtrl.EnableBinarization(lowerThreshold, upperthreshold, false, false, _isRGB, ChannelType.Color);
                        }
                    }
                    catch (Exception ex) { GLB.AddLog($@"[Review]", $@"{ex.Message}", SeverityLevel.ERROR); }
                }
            }
            else this.HistogramCtrl.HideThresholdGuideLine();
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

        private void sldrLowerThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldrLowerThreshold.Value >= 0 && sldrLowerThreshold.Value <= sldrUpperThreshold.Value)
            {
                this.HistogramCtrl.EnableBinarization((int)sldrLowerThreshold.Value, (int)sldrUpperThreshold.Value, IsSingleMode: false, isReference: false, isColor: _isRGB, ChannelType.Color);

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
                this.HistogramCtrl.EnableBinarization((int)sldrLowerThreshold.Value, (int)sldrUpperThreshold.Value, IsSingleMode: false, isReference: false, isColor: _isRGB, ChannelType.Color);

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
                this.HistogramCtrl.EnableBinarization((int)sldrThreshold.Value, (int)sldrThreshold.Value, true, isReference: false, isColor: _isRGB, ChannelType.Color);

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
        private void ClipPos(ref Int32Rect arcTarget, System.Windows.Size anBoundary)
        {
            if (arcTarget.X < 0)
                arcTarget.X = 0;
            if (arcTarget.X >= anBoundary.Width)
                arcTarget.X = (int)anBoundary.Width - 1;

            if (arcTarget.Y < 0)
                arcTarget.Y = 0;
            if (arcTarget.Y >= anBoundary.Height)
                arcTarget.Y = (int)anBoundary.Height - 1;

            if (arcTarget.X + arcTarget.Width >= anBoundary.Width)
                arcTarget.Width = (int)anBoundary.Width - arcTarget.X;
            if (arcTarget.Y + arcTarget.Height >= anBoundary.Height)
                arcTarget.Height = (int)anBoundary.Height - arcTarget.X;
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
                Binarization(BaseImageSource, nLowerThreshold, nUpperThreshold, nErosionIter, nDilationIter);
            }
            catch
            {
                Debug.WriteLine("Exception occured in Binarization(TeachingViewerCtrl.xaml.cs)");
            }
        }
        public void Binarization(BitmapSource bitmapSource, int anLowerThreshold, int anUpperThreshold, int anErosionIter, int anDilationIter)
        {
            try
            {
                if (bitmapSource == null) return;
                int width = bitmapSource.PixelWidth;
                int height = bitmapSource.PixelHeight;

                GraphicsBase graphic = BasedCanvas.SelectedGraphic;
                if (graphic == null)
                    return;

                Int32Rect region = new Int32Rect();
                if (graphic is GraphicsRectangleBase)
                {
                    region.X = 0;
                    region.Y = 0;
                    //region.Width = (int)Math.Round(((GraphicsRectangleBase)graphic).Right - ((GraphicsRectangleBase)graphic).Left);
                    //region.Height = (int)Math.Round(((GraphicsRectangleBase)graphic).Bottom - ((GraphicsRectangleBase)graphic).Top);
                    region.Width = (int)BasedCanvas.ActualWidth;
                    region.Height = (int)BasedCanvas.ActualHeight;

                    ClipPos(ref region, new System.Windows.Size(width, height));
                }
                else if (graphic is GraphicsPolyLine)
                {
                    region.X = ((GraphicsPolyLine)graphic).LeftProperty;
                    region.Y = ((GraphicsPolyLine)graphic).TopProperty;
                    region.Width = (int)Math.Round(((GraphicsPolyLine)graphic).WidthProperty) - 1;
                    region.Height = (int)Math.Round(((GraphicsPolyLine)graphic).HeightProperty) - 1;

                    ClipPos(ref region, new System.Windows.Size(width, height));
                }

                if (region.Width <= 0 || region.Height <= 0) // check region.
                    return;

                _algo.SetImage(bitmapSource);
                //_algo.SetImageROI(new System.Drawing.Rectangle(region.X, region.Y, region.Width, region.Height));
                //_algo.DoProcessing(anLowerThreshold, anUpperThreshold, anErosionIter, anDilationIter);
                _algo.GetBinaryImage(anLowerThreshold, anUpperThreshold, anErosionIter, anDilationIter);
                BasedImage.Source = _algo.GetImage();
            }
            catch (Exception ex)
            {
                GLB.AddLog("[Review]", $@"{ex.Message} - ImageReviewWindow.cs", SeverityLevel.ERROR);
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
                _dxRender.Load(orgMat);
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
                BitmapSource bitmapSource;
                Uri cvtUri = new Uri(aszFileName);
                using(Mat readMat = Cv2.ImRead(cvtUri.LocalPath, ImreadModes.Unchanged))
                {
                    if (readMat.Type() == MatType.CV_8UC1)
                        _isRGB = false;
                    else
                        _isRGB = true;

                    bitmapSource = BitmapSourceConverter.ToBitmapSource(readMat);
                }


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

        private void pnlOuter_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                // 휠의 회전 방향에 따라 배율 설정
                double zoom = e.Delta > 0 ? zoomFactor : 1.0 / zoomFactor;

                // 새로운 스케일 계산
                double newScaleX = imageScale.ScaleX * zoom;
                double newScaleY = imageScale.ScaleY * zoom;

                // 너무 작아지거나 커지지 않도록 제한 (최소 25%, 최대 1000%)
                if (newScaleX < _zoomToFitScale || newScaleX > sldrScale.Maximum)
                {
                    return;
                }

                // 마우스 포인트 기준 확대/축소
                System.Windows.Point mousePosition = e.GetPosition(pnlOuter);

                ZoomValue = newScaleX;

                // ScrollViewer의 스크롤 위치 조정
                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset + (mousePosition.X * (zoom - 1)));
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset + (mousePosition.Y * (zoom - 1)));

                e.Handled = true;
            }
        }

        private void pnlOuter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                _ptLastDragPoint = e.GetPosition(svTeaching);
            else
                BasedCanvas.DrawingCanvas_MouseDown(sender, e);
        }

        private void pnlOuter_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if(chkBinarization.IsChecked == true)
            {
                if (BasedCanvas.SelectedGraphic != null)
                {
                    if (radSingleThreshold.IsChecked == true)
                    {
                        this.HistogramCtrl.EnableBinarization((int)sldrThreshold.Value, (int)sldrThreshold.Value, true, false, _isRGB, ChannelType.Color);
                        Binarization();
                    }
                    else
                    {
                        this.HistogramCtrl.EnableBinarization((int)sldrLowerThreshold.Value, (int)sldrUpperThreshold.Value, false, false, _isRGB, ChannelType.Color);
                        Binarization();
                    }
                }
                else
                    BasedImage.Source = BaseImageSource;
            }
        }

        private void pnlOuter_MouseMove(object sender, MouseEventArgs e)
        {
            if (BaseImageSource == null || BasedImage == null || BasedCanvas == null)
                return;

            System.Windows.Point Image_panel_Point = Mouse.GetPosition(pnlInner);

            #region Draw Cross
            // Canvas 기준 현재 마우스 포지션 가져오기
            System.Windows.Point ptCvsCanvas = e.GetPosition(cvsCross);

            // 수직선 위치 조정 (X좌표 고정)
            VerticalLine.X1 = ptCvsCanvas.X;
            VerticalLine.X2 = ptCvsCanvas.X;

            // 수평선 위치 조정 (Y좌표 고정)
            HorizontalLine.Y1 = ptCvsCanvas.Y;
            HorizontalLine.Y2 = ptCvsCanvas.Y;

            HorizontalLine.X2 = cvsCross.ActualWidth;
            VerticalLine.Y2 = cvsCross.ActualHeight;
            #endregion

            #region Drag Image
            if ((Mouse.MiddleButton == MouseButtonState.Pressed) && _ptLastDragPoint != null)
            {
                System.Windows.Point currentPoint = Mouse.GetPosition(svTeaching);

                decimal changeX = (decimal)currentPoint.X - (decimal)_ptLastDragPoint.Value.X;
                decimal changeY = (decimal)currentPoint.Y - (decimal)_ptLastDragPoint.Value.Y;

                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset - (double)changeX);
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset - (double)changeY);

                _ptLastDragPoint = currentPoint;
            }
            #endregion

            #region Draw Line Profile

            if (BasedCanvas.Tool == ToolType.Pointer && Mouse.LeftButton == MouseButtonState.Released)
            {
                // Draw Line profile.

                double fScale = SourceHeight / BasedImage.ActualHeight;
                this.LineProfileCtrl.DrawLineProfile(BitmapSourceHelper.Mono_GetLinePixels(BaseImageSource, Convert.ToInt32(Image_panel_Point.Y * fScale)));
            }


            #endregion
        }

        #endregion

    }
}
