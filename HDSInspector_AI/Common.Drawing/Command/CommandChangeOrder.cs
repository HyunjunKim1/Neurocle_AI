// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   
    /// 			Command change z-order.
    /// 			Command keeps list of object IDs before and after operation.
    /// 			Using these lists, it is possible to undo / redo change z-order operation.
    /// </summary>
    class CommandChangeOrder : CommandBase
    {
        private List<int> listBefore;
        private List<int> listAfter;

        #region Ctor.
        public CommandChangeOrder(DrawingCanvas drawingCanvas)
        {
            FillList(drawingCanvas.GraphicsList, ref listBefore);
        }
        #endregion

        public void NewState(DrawingCanvas drawingCanvas)
        {
            FillList(drawingCanvas.GraphicsList, ref listAfter);
        }

        public override void Undo(DrawingCanvas drawingCanvas)
        {
            ChangeOrder(drawingCanvas.GraphicsList, listBefore);
        }

        public override void Redo(DrawingCanvas drawingCanvas)
        {
            ChangeOrder(drawingCanvas.GraphicsList, listAfter);
        }

        private static void FillList(VisualCollection graphicsList, ref List<int> listToFill)
        {
            listToFill = new List<int>(); // listBefore or listAfter

            foreach (GraphicsBase g in graphicsList)
            {
                listToFill.Add(g.ID);
            }
        }

        // Z-Order º¯°æ.
        private static void ChangeOrder(VisualCollection graphicsList, List<int> indexList)
        {
            List<GraphicsBase> correctOrderList = new List<GraphicsBase>();

            // Read indexList, and re-arrange.
            foreach (int id in indexList)
            {
                foreach (GraphicsBase g in graphicsList)
                {
                    if (g.ID == id)
                    {
                        correctOrderList.Add(g); // Correct order.
                        graphicsList.Remove(g);
                        break;
                    }
                }
            }

            foreach (GraphicsBase g in correctOrderList)
            {
                graphicsList.Add(g); // Sets to DrawingCanvas.
            }
        }
    }
}
