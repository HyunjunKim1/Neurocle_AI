using System;
using System.Windows;
using System.Windows.Input;
using System.IO;

// 2012.04.09 suoow2.

namespace Common.Drawing
{
    /// <summary>   Tool rectangle.  </summary>
    class ToolGuideLine : ToolRectangleBase
    {
        #region Ctor.
        public ToolGuideLine()
        {
            // Cursor는 Rectangle 타입을 사용하도록 한다.
            using (MemoryStream stream = new MemoryStream(Properties.Resources.Rectangle))
            {
                ToolCursor = new Cursor(stream); // Set Cursor.
            }
        }
        #endregion

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

            // Add this Rectangle to GraphicList of drawingCanvas.
            AddNewObject(drawingCanvas, new GraphicsGuideLine(p.X, p.Y, p.X + 1, p.Y + 1, drawingCanvas.LineWidth, drawingCanvas.ObjectColor, drawingCanvas.ActualScale));
        }
    }

    class ToolTapeLocation : ToolRectangleBase
    {
        #region Ctor.
        public ToolTapeLocation()
        {
            // Cursor는 Rectangle 타입을 사용하도록 한다.
            using (MemoryStream stream = new MemoryStream(Properties.Resources.Rectangle))
            {
                ToolCursor = new Cursor(stream); // Set Cursor.
            }
        }
        #endregion

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
           // drawingCanvas.RegionType = GraphicsRegionType.TapeLoaction;
            SetGraphicsColor(drawingCanvas);

            // Add this Rectangle to GraphicList of drawingCanvas.
            AddNewObject(drawingCanvas, new GraphicsTapeLocation(0, p.X, p.Y, drawingCanvas.LineWidth, drawingCanvas.ObjectColor, drawingCanvas.ActualScale, 1, 0.1));
        }
    }
}
