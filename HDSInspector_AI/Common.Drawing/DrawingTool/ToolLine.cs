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
    /// <summary>   Line tool. </summary>
    class ToolLine : ToolObject
    {
        public ToolLine()
        {
            using (MemoryStream stream = new MemoryStream(Properties.Resources.Line))
            {
                ToolCursor = new Cursor(stream);
            }
        }

        // Create new line.
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
                new GraphicsLine(p, new Point(p.X + 1, p.Y + 1),
                                 drawingCanvas.LineWidth,
                                 drawingCanvas.RegionType,
                                 drawingCanvas.ObjectColor,
                                 drawingCanvas.ActualScale));
        }

        // Move line.
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(drawingCanvas);

                if (p.X > drawingCanvas.ActualWidth)
                    p.X = drawingCanvas.ActualWidth - 1;
                if (p.Y > drawingCanvas.ActualHeight)
                    p.Y = drawingCanvas.ActualHeight - 1;

                GraphicsLine currentLine = drawingCanvas[drawingCanvas.Count - 1] as GraphicsLine;
                if (currentLine != null)
                {
                    if (!(currentLine.StartPoint.X == p.X && currentLine.StartPoint.Y == p.Y))
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            if (Math.Abs(p.X - currentLine.Start.X) >= Math.Abs(p.Y - currentLine.Start.Y))
                            {
                                p.Y = currentLine.Start.Y;
                            }
                            else
                            {
                                p.X = currentLine.Start.X;
                            }
                        }
                        drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(p, 2);
                    }
                }
            }
        }
    }
}
