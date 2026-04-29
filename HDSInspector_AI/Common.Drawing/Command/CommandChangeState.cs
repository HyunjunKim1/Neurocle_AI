// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   
    /// 			Changing state of existing objects : MOVE, RESIZE, and change properties.
    /// 			This command is always applied to the list selection.
    /// 			It keeps selection clone before and after operation.
    /// 			Undo/Redo operations replace objects in the list
    /// 			using selection clone.
    /// </summary>
    class CommandChangeState : CommandBase
    {
        private List<PropertiesGraphicsBase> listBefore;
        private List<PropertiesGraphicsBase> listAfter;

        #region Ctor.
        public CommandChangeState(DrawingCanvas drawingCanvas)
        {
            FillList(drawingCanvas.GraphicsList, ref listBefore);
        }
        
        public CommandChangeState(GraphicsBase graphic)
        {
            if (graphic != null)
            {
                listBefore = new List<PropertiesGraphicsBase>();
                // graphic.ObjectColor = graphic.OriginObjectColor;
                listBefore.Add(graphic.CreateSerializedObject());
            }
        }
        #endregion

        public void NewState(DrawingCanvas drawingCanvas)
        {
            FillList(drawingCanvas.GraphicsList, ref listAfter);
        }

        public void NewState(GraphicsBase graphic)
        {
            if (graphic != null)
            {
                listAfter = new List<PropertiesGraphicsBase>();
                listAfter.Add(graphic.CreateSerializedObject());
            }
        }

        #region Undo & Redo.
        public override void Undo(DrawingCanvas drawingCanvas)
        {
            // Replace all objects in the list with objects from listBefore
            ReplaceObjects(drawingCanvas.GraphicsList, listBefore);

            drawingCanvas.RefreshClip();
        }

        public override void Redo(DrawingCanvas drawingCanvas)
        {
            ReplaceObjects(drawingCanvas.GraphicsList, listAfter);

            drawingCanvas.RefreshClip();
        }
        #endregion

        private static void ReplaceObjects(VisualCollection graphicsList, List<PropertiesGraphicsBase> list)
        {
            try
            {
                for (int i = 0; i < graphicsList.Count; i++)
                {
                    if (list == null)
                    {
                        return;
                    }

                    foreach (PropertiesGraphicsBase o in list)
                    {
                        if (o.Id == ((GraphicsBase)graphicsList[i]).ID)
                        {
                            // 내부 리스트에 임시 보관했던 도형들을 되돌려 놓는다.
                            graphicsList.RemoveAt(i);
                            graphicsList.Insert(i, o.CreateGraphics());
                            break;
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in ReplaceObjects(CommandChangeState.cs)");
            }
        }

        private static void FillList(VisualCollection graphicsList, ref List<PropertiesGraphicsBase> listToFill)
        {
            try
            {
                listToFill = new List<PropertiesGraphicsBase>();
                foreach (GraphicsBase g in graphicsList)
                {
                    if (g.IsSelected)
                    {
                        // g.ObjectColor = g.OriginObjectColor;
                        listToFill.Add(g.CreateSerializedObject()); // 선택된 도형들을 내부 리스트에 임시 보관한다.
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in FillList(CommandChangeState.cs)");
            }
        }
    }
}
