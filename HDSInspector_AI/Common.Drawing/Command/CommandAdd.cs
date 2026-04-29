// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;

// Modified & Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Command add.  </summary>
    class CommandAdd : CommandBase
    {
        private List<PropertiesGraphicsBase> addList;
        private List<int> indexes;

        #region Ctor.
        public CommandAdd(GraphicsBase newObject)
        {
            addList = new List<PropertiesGraphicsBase>();
            indexes = new List<int>();

            if (newObject != null)
            {
                newObject.ObjectColor = newObject.OriginObjectColor;
                addList.Add(newObject.CreateSerializedObject());
                indexes.Add(0);
            }
        }

        public CommandAdd(DrawingCanvas drawingCanvas)
        {
            addList = new List<PropertiesGraphicsBase>();
            indexes = new List<int>();

            int currentIndex = 0;
            foreach(GraphicsBase g in drawingCanvas.Selection) // IEnumerable<GraphicsBase> Selection
            {
                g.ObjectColor = g.OriginObjectColor;
                addList.Add(g.CreateSerializedObject());
                indexes.Add(currentIndex);

                currentIndex++;
            }
        }
        #endregion

        public override void Undo(DrawingCanvas drawingCanvas)
        {
            // Delete from list all objects kept in addedList.
            // Use object IDs for deleting.

            int graphicCount = drawingCanvas.GraphicsList.Count;
            for (int i = graphicCount - 1; i >= 0; i--)
            {
                GraphicsBase currentObject = (GraphicsBase)drawingCanvas.GraphicsList[i];

                foreach(PropertiesGraphicsBase o in addList)
                {
                    if (o.Id == currentObject.ID)
                    {
                        drawingCanvas.GraphicsList.RemoveAt(i); // delete object again.
                        break;
                    }
                }
            }
        }

        public override void Redo(DrawingCanvas drawingCanvas)
        {
            // Insert all objects from deletedList to GraphicsList

            int currentIndex = 0;
            int indexToInsert = -1;

            foreach (PropertiesGraphicsBase graphic in addList)
            {
                indexToInsert = indexes[currentIndex];

                if (indexToInsert >= 0 && indexToInsert <= drawingCanvas.GraphicsList.Count)  // "<=" is correct !
                {
                    drawingCanvas.GraphicsList.Insert(indexToInsert, graphic.CreateGraphics());
                }
                else
                {
                    // Bug: we should not be here.
                    // Add to the end anyway.
                    drawingCanvas.GraphicsList.Add(graphic.CreateGraphics());
                    System.Diagnostics.Trace.WriteLine("CommandDelete.Undo - incorrect index");
                }

                currentIndex++;
            }

            drawingCanvas.RefreshClip(); // refresh clip.
        }
    }
}
