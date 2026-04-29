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
    // Polyline object properties
    public class PropertiesGraphicsPolyLine : PropertiesGraphicsBase
    {
        private Point[] points;

        #region Ctor.
        // For XmlSerializer
        public PropertiesGraphicsPolyLine()
        {
            // XML로의 저장을 위해 기본 생성자가 필요하다.
        }

        public PropertiesGraphicsPolyLine(GraphicsPolyLine polyLine)
        {
            if (polyLine == null)
            {
                throw new ArgumentNullException("polyLine");
            }
            this.points = polyLine.GetPoints();
            this.lineWidth = polyLine.LineWidth;
            this.regionType = polyLine.RegionType;
            this.objectColor = polyLine.ObjectColor;
            this.actualScale = polyLine.ActualScale;
            this.Id = polyLine.ID;
            this.selected = polyLine.IsSelected;
            this.caption = polyLine.Caption;

        }
        #endregion

        public override GraphicsBase CreateGraphics()
        {
            lineWidth = 2; // Default thickness.

            // 닫힌 형태의 PolyLine을 그려내기 위해 List에 담아 생성자를 호출한다.
            List<Point> pointList = new List<Point>();
            foreach(Point point in Points)
            {
                pointList.Add(point);
            }


            actualScale = (actualScale > 0) ? actualScale : 1.0;
            GraphicsBase b = new GraphicsPolyLine(pointList, lineWidth, regionType, objectColor, actualScale, caption);

            if (this.Id != 0)
            {
                b.ID = this.Id;
                b.IsSelected = this.selected;
            }

            return b;
        }

        #region Properties
        public Point[] Points
        {
            get { return points; }
            set { points = value; }
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
