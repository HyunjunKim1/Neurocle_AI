using Common;
using Common.Drawing;
using ControlzEx.Standard;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.GUI.UserControls.ImageReivew;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
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
        private const int D3D_MAX_TEXTURE_SIZE = 16384;
        public static event ToolTypeChangeEventHandler ToolTypeChangeEvent;

        private devImageRendering _dxRender = new devImageRendering();

        // 멤버변수
        private int _originImageWidth = 0;
        private int _originImageHeight = 0;

        /// <summary>
        /// GPU Texture Render 최대 크기가 16384임. 
        /// 실제로 16384를 두번 읽어와서 이미지 Merge 하는거보다
        /// 그냥 최대크기를 16384로 가져오고, 이미지 Scale 관리를 하는게 좋을듯. 아니 이게 맞음.
        /// </summary>
        /// resize = original 사이즈 * _loadResizeRatio
        private double _loadResizeRatio = 1.0;

        private double _zoomToFitScale = 0.05;
        private System.Windows.Point? _ptLastDragPoint;

        private Algo _algo = new Algo();
        private bool _isRGB = true;
        private bool viewerInitialized = false; //이미지 로드시 초기화 변수

        private Mat _srcMat;            // 원본 Mat 저장용
        private Mat _processedMat;      // 전처리된 이미지
        private Mat _displayBaseMat;    // Display되는 기준 Mat

        private readonly Dictionary<int, BitmapSource> _pyramidSources = new Dictionary<int, BitmapSource>();
        private int _displayLevel = 0;

        /// <summary>
        /// Scale에 따라서 다르게 가져가자
        /// lv 0 = 1.0
        /// lv 1 = 0.5
        /// lv 2 = 0.25
        /// lv 3 = 0.125
        /// </summary>
        private double _displayImageRatio = 1.0;

        private bool _isChangingDisplayLevel = false;

        private BitmapSource _finalsource; //최종 display된 이미지



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
                double min = sldrScale.Minimum;
                double max = sldrScale.Maximum;

                double newValue = Math.Max(min, Math.Min(max, value));

                if (Math.Abs(sldrScale.Value - newValue) > 0.0001)
                    sldrScale.Value = newValue;

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
        private void ReviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.Visibility = Visibility.Hidden;
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

        #region Pyramid Functions.
        /// <summary>
        /// Scale에 맞춰서 확/축소 해보니까 걍 너~~~무 느려서 아예 못써먹을듯.
        /// 초기 선정한 Pyramid 방식으로 하자
        /// </summary>

        private void BuildDisplayPyramid(Mat source)
        {
            if (source == null || source.Empty())
                return;

            _pyramidSources.Clear();

            Mat current = source.Clone();

            for (int level = 0; level <= 3; level++)
            {
                BitmapSource bs = BitmapSourceConverter.ToBitmapSource(current);
                bs.Freeze();

                _pyramidSources[level] = bs;

                if (level < 3)
                {
                    Mat down = new Mat();
                    Cv2.PyrDown(current, down);
                    current.Dispose();
                    current = down;
                }
            }

            current.Dispose();
            _displayLevel = -1;
        }

        private int GetDisplayLevelByScale(double totalScale)
        {
            if (totalScale >= 0.75)
                return 0;
            if (totalScale >= 0.35)
                return 1;
            if (totalScale >= 0.15)
                return 2;
            return 3;

        }

        private void ChangeDisplayLevelNeeded(double totalScale)
        {
            if (_pyramidSources.Count == 0)
                return;

            int newLevel = GetDisplayLevelByScale(totalScale);

            if (newLevel == _displayLevel)
                return;

            if (!_pyramidSources.ContainsKey(newLevel))
                return;

            _isChangingDisplayLevel = true;

            // 여기서 혹여나 현재 적용된것들 적용하고, Scale 비율 다시 생성함.
            try
            {
                double oldRatio = _displayImageRatio;
                double oldZoom = ZoomValue;
                double oldTotalScale = oldRatio / oldZoom;

                _displayLevel = newLevel;
                _displayImageRatio = 1.0 / Math.Pow(2, _displayLevel);

                BitmapSource displaySource = _pyramidSources[_displayLevel];

                BasedImage.Source = displaySource;
                BasedImage.Width = displaySource.PixelWidth;
                BasedImage.Height = displaySource.PixelHeight;

                BasedCanvas.Width = displaySource.PixelWidth;
                BasedCanvas.Height = displaySource.PixelHeight;

                double newZoom = oldTotalScale / _displayImageRatio;

                if (newZoom < sldrScale.Minimum)
                    newZoom = sldrScale.Minimum;
                if (newZoom > sldrScale.Maximum)
                    newZoom -= sldrScale.Maximum;

                sldrScale.Value = newZoom;
                UpdateScale();
            }
            catch (Exception ex)
            {
                GLB.AddLog("[ImageReviewWindow]", $@"{ex.Message}", SeverityLevel.ERROR);
            }
            finally
            {
                _isChangingDisplayLevel = false;
            }
        }

        private void RefreshDisplayImage(Mat sourceMat)
        {
            if (sourceMat == null || sourceMat.Empty())
                return;

            _displayBaseMat?.Dispose();
            _displayBaseMat = sourceMat.Clone();

            BuildDisplayPyramid(_displayBaseMat);

            double totalScale = _displayImageRatio * ZoomValue;

            if (totalScale <= 0)
                totalScale = _zoomToFitScale;

            ChangeDisplayLevelNeeded(totalScale);
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
            UpdateScale();
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
        { /*
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
            _zoomToFitScale = Math.Min(1.0, fnumerator / fdenominator * 0.975);

            ZoomValue = _zoomToFitScale;
            sldrScale.Minimum = (_zoomToFitScale > 0) ? _zoomToFitScale : 0.1;
            */
            if (SourceWidth <= 0 || SourceHeight <= 0)
                return;

            ViewerWidth = svTeaching.ViewportWidth;
            ViewerHeight = svTeaching.ViewportHeight;

            if (ViewerWidth <= 0 || ViewerHeight <= 0)
            {
                svTeaching.UpdateLayout();
                ViewerWidth = svTeaching.ActualWidth;
                ViewerHeight = svTeaching.ActualHeight;
            }

            double scaleX = ViewerWidth / SourceWidth;
            double scaleY = ViewerHeight / SourceHeight;

            _zoomToFitScale = Math.Min(scaleX, scaleY) * 0.975;

            if (_zoomToFitScale <= 0)
                _zoomToFitScale = 0.05;

            sldrScale.Minimum = _zoomToFitScale;
            ZoomValue = _zoomToFitScale;
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
            double oldTotalScale = _displayImageRatio * ZoomValue;
            double newTotalScale = oldTotalScale;

            if (deltaValue > 0)
                newTotalScale *= 1.1;
            else if (deltaValue < 0)
                newTotalScale /= 1.1;
            else
                newTotalScale = _zoomToFitScale;

            if (newTotalScale < _zoomToFitScale)
                newTotalScale = _zoomToFitScale;

            if (newTotalScale > sldrScale.Maximum)
                newTotalScale = sldrScale.Maximum;

            ChangeDisplayLevelNeeded(newTotalScale);

            ZoomValue = newTotalScale / _displayImageRatio;
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
        private System.Windows.Point DisplayPointToResizedImagePoint(System.Windows.Point displayPoint)
        {
            double x = displayPoint.X / _displayImageRatio;
            double y = displayPoint.Y / _displayImageRatio;

            return new System.Windows.Point(x, y);
        }

        private System.Windows.Point DisplayPointToOriginalImagePoint(System.Windows.Point displayPoint)
        {
            double totalRatio = _displayImageRatio * _loadResizeRatio;

            if (totalRatio <= 0)
                totalRatio = 1.0;

            double x = displayPoint.X / totalRatio;
            double y = displayPoint.Y / totalRatio;

            return new System.Windows.Point(x, y);
        }

        private Mat ResizeForD3DLimit(Mat src)
        {
            if (src == null || src.Empty())
                return null;

            _originImageWidth = src.Width;
            _originImageHeight = src.Height;

            int maxSide = Math.Max(src.Width, src.Height);

            if (maxSide <= D3D_MAX_TEXTURE_SIZE)
            {
                _loadResizeRatio = 1.0;
                return src.Clone();
            }

            _loadResizeRatio = (double)D3D_MAX_TEXTURE_SIZE / maxSide;

            int newWidth = (int)Math.Round(src.Width * _loadResizeRatio);
            int newHeight = (int)Math.Round(src.Height * _loadResizeRatio);

            Mat resizeMat = new Mat();
            Cv2.Resize(src, resizeMat, new OpenCvSharp.Size(newWidth, newHeight), 0, 0, InterpolationFlags.Area);

            return resizeMat;
        }

        public void UpdateDxRendererSource(BitmapSource aBitmapSource)
        {
            sldrScale.Value = 1.0;
            svTeaching.ScrollToHorizontalOffset(0);
            svTeaching.ScrollToVerticalOffset(0);

            Mat orgMat = aBitmapSource.ToMat();

            if (orgMat != null)
            {
                BasedCanvas.Width = BasedImage.Width = orgMat.Width;
                BasedCanvas.Height = BasedImage.Height = orgMat.Height;
                BasedImage.Source = aBitmapSource;
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
            svTeaching.ScrollToHorizontalOffset(0);
            svTeaching.ScrollToVerticalOffset(0);

            if (aBitmapSource != null)
            {
                //BasedImage.Source = null;
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
            if (viewerInitialized == false)
            {


                cvsCross.UpdateLayout();

                ViewerWidth = cvsCross.ActualWidth; //실제 원래 캔버스 크기를 저장
                ViewerHeight = cvsCross.ActualHeight;

                viewerInitialized = true;
            }//뷰어 사이즈 고정되어 이미지 새로 로드해도 뷰어 원래 사이즈 유지
            LoadImage();
            chkReset();//이미지 로드할때 이전 이미지의 전처리 상태 리셋

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

            /*
            try
            {
                BitmapSource bitmapSource;
                Uri cvtUri = new Uri(aszFileName);
                using(Mat readMat = Cv2.ImRead(cvtUri.LocalPath, ImreadModes.Unchanged))
                {
                    if (readMat.Type() == MatType.CV_8UC1) //흑백인지 컬러인지
                        _isRGB = false;
                    else
                        _isRGB = true;

                    bitmapSource = BitmapSourceConverter.ToBitmapSource(readMat);
                }

                if (bitmapSource != null)
                {
                    int width = bitmapSource.PixelWidth;
                    int height = bitmapSource.PixelHeight;

                    // 큰 이미지만 축소
                    if (width > 4000 || height > 4000)
                    {
                        Mat mat = BitmapSourceConverter.ToMat(bitmapSource);

                        // 1/2 축소
                        Cv2.PyrDown(mat, mat);

                        // 필요하면 한 번 더
                        if (mat.Width > 4000 || mat.Height > 4000)
                        {
                            Cv2.PyrDown(mat, mat);
                        }

                        bitmapSource = BitmapSourceConverter.ToBitmapSource(mat);
                    }
                    BaseImageSource = bitmapSource;

                    _srcMat = BitmapSourceConverter.ToMat(BaseImageSource);
                    //UpdateViewerSource(BaseImageSource);
                    UpdateDxRendererSource(BaseImageSource);
                }
            }
            catch
            {
                System.Windows.MessageBox.Show(ResourceStringHelper.GetErrorMessage("I001", false), "Error"); //애매한 참조 오류로 messagebox->System.Windows.messagebox로 수정
            }
            */

            try
            {
                Uri cvtUri = new Uri(aszFileName);

                using (Mat originalMat = Cv2.ImRead(cvtUri.LocalPath, ImreadModes.Unchanged))
                {
                    if (originalMat == null || originalMat.Empty())
                        return;

                    if (originalMat.Type() == MatType.CV_8UC1)
                        _isRGB = false;
                    else
                        _isRGB = true;

                    _srcMat?.Dispose();
                    _srcMat = ResizeForD3DLimit(originalMat);
                }

                if (_srcMat == null || _srcMat.Empty())
                    return;

                BaseImageSource = BitmapSourceConverter.ToBitmapSource(_srcMat);
                BaseImageSource.Freeze();

                _processedMat?.Dispose();
                _processedMat = null;

                _finalsource = BaseImageSource;

                BuildDisplayPyramid(_srcMat);

                UpdateViewerSource(_pyramidSources[0]);

                CalculateZoomToFitScale();
                SetZoomToFit();

                ChangeDisplayLevelNeeded(_zoomToFitScale);
            }
            catch (Exception ex)
            {
                GLB.AddLog("[Review]", $@"{ex.Message} - DisplayImage", SeverityLevel.ERROR);
                System.Windows.MessageBox.Show(ResourceStringHelper.GetErrorMessage("I001", false), "Error");
            }
        }



        public void SaveImage()
        {
            string fileName = string.Empty;

            if (_finalsource != null)
            {
                ImageSave(_finalsource, fileName); //최종 이미지 저장
            }
        }

        private void ImageSave(BitmapSource source, string fileName)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Bitmap Images(.bmp; .png; .jpg; .jpeg; .gif; .tiff; .tif) | *.bmp; *.png; *.jpg; *.jpeg; *.gif; *.tiff; .tif";
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

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb != null)
            {
                string tagVal = cb.Tag as string;

                switch (tagVal)
                {
                    case "Erosion":
                        ApplyPreprocessing(E_IMAGE_STATUS.EROSION);
                        break;
                    case "Dilation":
                        ApplyPreprocessing(E_IMAGE_STATUS.DILATION);
                        break;
                    case "Canny":
                        ApplyPreprocessing(E_IMAGE_STATUS.CANNY_EDGE);
                        break;
                    case "Contrast":
                        ApplyPreprocessing(E_IMAGE_STATUS.CONTRAST);
                        break;
                    case "Clahe":
                        ApplyPreprocessing(E_IMAGE_STATUS.CLAHE);
                        break;
                    case "Sobel":
                        ApplyPreprocessing(E_IMAGE_STATUS.SOBEL_EDGE);
                        break;
                    case "Gaussian":
                        ApplyPreprocessing(E_IMAGE_STATUS.GAUSSIAN_FILTER);
                        break;
                    case "Median":
                        ApplyPreprocessing(E_IMAGE_STATUS.MEDIAN_FILTER);
                        break;
                    case "Extract_ROI":
                        ApplyPreprocessing(E_IMAGE_STATUS.EXTRACT);
                        break;
                }
            }
        }


        private void chkReset()
        {
            extract_ROI.IsChecked = false;
            chkErosion.IsChecked = false;
            chkContrast.IsChecked = false;
            chkCanny.IsChecked = false;
            chkClahe.IsChecked = false;
            chkDilation.IsChecked = false;
            chkSobel.IsChecked = false;
            chkGauss.IsChecked = false;
            chkMedian.IsChecked = false;
        }


        private void ApplyPreprocessing(E_IMAGE_STATUS status)
        {
            if (BaseImageSource == null)
                return;

            BitmapSource result = BaseImageSource;

            switch (status)
            {
                case E_IMAGE_STATUS.EROSION:
                    if (chkErosion.IsChecked == true)
                        result = GLB.ImgProc.ApplyErosion(result);
                    break;

                case E_IMAGE_STATUS.DILATION:
                    if (chkDilation.IsChecked == true)
                        result = GLB.ImgProc.ApplyDilation(result);
                    break;

                case E_IMAGE_STATUS.CANNY_EDGE:
                    if (chkCanny.IsChecked == true)
                        result = GLB.ImgProc.ApplyCanny(result);
                    break;

                case E_IMAGE_STATUS.CONTRAST:
                    if (chkContrast.IsChecked == true)
                        result = GLB.ImgProc.ApplyContrast(result);
                    break;

                case E_IMAGE_STATUS.CLAHE:
                    if (chkClahe.IsChecked == true)
                        result = GLB.ImgProc.ApplyClahe(result);
                    break;

                case E_IMAGE_STATUS.SOBEL_EDGE:
                    if (chkSobel.IsChecked == true)
                        result = GLB.ImgProc.ApplySobel(result);
                    break;

                case E_IMAGE_STATUS.GAUSSIAN_FILTER:
                    if (chkGauss.IsChecked == true)
                        result = GLB.ImgProc.ApplyGauss(result);
                    break;

                case E_IMAGE_STATUS.MEDIAN_FILTER:
                    if (chkMedian.IsChecked == true)
                        result = GLB.ImgProc.ApplyMedian(result);
                    break;

                case E_IMAGE_STATUS.EXTRACT:
                    if (extract_ROI.IsChecked == true)
                        result = GLB.ImgProc.ApplyExtract(result);
                    break;
            }

            _finalsource = result;

            _processedMat?.Dispose();
            _processedMat = BitmapSourceConverter.ToMat(result);

            RefreshDisplayImage(_processedMat);
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

        /*

        private int GetPyramidLevel(double scale)
        {
            if (scale > 2.0) return -1;  // PyrUp (확대)
            if (scale > 0.5) return 0;  // 원본
            if (scale > 0.25) return 1;  // PyrDown 1단계
            return 2;                      // PyrDown 2단계
        }
        

        private void UpdateImageWithPyramid(int level)
        {
            Mat sourceMat = _processedMat ?? _srcMat;

            if (sourceMat == null) return;

            Mat pyrMat = new Mat();

            if (level < 0)  // 확대 → PyrUp
            {
                Cv2.PyrUp(sourceMat, pyrMat, new OpenCvSharp.Size(sourceMat.Cols * 2, sourceMat.Rows * 2));
            }
            else if (level == 0)  // 원본 유지
            {
                pyrMat = sourceMat.Clone();
            }
            else  // 축소 → PyrDown 반복
            {
                Mat current = sourceMat.Clone();
                for (int i = 0; i < level; i++)
                {
                    Cv2.PyrDown(current, pyrMat);
                    current = pyrMat.Clone();
                }
            }

            // 기존 이미지 컨트롤에 교체 (imgDisplay는 실제 Image 컨트롤 이름으로 변경)
            BasedImage.Source = BitmapSourceConverter.ToBitmapSource(pyrMat);
            pyrMat.Dispose();
        }
        */

        private void pnlOuter_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                return;

            if (BaseImageSource == null)
                return;

            double oldTotalScale = _displayImageRatio * ZoomValue;
            double zoom = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double newTotalScale = oldTotalScale * zoom;

            if (newTotalScale < _zoomToFitScale)
                newTotalScale = _zoomToFitScale;

            if (newTotalScale > sldrScale.Maximum)
                newTotalScale = sldrScale.Maximum;

            System.Windows.Point mousePos = e.GetPosition(svTeaching);

            double imageX = (svTeaching.HorizontalOffset + mousePos.X) / ZoomValue;
            double imageY = (svTeaching.VerticalOffset + mousePos.Y) / ZoomValue;

            ChangeDisplayLevelNeeded(newTotalScale);

            double newZoomValue = newTotalScale / _displayImageRatio;
            ZoomValue = newZoomValue;

            svTeaching.UpdateLayout();

            svTeaching.ScrollToHorizontalOffset(imageX * ZoomValue - mousePos.X);
            svTeaching.ScrollToVerticalOffset(imageY * ZoomValue - mousePos.Y);

            e.Handled = true;
        }

        private void pnlOuter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || BasedCanvas.Tool == ToolType.Move || Keyboard.IsKeyDown(Key.Space))
            {
                _ptLastDragPoint = e.GetPosition(svTeaching);
                return;
            }
            else
                BasedCanvas.DrawingCanvas_MouseDown(sender, e);
        }

        private void pnlOuter_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (chkBinarization.IsChecked == true)
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
        /*
        private void pnlOuter_MouseMove(object sender, MouseEventArgs e)
        {
            if (BaseImageSource == null || BasedImage == null || BasedCanvas == null)
                return;


            System.Windows.Point Image_panel_Point = Mouse.GetPosition(BasedImage);
            double originX = displayPoint

            #region Calculate GrayValue

            if (BaseImageSource != null)
            {
                if (!_isRGB)
                {
                    // GV 표시는 등록된 이미지위 범위 내에서만 동작하도록 한다.
                    if (!(Image_panel_Point.X > BasedImage.ActualWidth) &&
                        !(Image_panel_Point.Y > BasedImage.ActualHeight) &&
                        (Image_panel_Point.X > 0) && (Image_panel_Point.Y > 0))
                    {
                        // Calculate GV Value.
                        byte[] pixel = new byte[1];

                        BaseImageSource.CopyPixels(new Int32Rect((int)Image_panel_Point.X, (int)Image_panel_Point.Y, 1, 1),
                                                          pixel, SourceWidth, 0);

                        // Update X, Y, GV
                        txtGVValue.Text = pixel[0].ToString();
                        txtXPosition.Text = Convert.ToInt32(Image_panel_Point.X).ToString();
                        txtYPosition.Text = Convert.ToInt32(Image_panel_Point.Y).ToString();

                        #region Unused Code. (Update X, Y by (mm))
                        //if (ptCurrentByImage.X != 0)
                        //{
                        //    txtXPositionMM.Text = string.Format("{0:f2}", Convert.ToDouble(ptCurrentByImage.X * CamResolutionX / 1000 / m_fReferenceImageScale));
                        //}
                        //if (ptCurrentByImage.Y != 0)
                        //{
                        //    txtYPositionMM.Text = string.Format("{0:f2}", Convert.ToDouble(ptCurrentByImage.Y * CamResolutionY / 1000 / m_fReferenceImageScale));
                        //}
                        #endregion

                        // Draw Line profile.
                        if (BasedCanvas.Tool == ToolType.Pointer && Mouse.LeftButton == MouseButtonState.Released)
                        {
                            double fScale = SourceHeight / BasedImage.ActualHeight;
                            LineProfileCtrl.DrawLineProfile(BitmapSourceHelper.GetLinePixels(BaseImageSource, Convert.ToInt32(Image_panel_Point.Y * fScale)));
                        }
                    }
                    else
                    {
                        this.txtGVValue.Text = "0";
                    }
                }
            }

            if ((BasedCanvas.Tool == ToolType.Move || (Keyboard.IsKeyDown(Key.Space) && Mouse.LeftButton == MouseButtonState.Pressed)) && _ptLastDragPoint != null)
            {
                double fdeltaX = Image_panel_Point.X - _ptLastDragPoint.Value.X;
                double fdeltaY = Image_panel_Point.Y - _ptLastDragPoint.Value.Y;

                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset - fdeltaX);
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset - fdeltaY);

                _ptLastDragPoint = Image_panel_Point;
            }
            else
            {
                BasedCanvas.DrawingCanvas_MouseMove(sender, e);
            }

            #endregion

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
        */
        private void pnlOuter_MouseMove(object sender, MouseEventArgs e)
        {
            if (BaseImageSource == null || BasedImage == null || BasedCanvas == null)
                return;

            // 현재 표시 중인 피라미드 이미지 기준 좌표
            System.Windows.Point displayPoint = Mouse.GetPosition(BasedImage);

            // 16384 제한 때문에 리사이즈된 기준 이미지 좌표
            System.Windows.Point resizedImagePoint = DisplayPointToResizedImagePoint(displayPoint);

            // 실제 원본 이미지 좌표
            System.Windows.Point originalImagePoint = DisplayPointToOriginalImagePoint(displayPoint);

            int resizedX = (int)Math.Floor(resizedImagePoint.X);
            int resizedY = (int)Math.Floor(resizedImagePoint.Y);

            int originalX = (int)Math.Floor(originalImagePoint.X);
            int originalY = (int)Math.Floor(originalImagePoint.Y);

            bool isInsideResizedImage =
                resizedX >= 0 &&
                resizedY >= 0 &&
                resizedX < BaseImageSource.PixelWidth &&
                resizedY < BaseImageSource.PixelHeight;

            bool isInsideDisplayImage =
                displayPoint.X >= 0 &&
                displayPoint.Y >= 0 &&
                displayPoint.X < BasedImage.ActualWidth &&
                displayPoint.Y < BasedImage.ActualHeight;

            #region X / Y / GV 표시

            if (isInsideResizedImage && isInsideDisplayImage)
            {
                txtXPosition.Text = originalX.ToString();
                txtYPosition.Text = originalY.ToString();

                try
                {
                    if (!_isRGB)
                    {
                        byte[] pixel = new byte[1];

                        BaseImageSource.CopyPixels(
                            new Int32Rect(resizedX, resizedY, 1, 1),
                            pixel,
                            BaseImageSource.PixelWidth,
                            0);

                        txtGVValue.Text = pixel[0].ToString();
                    }
                    else
                    {
                        int bytesPerPixel = (BaseImageSource.Format.BitsPerPixel + 7) / 8;

                        if (bytesPerPixel <= 0)
                            bytesPerPixel = 4;

                        byte[] pixel = new byte[bytesPerPixel];

                        BaseImageSource.CopyPixels(
                            new Int32Rect(resizedX, resizedY, 1, 1),
                            pixel,
                            bytesPerPixel,
                            0);

                        if (bytesPerPixel >= 3)
                        {
                            byte b = pixel[0];
                            byte g = pixel[1];
                            byte r = pixel[2];

                            int gv = (r + g + b) / 3;

                            txtGVValue.Text = gv.ToString();
                        }
                        else
                        {
                            txtGVValue.Text = pixel[0].ToString();
                        }
                    }
                }
                catch
                {
                    txtGVValue.Text = "0";
                }
            }
            else
            {
                txtXPosition.Text = "0";
                txtYPosition.Text = "0";
                txtGVValue.Text = "0";
            }

            #endregion

            #region Canvas Drawing / Move Tool

            if ((BasedCanvas.Tool == ToolType.Move ||
                (Keyboard.IsKeyDown(Key.Space) && Mouse.LeftButton == MouseButtonState.Pressed)) &&
                _ptLastDragPoint != null)
            {
                System.Windows.Point currentPoint = Mouse.GetPosition(svTeaching);

                double deltaX = currentPoint.X - _ptLastDragPoint.Value.X;
                double deltaY = currentPoint.Y - _ptLastDragPoint.Value.Y;

                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset - deltaX);
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset - deltaY);

                _ptLastDragPoint = currentPoint;
            }

            else
                BasedCanvas.DrawingCanvas_MouseMove(sender, e);

            #endregion

            #region Cross Line

            System.Windows.Point ptCvsCanvas = e.GetPosition(cvsCross);
            VerticalLine.X1 = ptCvsCanvas.X;
            VerticalLine.X2 = ptCvsCanvas.X;
            HorizontalLine.Y1 = ptCvsCanvas.Y;
            HorizontalLine.Y2 = ptCvsCanvas.Y;
            HorizontalLine.X1 = 0;
            HorizontalLine.X2 = cvsCross.ActualWidth;
            VerticalLine.Y1 = 0;
            VerticalLine.Y2 = cvsCross.ActualHeight;

            #endregion

            #region Middle Button Drag

            if (Mouse.MiddleButton == MouseButtonState.Pressed && _ptLastDragPoint != null)
            {
                System.Windows.Point currentPoint = Mouse.GetPosition(svTeaching);
                double deltaX = currentPoint.X - _ptLastDragPoint.Value.X;
                double deltaY = currentPoint.Y - _ptLastDragPoint.Value.Y;

                svTeaching.ScrollToHorizontalOffset(svTeaching.HorizontalOffset - deltaX);
                svTeaching.ScrollToVerticalOffset(svTeaching.VerticalOffset - deltaY);

                _ptLastDragPoint = currentPoint;
            }

            #endregion

            #region Line Profile

            if (isInsideResizedImage && BasedCanvas.Tool == ToolType.Pointer && Mouse.LeftButton == MouseButtonState.Released)
            {
                try
                {
                    LineProfileCtrl.DrawLineProfile(BitmapSourceHelper.Mono_GetLinePixels(BaseImageSource, resizedY));
                }
                catch
                {

                }
            }
            #endregion
        }
        #endregion
    }
}