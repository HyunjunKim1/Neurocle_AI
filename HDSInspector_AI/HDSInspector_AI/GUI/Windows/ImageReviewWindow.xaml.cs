using Common;
using Common.Drawing;
using ControlzEx.Standard;
using HandyControl.Expression.Shapes;
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
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;
using Point = OpenCvSharp.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = OpenCvSharp.Size;


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

        private BitmapSource _finalsource; //최종 display된 이미지

        private bool _isCropMode = false;
        private bool _isDragging = false;

        private Point _startPoint;
        private Rectangle _cropRect;

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

            this.btnAutoRotate.Click += AutoRotate_Click;
            this.btnCrop.Click += Crop_Click;

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

        private void InitializeImageViewToFit()
        {
            if (_pyramidSources.Count == 0)
                return;

            // 첫표시는 제일 축소된거로 level 3

            
            if (_srcMat != null && _srcMat.Width >= 16000 && _pyramidSources.ContainsKey(3))
            {
                // 원본 너비가 16000 이상일 때만 Level 3으로 시작
                SetDisplayLevel(3);
            }
            else
            {
                // 16000 미만일 때는 원본(Level 0) 혹은 적절한 기본 레벨 설정
                SetDisplayLevel(0);
            }
           
              
            svTeaching.UpdateLayout();
               

            double viewerWidth = svTeaching.ViewportWidth;
            double viewerHeight = svTeaching.ViewportHeight;

            if(viewerWidth <= 0 || viewerHeight <= 0)
            {
                viewerWidth = svTeaching.ActualWidth;
                viewerHeight = svTeaching.ActualHeight;
            }

            double scaleX = viewerWidth / BasedImage.Width;
            double scaleY = viewerHeight / BasedImage.Height;

            _zoomToFitScale = Math.Min(scaleX, scaleY) * 0.975;

            if (_zoomToFitScale <= 0)
                _zoomToFitScale = 0.05;
            if (_zoomToFitScale > 5)
                _zoomToFitScale = 1.0;

                ZoomValue = _zoomToFitScale;

            svTeaching.ScrollToHorizontalOffset(0);
            svTeaching.ScrollToVerticalOffset(0);
        }

        private void SetDisplayLevel(int newLevel)
        {
            if (!_pyramidSources.ContainsKey(newLevel))
                return;

            _displayLevel = newLevel;
            _displayImageRatio = 1.0 / Math.Pow(2, _displayLevel);

            BitmapSource displaySource = _pyramidSources[_displayLevel];

            BasedImage.Source = displaySource;
            BasedImage.Width = displaySource.PixelWidth;
            BasedImage.Height = displaySource.PixelHeight;

            BasedCanvas.Width = displaySource.PixelWidth;
            BasedCanvas.Height = displaySource.PixelHeight;

            UpdateScale();
        }

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
        }

        private void RefreshDisplayImage(Mat sourceMat)
        {
            if (sourceMat == null || sourceMat.Empty())
                return;

            double oldTotalScale = GetTotalScale();
            System.Windows.Point oldCenter = GetCurrentCenterResizedPoint();

            _displayBaseMat?.Dispose();
            _displayBaseMat = sourceMat.Clone();

            BuildDisplayPyramid(_displayBaseMat);

            ResotreView(oldTotalScale, oldCenter);
        }

        #endregion

        #region Zooming func
        
        private void UpdateScale()
        {
            if (BasedCanvas != null)
                BasedCanvas.ActualScale = ZoomValue;

            if (tbk_ScaleValue != null)
                tbk_ScaleValue.Text = GetTotalScale().ToString("0.00");
        }

        private void sldrScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScale();
        }

        private double GetTotalScale()
        {
            return _displayImageRatio * ZoomValue;
        }

        private System.Windows.Point GetViewportCenterPoint()
        {
            return new System.Windows.Point(
                svTeaching.ViewportWidth / 2.0,
                svTeaching.ViewportHeight / 2.0);
        }

        private System.Windows.Point GetCurrentCenterResizedPoint()
        {
            System.Windows.Point center = GetViewportCenterPoint();

            double totalRatio = ZoomValue * _displayImageRatio;

            if (totalRatio <= 0)
                totalRatio = 1.0;

            double x = (svTeaching.HorizontalOffset + center.X) / totalRatio;
            double y = (svTeaching.VerticalOffset + center.Y) / totalRatio;

            return new System.Windows.Point(x, y);
        }

        private void ResotreView(double totalScale, System.Windows.Point resizedCenterPoint)
        {
            int level = GetDisplayLevelByScale(totalScale);

            SetDisplayLevel(level);

            double newZoom = totalScale / _displayImageRatio;
            ZoomValue = newZoom;

            svTeaching.UpdateLayout();

            System.Windows.Point center = GetViewportCenterPoint();

            double displayX = resizedCenterPoint.X * _displayImageRatio;
            double displayY = resizedCenterPoint.Y * _displayImageRatio;

            svTeaching.ScrollToHorizontalOffset(displayX * ZoomValue - center.X);
            svTeaching.ScrollToVerticalOffset(displayY * ZoomValue - center.Y);
        }

        private void ZoomAtPointLoc(double newTotalScale, System.Windows.Point viewportPoint)
        {
            if (_pyramidSources.Count == 0)
                return;

            double oldTotalScale = _displayImageRatio * ZoomValue;

            if (oldTotalScale <= 0)
                oldTotalScale = 1.0;

            // 현재 마우스가 가르키는 리사이즈 기준의 이미지 좌표. 이거중요함
            double resizedImageX = (svTeaching.HorizontalOffset + viewportPoint.X) / ZoomValue / _displayImageRatio;
            double resizedImageY = (svTeaching.VerticalOffset + viewportPoint.Y) / ZoomValue / _displayImageRatio;

            int newLevel = GetDisplayLevelByScale(newTotalScale);

            SetDisplayLevel(newLevel);

            double newZoomValue = newTotalScale / _displayImageRatio;

            if (newZoomValue < sldrScale.Minimum)
                newZoomValue = sldrScale.Minimum;

            if (newZoomValue > sldrScale.Maximum)
                newZoomValue = sldrScale.Maximum;

            ZoomValue = newZoomValue;

            svTeaching.UpdateLayout();

            double newDisplayX = resizedImageX * _displayImageRatio;
            double newDisplayY = resizedImageY * _displayImageRatio;

            svTeaching.ScrollToHorizontalOffset(newDisplayX * ZoomValue - viewportPoint.X);
            svTeaching.ScrollToVerticalOffset(newDisplayY * ZoomValue - viewportPoint.Y);
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
            if (BasedImage == null || BasedImage.Source == null)
                return;

            svTeaching.UpdateLayout();

            double viewerWidth = svTeaching.ViewportWidth;
            double viewerHeight = svTeaching.ViewportHeight;

            if(viewerWidth <= 0 || viewerHeight <= 0)
            {
                viewerWidth = svTeaching.ActualWidth;
                viewerHeight = svTeaching.ActualHeight;
            }

            double imageWidth = BasedImage.Width;
            double imageHeight = BasedImage.Height;

            if (imageWidth <= 0 || imageHeight <= 0)
                return;

            double scaleX = viewerWidth / imageWidth;
            double scaleY = viewerHeight / imageHeight;

            _zoomToFitScale = Math.Min(scaleX, scaleY) * 0.975;

            if (_zoomToFitScale <= 0)
                _zoomToFitScale = 0.05;

            sldrScale.Minimum = 0.01;
            ZoomValue = _zoomToFitScale;
        }

        public void SetZoomToFit()
        {
            InitializeImageViewToFit();
        }

        private void Zoom(int deltaValue)
        {
            if (_pyramidSources.Count == 0)
                return;

            double oldTotalScale = _displayImageRatio * ZoomValue;
            double newTotalScale = oldTotalScale;

            if (deltaValue > 0)
                newTotalScale *= 1.1;
            else if (deltaValue < 0)
                newTotalScale /= 1.1;
            else
                newTotalScale = _zoomToFitScale * _displayImageRatio;

            double minTotalScale = _zoomToFitScale * _displayImageRatio;

            if (newTotalScale < minTotalScale)
                newTotalScale = minTotalScale;

            if (newTotalScale > sldrScale.Maximum)
                newTotalScale = sldrScale.Maximum;

            System.Windows.Point centerPoint = new System.Windows.Point(svTeaching.ViewportWidth / 2.0, svTeaching.ViewportHeight / 2.0);

            ZoomAtPointLoc(newTotalScale, centerPoint);
        }
        #endregion

        #region Rotate
        private void AutoRotate_Click(object sender, RoutedEventArgs e)
        {
            if (BaseImageSource != null)
                return;

            Mat src = BaseImageSource.ToMat().Clone();
            Mat rotated = new Mat(); //회전 이미지

            Point2f[] corners = FindRectangleCorners(src);

            if (corners != null)
                return;

            Point2f leftTop = corners[0];
            Point2f rightTop = corners[1];

            double angle = Math.Atan2(rightTop.Y - leftTop.Y, rightTop.X - leftTop.X)*180/Math.PI;

            // 수평이면 무시
            if (Math.Abs(angle) == 0.1)
                return;

            Point2f center = new Point2f(src.Width/2, src.Height/2);

            Mat rotMat = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            Cv2.WarpAffine(src, rotated, rotMat, src.Size()); //아핀 변환

            BitmapSource result = BitmapSourceConverter.ToBitmapSource(rotated);

            UpdateDxRendererSource(result);

        }

        private Point2f[] FindRectangleCorners(Mat src)
        {
            Mat gray = new Mat();

            Cv2.CvtColor(
                src,
                gray,
                ColorConversionCodes.BGR2GRAY
            );


            Cv2.GaussianBlur(
                gray,
                gray,
                new Size(5, 5),
                0
            );


            Mat edge = new Mat();

            Cv2.Canny(
                gray,
                edge,
                50,
                150
            );


            Cv2.FindContours(
                edge,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple
            );


            double maxArea = 0;
            Point2f[] result = null;


            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);

                if (area < maxArea)
                    continue;


                double peri = Cv2.ArcLength(contour, true);


                Point[] approx =
                    Cv2.ApproxPolyDP(
                        contour,
                        peri * 0.02,
                        true
                    );


                // 꼭짓점 4개인 사각형
                if (approx.Length == 4)
                {
                    maxArea = area;

                    result =
                        approx
                        .Select(p => new Point2f(p.X, p.Y))
                        .ToArray();
                }
            }


            if (result == null)
                return null;


            return SortCorners(result);
        }

        private Point2f[] SortCorners(Point2f[] pts)
        {
            var ordered = new Point2f[4];


            // 좌상단 = x+y 최소
            ordered[0] =
                pts.OrderBy(p => p.X + p.Y).First();


            // 우하단 = x+y 최대
            ordered[2] =
                pts.OrderByDescending(p => p.X + p.Y).First();


            // 우상단 = x-y 최대
            ordered[1] =
                pts.OrderByDescending(p => p.X - p.Y).First();


            // 좌하단 = x-y 최소
            ordered[3] =
                pts.OrderBy(p => p.X - p.Y).First();


            return ordered;
        }
        #endregion

        #region Crop
        private void Crop_Click(object sender, RoutedEventArgs e)
        {
           
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
            if (BaseImageSource == null)
                return;

            if (chkBinarization.IsChecked == true)
                Binarization();
            else
            {
                HistogramCtrl.HideThresholdGuideLine();
                ApplyPreprocessing();
            }
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

                if (chkBinarization != null && chkBinarization.IsChecked == true)
                    Binarization();
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
            /*
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
            */
            try
            {
                BitmapSource preprocessed = MakePreprocessedSource();

                if (preprocessed == null)
                    return;

                int lower, upper;

                if (radSingleThreshold.IsChecked == true)
                {
                    lower = (int)sldrThreshold.Value;
                    upper = 255;
                }
                else
                {
                    lower = (int)sldrLowerThreshold.Value;
                    upper = (int)sldrUpperThreshold.Value;
                }

                int erosionIter = (int)sldrErosionIter.Value;
                int dilationIter = (int)sldrDilationIter.Value;

                _algo.SetImage(preprocessed);
                _algo.GetBinaryImage(lower, upper, erosionIter, dilationIter);

                BitmapSource binarySource = _algo.GetImage();

                if (binarySource == null)
                    return;

                _finalsource = binarySource;

                _processedMat?.Dispose();
                _processedMat = BitmapSourceConverter.ToMat(binarySource);

                RefreshDisplayImage(_processedMat);
            }

            catch (Exception ex)
            {
                GLB.AddLog("[ImageReviewWindow]", $@"{ex.Message} - Binarization", SeverityLevel.ERROR);
            }
        }
        /*
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
                _algo.GetBinaryImage(anLowerThreshold, anUpperThreshold, anErosionIter, anDilationIter);
                BasedImage.Source = _algo.GetImage();
            }
            catch (Exception ex)
            {
                GLB.AddLog("[Review]", $@"{ex.Message} - ImageReviewWindow.cs", SeverityLevel.ERROR);
            }
        }
        */

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

            try
            {
                Uri cvtUri = new Uri(aszFileName);

                using (Mat originalMat = Cv2.ImRead(cvtUri.LocalPath, ImreadModes.Unchanged))
                {
                    if (originalMat == null || originalMat.Empty())
                        return;

                    _isRGB = originalMat.Channels() >= 3;

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

                pnlInner.Children.Clear();
                pnlInner.Children.Add(BasedImage);
                pnlInner.Children.Add(BasedCanvas);

                InitializeImageViewToFit();

                BasedCanvas.GraphicsList.Clear();
                BasedCanvas.SelectedGraphic = null;

                LineProfileCtrl.SetLineProfileSource(BaseImageSource);
                LineProfileCtrl.Refresh();

                ToolChange(ToolType.Pointer);
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
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.EROSION);
                        break;
                    case "Dilation":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.DILATION);
                        break;
                    case "Canny":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.CANNY_EDGE);
                        break;
                    case "Contrast":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.CONTRAST);
                        break;
                    case "Clahe":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.CLAHE);
                        break;
                    case "Sobel":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.SOBEL_EDGE);
                        break;
                    case "Gaussian":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.GAUSSIAN_FILTER);
                        break;
                    case "Median":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
                            ApplyPreprocessing(E_IMAGE_STATUS.MEDIAN_FILTER);
                        break;
                    case "Extract_ROI":
                        if (chkBinarization.IsChecked == true)
                            Binarization();
                        else
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

        private BitmapSource MakePreprocessedSource()
        {
            if (BaseImageSource == null)
                return null;
            BitmapSource result = BaseImageSource;

            if (chkErosion.IsChecked == true)
                result = GLB.ImgProc.ApplyErosion(result);
            if (chkDilation.IsChecked == true)
                result = GLB.ImgProc.ApplyDilation(result);
            if (chkCanny.IsChecked == true)
                result = GLB.ImgProc.ApplyCanny(result);
            if (chkContrast.IsChecked == true)
                result = GLB.ImgProc.ApplyContrast(result);
            if (chkClahe.IsChecked == true)
                result = GLB.ImgProc.ApplyClahe(result);
            if (chkSobel.IsChecked == true)
                result = GLB.ImgProc.ApplySobel(result);
            if (chkGauss.IsChecked == true)
                result = GLB.ImgProc.ApplyGauss(result);
            if (chkMedian.IsChecked == true)
                result = GLB.ImgProc.ApplyMedian(result);
            if (extract_ROI.IsChecked == true)
                result = GLB.ImgProc.ApplyExtract(result);

            return result;
        }
        private BitmapSource MakePreprocessedSource(E_IMAGE_STATUS status)
        {
            if (BaseImageSource == null)
                return null;

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

            return result;
        }

        private void ApplyPreprocessing()
        {
            BitmapSource result = MakePreprocessedSource();

            if (result == null)
                return;

            _finalsource = result;

            _processedMat?.Dispose();
            _processedMat = BitmapSourceConverter.ToMat(result);

            RefreshDisplayImage(_processedMat);
        }

        private void ApplyPreprocessing(E_IMAGE_STATUS status)
        {
            BitmapSource result = MakePreprocessedSource();

            if (result == null)
                return;

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

        private void pnlOuter_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                return;

            if (BaseImageSource == null || _pyramidSources.Count == 0)
                return;

            double oldTotalScale = _displayImageRatio * ZoomValue;

            double zoom = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double newTotalScale = oldTotalScale * zoom;
            double minTotalScale = _zoomToFitScale * _displayImageRatio;

            if (newTotalScale < minTotalScale)
                newTotalScale = minTotalScale;

            if (newTotalScale > sldrScale.Maximum)
                newTotalScale = sldrScale.Maximum;

            System.Windows.Point mousePos = e.GetPosition(svTeaching);

            ZoomAtPointLoc(newTotalScale, mousePos);

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
                    int lineY = Math.Max(0, Math.Min(resizedY, BaseImageSource.PixelHeight - 1));
                    LineProfileCtrl.DrawLineProfile(BitmapSourceHelper.Mono_GetLinePixels(BaseImageSource, lineY));
                }
                catch(Exception ex)
                {
                    GLB.AddLog("[ImageReviewWindow]", $@"{ex.Message}", SeverityLevel.ERROR);
                }
            }
            #endregion
        }
        #endregion
    }
}