// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Input;
using System.IO;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Ellipse tool. </summary>
    class ToolEllipse : ToolRectangleBase
    {
        public ToolEllipse()
        {
            using (MemoryStream stream = new MemoryStream(Properties.Resources.Ellipse))
            {
                ToolCursor = new Cursor(stream);
            }
        }

        // Create new Ellipse.
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(drawingCanvas);
            if (p.X >= drawingCanvas.ActualWidth - 1 || p.Y >= drawingCanvas.ActualHeight - 1)
            {
                return;
            }

            if (p.X < 0)
                p.X = 0;
            if (p.Y < 0)
                p.Y = 0;

            SetGraphicsColor(drawingCanvas);

            AddNewObject(drawingCanvas,
                new GraphicsEllipse(p.X, p.Y, p.X + 1, p.Y + 1,
                                    drawingCanvas.LineWidth,
                                    drawingCanvas.RegionType,
                                    drawingCanvas.ObjectColor,
                                    drawingCanvas.ActualScale));
        }
    }
}
