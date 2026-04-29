// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>
    /// Helper class which contains general helper functions and properties.
    /// 
    /// Most functions in this class replace VisualCollection-derived class
    /// methods, because I cannot derive from VisualCollection.
    /// They make different operations with GraphicsBase list.
    /// </summary>
    /// 
    static class HelperFunctions
    {
        /// <summary>
        /// Select all graphic objects
        /// </summary>
        public static void SelectAll(DrawingCanvas drawingCanvas)
        {
            foreach (GraphicsBase g in drawingCanvas.GraphicsList)
            {
                if (!g.IsSelected)
                    g.IsSelected = true;
                if (g.RegionType == GraphicsRegionType.InspectArea)
                    g.IsSelected = false;
            }

            #region Select All Detail version.
            // Select All Detail.
            //if (drawingCanvas.IsBasedCanvas)
            //{
            //    foreach (GraphicsRectangle g in drawingCanvas.GraphicsRectangleList)
            //    {
            //        g.IsSelected = false;
            //        if (!g.IsValidRegion)
            //            g.IsSelected = true;
            //    }
            //    if (drawingCanvas.SelectionCount == 0)
            //    {
            //        foreach (GraphicsRectangle g in drawingCanvas.GraphicsRectangleList)
            //        {
            //            if (!g.IsSelected)
            //                g.IsSelected = true;
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (GraphicsBase g in drawingCanvas.GraphicsList)
            //    {
            //        if (!g.IsSelected)
            //            g.IsSelected = true;
            //    }
            //}
            #endregion Select All Detail version.
        }

        /// <summary>
        /// Unselect all graphic objects
        /// </summary>
        public static void UnselectAll(DrawingCanvas drawingCanvas)
        {
            foreach (GraphicsBase g in drawingCanvas.GraphicsList)
            {
                if (g.IsSelected)
                {
                    if (g.LocalAligns != null)
                    {
                        for (int nIndex = 0; nIndex < g.LocalAligns.Length; nIndex++)
                        {
                            if (g.LocalAligns[nIndex] != null)
                                g.LocalAligns[nIndex].ObjectColor = g.LocalAligns[nIndex].OriginObjectColor;
                        }
                    }
                    g.ObjectColor = g.OriginObjectColor;
                    g.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Delete selected graphic objects
        /// </summary>
        public static void DeleteSelection(DrawingCanvas drawingCanvas)
        {
            bool wasChange = false;
            CommandDelete command = new CommandDelete(drawingCanvas);

            for (int i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    drawingCanvas.GraphicsList.RemoveAt(i);
                    wasChange = true;
                }
            }

            if (!drawingCanvas.IsBasedCanvas && wasChange)
            {
                drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>
        /// Delete all graphic objects
        /// </summary>
        public static void DeleteAll(DrawingCanvas drawingCanvas)
        {
            if (drawingCanvas.GraphicsList.Count > 0)
            {
                // 전체 영상 View에서는 History 관리를 하지 않는다.
                if (!drawingCanvas.IsBasedCanvas)
                {
                    drawingCanvas.AddCommandToHistory(new CommandDeleteAll(drawingCanvas));
                }
                drawingCanvas.GraphicsList.Clear();
            }
        }

        /// <summary>
        /// Move selection to front
        /// </summary>
        public static void MoveSelectionToFront(DrawingCanvas drawingCanvas)
        {
            // Moving to front of z-order means moving
            // to the end of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.
            CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);
            List<GraphicsBase> list = new List<GraphicsBase>();

            for (int i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Insert(0, drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the end of GraphicsList
            foreach (GraphicsBase g in list)
            {
                drawingCanvas.GraphicsList.Add(g);
            }

            if (!drawingCanvas.IsBasedCanvas && list.Count > 0)
            {
                command.NewState(drawingCanvas);
                drawingCanvas.AddCommandToHistory(command);
            }
        }

        public static void MoveToFront(DrawingCanvas drawingCanvas, GraphicsRegionType regionType)
        {
            if (!drawingCanvas.IsBasedCanvas)
            {
                List<GraphicsBase> list = new List<GraphicsBase>();
                for (int i = drawingCanvas.Count - 1; i >= 0; i--)
                {
                    if (drawingCanvas[i].RegionType == regionType)
                    {
                        list.Add(drawingCanvas[i]);
                        drawingCanvas.GraphicsList.RemoveAt(i);
                    }
                }

                // Add all items from temporary list to the beginning of GraphicsList
                foreach (GraphicsBase g in list)
                {
                    drawingCanvas.GraphicsList.Add(g);
                }
            }
        }

        /// <summary>
        /// Move selection to back
        /// </summary>
        public static void MoveSelectionToBack(DrawingCanvas drawingCanvas)
        {
            // Moving to back of z-order means moving
            // to the beginning of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.
            CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);
            List<GraphicsBase> list = new List<GraphicsBase>();

            for (int i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Add(drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the beginning of GraphicsList
            foreach (GraphicsBase g in list)
            {
                drawingCanvas.GraphicsList.Insert(0, g);
            }

            if (!drawingCanvas.IsBasedCanvas && list.Count > 0)
            {
                command.NewState(drawingCanvas);
                drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>   Move graphics. </summary>
        /// <remarks>   suoow2, 2014-10-06. </remarks>
        public static void MoveGraphics(DrawingCanvas drawingCanvas, double deltaX, double deltaY)
        {
            CommandChangeState command = new CommandChangeState(drawingCanvas);
            foreach (GraphicsBase g in drawingCanvas.Selection)
            {
                g.Move(deltaX, deltaY, drawingCanvas.ActualWidth, drawingCanvas.ActualHeight);
                if (g.LocalAligns != null)
                {
                    for (int nIndex = 0; nIndex < g.LocalAligns.Length; nIndex++)
                    {
                        if (g.LocalAligns[nIndex] != null)
                        {
                            g.LocalAligns[nIndex].Move(deltaX, deltaY, drawingCanvas.ActualWidth, drawingCanvas.ActualHeight);
                        }
                    }
                }
            }
            command.NewState(drawingCanvas);
            if (!drawingCanvas.IsBasedCanvas)
                drawingCanvas.AddCommandToHistory(command);
        }

        /// <summary>
        /// Apply new line width
        /// </summary>
        public static bool ApplyLineWidth(DrawingCanvas drawingCanvas, double value, bool addToHistory)
        {
            CommandChangeState command = new CommandChangeState(drawingCanvas);
            bool wasChange = false;

            // LineWidth is set for all objects except of GraphicsText.
            // Though GraphicsText has this property, it should remain constant.

            foreach (GraphicsBase g in drawingCanvas.Selection)
            {
                if (g is GraphicsRectangleBase || g is GraphicsPolyLine || g is GraphicsLine)
                {
                    if (g.LineWidth != value)
                    {
                        g.LineWidth = value;
                        wasChange = true;
                    }
                }
            }

            if (!drawingCanvas.IsBasedCanvas && wasChange && addToHistory)
            {
                command.NewState(drawingCanvas);
                drawingCanvas.AddCommandToHistory(command);
            }
            return wasChange;
        }

        /// <summary>
        /// Apply new color
        /// </summary>
        public static bool ApplyColor(DrawingCanvas drawingCanvas, Color value, bool addToHistory)
        {
            CommandChangeState command = new CommandChangeState(drawingCanvas);
            bool wasChange = false;

            foreach (GraphicsBase g in drawingCanvas.Selection)
            {
                if (g.ObjectColor != value)
                {
                    g.ObjectColor = value;
                    wasChange = true;
                }
            }

            if (!drawingCanvas.IsBasedCanvas && wasChange && addToHistory)
            {
                command.NewState(drawingCanvas);
                drawingCanvas.AddCommandToHistory(command);
            }
            return wasChange;
        }

        /// <summary>
        /// Dump graphics list (for debugging)
        /// </summary>
        [Conditional("DEBUG")]
        public static void Dump(VisualCollection graphicsList, string header)
        {
            Trace.WriteLine("");
            Trace.WriteLine(header);
            Trace.WriteLine("");

            foreach (GraphicsBase g in graphicsList)
            {
                g.Dump();
            }
        }

        /// <summary>
        /// Dump graphics list overload
        /// </summary>
        [Conditional("DEBUG")]
        public static void Dump(VisualCollection graphicsList)
        {
            Dump(graphicsList, "Graphics List");
        }

        /// <summary>
        /// Return true if currently active properties (line width, color etc.)
        /// can be applied to selected items.
        /// 
        /// If at least one selected object has property different from currently
        /// active property value, properties can be applied.
        /// </summary>
        public static bool CanApplyProperties(DrawingCanvas drawingCanvas)
        {
            foreach (GraphicsBase graphicsBase in drawingCanvas.GraphicsList)
            {
                if (!graphicsBase.IsSelected)
                {
                    continue;
                }

                // ObjectColor - used in all graphics objects
                if (graphicsBase.ObjectColor != drawingCanvas.ObjectColor)
                {
                    return true;
                }
                else if (graphicsBase.LineWidth != drawingCanvas.LineWidth)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Apply currently active properties to selected objects
        /// </summary>
        public static void ApplyProperties(DrawingCanvas drawingCanvas)
        {
            // Apply every property.
            // Call every Apply* function with addToHistory = false.
            // History is updated here and not in called functions.
            CommandChangeState command = new CommandChangeState(drawingCanvas);
            bool wasChange = false;

            // Line Width
            if (ApplyLineWidth(drawingCanvas, drawingCanvas.LineWidth, false))
            {
                wasChange = true;
            }

            // Color
            if (ApplyColor(drawingCanvas, drawingCanvas.ObjectColor, false))
            {
                wasChange = true;
            }

            if (!drawingCanvas.IsBasedCanvas && wasChange)
            {
                command.NewState(drawingCanvas);
                drawingCanvas.AddCommandToHistory(command);
            }
        }
    }
}
