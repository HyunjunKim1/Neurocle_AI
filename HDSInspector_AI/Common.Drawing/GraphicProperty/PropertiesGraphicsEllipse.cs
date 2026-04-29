// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Collections.Generic;
using System.Windows.Media;

// Modify & Comment by suoow2.

namespace Common.Drawing
{
    /// <summary>   Ellipse object properties. </summary>
    public class PropertiesGraphicsEllipse : PropertiesGraphicsBase
    {
        #region Member variables.
        private double left;
        private double top;
        private double right;
        private double bottom;
        #endregion

        #region Ctor.
        // For XmlSerializer
        public PropertiesGraphicsEllipse()
        {
            // XML로의 저장을 위해 기본 생성자가 필요하다.
        }

        public PropertiesGraphicsEllipse(GraphicsEllipse ellipse)
        {
            if (ellipse == null)
            {
                throw new ArgumentNullException("ellipse");
            }

            this.left = ellipse.Left;
            this.top = ellipse.Top;
            this.right = ellipse.Right;
            this.bottom = ellipse.Bottom;
            this.lineWidth = ellipse.LineWidth;
            this.regionType = ellipse.RegionType;
            this.objectColor = ellipse.ObjectColor;
            this.actualScale = ellipse.ActualScale;
            this.Id = ellipse.ID;
            this.selected = ellipse.IsSelected;
            this.caption = ellipse.Caption;

        }
        #endregion

        public override GraphicsBase CreateGraphics()
        {
            lineWidth = 2; // Default thickness.

            actualScale = (actualScale > 0) ? actualScale : 1.0;
            GraphicsBase b = new GraphicsEllipse(left, top, right, bottom, lineWidth, regionType, objectColor, actualScale, caption);

            if (this.Id != 0)
            {
                b.ID = this.Id;
                b.IsSelected = this.selected;
            }

            return b;
        }

        #region Properties
        public double Left
        {
            get { return left; }
            set { left = value; }
        }

        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        public double Right
        {
            get { return right; }
            set { right = value; }
        }

        public double Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        public GraphicsRegionType RegionType
        {
            get { return regionType; }
            set { regionType = value; }
        }

        public string Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        public Color ObjectColor
        {
            get { return objectColor; }
            set { objectColor = value; }
        }

        #endregion Properties
    }
}
