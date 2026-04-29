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
    /// <summary>   Tool polygon line.  </summary>
    class ToolPolyLine : ToolObject
    {
        public bool IsFirstClicked = true;
        public bool IsShiftPushed;

        public bool CanShowContextMenu
        {
            get
            {
                // 폴리곤 툴을 선택 후 그리기를 수행하지 않고 팝업 메뉴를 호출하는 경우에 한하여
                // 팝업 메뉴를 출력시키도록 한다. (SQE 요청사항).
                return IsFirstClicked;
            }
        }

        // 새로 생성하려는 PolyLine 개체 포인터
        private GraphicsPolyLine newPolyLine;

        #region Ctor.
        public ToolPolyLine()
        {
            using (MemoryStream stream = new MemoryStream(Properties.Resources.Pencil))
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

            if (IsFirstClicked)
            {
                IsFirstClicked = false;

                SetGraphicsColor(drawingCanvas);

                newPolyLine = new GraphicsPolyLine(new Point[] { p, new Point(p.X + 1, p.Y + 1) },
                                                   drawingCanvas.LineWidth,
                                                   drawingCanvas.RegionType,
                                                   drawingCanvas.ObjectColor,
                                                   drawingCanvas.ActualScale);

                AddNewObject(drawingCanvas, newPolyLine);
            }
            else // is not first clicked.
            {
                if (!IsShiftPushed)
                {
                    if (newPolyLine.Points.Length < 400) // n각형 이상으로 생성되지 않도록 함.
                    {
                        // If not first-clicked, just add point.
                        newPolyLine.AddPoint(p);
                    }
                    else
                    {
                        drawingCanvas.DrawingFinished = true;
                    }
                }
                else
                {
                    IsShiftPushed = false;
                }
            }
        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (!drawingCanvas.IsMouseCaptured)
            {
                return;
            }

            if (newPolyLine == null)
            {
                return; // precaution.
            }

            Point p = e.GetPosition(drawingCanvas);

            if (p.X > drawingCanvas.ActualWidth)
                p.X = drawingCanvas.ActualWidth - 1;
            if (p.Y > drawingCanvas.ActualHeight)
                p.Y = drawingCanvas.ActualHeight - 1;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (newPolyLine.Points.Length - 2 >= 0)
                {
                    IsShiftPushed = true;
                    
                    Point beforePoint = newPolyLine.Points[newPolyLine.Points.Length - 2];
                    if (Math.Abs(p.X - beforePoint.X) >= Math.Abs(p.Y - beforePoint.Y))
                    {
                        p.Y = beforePoint.Y;
                    }
                    else
                    {
                        p.X = beforePoint.X;
                    }
                }
            }
            
            newPolyLine.MoveHandleTo(p, newPolyLine.HandleCount);
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e)
        {
            // reset.
            newPolyLine = null;
            IsFirstClicked = true;

            base.OnMouseUp(drawingCanvas, e);
        }
    }
}
