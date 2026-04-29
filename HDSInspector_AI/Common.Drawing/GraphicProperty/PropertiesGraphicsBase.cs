// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>
    /// Base class for all serialization helper classes.
    /// PropertiesGraphics* class hierarchy contains base class
    /// and class for every non-abstract Graphics* class.
    /// Since Graphics* classes are derived from DrawingVisual, I
    /// cannot serialize them directly.
    /// 
    /// Every PropertiesGraphics* class knows to create instance
    /// of Graphics* class - see CreateGraphics function.
    /// This function is called during deserialization, when 
    /// PropertiesGraphics* class is loaded from XML file.
    /// 
    /// On the other hand, every non-abstract Graphics* class
    /// can create PropertiesGraphics* class: see 
    /// GraphicsBase.CreateSerializedObject function.
    /// It is called during serialization, when every Graphics*
    /// object must create PropertiesGraphics*.
    /// 
    /// 
    /// PropertiesGraphics* classes are also used in UndoManager
    /// as light-weight clones of Graphics* classes.
    /// These classes are also used in DrawingCanvas.GetListOfGraphicObjects
    /// function for client which needs to get all data from DrawingCanvas
    /// directly.
    /// 
    /// </summary>
    public abstract class PropertiesGraphicsBase
    {
        [XmlIgnore]
        internal int Id;

        [XmlIgnore]
        internal bool selected;

        [XmlIgnore]
        internal double actualScale;

        [XmlIgnore]
        internal GraphicsRegionType regionType;

        [XmlIgnore]
        internal Color objectColor;

        [XmlIgnore]
        internal double lineWidth;

        [XmlIgnore]
        internal string caption;

        [XmlArrayItem(typeof(Point), ElementName = "LocalAlign")]
        public Point[] LocalAligns
        {
            get;
            set;
        }
        
        public abstract GraphicsBase CreateGraphics();
    }
}
