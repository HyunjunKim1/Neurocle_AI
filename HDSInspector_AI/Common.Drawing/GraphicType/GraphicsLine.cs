// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Collections.Generic;

// Modified & Commented by suoow2.

namespace Common.Drawing
{
    public class GraphicsLine : GraphicsBase
    {
        internal void CalcBoundaryRect()
        {
            boundaryRect.X = Math.Min(lineStart.X, lineEnd.X);
            boundaryRect.Y = Math.Min(lineStart.Y, lineEnd.Y);
            //boundaryRect.Width = Math.Abs(lineStart.X - lineEnd.X) + 1;
            //boundaryRect.Height = Math.Abs(lineStart.Y - lineEnd.Y) + 1;
            boundaryRect.Width = Math.Abs(lineStart.X - lineEnd.X);
            boundaryRect.Height = Math.Abs(lineStart.Y - lineEnd.Y);

            LeftProperty = (int)boundaryRect.Left;
            TopProperty = (int)boundaryRect.Top;
            double fWidth = boundaryRect.Width;
            double fHeight = boundaryRect.Height;

            LengthProperty = (int)(Math.Sqrt((fWidth * fWidth) + (fHeight * fHeight)));
        }

        #region Member variables.
        public Point Start
        {
            get
            {
                return lineStart;
            }
            set
            {
                lineStart = value;

                CalcBoundaryRect();        
            }
        }
        protected Point lineStart;

        public Point End
        {
            get
            {
                return lineEnd;
            }
            set
            {
                lineEnd = value;

                CalcBoundaryRect();
            }
        }
        protected Point lineEnd;
        #endregion

        #region Constructors
        public GraphicsLine(Point start, Point end, double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale)
            : this(start, end, lineWidth, regionType, objectColor, actualScale, string.Empty)
        {
			//
        }

        public GraphicsLine(Point start, Point end, double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale, 
                            string caption)
        {
            Start = start;
            End = end;
            
            this.graphicsLineWidth = lineWidth;
            this.graphicsRegionType = regionType;
            this.graphicsObjectColor = objectColor;
            this.OriginObjectColor = objectColor;
            this.graphicsActualScale = actualScale;
            this.caption = caption;

        }

        public GraphicsLine()
            : this(new Point(0.0, 0.0), new Point(100.0, 100.0), 1.0, GraphicsRegionType.Inspection, Colors.Black, 1.0)
        {
        }
        #endregion Constructors

        #region Overrides
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            if (this.ActualScale >= 0.5)
            {
                drawingContext.DrawText(
                    CreateCaptionString(),
                    new Point(lineStart.X, lineStart.Y - (15 / graphicsActualScale)));
            }

            // 2012-03-30 suoow2 modified. (Line tool을 길이 측정 용도로 사용하고자 함.)
            DashStyle dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);
            Pen dashedPen = new Pen(Brushes.Yellow, ActualLineWidth) { DashStyle = dashStyle };

            // Draw line.
            drawingContext.DrawLine(dashedPen, lineStart, lineEnd);

            base.Draw(drawingContext);
        }

        // 선 개체가 인자로 넘어오는 point를 포함하는지에 대한 bool 값을 반환한다.
        public override bool Contains(Point point)
        {
            LineGeometry g = new LineGeometry(lineStart, lineEnd);

            return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsLine(this);
        }
      
        // Get number of handles.
        // It always return 2.
        public override int HandleCount
        {
            get
            {
                return 2;
            }
        }

        public override Point GetHandle(int handleNumber)
        {
            if (handleNumber == 1)
            {
                return lineStart;
            }
            else
            {
                return lineEnd;
            }
        }

        // Hit test.
        // -1 : no hit.
        //  0 : hit anywhere.
        // >1 : handle number.
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

            return -1;
        }


        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            RectangleGeometry rg = new RectangleGeometry(rectangle);

            LineGeometry lg = new LineGeometry(lineStart, lineEnd);
            PathGeometry widen = lg.GetWidenedPathGeometry(new Pen(Brushes.Black, LineHitTestWidth));

            PathGeometry p = Geometry.Combine(rg, widen, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1: // start point.
                case 2: // end point.
                    return Cursors.SizeAll;
                default:
                    return Cursors.Arrow;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;

            if (handleNumber == 1)
                Start = point;
            else
                End = point;
            
            RefreshDrawing();
        }

        public override void Move(double deltaX, double deltaY, double maxWidth, double maxHeight)
        {
            if ((boundaryRect.Left + deltaX) < 0)
            {
                deltaX = -boundaryRect.Left;
            }
            if ((boundaryRect.Top + deltaY) < 0)
            {
                deltaY = -boundaryRect.Top;
            }
            if ((boundaryRect.Right + deltaX) >= maxWidth)
            {
                deltaX = (maxWidth - 1) - boundaryRect.Right;
            }
            if ((boundaryRect.Bottom + deltaY) >= maxHeight)
            {
                deltaY = (maxHeight - 1) - boundaryRect.Bottom;
            }

            Start = new Point(lineStart.X + deltaX, lineStart.Y + deltaX);
            End  = new Point(lineEnd.X + deltaY, lineEnd.Y + deltaY);
            
            RefreshDrawing();
        }

        #endregion Overrides

        #region Binding Properties
        private int left;
        private int top;
        private int length;

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
        public int LengthProperty
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                Notify("LengthProperty");
            }
        }
        #endregion
    }
}
