/*********************************************************************************
 * Copyright(c) 2015 by Haesung DS.
 * 
 * This software is copyrighted by, and is the sole property of Haesung DS.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Haesung DS. This software may only be used in accordance with
 * the corresponding license agreement. Any unauthorized use, duplication, 
 * transmission, distribution, or disclosure of this software is expressly 
 * forbidden.
 *
 * This Copyright notice may not be removed or modified without prior written
 * consent of Haesung DS reserves the right to modify this 
 * software without notice.
 *
 * Haesung DS.
 * KOREA 
 * http://www.HaesungDS.com
 *********************************************************************************/
/**
 * @file  SectionSettingStorage.cs
 * @brief 
 *  Section Region 설정을 복사하여 다른 면에 바로 적용할 수 있도록 함.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2012.03.11
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2012.03.11 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Drawing
{
    public class SectionSettingStorage
    {
        public List<GraphicsRectangle> FiduGraphicsList { get; set; }
        public List<GraphicsRectangle> GraphicsList { get; set; }

        bool CanPaste { get; set; }
        public int ReferenceID { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public System.Windows.Point StartPoint { get; private set; }

        private SectionSettingStorage()
        {
            ClearSetting();
        }

        private static SectionSettingStorage m_originSetting = new SectionSettingStorage();
        public static SectionSettingStorage OriginSetting
        {
            get
            {
                return m_originSetting;
            }
        }

        public void ClearSetting()
        {
            CanPaste = false;
            ReferenceID = -1;
            FiduGraphicsList = new List<GraphicsRectangle>();
            GraphicsList = new List<GraphicsRectangle>();
        }

        public void CopySectionSetting(DrawingCanvas aDrawingCanvas)
        {
            CanPaste = false;
            ReferenceID = aDrawingCanvas.ID;
            FiduGraphicsList.Clear();
            GraphicsList.Clear();
            
            double fWidth = aDrawingCanvas.Width;
            double fHeight = aDrawingCanvas.Height;

            Left = fWidth;
            Top = fHeight;
            Right = Bottom = 0;
            foreach (GraphicsRectangle fiduGraphic in aDrawingCanvas.FiduGraphicsList)
            {
                if (fiduGraphic.RegionType == GraphicsRegionType.OuterRegion)
                    continue; // 외곽 섹션은 복사하지 않는다.

                Left = Math.Min(Left, fiduGraphic.Left);
                Top = Math.Min(Top, fiduGraphic.Top);
                Right = Math.Max(Right, fiduGraphic.Right);
                Bottom = Math.Max(Bottom, fiduGraphic.Bottom);

                GraphicsBase rectFiduGraphic = fiduGraphic.CreateSerializedObject().CreateGraphics();
                rectFiduGraphic.OriginObjectColor = rectFiduGraphic.ObjectColor = fiduGraphic.OriginObjectColor;

                FiduGraphicsList.Add(rectFiduGraphic as GraphicsRectangle);
                GraphicsList.Add(rectFiduGraphic as GraphicsRectangle);

                if (rectFiduGraphic.RegionType == GraphicsRegionType.UnitRegion)
                {
                    foreach (GraphicsRectangle childGraphic in aDrawingCanvas.GraphicsRectangleList)
                    {
                        if (childGraphic.IsFiducialRegion)
                            continue;
                        if (childGraphic.MotherROI != fiduGraphic)
                            continue;

                        Left = Math.Min(Left, childGraphic.Left);
                        Top = Math.Min(Top, childGraphic.Top);
                        Right = Math.Max(Right, childGraphic.Right);
                        Bottom = Math.Max(Bottom, childGraphic.Bottom);

                        GraphicsBase rectChildGraphic = childGraphic.CreateSerializedObject().CreateGraphics();
                        rectChildGraphic.OriginObjectColor = rectChildGraphic.ObjectColor = childGraphic.OriginObjectColor;
                        ((GraphicsRectangle)rectChildGraphic).MotherROI = rectFiduGraphic;
                        GraphicsList.Add(rectChildGraphic as GraphicsRectangle);

                        Right = Math.Max(Right, ((GraphicsRectangle)rectChildGraphic).Right);
                        Bottom = Math.Max(Bottom, ((GraphicsRectangle)rectChildGraphic).Bottom);
                    }
                }
            }

            if (Math.Max(Left, Right) < fWidth && Math.Max(Top, Bottom) < fHeight)
            {
                CanPaste = true;
            }
        }

        public bool CanPasteSetting(DrawingCanvas aDrawingCanvas, System.Windows.Point aptStartPoint)
        {
            bool bIsValidSetting = CanPaste; // && (ReferenceID != aDrawingCanvas.ID);

            double fDeltaX = aptStartPoint.X - Left;
            double fDeltaY = aptStartPoint.Y - Top;
            bool bIsValidStartPoint = aptStartPoint.X >= 10 && aptStartPoint.X <= aDrawingCanvas.Width - 10 &&
                                      aptStartPoint.Y >= 10 && aptStartPoint.Y <= aDrawingCanvas.Width - 10;
            bool bIsValidSize = aDrawingCanvas.Width >= (fDeltaX + Right) && aDrawingCanvas.Height >= (fDeltaY + Bottom);

            if (bIsValidSetting && bIsValidStartPoint && bIsValidSize)
                StartPoint = aptStartPoint;

            return (bIsValidSetting && bIsValidStartPoint && bIsValidSize);
        }

        public void PasteSectionSetting(DrawingCanvas aDrawingCanvas)
        {
            if (CanPasteSetting(aDrawingCanvas, StartPoint))
            {
                aDrawingCanvas.Clear();

                double fDeltaX = 0.0;
                double fDeltaY = 0.0;
                foreach (GraphicsRectangle graphic in GraphicsList)
                {
                    if ((graphic.IterationXPosition == 0 && graphic.IterationYPosition == 0) || graphic.RegionType == GraphicsRegionType.OuterRegion)
                    {
                        fDeltaX = StartPoint.X - graphic.Left;
                        fDeltaY = StartPoint.Y - graphic.Top;
                        break;
                    }
                }

                // Delta Offset 만큼 이동하며 복사 수행함.
                foreach (GraphicsRectangle fiduGraphic in FiduGraphicsList)
                {
                    GraphicsBase rectFiduGraphic = fiduGraphic.CreateSerializedObject().CreateGraphics();
                    rectFiduGraphic.OriginObjectColor = rectFiduGraphic.ObjectColor = fiduGraphic.OriginObjectColor;
                    rectFiduGraphic.ActualScale = aDrawingCanvas.ActualScale;
                    ((GraphicsRectangle)rectFiduGraphic).Move(fDeltaX, fDeltaY, aDrawingCanvas.Width, aDrawingCanvas.Height);

                    aDrawingCanvas.FiduGraphicsList.Add(rectFiduGraphic as GraphicsRectangle);
                    aDrawingCanvas.GraphicsList.Add(rectFiduGraphic);

                    if (rectFiduGraphic.RegionType == GraphicsRegionType.UnitRegion)
                    {
                        foreach (GraphicsRectangle childGraphic in GraphicsList)
                        {
                            if (childGraphic.IsFiducialRegion)
                                continue;
                            if (childGraphic.MotherROI != fiduGraphic)
                                continue;

                            GraphicsBase rectChildGraphic = childGraphic.CreateSerializedObject().CreateGraphics();
                            rectChildGraphic.OriginObjectColor = rectChildGraphic.ObjectColor = childGraphic.OriginObjectColor;
                            rectChildGraphic.ActualScale = aDrawingCanvas.ActualScale;
                            ((GraphicsRectangle)rectChildGraphic).MotherROI = rectFiduGraphic;
                            ((GraphicsRectangle)rectChildGraphic).Move(fDeltaX, fDeltaY, aDrawingCanvas.Width, aDrawingCanvas.Height);

                            aDrawingCanvas.GraphicsList.Add(rectChildGraphic as GraphicsRectangle);
                        }
                    }
                }
                aDrawingCanvas.Refresh();
            }
        }
    }
}
