// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>
    /// Selection Rectangle graphics object, used for group selection.
    /// 
    /// Instance of this class should be created only for group selection
    /// and removed immediately after group selection finished.
    /// </summary>
    public class GraphicsSelectionRectangle : GraphicsRectangleBase
    {
        #region Constructors
        public GraphicsSelectionRectangle()
            :
            this(0.0 /* rectangleLeft */, 0.0 /* rectangleTop */, 100.0 /* rectangleRight */, 100.0 /* rectangleBottom */, 1.0 /* graphicsActualScale */)
        {
        }

        public GraphicsSelectionRectangle(double left, double top, double right, double bottom, double actualScale)
        {
            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;

            this.LeftProperty = (int)left;
            this.TopProperty = (int)top;
            this.WidthProperty = Math.Abs(rectangleRight - rectangleLeft);
            this.HeightProperty = Math.Abs(rectangleBottom - rectangleTop);

            this.graphicsLineWidth = 2.0;
            this.graphicsActualScale = actualScale;
        }
        #endregion Constructors

        // Draw graphics object.
        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(
                null,
                new Pen(Brushes.Transparent, ActualLineWidth),
                Rectangle);

            DashStyle dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);

            Pen dashedPen = new Pen(Brushes.Orange, ActualLineWidth) { DashStyle = dashStyle };

            drawingContext.DrawRectangle(
                null,
                dashedPen,
                Rectangle);

            base.Draw(drawingContext);
        }

        public override bool Contains(Point point)
        {
            return this.Rectangle.Contains(point);
        }

        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return null;
        }
    }
}
