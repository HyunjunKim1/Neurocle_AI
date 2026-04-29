// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;
using System.Diagnostics;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Command delete all.  </summary>
    class CommandDeleteAll : CommandBase
    {
        List<PropertiesGraphicsBase> deletedList;

        #region Ctor.
        public CommandDeleteAll(DrawingCanvas drawingCanvas)
        {
            deletedList = new List<PropertiesGraphicsBase>();

            // Make clone of the whole list. (for Undo/Redo)
            foreach (GraphicsBase g in drawingCanvas.GraphicsList)
            {
                g.ObjectColor = g.OriginObjectColor;
                deletedList.Add(g.CreateSerializedObject());
            }
        }
        #endregion

        public override void Undo(DrawingCanvas drawingCanvas)
        {
            try
            {
                foreach (PropertiesGraphicsBase o in deletedList)
                {
                    drawingCanvas.GraphicsList.Add(o.CreateGraphics());
                }
                drawingCanvas.RefreshClip();
            }
            catch
            {
                Debug.WriteLine("Exception occured in Undo(CommandDeleteAll.cs)");
            }
        }

        public override void Redo(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.GraphicsList.Clear();
        }
    }
}
