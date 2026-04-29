// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Modified & Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Base class for all drawing tools.  </summary>
    abstract class Tool
    {
        // 마우스 조작.
        public abstract void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e);
        public abstract void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e);
        public abstract void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e);

        public abstract void SetCursor(DrawingCanvas drawingCanvas);

        // 기본 ROI 색상이 정의되어 있다.
        protected static void SetGraphicsColor(DrawingCanvas drawingCanvas)
        {
            switch (drawingCanvas.RegionType)
            {
                case GraphicsRegionType.Inspection:
                case GraphicsRegionType.UnitRegion:
                case GraphicsRegionType.OuterRegion:
                    drawingCanvas.ObjectColor = GraphicsColors.Green;
                    drawingCanvas.TextColor = GraphicsColors.Green;
                    break;
                case GraphicsRegionType.StripAlign:
                case GraphicsRegionType.UnitAlign:
                case GraphicsRegionType.LocalAlign:
                    drawingCanvas.ObjectColor = GraphicsColors.Red;
                    drawingCanvas.TextColor = GraphicsColors.Red;
                    break;
                case GraphicsRegionType.Except:
                    drawingCanvas.ObjectColor = GraphicsColors.Blue;
                    drawingCanvas.TextColor = GraphicsColors.Blue;
                    break;
                case GraphicsRegionType.GuideLine:
                    drawingCanvas.ObjectColor = GraphicsColors.Yellow;
                    drawingCanvas.TextColor = GraphicsColors.Yellow;
                    break;
                case GraphicsRegionType.TapeLoaction:
                    drawingCanvas.ObjectColor = GraphicsColors.Yellow;
                    drawingCanvas.TextColor = GraphicsColors.Yellow;
                    break;
                default:
                    drawingCanvas.ObjectColor = Colors.Transparent;
                    drawingCanvas.TextColor = Colors.Transparent;
                    break;
            }
        }
    }
}
