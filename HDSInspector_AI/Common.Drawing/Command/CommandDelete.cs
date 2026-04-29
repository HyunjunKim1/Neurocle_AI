// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Command delete.  </summary>
    class CommandDelete : CommandBase
    {
        private List<PropertiesGraphicsBase> deletedList;
        private List<int> indexes;

        #region Ctor.
        public CommandDelete(DrawingCanvas drawingCanvas)
        {
            deletedList = new List<PropertiesGraphicsBase>();
            indexes = new List<int>();

            int currentIndex = 0;
            foreach (GraphicsBase g in drawingCanvas.Selection) // IEnumerable<GraphicsBase> Selection
            {
                g.ObjectColor = g.OriginObjectColor;
                deletedList.Add(g.CreateSerializedObject());
                indexes.Add(currentIndex);

                currentIndex++;
            }
        }
        #endregion

        public override void Undo(DrawingCanvas drawingCanvas)
        {
            // Insert all objects from deletedList to GraphicsList

            int currentIndex = 0;
            int indexToInsert = -1;

            foreach (PropertiesGraphicsBase o in deletedList)
            {
                indexToInsert = indexes[currentIndex];

                if (indexToInsert >= 0 && indexToInsert <= drawingCanvas.GraphicsList.Count)  // "<=" is correct !
                {
                    drawingCanvas.GraphicsList.Insert(indexToInsert, o.CreateGraphics());
                }
                else
                {
                    // Bug: we should not be here.
                    // Add to the end anyway.
                    drawingCanvas.GraphicsList.Add(o.CreateGraphics());
                    System.Diagnostics.Trace.WriteLine("CommandDelete.Undo - incorrect index");
                }

                currentIndex++;
            }

            drawingCanvas.RefreshClip(); // refresh clip.
        }

        public override void Redo(DrawingCanvas drawingCanvas)
        {
            // Delete from list all objects kept in deletedList.
            // Use object IDs for deleting.

            int graphicCount = drawingCanvas.GraphicsList.Count;
            for (int i = graphicCount - 1; i >= 0; i--)
            {
                GraphicsBase currentObject = (GraphicsBase)drawingCanvas.GraphicsList[i];

                foreach (PropertiesGraphicsBase o in deletedList)
                {
                    if (o.Id == currentObject.ID)
                    {
                        drawingCanvas.GraphicsList.RemoveAt(i); // delete object again.
                        break;
                    }
                }
            }
        }
    }
}
