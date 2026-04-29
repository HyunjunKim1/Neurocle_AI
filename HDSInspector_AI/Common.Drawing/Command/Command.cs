// http://www.codeproject.com/cs/design/commandpatterndemo.asp
// The Command Pattern and MVC Architecture
// By David Veeneman.
// 
using System;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>   Command base.  </summary>
    abstract class CommandBase
    {
        // Undo
        public abstract void Undo(DrawingCanvas drawingCanvas);

        // Redo
        public abstract void Redo(DrawingCanvas drawingCanvas);
    }
}
