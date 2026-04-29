// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.

using System;
using System.Collections.Generic;
using System.Text;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Manager for undoes. It has history list of commands. </summary>
    class UndoManager
    {
        #region member variables.
        /// <summary> The drawing canvas </summary>
        private DrawingCanvas drawingCanvas;

        /// <summary> List of command histories </summary>
        private List<CommandBase> historyList;

        /// <summary> The next undo position. </summary>
        int nextUndo;

        /// <summary> State changed Delegate. </summary>
        public event EventHandler StateChanged;
        #endregion

        public UndoManager(DrawingCanvas drawingCanvas)
        {
            this.drawingCanvas = drawingCanvas; // save pointer of drawingCanvas.
            ClearHistory();
        }

        public bool CanUndo
        {
            get
            {
                if (nextUndo < 0 || nextUndo > historyList.Count - 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                if (nextUndo == historyList.Count - 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsDirty
        {
            get
            {
                // IsDirty == CanUndo
                return CanUndo;
            }
        }

        public CommandBase Command
        {
            get
            {
                return historyList[nextUndo + 1];
            }
        }

        public int GetHistoryCount()
        {
            return historyList.Count;
        }

        public void ClearHistory()
        {
            historyList = new List<CommandBase>();

            nextUndo = -1;            
            RaiseStateChangedEvent();
        }

        public void AddCommandToHistory(CommandBase command)
        {
            TrimHistoryList();

            historyList.Add(command);

            nextUndo++;
            RaiseStateChangedEvent();
        }

        public void Undo()
        {
            CommandBase command = historyList[nextUndo];

            // Undo.
            command.Undo(drawingCanvas);

            nextUndo--;
            RaiseStateChangedEvent();
        }

        public void Redo()
        {
            int itemToRedo = nextUndo + 1;
            CommandBase command = historyList[itemToRedo];

            // Redo.
            command.Redo(drawingCanvas);

            nextUndo++;
            RaiseStateChangedEvent();
        }

        private void TrimHistoryList()
        {
            if (historyList.Count == 0 || nextUndo == historyList.Count - 1)
            {
                return;
            }

            // Delete un-used part of historyList. (nextUndo to end of historyList)
            for (int i = historyList.Count - 1; i > nextUndo; i--)
            {
                historyList.RemoveAt(i);
            }
        }

        private void RaiseStateChangedEvent()
        {
            if (StateChanged != null)
            {
                // State changed !
                StateChanged(this, EventArgs.Empty);
            }
        }
    }
}
