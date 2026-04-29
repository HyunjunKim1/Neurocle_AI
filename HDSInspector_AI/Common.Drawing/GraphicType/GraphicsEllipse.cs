// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Xml.Serialization;

// Commented by suoow2.

namespace Common.Drawing
{
    // Ellipse graphics object.
    public class GraphicsEllipse : GraphicsRectangleBase
    {
        #region Constructors
        public GraphicsEllipse(double left, double top, double right, double bottom,
            double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale)
            : this(left, top, right, bottom, lineWidth, regionType, objectColor, actualScale, string.Empty)
        {

        }

        public GraphicsEllipse(double left, double top, double right, double bottom,
            double lineWidth, GraphicsRegionType regionType, Color objectColor, double actualScale, string caption)
        {
            this.startPoint = new Point(left, top);
            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;
            this.graphicsLineWidth = lineWidth;
            this.graphicsRegionType = regionType;
            this.graphicsObjectColor = objectColor;
            this.OriginObjectColor = objectColor;
            this.graphicsActualScale = actualScale;
            this.caption = caption;

            this.LeftProperty = (int)left;
            this.TopProperty = (int)top;
            this.WidthProperty = Math.Abs(rectangleRight - rectangleLeft);
            this.HeightProperty = Math.Abs(rectangleBottom - rectangleTop);

            //RefreshDrawng();
        }

        public GraphicsEllipse()
            : this(0.0, 0.0, 100.0, 100.0, 1.0, GraphicsRegionType.Inspection, Colors.Black, 1.0)
        {
        }

        #endregion Constructors

        #region Overrides
        // Ellipse를 화면에 출력한다.
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            Rect r = Rectangle;
            Point center = new Point((r.Left + r.Right) / 2.0, (r.Top + r.Bottom) / 2.0);

            double radiusX = (r.Right - r.Left) / 2.0;
            double radiusY = (r.Bottom - r.Top) / 2.0;

            drawingContext.DrawEllipse(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                center, radiusX, radiusY);

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
                    foreach (GraphicsSkeletonLine lineSegment in LineSegments)
                        lineSegment.Draw(drawingContext, new Point(rectangleLeft, rectangleTop), this.ActualScale);
                }
            }
            base.Draw(drawingContext);
        }

        public override bool Contains(Point point)
        {
            if (IsSelected)
            {
                return this.Rectangle.Contains(point);
            }
            else
            {
                EllipseGeometry g = new EllipseGeometry(Rectangle);

                return g.FillContains(point) || g.StrokeContains(new Pen(Brushes.Black, ActualLineWidth), point);
            }
        }

        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsEllipse(this);
        }

        public override void Move(double deltaX, double deltaY, double maxWidth, double maxHeight)
        {
            LineSegments = null;

            base.Move(deltaX, deltaY, maxWidth, maxHeight);
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
            base.MoveHandleTo(point, handleNumber);
        }
        #endregion Overrides
    }
}
