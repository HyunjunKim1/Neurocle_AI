// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Base class for all tools which create new graphic object. </summary>
    abstract class ToolObject : Tool
    {
        protected Cursor ToolCursor
        {
            get
            {
                return toolCursor;
            }
            set
            {
                toolCursor = value;
            }
        }
        private Cursor toolCursor;

        // 마우스 버튼 업.
        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            if (drawingCanvas.Count > 0)
            {
                drawingCanvas[drawingCanvas.Count - 1].Normalize();
                if (drawingCanvas[drawingCanvas.Count - 1] is GraphicsLine)
                {
                    GraphicsLine line = drawingCanvas[drawingCanvas.Count - 1] as GraphicsLine;
                    if (line != null)
                    {
                        drawingCanvas.GraphicsList.Remove(line);
                        //if (line.LengthProperty == 0)
                        //{
                        //    drawingCanvas.GraphicsList.Remove(line);
                        //    drawingCanvas.SelectedGraphic = null;
                        //}
                    }
                }

                // 전체 영상 View에서는 History 관리를 하지 않는다.
                if (!drawingCanvas.IsBasedCanvas && drawingCanvas.Tool != ToolType.PolyLine)
                {
                    drawingCanvas.AddCommandToHistory(new CommandAdd(drawingCanvas));
                }
            }

            drawingCanvas.Tool = ToolType.Pointer;
            drawingCanvas.Cursor = Cursors.Arrow;
            drawingCanvas.ReleaseMouseCapture();
        }

        // 개체 추가.
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase newObject)
        {
            HelperFunctions.UnselectAll(drawingCanvas);

            newObject.IsSelected = true;
            newObject.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth, drawingCanvas.ActualHeight));

            drawingCanvas.GraphicsList.Add(newObject);
            drawingCanvas.CaptureMouse();
        }

        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = this.toolCursor;
        }
    }
}
