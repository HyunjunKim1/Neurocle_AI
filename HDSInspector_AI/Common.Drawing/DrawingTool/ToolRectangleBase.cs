// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Input;

namespace Common.Drawing
{
    /// <summary>   Tool rectangle base.  </summary>
    abstract class ToolRectangleBase : ToolObject
    {
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (e.LeftButton == MouseButtonState.Pressed
                && drawingCanvas.IsMouseCaptured
                && drawingCanvas.Count > 0)
            {
                Point p = e.GetPosition(drawingCanvas);

                if (p.X > drawingCanvas.ActualWidth)
                    p.X = drawingCanvas.ActualWidth - 1;
                if (p.Y > drawingCanvas.ActualHeight)
                    p.Y = drawingCanvas.ActualHeight - 1;

                drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(p, 5);
            }
        }
    }
}
