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
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.GUI.UserControls.ImageReivew
{
    public struct Histogram_struct
    {
        public long[] Pixel_Datas { get; set; }

        public System.Windows.Point[] Point_Datas { get; set; }

        public System.Windows.Shapes.Path Path_Data { get; set; }

    }

    /// <summary>
    /// Uc_Histogram.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_Histogram : UserControl
    {
        private const int CONTROL_WIDTH = 350;
        private const int CONTROL_HEIGHT = 180;

        // Single
        private Line m_DivideSingle = new Line();

        // Multi
        private Line m_DivideLeft = new Line();
        private Line m_DivideRight = new Line();
        private Rectangle m_DivideMiddle = new Rectangle();
        #region Mono Member variables.
        public Histogram_struct Mono_Histogram = new Histogram_struct();
        public Histogram_struct Mono_Ref_Histogram = new Histogram_struct();

        public long[] Mono_Histogram_All_Pixel = new long[256]; // [0~255]
        public long[] Mono_Ref_All_Pixel = new long[256];       // [0~255]    
        #endregion


        #region Color Member variables.
        public Histogram_struct[] Color_Histogram = new Histogram_struct[3];
        public Histogram_struct[] Color_Ref_Histogram = new Histogram_struct[3];

        public long[,] Color_Histogram_All_Pixel = new long[3, 256]; // [RGB, 0~255]
        public long[,] Color_Ref_All_Pixel = new long[3, 256];       // [RGB, 0~255]



        public int R = 0;
        public int G = 1;
        public int B = 2;

        public int RGB = 3;
        #endregion


        #region Member variables.
        //private long[] m_HistogramData = new long[256];
        //private long[] m_RefData = new long[256];
        //private Point[] m_ptHistogramData = new Point[256];
        //private Point[] m_ptRefData = new Point[256];

        //private static Path m_HistogramPath = new Path();
        //private static Path m_RefPath = new Path();

        private double m_fIntervalX = 0.0;
        private double m_fIntervalY = 0.0;

        private double m_fMarginX = 30.0;
        private static readonly double m_fMarginY = 20.0;
        private static readonly double m_XMarginOffset = 2.0;
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
            //DrawingCanvas.SelectedGraphicChangeEvent += DrawingCanvas_SelectedGraphicChangeEvent;

            this.SizeChanged += (s, e) =>
            {
                Histogram.RenderTransform = new ScaleTransform(this.ActualWidth / CONTROL_WIDTH, this.ActualHeight / CONTROL_HEIGHT);
            };
        }

        private void InitializeDialog()
        {
            Mono_Histogram.Pixel_Datas = new long[256];
            Mono_Histogram.Point_Datas = new System.Windows.Point[256];
            Mono_Histogram.Path_Data = new System.Windows.Shapes.Path();

            Mono_Ref_Histogram.Pixel_Datas = new long[256];
            Mono_Ref_Histogram.Point_Datas = new System.Windows.Point[256];
            Mono_Ref_Histogram.Path_Data = new System.Windows.Shapes.Path();

            for (int color = 0; color < RGB; color++)
            {
                Color_Histogram[color].Pixel_Datas = new long[256];
                Color_Histogram[color].Point_Datas = new System.Windows.Point[256];
                Color_Histogram[color].Path_Data = new System.Windows.Shapes.Path();
            }

            for (int color = 0; color < RGB; color++)
            {
                Color_Ref_Histogram[color].Pixel_Datas = new long[256];
                Color_Ref_Histogram[color].Point_Datas = new System.Windows.Point[256];
                Color_Ref_Histogram[color].Path_Data = new System.Windows.Shapes.Path();
            }


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
            DotLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
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

        public void Histogram_Calculate(BitmapSource ref_Image, BitmapSource histogram_Image)
        {
            try
            {
                if (ref_Image != null)
                {
                    if (ref_Image.Format.BitsPerPixel == 24) Color_Ref_All_Pixel = BitmapSourceHelper.Color_CalculateHistogramData(ref_Image);
                    if (ref_Image.Format.BitsPerPixel == 8) Mono_Ref_All_Pixel = BitmapSourceHelper.Mono_CalculateHistogramData(ref_Image);
                }
                else
                {
                    Color_Ref_All_Pixel = new long[3, 256];
                    Mono_Ref_All_Pixel = new long[256];
                }

                if (histogram_Image != null)
                {
                    if (histogram_Image.Format.BitsPerPixel == 24) Color_Histogram_All_Pixel = BitmapSourceHelper.Color_CalculateHistogramData(histogram_Image);
                    if (histogram_Image.Format.BitsPerPixel == 8) Mono_Histogram_All_Pixel = BitmapSourceHelper.Mono_CalculateHistogramData(histogram_Image);
                }
                else
                {
                    Color_Histogram_All_Pixel = new long[3, 256];
                    Mono_Histogram_All_Pixel = new long[256];
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in DrawingCanvas_SelectedGraphicChangeEvent(HistogramCtrl.xaml.cs)");
            }
        }

        public void ImageChanged(ChannelType channel)
        {
            try
            {
                Histogram.Children.Clear();

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

                if (channel == ChannelType.Mono)
                {
                    Mono_Histogram.Pixel_Datas = (long[])Mono_Histogram_All_Pixel.Clone();
                    Mono_Ref_Histogram.Pixel_Datas = (long[])Mono_Ref_All_Pixel.Clone();

                    Mono_DrawHistogram(Mono_Ref_All_Pixel, Mono_Histogram_All_Pixel);
                }
                else
                {
                    Color_Histogram[R].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Histogram_All_Pixel[R, i]).ToArray();
                    Color_Histogram[G].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Histogram_All_Pixel[G, i]).ToArray();
                    Color_Histogram[B].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Histogram_All_Pixel[B, i]).ToArray();

                    Color_Ref_Histogram[R].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Ref_All_Pixel[R, i]).ToArray();
                    Color_Ref_Histogram[G].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Ref_All_Pixel[G, i]).ToArray();
                    Color_Ref_Histogram[B].Pixel_Datas = Enumerable.Range(0, 256).Select(i => Color_Ref_All_Pixel[B, i]).ToArray();

                    Color_DrawHistogram(Color_Ref_All_Pixel, Color_Histogram_All_Pixel, channel);
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in DrawingCanvas_SelectedGraphicChangeEvent(HistogramCtrl.xaml.cs)");
            }
        }

        public void Color_DrawHistogram(long[,] Ref_Data, long[,] Histogram_Data, ChannelType channel)
        {

            long max = new long();

            long[] Ref_R_Data = new long[256];
            long[] Ref_G_Data = new long[256];
            long[] Ref_B_Data = new long[256];

            long[] Histogram_R_Data = new long[256];
            long[] Histogram_G_Data = new long[256];
            long[] Histogram_B_Data = new long[256];

            if (Histogram_Data == null && Ref_R_Data == null) return;

            if (Histogram_Data != null)
            {
                for (int i = 0; i < 256; i++)
                {
                    Histogram_R_Data[i] = Histogram_Data[R, i];
                    Histogram_G_Data[i] = Histogram_Data[G, i];
                    Histogram_B_Data[i] = Histogram_Data[B, i];
                }
            }


            if (Ref_Data != null)
            {
                for (int i = 0; i < 256; i++)
                {
                    Ref_R_Data[i] = Ref_Data[R, i];
                    Ref_G_Data[i] = Ref_Data[G, i];
                    Ref_B_Data[i] = Ref_Data[B, i];
                }
            }


            long histo_max = 0;
            long ref_max = 0;

            if (Histogram_Data != null && Ref_R_Data != null)
            {
                histo_max = (Histogram_R_Data.Max() > Histogram_G_Data.Max()) ? Histogram_R_Data.Max() : Histogram_G_Data.Max();
                histo_max = (Histogram_B_Data.Max() > histo_max) ? Histogram_B_Data.Max() : histo_max;

                ref_max = (Ref_R_Data.Max() > Ref_G_Data.Max()) ? Ref_R_Data.Max() : Ref_G_Data.Max();
                ref_max = (Ref_B_Data.Max() > ref_max) ? Ref_B_Data.Max() : ref_max;


                max = (histo_max >= ref_max) ? histo_max : ref_max;

            }
            else if (Histogram_Data != null && Ref_R_Data == null)
            {
                histo_max = (Histogram_R_Data.Max() > Histogram_G_Data.Max()) ? Histogram_R_Data.Max() : Histogram_G_Data.Max();
                histo_max = (Histogram_B_Data.Max() > histo_max) ? Histogram_B_Data.Max() : histo_max;
                max = histo_max;
            }
            else if (Histogram_Data == null && Ref_R_Data != null)
            {
                ref_max = (Ref_R_Data.Max() > Ref_G_Data.Max()) ? Ref_R_Data.Max() : Ref_G_Data.Max();
                ref_max = (Ref_B_Data.Max() > ref_max) ? Ref_B_Data.Max() : ref_max;
                max = ref_max;
            }
            else
            {
                max = 0;
            }


            long lMaxValue = max;
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

                Color_Histogram[R].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Histogram[R].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Histogram_R_Data[nIndex] * m_fIntervalY - m_fMarginY);

                Color_Histogram[G].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Histogram[G].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Histogram_G_Data[nIndex] * m_fIntervalY - m_fMarginY);

                Color_Histogram[B].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Histogram[B].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Histogram_B_Data[nIndex] * m_fIntervalY - m_fMarginY);



                Color_Ref_Histogram[R].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Ref_Histogram[R].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Ref_R_Data[nIndex] * m_fIntervalY - m_fMarginY);

                Color_Ref_Histogram[G].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Ref_Histogram[G].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Ref_G_Data[nIndex] * m_fIntervalY - m_fMarginY);

                Color_Ref_Histogram[B].Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Color_Ref_Histogram[B].Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Ref_B_Data[nIndex] * m_fIntervalY - m_fMarginY);
            }


            #region Draw Lines & Texts.
            // Draw X-axis labels.

            if (Histogram_Data != null)
            {
                TextBlock Label = new TextBlock();
                Label.Text = "0";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Histogram[R].Point_Datas[0].X - 3);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "100";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Histogram[R].Point_Datas[92].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "200";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Histogram[R].Point_Datas[192].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "255";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Histogram[R].Point_Datas[246].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                //Draw Y-axis label & dotted lines.
                for (int i = 0; i < nDottedLineCnt; i++)
                {
                    Line DotLine = new Line();
                    DotLine.X1 = m_fMarginX * 10 - 1;
                    DotLine.X2 = CONTROL_WIDTH - 1;
                    DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                    DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                    DotLine.StrokeThickness = 2;
                    DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                    //DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                    this.Histogram.Children.Add(DotLine);

                    if (i != 0)
                    {
                        Label = new TextBlock();
                        Label.Text = (nDottedLineGap * i).ToString();
                        Label.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(Label, 5);
                        Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                        this.Histogram.Children.Add(Label);
                    }
                }

            }
            else
            {
                TextBlock Label = new TextBlock();
                Label.Text = "0";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Ref_Histogram[R].Point_Datas[0].X - 3);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "100";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Ref_Histogram[R].Point_Datas[92].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "200";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Ref_Histogram[R].Point_Datas[192].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "255";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Color_Ref_Histogram[R].Point_Datas[246].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                //Draw Y-axis label & dotted lines.
                for (int i = 0; i < nDottedLineCnt; i++)
                {
                    Line DotLine = new Line();
                    DotLine.X1 = m_fMarginX * 10 - 1;
                    DotLine.X2 = CONTROL_WIDTH - 1;
                    DotLine.Y1 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                    DotLine.Y2 = (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt));
                    DotLine.StrokeThickness = 2;
                    DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                    // DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                    this.Histogram.Children.Add(DotLine);

                    if (i != 0)
                    {
                        Label = new TextBlock();
                        Label.Text = (nDottedLineGap * i).ToString();
                        Label.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(Label, 5);
                        Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                        this.Histogram.Children.Add(Label);
                    }
                }

            }

            // Draw axis lines.
            Line AxisX = new Line();
            AxisX.X1 = m_fMarginX * 10 - 1;
            AxisX.X2 = CONTROL_WIDTH - 1;
            AxisX.Y1 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisX.StrokeThickness = 2;
            AxisX.Stroke = new SolidColorBrush(Colors.White);
            this.Histogram.Children.Add(AxisX);

            Line AxisY = new Line();
            AxisY.X1 = m_fMarginX * 10 - 1;
            AxisY.X2 = m_fMarginX * 10 - 1;
            AxisY.Y1 = m_fMarginY;
            AxisY.Y2 = CONTROL_HEIGHT - m_fMarginY;
            AxisY.StrokeThickness = 2;
            AxisY.Stroke = new SolidColorBrush(Colors.White);
            this.Histogram.Children.Add(AxisY);
            #endregion

            #region Draw histogram.
            StreamGeometry R_historgamGeometry = new StreamGeometry();
            StreamGeometry G_historgamGeometry = new StreamGeometry();
            StreamGeometry B_historgamGeometry = new StreamGeometry();

            using (StreamGeometryContext ctx = R_historgamGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Histogram[R].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Histogram[R].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Histogram[R].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            R_historgamGeometry.Freeze();

            using (StreamGeometryContext ctx = G_historgamGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Histogram[G].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Histogram[G].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Histogram[G].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            G_historgamGeometry.Freeze();

            using (StreamGeometryContext ctx = B_historgamGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Histogram[B].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Histogram[B].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Histogram[B].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            B_historgamGeometry.Freeze();

            Color_Histogram[R].Path_Data.Data = R_historgamGeometry;
            Color_Histogram[R].Path_Data.Fill = new SolidColorBrush(Colors.Red);
            Color_Histogram[R].Path_Data.Opacity = 0.6;


            Color_Histogram[G].Path_Data.Data = G_historgamGeometry;
            Color_Histogram[G].Path_Data.Fill = new SolidColorBrush(Colors.Green);
            Color_Histogram[G].Path_Data.Opacity = 0.6;


            Color_Histogram[B].Path_Data.Data = B_historgamGeometry;
            Color_Histogram[B].Path_Data.Fill = new SolidColorBrush(Colors.Blue);
            Color_Histogram[B].Path_Data.Opacity = 0.6;

            if (channel == ChannelType.Color)
            {
                this.Histogram.Children.Add(Color_Histogram[R].Path_Data);
                this.Histogram.Children.Add(Color_Histogram[G].Path_Data);
                this.Histogram.Children.Add(Color_Histogram[B].Path_Data);
            }

            if (channel == ChannelType.RED) this.Histogram.Children.Add(Color_Histogram[R].Path_Data);
            if (channel == ChannelType.GREEN) this.Histogram.Children.Add(Color_Histogram[G].Path_Data);
            if (channel == ChannelType.BLUE) this.Histogram.Children.Add(Color_Histogram[B].Path_Data);



            #endregion

            #region Draw Reference histogram.
            StreamGeometry R_RefGeometry = new StreamGeometry();
            StreamGeometry G_RefGeometry = new StreamGeometry();
            StreamGeometry B_RefGeometry = new StreamGeometry();


            using (StreamGeometryContext ctx = R_RefGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Ref_Histogram[R].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Ref_Histogram[R].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Ref_Histogram[R].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            R_RefGeometry.Freeze();

            using (StreamGeometryContext ctx = G_RefGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Ref_Histogram[G].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Ref_Histogram[G].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Ref_Histogram[G].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            G_RefGeometry.Freeze();

            using (StreamGeometryContext ctx = B_RefGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Color_Ref_Histogram[B].Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Color_Ref_Histogram[B].Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Color_Ref_Histogram[B].Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            B_RefGeometry.Freeze();


            Color_Ref_Histogram[R].Path_Data.Data = R_RefGeometry;
            Color_Ref_Histogram[R].Path_Data.Stroke = new SolidColorBrush(Colors.Red);
            Color_Ref_Histogram[R].Path_Data.StrokeThickness = m_fIntervalX;


            Color_Ref_Histogram[G].Path_Data.Data = G_RefGeometry;
            Color_Ref_Histogram[G].Path_Data.Stroke = new SolidColorBrush(Colors.Green);
            Color_Ref_Histogram[G].Path_Data.StrokeThickness = m_fIntervalX;


            Color_Ref_Histogram[B].Path_Data.Data = B_RefGeometry;
            Color_Ref_Histogram[B].Path_Data.Stroke = new SolidColorBrush(Colors.Blue);
            Color_Ref_Histogram[B].Path_Data.StrokeThickness = m_fIntervalX;



            if (channel == ChannelType.Color)
            {
                this.Histogram.Children.Add(Color_Ref_Histogram[R].Path_Data);
                this.Histogram.Children.Add(Color_Ref_Histogram[G].Path_Data);
                this.Histogram.Children.Add(Color_Ref_Histogram[B].Path_Data);
            }

            if (channel == ChannelType.RED) this.Histogram.Children.Add(Color_Ref_Histogram[R].Path_Data);
            if (channel == ChannelType.GREEN) this.Histogram.Children.Add(Color_Ref_Histogram[G].Path_Data);
            if (channel == ChannelType.BLUE) this.Histogram.Children.Add(Color_Ref_Histogram[B].Path_Data);

            #endregion
        }

        public void Mono_DrawHistogram(long[] Ref_Data, long[] Histogram_Data)
        {

            long lMaxValue = Ref_Data.Max() >= Histogram_Data.Max() ? Ref_Data.Max() : Histogram_Data.Max();
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
                Mono_Histogram.Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Mono_Histogram.Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Histogram_Data[nIndex] * m_fIntervalY - m_fMarginY);

                Mono_Ref_Histogram.Point_Datas[nIndex].X = Math.Round(nIndex * m_fIntervalX + (m_fMarginX + 1) * 10);
                Mono_Ref_Histogram.Point_Datas[nIndex].Y = Math.Round(CONTROL_HEIGHT - 1 - Ref_Data[nIndex] * m_fIntervalY - m_fMarginY);
            }


            #region Draw Lines & Texts.
            // Draw X-axis labels.

            if (Histogram_Data != null)
            {
                TextBlock Label = new TextBlock();
                Label.Text = "0";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Histogram.Point_Datas[0].X - 3);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "100";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Histogram.Point_Datas[92].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "200";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Histogram.Point_Datas[192].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "255";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Histogram.Point_Datas[246].X);
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
                    //DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                    this.Histogram.Children.Add(DotLine);

                    if (i != 0)
                    {
                        Label = new TextBlock();
                        Label.Text = (nDottedLineGap * i).ToString();
                        Label.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(Label, 5);
                        Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                        this.Histogram.Children.Add(Label);
                    }
                }

            }
            else
            {
                TextBlock Label = new TextBlock();
                Label.Text = "0";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Ref_Histogram.Point_Datas[0].X - 3);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "100";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Ref_Histogram.Point_Datas[92].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "200";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Ref_Histogram.Point_Datas[192].X);
                Canvas.SetTop(Label, CONTROL_HEIGHT - m_fMarginY);
                this.Histogram.Children.Add(Label);

                Label = new TextBlock();
                Label.Text = "255";
                Label.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetLeft(Label, Mono_Ref_Histogram.Point_Datas[246].X);
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
                    //DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
                    this.Histogram.Children.Add(DotLine);

                    if (i != 0)
                    {
                        Label = new TextBlock();
                        Label.Text = (nDottedLineGap * i).ToString();
                        Label.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(Label, 5);
                        Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) * (1 - (i * 1 / (double)nDottedLineCnt)));
                        this.Histogram.Children.Add(Label);
                    }
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
                ctx.BeginFigure(new System.Windows.Point(Mono_Histogram.Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Mono_Histogram.Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Mono_Histogram.Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            historgamGeometry.Freeze();


            Mono_Histogram.Path_Data.Data = historgamGeometry;
            Mono_Histogram.Path_Data.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));
            Mono_Histogram.Path_Data.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(Mono_Histogram.Path_Data);

            #endregion

            #region Draw Reference histogram.
            StreamGeometry RefGeometry = new StreamGeometry();

            using (StreamGeometryContext ctx = RefGeometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(Mono_Ref_Histogram.Point_Datas[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < 256; k++)
                {
                    ctx.LineTo(Mono_Ref_Histogram.Point_Datas[k], true, true);
                }
                ctx.LineTo(new System.Windows.Point(Mono_Ref_Histogram.Point_Datas[255].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            RefGeometry.Freeze();

            Mono_Ref_Histogram.Path_Data.Data = RefGeometry;
            Mono_Ref_Histogram.Path_Data.Stroke = new SolidColorBrush(Colors.Red);
            Mono_Ref_Histogram.Path_Data.StrokeThickness = m_fIntervalX;
            this.Histogram.Children.Add(Mono_Ref_Histogram.Path_Data);
            #endregion
        }

        #region Supports binarization
        public void EnableBinarization(int lowerThreshold, int upperThreshold, bool IsSingleMode, bool isReference, bool isColor, ChannelType channel)
        {
            Point[] HistogramData = new Point[256];
            if (channel == ChannelType.Color) return;

            if (isReference)
            {
                if (isColor) HistogramData = (Point[])Color_Ref_Histogram[(int)channel].Point_Datas.Clone();
                else HistogramData = (Point[])Mono_Ref_Histogram.Point_Datas.Clone();
            }
            else
            {
                if (isColor) HistogramData = (Point[])Color_Histogram[(int)channel].Point_Datas.Clone();
                else HistogramData = (Point[])Mono_Histogram.Point_Datas.Clone();
            }



            if (GLB.Windows.Review.BaseImageSource != null)
            {
                if (IsSingleMode)
                {
                    m_DivideSingle.X1 = HistogramData[lowerThreshold].X;
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
                    m_DivideLeft.X1 = HistogramData[lowerThreshold].X;
                    m_DivideLeft.X2 = m_DivideLeft.X1;

                    m_DivideRight.X1 = HistogramData[upperThreshold].X;
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
