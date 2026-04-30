using Common;
using Common.Drawing;
using HDSInspector_AI.Class.GlobalFunctions;
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

namespace HDSInspector_AI.GUI.UserControls.ImageReivew
{
    /// <summary>
    /// Uc_Histogram.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Histogram : UserControl
    {
        private readonly GlobalFunction GLB = GlobalFunction.GLB;

        private const int CONTROL_WIDTH = 350;
        private const int CONTROL_HEIGHT = 180;

        // Single
        private Line m_DivideSingle = new Line();

        // Multi
        private Line m_DivideLeft = new Line();
        private Line m_DivideRight = new Line();
        private Rectangle m_DivideMiddle = new Rectangle();

        #region Member variables.
        private long[] m_HistogramData = new long[256];
        private long[] m_RefData = new long[256];
        private Point[] m_ptHistogramData = new Point[256];
        private Point[] m_ptRefData = new Point[256];

        private double m_fIntervalX = 0.0;
        private double m_fIntervalY = 0.0;

        private double m_fMarginX = 30.0;
        private static readonly double m_fMarginY = 20.0;
        private static readonly double m_XMarginOffset = 2.0;

        private static Path m_HistogramPath = new Path();
        private static Path m_RefPath = new Path();
        #endregion

        #region Constructor & InitializeDialog
        public Uc_Histogram()
        {
            InitializeComponent();
            InitializeDialog();
            InitializeEvent();
        }
        private void InitializeEvent()
        {
            // SelectedGraphic 변화 시점.
            DrawingCanvas.SelectedGraphicChangeEvent += DrawingCanvas_SelectedGraphicChangeEvent;

            this.SizeChanged += (s, e) =>
            {
                Histogram.RenderTransform = new ScaleTransform(this.ActualWidth / CONTROL_WIDTH, this.ActualHeight / CONTROL_HEIGHT);
            };
        }

        private void InitializeDialog()
        {
            m_DivideSingle.StrokeThickness = 1;
            m_DivideSingle.Stroke = new SolidColorBrush(Colors.Red);

            m_DivideLeft.StrokeThickness = 1;
            m_DivideLeft.Stroke = new SolidColorBrush(Colors.Red);

            m_DivideRight.StrokeThickness = 1;
            m_DivideRight.Stroke = new SolidColorBrush(Colors.Red);

            m_DivideMiddle.Stroke = new SolidColorBrush(Colors.Red);
            m_DivideMiddle.StrokeThickness = 1;
            m_DivideMiddle.Fill = new SolidColorBrush(Colors.Red);

            m_fIntervalX = 1.0 / 255.0 * (CONTROL_WIDTH - m_fMarginX);
            m_fIntervalY = 1.0 / 255.0 * (CONTROL_HEIGHT - m_fMarginY);

            #region Draw Lines & Texts.
            // Draw X-axis labels.
            TextBlock Label = new TextBlock();
            Label.Text = "0";
            Canvas.SetLeft(Label, m_fIntervalX * 25);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "100";
            Canvas.SetLeft(Label, m_fIntervalX * 95);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "200";
            Canvas.SetLeft(Label, m_fIntervalX * 195);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "255";
            Canvas.SetLeft(Label, m_fIntervalX * 245);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            Histogram.Children.Add(Label);

            int nDashLineCnt = (255 / 50 /* Dotted Line Profile Gap */ ) + 1;
            double fLineProfileMaxHeight = (CONTROL_HEIGHT - m_fMarginY * 2) * 250.0 / 255.0;

            Line DotLine;
            for (int i = 0; i < nDashLineCnt - 1; i++)
            {
                DotLine = new Line();
                DotLine.X1 = m_fMarginX - 1;
                DotLine.X2 = CONTROL_WIDTH - 1;
                DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) - (fLineProfileMaxHeight) * (i / ((double)nDashLineCnt - 1));
                DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) - (fLineProfileMaxHeight) * (i / ((double)nDashLineCnt - 1));
                DotLine.StrokeThickness = 2;
                DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                Histogram.Children.Add(DotLine);
            }
            DotLine = new Line();
            DotLine.X1 = m_fMarginX - 1;
            DotLine.X2 = CONTROL_WIDTH - 1;
            DotLine.Y1 = m_fMarginY;
            DotLine.Y2 = m_fMarginY;
            DotLine.StrokeThickness = 2;
            DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
            DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
            Histogram.Children.Add(DotLine);

            // Draw axis lines.
            Line AxisX = new Line
            {
                X1 = m_fMarginX,
                X2 = CONTROL_WIDTH - 1,
                Y1 = CONTROL_HEIGHT - m_fMarginY,
                Y2 = CONTROL_HEIGHT - m_fMarginY,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            Histogram.Children.Add(AxisX);

            Line AxisY = new Line
            {
                X1 = m_fMarginX,
                X2 = m_fMarginX,
                Y1 = m_fMarginY,
                Y2 = CONTROL_HEIGHT - m_fMarginY,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            Histogram.Children.Add(AxisY);
            #endregion
        }
        #endregion

        public void Refresh()
        {
            Histogram.Children.Clear();
            InitializeDialog();
        }

        public void SetRefImage(BitmapSource refImage)
        {
            m_RefData = BitmapSourceHelper.CalculateHistogramData(refImage);
        }

        public void ImageChanged(BitmapSource refImage, BitmapSource currImage)
        {
            m_RefData = BitmapSourceHelper.CalculateHistogramData(refImage);

            try
            {
                DrawHistogram2(BitmapSourceHelper.CalculateHistogramData(currImage));

            }
            catch
            {
                Debug.WriteLine("Exception occured in DrawingCanvas_SelectedGraphicChangeEvent(HistogramCtrl.xaml.cs)");
            }
        }

        // Redraws histogram.
        private void DrawingCanvas_SelectedGraphicChangeEvent(GraphicsBase newGraphic)
        {
            BitmapSource teachingImage = GLB.Windows.Review.BaseImageSource;
            if (teachingImage == null || newGraphic == null)
            {
                return;
            }

            try
            {
                CroppedBitmap croppedBitmap; // Histogram 수행 단위.
                int nWidth;
                int nHeight;

                if (newGraphic is GraphicsRectangleBase)
                {
                    GraphicsRectangleBase graphic = newGraphic as GraphicsRectangleBase;
                    nWidth = Convert.ToInt32(graphic.WidthProperty);
                    nHeight = Convert.ToInt32(graphic.HeightProperty);

                    if (nWidth < 1 || nHeight < 1)
                    {
                        return;
                    }

                    croppedBitmap = new CroppedBitmap(teachingImage, new Int32Rect(graphic.LeftProperty, graphic.TopProperty, nWidth, nHeight));
                    DrawHistogram(BitmapSourceHelper.CalculateHistogramData(croppedBitmap));
                }
                else if (newGraphic is GraphicsPolyLine)
                {
                    GraphicsPolyLine graphic = newGraphic as GraphicsPolyLine;
                    nWidth = Convert.ToInt32(graphic.WidthProperty);
                    nHeight = Convert.ToInt32(graphic.HeightProperty);

                    if (nWidth < 1 || nHeight < 1)
                    {
                        return;
                    }

                    croppedBitmap = new CroppedBitmap(teachingImage, new Int32Rect(graphic.LeftProperty, graphic.TopProperty, nWidth, nHeight));
                    DrawHistogram(BitmapSourceHelper.CalculateHistogramData(croppedBitmap));
                }
                //else // newGraphic is GraphicsLine
                //{
                //    DrawHistogram(new byte[256]);
                //}
            }
            catch
            {
                Debug.WriteLine("Exception occured in DrawingCanvas_SelectedGraphicChangeEvent(HistogramCtrl.xaml.cs)");
            }
        }

        public void DrawHistogram(byte[] pixelData)
        {
            if (pixelData == null)
            {
                return;
            }

            this.Histogram.Children.Clear();
            if (GLB.Windows.Review.chkBinarization.IsChecked == true)
            {
                if (GLB.Windows.Review.radSingleThreshold.IsChecked == true)
                {
                    if (!Histogram.Children.Contains(m_DivideSingle))
                    {
                        this.Histogram.Children.Add(m_DivideSingle);
                    }
                }
                else
                {
                    if (!Histogram.Children.Contains(m_DivideLeft))
                    {
                        this.Histogram.Children.Add(m_DivideLeft);
                        this.Histogram.Children.Add(m_DivideRight);
                        this.Histogram.Children.Add(m_DivideMiddle);
                    }
                }
            }

            foreach (byte data in pixelData)
            {
                m_HistogramData[data]++;
            }
            long lMaxValue = m_HistogramData.Max();

            // Y축 점선 간격 & 점선 갯수 정하기
            int nIndex = 0;
            int nRemain = 0;
            int nShare = 10;
            do
            {
                nIndex++;
                nRemain = (int)lMaxValue / (nShare * nIndex);

                if (nRemain > 100)
                {
                    nShare *= 10;
                }
            }
            while (nRemain > 10);
            int nDottedLineCnt = ++nRemain;
            int nDottedLineGap = nIndex * nShare;
            int nMaxHeight = nDottedLineCnt * nDottedLineGap;

            // Y축 Value-Text 길이에 따른 Left-offset값 설정
            int nOffset = 0;
            int nDivideValue = 1;
            int nPlusOffset = 0;
            do
            {
                nDivideValue *= 10;
                nPlusOffset++;
                nOffset = (int)lMaxValue / nDivideValue;
            }
            while (nOffset != 0);

            m_fMarginX = m_XMarginOffset + nPlusOffset * 0.6; // 자리수에 따라 offset * 0.6만큼 Y축 밀기
            m_fIntervalX = 1.0 / 257.0 * (CONTROL_WIDTH - (m_fMarginX + 2) * 10);
            m_fIntervalY = 1.0 / nMaxHeight * (CONTROL_HEIGHT - m_fMarginY);
            for (nIndex = 0; nIndex < 256; nIndex++)
            {
                m_ptHistogramData[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                m_ptHistogramData[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - m_HistogramData[nIndex] * m_fIntervalY - m_fMarginY);
            }

            #region Draw Lines & Texts.
            // Draw X-axis labels.
            TextBlock Label = new TextBlock();
            Label.Text = "0";
            Canvas.SetLeft(Label, m_ptHistogramData[0].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "100";
            Canvas.SetLeft(Label, m_ptHistogramData[92].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "200";
            Canvas.SetLeft(Label, m_ptHistogramData[192].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "255";
            Canvas.SetLeft(Label, m_ptHistogramData[246].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            // Draw Y-axis label & dotted lines.
            for (int i = 0; i < nDottedLineCnt; i++)
            {
                Line DotLine = new Line();
                DotLine.X1 = m_fMarginX * 10 - 1;
                DotLine.X2 = CONTROL_WIDTH - 1;
                DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.StrokeThickness = 2;
                DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                this.Histogram.Children.Add(DotLine);

                if (i != 0)
                {
                    Label = new TextBlock();
                    Label.Text = (nDottedLineGap * i).ToString();
                    Canvas.SetLeft(Label, 5);
                    Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                    this.Histogram.Children.Add(Label);
                }
            }

            // Draw axis lines.
            Line AxisX = new Line();
            AxisX.X1 = m_fMarginX * 10 - 1;
            AxisX.X2 = CONTROL_WIDTH - 1;
            AxisX.Y1 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.StrokeThickness = 2;
            AxisX.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisX);

            Line AxisY = new Line();
            AxisY.X1 = m_fMarginX * 10 - 1;
            AxisY.X2 = m_fMarginX * 10 - 1;
            AxisY.Y1 = m_fMarginY;
            AxisY.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisY.StrokeThickness = 2;
            AxisY.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisY);
            #endregion

            #region Draw histogram.
            StreamGeometry historgamGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = historgamGeometry.Open())
            {
                ctx.BeginFigure(new Point(m_ptHistogramData[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < m_HistogramData.Length; k++)
                {
                    ctx.LineTo(m_ptHistogramData[k], true, true);
                }
                ctx.LineTo(new Point(m_ptHistogramData[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            historgamGeometry.Freeze();

            m_HistogramPath.Data = historgamGeometry;
            m_HistogramPath.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
            m_HistogramPath.StrokeThickness = m_fIntervalX / 2;
            this.Histogram.Children.Add(m_HistogramPath);
            #endregion
            #region Draw Reference histogram.
            StreamGeometry RefGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = RefGeometry.Open())
            {
                ctx.BeginFigure(new Point(m_ptHistogramData[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(m_ptHistogramData[k], true, true);
                }
                ctx.LineTo(new Point(m_ptHistogramData[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            RefGeometry.Freeze();

            m_RefPath.Data = RefGeometry;
            //m_HistogramPath.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
            m_RefPath.Stroke = new SolidColorBrush(Colors.Red);
            m_RefPath.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(m_RefPath);
            #endregion
        }

        public void DrawHistogram(long[] pixelData)
        {
            if (pixelData == null)
            {
                return;
            }

            this.Histogram.Children.Clear();
            if (GLB.Windows.Review.chkBinarization.IsChecked == true)
            {
                if (GLB.Windows.Review.radSingleThreshold.IsChecked == true)
                {
                    if (!Histogram.Children.Contains(m_DivideSingle))
                    {
                        this.Histogram.Children.Add(m_DivideSingle);
                    }
                }
                else
                {
                    if (!Histogram.Children.Contains(m_DivideLeft))
                    {
                        this.Histogram.Children.Add(m_DivideLeft);
                        this.Histogram.Children.Add(m_DivideRight);
                        this.Histogram.Children.Add(m_DivideMiddle);
                    }
                }
            }

            long lMaxValue = pixelData.Max();
            //long lMaxValue = m_RefData.Max();
            // Y축 점선 간격 & 점선 갯수 정하기
            int nIndex = 0;
            int nRemain = 0;
            int nShare = 10;
            do
            {
                nIndex++;
                nRemain = (int)lMaxValue / (nShare * nIndex);

                if (nRemain > 100)
                {
                    nShare *= 10;
                }
            }
            while (nRemain > 10);
            int nDottedLineCnt = ++nRemain;
            int nDottedLineGap = nIndex * nShare;
            int nMaxHeight = nDottedLineCnt * nDottedLineGap;

            // Y축 Value-Text 길이에 따른 Left-offset값 설정
            int nOffset = 0;
            int nDivideValue = 1;
            int nPlusOffset = 0;
            do
            {
                nDivideValue *= 10;
                nPlusOffset++;
                nOffset = (int)lMaxValue / nDivideValue;
            }
            while (nOffset != 0);

            m_fMarginX = m_XMarginOffset + nPlusOffset * 0.6; // 자리수에 따라 offset * 0.6만큼 Y축 밀기
            m_fIntervalX = 1.0 / 257.0 * (CONTROL_WIDTH - (m_fMarginX + 2) * 10);
            m_fIntervalY = 1.0 / nMaxHeight * (CONTROL_HEIGHT - m_fMarginY);
            for (nIndex = 0; nIndex < 256; nIndex++)
            {
                m_ptHistogramData[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                m_ptHistogramData[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - pixelData[nIndex] * m_fIntervalY - m_fMarginY);
            }

            #region Draw Lines & Texts.
            // Draw X-axis labels.
            TextBlock Label = new TextBlock();
            Label.Text = "0";
            Canvas.SetLeft(Label, m_ptHistogramData[0].X - 3);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "100";
            Canvas.SetLeft(Label, m_ptHistogramData[92].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "200";
            Canvas.SetLeft(Label, m_ptHistogramData[192].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "255";
            Canvas.SetLeft(Label, m_ptHistogramData[246].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            // Draw Y-axis label & dotted lines.
            for (int i = 0; i < nDottedLineCnt; i++)
            {
                Line DotLine = new Line();
                DotLine.X1 = m_fMarginX * 10 - 1;
                DotLine.X2 = CONTROL_WIDTH - 1;
                DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.StrokeThickness = 2;
                DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                this.Histogram.Children.Add(DotLine);

                if (i != 0)
                {
                    Label = new TextBlock();
                    Label.Text = (nDottedLineGap * i).ToString();
                    Canvas.SetLeft(Label, 5);
                    Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                    this.Histogram.Children.Add(Label);
                }
            }

            // Draw axis lines.
            Line AxisX = new Line();
            AxisX.X1 = m_fMarginX * 10 - 1;
            AxisX.X2 = CONTROL_WIDTH - 1;
            AxisX.Y1 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.StrokeThickness = 2;
            AxisX.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisX);

            Line AxisY = new Line();
            AxisY.X1 = m_fMarginX * 10 - 1;
            AxisY.X2 = m_fMarginX * 10 - 1;
            AxisY.Y1 = m_fMarginY;
            AxisY.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisY.StrokeThickness = 2;
            AxisY.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisY);
            #endregion

            #region Draw histogram.
            StreamGeometry historgamGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = historgamGeometry.Open())
            {
                ctx.BeginFigure(new Point(m_ptHistogramData[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(m_ptHistogramData[k], true, true);
                }
                ctx.LineTo(new Point(m_ptHistogramData[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            historgamGeometry.Freeze();

            m_HistogramPath.Data = historgamGeometry;
            m_HistogramPath.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
            m_HistogramPath.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(m_HistogramPath);
            #endregion
        }

        public void DrawHistogram2(long[] pixelData)
        {
            if (pixelData == null)
            {
                return;
            }

            this.Histogram.Children.Clear();
            if (GLB.Windows.Review.chkBinarization.IsChecked == true)
            {
                if (GLB.Windows.Review.radSingleThreshold.IsChecked == true)
                {
                    if (!Histogram.Children.Contains(m_DivideSingle))
                    {
                        this.Histogram.Children.Add(m_DivideSingle);
                    }
                }
                else
                {
                    if (!Histogram.Children.Contains(m_DivideLeft))
                    {
                        this.Histogram.Children.Add(m_DivideLeft);
                        this.Histogram.Children.Add(m_DivideRight);
                        this.Histogram.Children.Add(m_DivideMiddle);
                    }
                }
            }

            //long lMaxValue = pixelData.Max();
            long lMaxValue = m_RefData.Max();
            // Y축 점선 간격 & 점선 갯수 정하기
            int nIndex = 0;
            int nRemain = 0;
            int nShare = 10;
            do
            {
                nIndex++;
                nRemain = (int)lMaxValue / (nShare * nIndex);

                if (nRemain > 100)
                {
                    nShare *= 10;
                }
            }
            while (nRemain > 10);
            int nDottedLineCnt = ++nRemain;
            int nDottedLineGap = nIndex * nShare;
            int nMaxHeight = nDottedLineCnt * nDottedLineGap;

            // Y축 Value-Text 길이에 따른 Left-offset값 설정
            int nOffset = 0;
            int nDivideValue = 1;
            int nPlusOffset = 0;
            do
            {
                nDivideValue *= 10;
                nPlusOffset++;
                nOffset = (int)lMaxValue / nDivideValue;
            }
            while (nOffset != 0);

            m_fMarginX = m_XMarginOffset + nPlusOffset * 0.6; // 자리수에 따라 offset * 0.6만큼 Y축 밀기
            m_fIntervalX = 1.0 / 257.0 * (CONTROL_WIDTH - (m_fMarginX + 2) * 10);
            m_fIntervalY = 1.0 / nMaxHeight * (CONTROL_HEIGHT - m_fMarginY);
            for (nIndex = 0; nIndex < 256; nIndex++)
            {
                m_ptHistogramData[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                m_ptHistogramData[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - pixelData[nIndex] * m_fIntervalY - m_fMarginY);
            }

            for (nIndex = 0; nIndex < 256; nIndex++)
            {
                m_ptRefData[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                m_ptRefData[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - m_RefData[nIndex] * m_fIntervalY - m_fMarginY);
            }

            #region Draw Lines & Texts.
            // Draw X-axis labels.
            TextBlock Label = new TextBlock();
            Label.Text = "0";
            Canvas.SetLeft(Label, m_ptHistogramData[0].X - 3);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "100";
            Canvas.SetLeft(Label, m_ptHistogramData[92].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "200";
            Canvas.SetLeft(Label, m_ptHistogramData[192].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            Label = new TextBlock();
            Label.Text = "255";
            Canvas.SetLeft(Label, m_ptHistogramData[246].X);
            Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
            this.Histogram.Children.Add(Label);

            // Draw Y-axis label & dotted lines.
            for (int i = 0; i < nDottedLineCnt; i++)
            {
                Line DotLine = new Line();
                DotLine.X1 = m_fMarginX * 10 - 1;
                DotLine.X2 = CONTROL_WIDTH - 1;
                DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                DotLine.StrokeThickness = 2;
                DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                this.Histogram.Children.Add(DotLine);

                if (i != 0)
                {
                    Label = new TextBlock();
                    Label.Text = (nDottedLineGap * i).ToString();
                    Canvas.SetLeft(Label, 5);
                    Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                    this.Histogram.Children.Add(Label);
                }
            }

            // Draw axis lines.
            Line AxisX = new Line();
            AxisX.X1 = m_fMarginX * 10 - 1;
            AxisX.X2 = CONTROL_WIDTH - 1;
            AxisX.Y1 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.StrokeThickness = 2;
            AxisX.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisX);

            Line AxisY = new Line();
            AxisY.X1 = m_fMarginX * 10 - 1;
            AxisY.X2 = m_fMarginX * 10 - 1;
            AxisY.Y1 = m_fMarginY;
            AxisY.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisY.StrokeThickness = 2;
            AxisY.Stroke = new SolidColorBrush(Colors.Black);
            this.Histogram.Children.Add(AxisY);
            #endregion

            #region Draw histogram.
            StreamGeometry historgamGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = historgamGeometry.Open())
            {
                ctx.BeginFigure(new Point(m_ptHistogramData[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(m_ptHistogramData[k], true, true);
                }
                ctx.LineTo(new Point(m_ptHistogramData[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            historgamGeometry.Freeze();

            m_HistogramPath.Data = historgamGeometry;
            m_HistogramPath.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
            m_HistogramPath.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(m_HistogramPath);
            #endregion

            #region Draw Reference histogram.
            StreamGeometry RefGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = RefGeometry.Open())
            {
                ctx.BeginFigure(new Point(m_ptRefData[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(m_ptRefData[k], true, true);
                }
                ctx.LineTo(new Point(m_ptRefData[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            RefGeometry.Freeze();

            m_RefPath.Data = RefGeometry;
            //m_HistogramPath.Fill = new SolidColorBrush(Colors.Transparent);
            m_RefPath.Stroke = new SolidColorBrush(Colors.Red);
            m_RefPath.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(m_RefPath);
            #endregion
        }

        #region Supports binarization
        public void EnableBinarization(int lowerThreshold, int upperThreshold, bool IsSingleMode)
        {
            if (GLB.Windows.Review.BaseImageSource != null)
            {
                if (IsSingleMode)
                {
                    m_DivideSingle.X1 = m_ptHistogramData[lowerThreshold].X;
                    m_DivideSingle.X2 = m_DivideSingle.X1;

                    m_DivideSingle.Y1 = CONTROL_HEIGHT - m_fMarginY;
                    m_DivideSingle.Y2 = Histogram.MinHeight;

                    if (!Histogram.Children.Contains(m_DivideSingle))
                    {
                        this.Histogram.Children.Add(m_DivideSingle);
                    }
                }
                else
                {
                    m_DivideLeft.X1 = m_ptHistogramData[lowerThreshold].X;
                    m_DivideLeft.X2 = m_DivideLeft.X1;

                    m_DivideRight.X1 = m_ptHistogramData[upperThreshold].X;
                    m_DivideRight.X2 = m_DivideRight.X1;

                    if (m_DivideLeft.X1 >= m_fMarginX || m_DivideRight.X1 >= m_fMarginX)
                    {
                        m_DivideLeft.Y1 = CONTROL_HEIGHT - m_fMarginY;
                        m_DivideLeft.Y2 = Histogram.MinHeight;

                        m_DivideRight.Y1 = CONTROL_HEIGHT - m_fMarginY;
                        m_DivideRight.Y2 = Histogram.MinHeight;

                        m_DivideMiddle.Width = m_DivideRight.X1 - m_DivideLeft.X1;
                        m_DivideMiddle.Height = CONTROL_HEIGHT - m_fMarginY;

                        m_DivideMiddle.Opacity = 0.5;

                        Canvas.SetLeft(m_DivideMiddle, m_DivideLeft.X2);
                        Canvas.SetTop(m_DivideMiddle, m_DivideLeft.Y2);

                        if (!Histogram.Children.Contains(m_DivideLeft))
                        {
                            this.Histogram.Children.Add(m_DivideLeft);
                            this.Histogram.Children.Add(m_DivideRight);
                            this.Histogram.Children.Add(m_DivideMiddle);
                        }
                    }
                }
            }
        }

        public void HideThresholdGuideLine()
        {
            if (Histogram.Children.Contains(m_DivideSingle))
            {
                this.Histogram.Children.Remove(m_DivideSingle);
            }
            if (Histogram.Children.Contains(m_DivideLeft))
            {
                this.Histogram.Children.Remove(m_DivideLeft);
                this.Histogram.Children.Remove(m_DivideRight);
                this.Histogram.Children.Remove(m_DivideMiddle);
            }
            if (GLB.Windows.Review.BaseImageSource != null)
                GLB.Windows.Review.BasedImage.Source = GLB.Windows.Review.BaseImageSource;
        }
        #endregion
    }
}
