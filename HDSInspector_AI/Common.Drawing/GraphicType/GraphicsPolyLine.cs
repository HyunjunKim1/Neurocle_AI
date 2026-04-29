// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

// Modified & Commented by suoow2.

namespace Common.Drawing
{
    // PolyLine graphics object.
    public class GraphicsPolyLine : GraphicsBase
    {
        internal void CalcBoundaryRect()
        {
            double fMinX, fMaxX, fMinY, fMaxY;
            fMinX = fMaxX = points[0].X;
            fMinY = fMaxY = points[0].Y;

            for (int i = 1; i < points.Length; i++)
            {
                if (fMinX > points[i].X)
                    fMinX = points[i].X;
                if (fMaxX < points[i].X)
                    fMaxX = points[i].X;
                if (fMinY > points[i].Y)
                    fMinY = points[i].Y;
                if (fMaxY < points[i].Y)
                    fMaxY = points[i].Y;
            }

            boundaryRect.X = fMinX;
            boundaryRect.Y = fMinY;
            //boundaryRect.Width = fMaxX - fMinX + 1;
            //boundaryRect.Height = fMaxY - fMinY + 1;
            boundaryRect.Width = fMaxX - fMinX;
            boundaryRect.Height = fMaxY - fMinY;

            startPoint = boundaryRect.TopLeft;

            LeftProperty = (int)boundaryRect.Left;
            TopProperty = (int)boundaryRect.Top;
            WidthProperty = boundaryRect.Width;
            HeightProperty = boundaryRect.Height;

        }

        #region Class Members
        private static Cursor handleCursor;
        private bool IsClosed = false;

        protected PathGeometry pathGeometry;
        protected Point[] points;
        #endregion Class Members

        #region Constructors
        public GraphicsPolyLine(Point[] points, double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale)
        {
            this.startPoint = new Point(points[0].X, points[0].Y);

            MakeGeometryFromPoints(ref points);
            CalcBoundaryRect();

            this.graphicsLineWidth = lineWidth;
            this.graphicsRegionType = regionType;
            this.graphicsObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            //RefreshDrawng();
        }

        public GraphicsPolyLine(List<Point> pointList, double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale)
            : this(pointList, lineWidth, regionType, objectColor, actualScale, string.Empty)
        {

        }

        public GraphicsPolyLine(List<Point> pointList, double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale, string caption)
        {
            this.points = new Point[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
            {
                points[i] = pointList[i];
            }

            CalcBoundaryRect();
            MakeGeometryFromPointsByEndTime(ref points);

            this.graphicsLineWidth = lineWidth;
            this.graphicsRegionType = regionType;
            this.graphicsObjectColor = objectColor;
            this.OriginObjectColor = objectColor;
            this.graphicsActualScale = actualScale;
            this.caption = caption;

            // Refresh Drawing.
            DrawingContext dc = this.RenderOpen();
            Draw(dc);
            dc.Close();
        }

        public GraphicsPolyLine()
            : this(new Point[] { new Point(0.0, 0.0), new Point(100.0, 100.0) }, 1.0, GraphicsRegionType.Inspection, Colors.Black, 1.0)
        {
        }

        static GraphicsPolyLine()
        {
            using (MemoryStream stream = new MemoryStream(Properties.Resources.PolyHandle))
            {
                handleCursor = new Cursor(stream);
            }
        }
        #endregion Constructors

        #region Other Functions
        /// <summary>   Convert geometry to array of points. </summary>
        private void MakePoints()
        {
            points = new Point[pathGeometry.Figures[0].Segments.Count + 1];
            points[0] = pathGeometry.Figures[0].StartPoint;

            for (int i = 0; i < pathGeometry.Figures[0].Segments.Count; i++)
            {
                points[i + 1] = ((LineSegment)(pathGeometry.Figures[0].Segments[i])).Point;
            }
        }

        /// <summary>   Return array of points. </summary>
        /// <returns>   The points. </returns>
        public Point[] GetPoints()
        {
            return points;
        }

        private void MakeGeometryFromPoints(ref Point[] points)
        {
            if (points == null)
            {
                // This really sucks, XML file contains Points object,
                // but list of points is empty. Do something to prevent program crush.
                points = new Point[2];
            }

            PathFigure figure = new PathFigure { IsClosed = IsClosed };
            if (points.Length >= 1)
            {
                figure.StartPoint = points[0];
            }

            for (int i = 1; i < points.Length; i++)
            {
                LineSegment segment = new LineSegment(points[i], true) { IsSmoothJoin = true };
                figure.Segments.Add(segment);
            }

            pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(figure);

            MakePoints(); // keep points array up to date
        }

        private void MakeGeometryFromPointsByEndTime(ref Point[] points)
        {
            if (points.Length < 1)
            {
                return;
            }

            PathFigure figure = new PathFigure { IsClosed = true };
            IsClosed = true;

            figure.StartPoint = points[0];
            
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment segment = new LineSegment(points[i], true) { IsSmoothJoin = true };
                figure.Segments.Add(segment);
            }

            pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(figure);
        }

        public void AddPoint(Point point)
        {
            LineSegment segment = new LineSegment(point, true) { IsSmoothJoin = true };

            pathGeometry.Figures[0].Segments.Add(segment);

            MakePoints();   // keep points array up to date
            CalcBoundaryRect();
        }

        #endregion Other Functions

        #region Overrides
        // PolyLine을 화면에 출력한다.
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            MakeGeometryFromPoints(ref points);

            drawingContext.DrawGeometry(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                pathGeometry);

            if (this.ActualScale >= 0.5)
            {
                drawingContext.DrawText(
                    CreateCaptionString(),
                    new Point(startPoint.X, startPoint.Y - (15 / graphicsActualScale)));
            }

            if (IsSelected)
            {
                if (LineSegments != null && LineSegments.Length > 0)
                {
                    double mostLeft = 0.0;
                    double mostTop = 0.0;
                    if (points != null && points.Length > 0)
                    {
                        mostLeft = points[0].X;
                        mostTop = points[0].Y;
                        foreach (Point point in points)
                        {
                            mostLeft = Math.Min(mostLeft, point.X);
                            mostTop = Math.Min(mostTop, point.Y);
                        }
                    }
                    foreach (GraphicsSkeletonLine lineSegment in LineSegments)
                        lineSegment.Draw(drawingContext, new Point(mostLeft, mostTop), this.ActualScale);
                }
            }
            base.Draw(drawingContext);
        }

        // Point가 PolyLine 내부에 위치했는가를 판정한다.
        public override bool Contains(Point point)
        {
            return pathGeometry.FillContains(point) || 
                pathGeometry.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsPolyLine(this);
        }

        public override int HandleCount
        {
            get
            {
                return pathGeometry.Figures[0].Segments.Count + 1;
            }
        }

        public override Point GetHandle(int handleNumber)
        {
            if (handleNumber < 1)
            {
                handleNumber = 1;
            }

            if (handleNumber > points.Length)
            {
                handleNumber = points.Length;
            }

            return points[handleNumber - 1];
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return handleCursor;
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
            LineSegments = null;

            point.X = (point.X < 0) ? 0 : point.X;
            point.Y = (point.Y < 0) ? 0 : point.Y;

            if (handleNumber == 1)
            {
                pathGeometry.Figures[0].StartPoint = point;
            }
            else
            {
                ((LineSegment)(pathGeometry.Figures[0].Segments[handleNumber - 2])).Point = point;
            }
            
            MakePoints();
            CalcBoundaryRect();
            RefreshDrawing();
        }

        public override void Move(double deltaX, double deltaY, double maxWidth, double maxHeight)
        {

            CalcBoundaryRect();

            if ((boundaryRect.Left + deltaX) < 0)
                deltaX = -boundaryRect.Left;
            if ((boundaryRect.Top + deltaY) < 0)
                deltaY = -boundaryRect.Top;
            if ((boundaryRect.Right + deltaX) >= maxWidth)
                deltaX = (maxWidth - 1) - boundaryRect.Right;
            if ((boundaryRect.Bottom + deltaY) >= maxHeight)
                deltaY = (maxHeight - 1) - boundaryRect.Bottom;

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += deltaX;
                points[i].Y += deltaY;
            }

            boundaryRect.X += deltaX;
            boundaryRect.Y += deltaY;
            
            MakeGeometryFromPoints(ref points);

            RefreshDrawing();
        }

        public override int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                    {
                        return i;
                    }
                }
            }

            if (Contains(point))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public override bool IntersectsWith(Rect rectangle)
        {
            RectangleGeometry rg = new RectangleGeometry(rectangle);

            PathGeometry p = Geometry.Combine(rg, pathGeometry, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }
        #endregion Overrides

        #region Binding Properties
        private int left;
        private int top;
        private double width;
        private double height;

        [XmlIgnore]
        public int LeftProperty
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
                Notify("LeftProperty");
            }
        }

        [XmlIgnore]
        public int TopProperty
        {
            get
            {
                return top;
            }
            set
            {
                top = value;
                Notify("TopProperty");
            }
        }

        [XmlIgnore]
        public double WidthProperty
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                Notify("WidthProperty");
            }
        }

        [XmlIgnore]
        public double HeightProperty
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                Notify("HeightProperty");
            }
        }

        [XmlIgnore]
        public Point[] Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }
        #endregion
    }
}
