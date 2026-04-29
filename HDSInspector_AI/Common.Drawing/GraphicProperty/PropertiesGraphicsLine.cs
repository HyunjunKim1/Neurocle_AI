// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>
    /// Line object properties
    /// </summary>
    public class PropertiesGraphicsLine : PropertiesGraphicsBase
    {
        private Point start;
        private Point end;

        #region Ctor.
        // For XmlSerializer
        public PropertiesGraphicsLine()
        {
            // XML로의 저장을 위해 기본 생성자가 필요하다.
        }

        public PropertiesGraphicsLine(GraphicsLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            this.start = line.Start;
            this.end = line.End;
            this.lineWidth = line.LineWidth;
            this.regionType = line.RegionType;
            this.objectColor = line.ObjectColor;
            this.actualScale = line.ActualScale;
            this.Id = line.ID;
            this.selected = line.IsSelected;
            this.caption = line.Caption;

        }
        #endregion

        public override GraphicsBase CreateGraphics()
        {
            lineWidth = 2; // Default thickness.


            actualScale = (actualScale > 0) ? actualScale : 1.0;
            GraphicsBase b = new GraphicsLine(start, end, lineWidth, regionType, objectColor, actualScale, caption);

            if (this.Id != 0)
            {
                b.ID = this.Id;
                b.IsSelected = this.selected;
            }

            return b;
        }

        #region Properties
        public Point Start
        {
            get { return start; }
            set { start = value; }
        }

        public Point End
        {
            get { return end; }
            set { end = value; }
        }

        public GraphicsRegionType RegionType
        {
            get
            {
                return regionType;
            }
            set
            {
                regionType = value;
            }
        }

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
            }
        }

        public Color ObjectColor
        {
            get
            {
                return objectColor;
            }
            set
            {
                objectColor = value;
            }
        }

        #endregion Properties
    }
}
