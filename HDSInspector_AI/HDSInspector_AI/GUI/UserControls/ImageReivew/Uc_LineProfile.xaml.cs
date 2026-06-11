using HDSInspector_AI.Class.GlobalFunctions;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Uc_LineProfile.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Uc_LineProfile : UserControl
    {
        private const int CONTROL_WIDTH = 350;
        private const int CONTROL_HEIGHT = 180;

        private const int SAMPLING_RATE = 5; // n픽셀 단위로 한번씩 추출.

        private double m_fIntervalX = 0.0;
        private double m_fIntervalY = 0.0;

        private double c_fIntervalX = 0.0;
        private double c_fIntervalY = 0.0;

        private static double m_fMarginX = 30;
        private static double m_fMarginY = 20;

        private int m_nPixelWidth = 0;
        private int color_nPixelWidth = 0;

        #region Private member variables.

        public Point[] m_ptLineProfileDataList; // X,Y interval에 의해 측정된 Line Profile 좌표리스트
        public Path m_LineProfilePath = new Path(); // 좌표리스트를 이용하여 화면에 Line Profile을 그려낸다.

        public Point[] R_ptLineProfileDataList; // X,Y interval에 의해 측정된Red   Line Profile 좌표리스트
        public Point[] G_ptLineProfileDataList; // X,Y interval에 의해 측정된Green Line Profile 좌표리스트
        public Point[] B_ptLineProfileDataList; // X,Y interval에 의해 측정된Blue  Line Profile 좌표리스트

        public Path R_LineProfilePath = new Path(); // 좌표리스트를 이용하여 화면에 Line Profile을 그려낸다.
        public Path G_LineProfilePath = new Path(); // 좌표리스트를 이용하여 화면에 Line Profile을 그려낸다.
        public Path B_LineProfilePath = new Path(); // 좌표리스트를 이용하여 화면에 Line Profile을 그려낸다.

        #endregion

        #region Constructor & InitializeDialog
        public Uc_LineProfile()
        {
            InitializeComponent();
            InitializeDialog();
            InitializeEvent();
        }
        private void InitializeEvent()
        {
            this.SizeChanged += (s, e) =>
            {
                LineProfile.RenderTransform = new ScaleTransform(this.ActualWidth / CONTROL_WIDTH, this.ActualHeight / CONTROL_HEIGHT);
            };
        }

        private void InitializeDialog()
        {
            int nDottedLineProfileGap = 50;
            int nDashLineCnt = (255 / nDottedLineProfileGap) + 1;
            double fLineProfileMaxHeight = (CONTROL_HEIGHT - m_fMarginY * 2) * 250.0 / 255.0;

            Line DotLine;
            TextBlock Label;
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
                LineProfile.Children.Add(DotLine);

                Label = new TextBlock();
                Label.Text = (nDottedLineProfileGap * i).ToString();
                Canvas.SetLeft(Label, 5);
                Canvas.SetTop(Label, (CONTROL_HEIGHT - m_fMarginY) - (fLineProfileMaxHeight) * (i / ((double)nDashLineCnt - 1)));
                LineProfile.Children.Add(Label);
            }

            DotLine = new Line();
            DotLine.X1 = m_fMarginX - 1;
            DotLine.X2 = CONTROL_WIDTH - 1;
            DotLine.Y1 = m_fMarginY;
            DotLine.Y2 = m_fMarginY;
            DotLine.StrokeThickness = 2;
            DotLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
            DotLine.Stroke = new SolidColorBrush(Colors.DarkGray);
            LineProfile.Children.Add(DotLine);

            Label = new TextBlock();
            Label.Text = "255";
            Canvas.SetLeft(Label, 5);
            Canvas.SetTop(Label, m_fMarginY);
            LineProfile.Children.Add(Label);

            // Draw X-Axis line of LineProfile
            Line AxisX = new Line
            {
                X1 = m_fMarginX - 1,
                X2 = CONTROL_WIDTH - 1,
                Y1 = CONTROL_HEIGHT - m_fMarginY,
                Y2 = CONTROL_HEIGHT - m_fMarginY,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            LineProfile.Children.Add(AxisX);

            // Draw Y-Axis line of LineProfile
            Line AxisY = new Line
            {
                X1 = m_fMarginX - 1,
                X2 = m_fMarginX - 1,
                Y1 = m_fMarginY - 1,
                Y2 = CONTROL_HEIGHT - m_fMarginY,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            LineProfile.Children.Add(AxisY);
        }
        #endregion

        public void Refresh()
        {
            LineProfile.Children.Clear();
            InitializeDialog();
        }

        public void SetLineProfileSource(BitmapSource source)
        {
            if (source == null)
            {
                return;
            }

            // TBD : 샘플링을 통한 LineProfile 그리기.

            //if (source.PixelWidth < MAX_SOURCE_WIDTH)
            //{
            m_nPixelWidth = source.PixelWidth;
            //}
            //else
            //{
            //    m_nPixelWidth = source.PixelWidth / SAMPLING_RATE;
            //}

            m_ptLineProfileDataList = new Point[m_nPixelWidth];
            m_LineProfilePath.Fill = new SolidColorBrush(Color.FromArgb(255, 68, 68, 68));

            m_fIntervalX = 1.0 / m_nPixelWidth * (CONTROL_WIDTH - m_fMarginX);
            m_fIntervalY = 1.0 / 255.0 * (CONTROL_HEIGHT - m_fMarginY * 2);
        }

        public void DrawLineProfile(byte[] lineData)
        {
            if (lineData == null || m_ptLineProfileDataList == null)
            {
                return;
            }

            int nIndex = 0;
            int stepCount = 0;
            foreach (byte data in lineData)
            {
                if (stepCount % SAMPLING_RATE != 0)
                {
                    stepCount++;
                    continue;
                }

                try
                {
                    if (nIndex >= m_ptLineProfileDataList.Length)
                    {
                        break;
                    }
                    m_ptLineProfileDataList[nIndex].X = nIndex * m_fIntervalX + m_fMarginX;
                    m_ptLineProfileDataList[nIndex++].Y = CONTROL_HEIGHT - 1 - data * m_fIntervalY - m_fMarginY;

                    stepCount = (stepCount == 5) ? 0 : stepCount;
                }
                catch
                {

                }
            }

            StreamGeometry lineProfile = new StreamGeometry();
            using (StreamGeometryContext ctx = lineProfile.Open())
            {
                ctx.BeginFigure(new Point(m_ptLineProfileDataList[0].X, CONTROL_HEIGHT - m_fMarginY), true, true);
                for (int k = 0; k < m_nPixelWidth; k++)
                {
                    ctx.LineTo(m_ptLineProfileDataList[k], false, true);
                }
                ctx.LineTo(new Point(m_ptLineProfileDataList[m_nPixelWidth - 1].X, CONTROL_HEIGHT - m_fMarginY), true, true);
            }
            lineProfile.Freeze();

            this.LineProfile.Children.Remove(m_LineProfilePath);
            m_LineProfilePath.Data = lineProfile;
            this.LineProfile.Children.Add(m_LineProfilePath);
        }
    }
}
