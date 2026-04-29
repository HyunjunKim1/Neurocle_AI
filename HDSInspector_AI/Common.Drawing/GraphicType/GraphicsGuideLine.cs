using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

// 2012.04.09 suoow2.

namespace Common.Drawing
{
    // 2012.04.05 suoow2.
    public class Margin
    {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Spec;

        public Margin()
        {
            SetMargin(0, 0, 0, 0, 0);
        }

        public Margin(double afLeft, double afTop, double afRight, double afBottom, double afSpec)
        {
            SetMargin(afLeft, afTop, afRight, afBottom, afSpec);
        }

        public void SetMargin(double afLeft, double afTop, double afRight, double afBottom, double afSpec)
        {
            Left = afLeft;
            Top = afTop;
            Right = afRight;
            Bottom = afBottom;
            Spec = afSpec;
        }

        public override string ToString()
        {
            return string.Format("Left:{0}, Top:{1}, Right:{2}, Bottom:{3}, Margin:{4}", Left, Top, Right, Bottom, Spec);
        }

        public Margin Clone()
        {
            return new Margin(Left, Top, Right, Bottom, Spec);
        }
    }

    public class GraphicsGuideLine : GraphicsRectangleBase
    {
        protected GraphicsRectangle outerGuideLine;
        protected GraphicsRectangle innerGuideLine;

        protected GraphicsRectangle outerGuideLine1;
        protected GraphicsRectangle innerGuideLine1;
        protected GraphicsRectangle outerGuideLine2;
        protected GraphicsRectangle innerGuideLine2;

        protected Margin outerGuideMargin;
        protected Margin innerGuideMargin;

        private double cpX;
        private double cpY;

        #region Constructors
        public GraphicsGuideLine(double left, double top, double right, double bottom, double lineWidth, Color objectColor, double actualScale, Margin outerMargin = null, Margin innerMargin = null)
        {
            this.startPoint = new Point(left, top);

            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;

            this.LeftProperty = (int)left;
            this.TopProperty = (int)top;
            this.WidthProperty = Math.Abs(rectangleRight - rectangleLeft);
            this.HeightProperty = Math.Abs(rectangleBottom - rectangleTop);

            this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
            this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);

            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.OriginObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            // Guide Line.
            this.graphicsRegionType = GraphicsRegionType.GuideLine;
            this.caption = CaptionHelper.GuideLineCaption;

            OuterMargin = outerMargin;
            InnerMargin = innerMargin;

            RefreshDrawing();
        }
        #endregion Constructors

        public GraphicsRectangle CreateGuideLine(Margin aRect)
        {
            double fLeft, fRight, fTop, fBottom;

            // left, right.
            fLeft = cpX - aRect.Left;
            fRight = cpX + aRect.Right;

            // top, bottom.
            fTop = cpY - aRect.Top;
            fBottom = cpY + aRect.Bottom;

            // create outer guide line.
            return new GraphicsRectangle(Math.Min(fLeft, fRight), Math.Min(fTop, fBottom), Math.Max(fLeft, fRight), Math.Max(fTop, fBottom),
                                         graphicsLineWidth, GraphicsRegionType.GuideLine, GraphicsColors.Yellow, graphicsActualScale);
        }

        public GraphicsRectangle CreateGuideLine1(Margin aRect)
        {
            double fLeft, fRight, fTop, fBottom;

            // left, right.
            fLeft = cpX - aRect.Left - aRect.Spec;
            fRight = cpX + aRect.Right + aRect.Spec;
            // top, bottom.
            fTop = cpY - aRect.Top - aRect.Spec;
            fBottom = cpY + aRect.Bottom + aRect.Spec;

            // create outer guide line.
            return new GraphicsRectangle(Math.Min(fLeft, fRight), Math.Min(fTop, fBottom), Math.Max(fLeft, fRight), Math.Max(fTop, fBottom),
                                         graphicsLineWidth, GraphicsRegionType.GuideLine, GraphicsColors.Yellow, graphicsActualScale);
        }

        public GraphicsRectangle CreateGuideLine2(Margin aRect)
        {
            double fLeft, fRight, fTop, fBottom;

            // left, right.
            fLeft = cpX - aRect.Left + aRect.Spec;
            fRight = cpX + aRect.Right - aRect.Spec;

            // top, bottom.
            fTop = cpY - aRect.Top + aRect.Spec;
            fBottom = cpY + aRect.Bottom - aRect.Spec;

            // create outer guide line.
            return new GraphicsRectangle(Math.Min(fLeft, fRight), Math.Min(fTop, fBottom), Math.Max(fLeft, fRight), Math.Max(fTop, fBottom),
                                         graphicsLineWidth, GraphicsRegionType.GuideLine, GraphicsColors.Yellow, graphicsActualScale);
        }
        #region Overrides
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            drawingContext.DrawRectangle(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                Rectangle);

            if (this.ActualScale >= 0.5)
            {
                drawingContext.DrawText(
                    CreateCaptionString(),
                    new Point(startPoint.X, startPoint.Y - (15 / graphicsActualScale)));
            }

            // 2012-03-30 suoow2 modified. (Line tool을 길이 측정 용도로 사용하고자 함.)
            DashStyle dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);
            Pen dashedPen = new Pen(Brushes.Yellow, ActualLineWidth) { DashStyle = dashStyle };

            // Draw line.
            if (outerGuideLine != null)
            {
                drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.Fuchsia), ActualLineWidth), outerGuideLine.Rectangle);
                drawingContext.DrawRectangle(null, dashedPen, outerGuideLine1.Rectangle);
                drawingContext.DrawRectangle(null, dashedPen, outerGuideLine2.Rectangle);
            }
            if (innerGuideLine != null)
            {
                drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.Fuchsia), ActualLineWidth), innerGuideLine.Rectangle);
                drawingContext.DrawRectangle(null, dashedPen, innerGuideLine1.Rectangle);
                drawingContext.DrawRectangle(null, dashedPen, innerGuideLine2.Rectangle);
            }

            base.Draw(drawingContext);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return this.Rectangle.Contains(point);
        }

        public override void Move(double deltaX, double deltaY, double maxWidth, double maxHeight)
        {
            double farLeft = maxWidth;
            double farTop = maxHeight;
            double farRight = 0;
            double farBottom = 0;

            bool bCanMove = false;
            if (outerGuideLine != null)
            {
                farLeft = Math.Min(outerGuideLine.Left, farLeft);
                farTop = Math.Min(outerGuideLine.Top, farTop);
                farRight = Math.Max(outerGuideLine.Right, farRight);
                farBottom = Math.Max(outerGuideLine.Bottom, farBottom);
            }
            if (innerGuideLine != null)
            {
                farLeft = Math.Min(innerGuideLine.Left, farLeft);
                farTop = Math.Min(innerGuideLine.Top, farTop);
                farRight = Math.Max(innerGuideLine.Right, farRight);
                farBottom = Math.Max(innerGuideLine.Bottom, farBottom);
            }
            farLeft = Math.Min(rectangleLeft, farLeft);
            farTop = Math.Min(rectangleTop, farTop);
            farRight = Math.Max(rectangleRight, farRight);
            farBottom = Math.Max(rectangleBottom, farBottom);

            bCanMove = (farLeft + deltaX > 0) && (farTop + deltaY > 0) && (farRight + deltaX < maxWidth) && (farBottom + deltaY < maxHeight);
            if (bCanMove)
            {
                if (outerGuideLine != null)
                {
                    outerGuideLine.Move(deltaX, deltaY, maxWidth, maxHeight);
                    outerGuideLine1.Move(deltaX, deltaY, maxWidth, maxHeight);
                    outerGuideLine2.Move(deltaX, deltaY, maxWidth, maxHeight);
                }
                if (innerGuideLine != null)
                {
                    innerGuideLine.Move(deltaX, deltaY, maxWidth, maxHeight);
                    innerGuideLine1.Move(deltaX, deltaY, maxWidth, maxHeight);
                    innerGuideLine2.Move(deltaX, deltaY, maxWidth, maxHeight);
                }
                base.Move(deltaX, deltaY, maxWidth, maxHeight);
            }
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
            point.X = (point.X < 0) ? 0 : point.X;
            point.Y = (point.Y < 0) ? 0 : point.Y;

            switch (handleNumber)
            {
                case 1:
                    rectangleLeft = point.X;
                    rectangleTop = point.Y;
                    /*this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Left = cpX + outerGuideMargin.Left;
                        outerGuideLine.Top = cpY + outerGuideMargin.Top;
                        outerGuideLine1.Left = cpX + outerGuideMargin.Left + outerGuideMargin.Spec;
                        outerGuideLine1.Top = cpY + outerGuideMargin.Top + outerGuideMargin.Spec;
                        outerGuideLine2.Left = cpX + outerGuideMargin.Left - outerGuideMargin.Spec;
                        outerGuideLine2.Top = cpY + outerGuideMargin.Top - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Left = rectangleLeft + innerGuideMargin.Left;
                        innerGuideLine.Top = rectangleTop + innerGuideMargin.Top;
                        innerGuideLine1.Left = rectangleLeft + innerGuideMargin.Left + innerGuideMargin.Spec;
                        innerGuideLine1.Top = rectangleTop + innerGuideMargin.Top + innerGuideMargin.Spec;
                        innerGuideLine2.Left = rectangleLeft + innerGuideMargin.Left - innerGuideMargin.Spec;
                        innerGuideLine2.Top = rectangleTop + innerGuideMargin.Top - innerGuideMargin.Spec;
                    }*/
                    break;
                case 2:
                    rectangleTop = point.Y;
                    /*this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Top = rectangleTop + outerGuideMargin.Top;
                        outerGuideLine1.Top = rectangleTop + outerGuideMargin.Top + outerGuideMargin.Spec;
                        outerGuideLine2.Top = rectangleTop + outerGuideMargin.Top - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Top = rectangleTop + innerGuideMargin.Top;
                        innerGuideLine1.Top = rectangleTop + innerGuideMargin.Top + innerGuideMargin.Spec;
                        innerGuideLine2.Top = rectangleTop + innerGuideMargin.Top - innerGuideMargin.Spec;
                    }*/
                    break;
                case 3:
                    rectangleRight = point.X;
                    rectangleTop = point.Y;
                   /* this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Right = rectangleRight - outerGuideMargin.Right;
                        outerGuideLine.Top = rectangleTop + outerGuideMargin.Top;
                        outerGuideLine1.Right = rectangleRight - outerGuideMargin.Right + outerGuideMargin.Spec;
                        outerGuideLine1.Top = rectangleTop + outerGuideMargin.Top + outerGuideMargin.Spec;
                        outerGuideLine2.Right = rectangleRight - outerGuideMargin.Right - outerGuideMargin.Spec;
                        outerGuideLine2.Top = rectangleTop + outerGuideMargin.Top - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Right = rectangleRight - innerGuideMargin.Right;
                        innerGuideLine.Top = rectangleTop + innerGuideMargin.Top;
                        innerGuideLine1.Right = rectangleRight - innerGuideMargin.Right + innerGuideMargin.Spec;
                        innerGuideLine1.Top = rectangleTop + innerGuideMargin.Top + innerGuideMargin.Spec;
                        innerGuideLine2.Right = rectangleRight - innerGuideMargin.Right - innerGuideMargin.Spec;
                        innerGuideLine2.Top = rectangleTop + innerGuideMargin.Top - innerGuideMargin.Spec;
                    }*/
                    break;
                case 4:
                    rectangleRight = point.X;
                   /* this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Right = rectangleRight - outerGuideMargin.Right;
                        outerGuideLine1.Right = rectangleRight - outerGuideMargin.Right + outerGuideMargin.Spec;
                        outerGuideLine2.Right = rectangleRight - outerGuideMargin.Right - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Right = rectangleRight - innerGuideMargin.Right;
                        innerGuideLine1.Right = rectangleRight - innerGuideMargin.Right + innerGuideMargin.Spec;
                        innerGuideLine2.Right = rectangleRight - innerGuideMargin.Right - innerGuideMargin.Spec;
                    }*/
                    break;
                case 5:
                    rectangleRight = point.X;
                    rectangleBottom = point.Y;
                    /*this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Right = rectangleRight - outerGuideMargin.Right;
                        outerGuideLine.Bottom = rectangleBottom - outerGuideMargin.Bottom;
                        outerGuideLine1.Right = rectangleRight - outerGuideMargin.Right + outerGuideMargin.Spec;
                        outerGuideLine1.Bottom = rectangleBottom - outerGuideMargin.Bottom + outerGuideMargin.Spec;
                        outerGuideLine2.Right = rectangleRight - outerGuideMargin.Right - outerGuideMargin.Spec;
                        outerGuideLine2.Bottom = rectangleBottom - outerGuideMargin.Bottom - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Right = rectangleRight - innerGuideMargin.Right;
                        innerGuideLine.Bottom = rectangleBottom - innerGuideMargin.Bottom;
                        innerGuideLine1.Right = rectangleRight - innerGuideMargin.Right + innerGuideMargin.Spec;
                        innerGuideLine1.Bottom = rectangleBottom - innerGuideMargin.Bottom + innerGuideMargin.Spec;
                        innerGuideLine2.Right = rectangleRight - innerGuideMargin.Right - innerGuideMargin.Spec;
                        innerGuideLine2.Bottom = rectangleBottom - innerGuideMargin.Bottom - innerGuideMargin.Spec;
                    }*/
                    break;
                case 6:
                    rectangleBottom = point.Y;
                    /*this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Bottom = rectangleBottom - outerGuideMargin.Bottom;
                        outerGuideLine1.Bottom = rectangleBottom - outerGuideMargin.Bottom + outerGuideMargin.Spec;
                        outerGuideLine2.Bottom = rectangleBottom - outerGuideMargin.Bottom - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Bottom = rectangleBottom - innerGuideMargin.Bottom;
                        innerGuideLine1.Bottom = rectangleBottom - innerGuideMargin.Bottom + innerGuideMargin.Spec;
                        innerGuideLine2.Bottom = rectangleBottom - innerGuideMargin.Bottom - innerGuideMargin.Spec;
                    }*/
                    break;
                case 7:
                    rectangleLeft = point.X;
                    rectangleBottom = point.Y;
                   /*this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
                    this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
                    if (outerGuideLine != null)
                    {
                        outerGuideLine.Left = rectangleLeft + outerGuideMargin.Left;
                        outerGuideLine.Bottom = rectangleBottom - outerGuideMargin.Bottom;
                        outerGuideLine1.Left = rectangleLeft + outerGuideMargin.Left + outerGuideMargin.Spec;
                        outerGuideLine1.Bottom = rectangleBottom - outerGuideMargin.Bottom + outerGuideMargin.Spec;
                        outerGuideLine2.Left = rectangleLeft + outerGuideMargin.Left - outerGuideMargin.Spec;
                        outerGuideLine2.Bottom = rectangleBottom - outerGuideMargin.Bottom - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Left = rectangleLeft + innerGuideMargin.Left;
                        innerGuideLine.Bottom = rectangleBottom - innerGuideMargin.Bottom;
                        innerGuideLine1.Left = rectangleLeft + innerGuideMargin.Left + innerGuideMargin.Spec;
                        innerGuideLine1.Bottom = rectangleBottom - innerGuideMargin.Bottom + innerGuideMargin.Spec;
                        innerGuideLine2.Left = rectangleLeft + innerGuideMargin.Left - innerGuideMargin.Spec;
                        innerGuideLine2.Bottom = rectangleBottom - innerGuideMargin.Bottom - innerGuideMargin.Spec;
                    }*/
                    break;
                case 8:
                    rectangleLeft = point.X;  
                   /* if (outerGuideLine != null)
                    {
                        outerGuideLine.Left = rectangleLeft + outerGuideMargin.Left;
                        outerGuideLine1.Left = rectangleLeft + outerGuideMargin.Left + outerGuideMargin.Spec;
                        outerGuideLine2.Left = rectangleLeft + outerGuideMargin.Left - outerGuideMargin.Spec;
                    }
                    if (innerGuideLine != null)
                    {
                        innerGuideLine.Left = rectangleLeft + innerGuideMargin.Left;
                        innerGuideLine1.Left = rectangleLeft + innerGuideMargin.Left + innerGuideMargin.Spec;
                        innerGuideLine2.Left = rectangleLeft + innerGuideMargin.Left - innerGuideMargin.Spec;
                    }*/
                    break;
            }
            this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
            this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
            if (outerGuideLine != null)
            {
                outerGuideLine.Left = cpX - outerGuideMargin.Left;
                outerGuideLine.Top = cpY - outerGuideMargin.Top;
                outerGuideLine.Right = cpX + outerGuideMargin.Right;
                outerGuideLine.Bottom = cpY + outerGuideMargin.Bottom;
                outerGuideLine1.Left = cpX - outerGuideMargin.Left - outerGuideMargin.Spec;
                outerGuideLine1.Top = cpY - outerGuideMargin.Top - outerGuideMargin.Spec;
                outerGuideLine1.Right = cpX + outerGuideMargin.Right + outerGuideMargin.Spec;
                outerGuideLine1.Bottom = cpY + outerGuideMargin.Bottom + outerGuideMargin.Spec;
                outerGuideLine2.Left = cpX - outerGuideMargin.Left + outerGuideMargin.Spec;
                outerGuideLine2.Top = cpY - outerGuideMargin.Top + outerGuideMargin.Spec;
                outerGuideLine2.Right = cpX + outerGuideMargin.Right - outerGuideMargin.Spec;
                outerGuideLine2.Bottom = cpY + outerGuideMargin.Bottom - outerGuideMargin.Spec;
            }
            if (innerGuideLine != null)
            {
                innerGuideLine.Left = cpX - innerGuideMargin.Left;
                innerGuideLine.Top = cpY - innerGuideMargin.Top;
                innerGuideLine.Right = cpX + innerGuideMargin.Right;
                innerGuideLine.Bottom = cpY + innerGuideMargin.Bottom;
                innerGuideLine1.Left = cpX - innerGuideMargin.Left - innerGuideMargin.Spec;
                innerGuideLine1.Top = cpY - innerGuideMargin.Top - innerGuideMargin.Spec;
                innerGuideLine1.Right = cpX + innerGuideMargin.Right + innerGuideMargin.Spec;
                innerGuideLine1.Bottom = cpY + innerGuideMargin.Bottom + innerGuideMargin.Spec;
                innerGuideLine2.Left = cpX - innerGuideMargin.Left + innerGuideMargin.Spec;
                innerGuideLine2.Top = cpY - innerGuideMargin.Top + innerGuideMargin.Spec;
                innerGuideLine2.Right = cpX + innerGuideMargin.Right - innerGuideMargin.Spec;
                innerGuideLine2.Bottom = cpY + innerGuideMargin.Bottom - innerGuideMargin.Spec;
            }
            CalcBoundaryRect();
            RefreshDrawing();
            this.cpX = this.rectangleLeft + (Math.Abs(rectangleRight - rectangleLeft) / 2);
            this.cpY = this.rectangleTop + (Math.Abs(rectangleBottom - rectangleTop) / 2);
        }

        /// <summary>
        /// Serialization support
        /// </summary>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsGuideLine(this);
        }
        #endregion Overrides

        #region Properties.
        public GraphicsRectangle OuterGuideLine
        {
            get
            {
                return outerGuideLine;
            }
            set
            {
                outerGuideLine = value;
            }
        }

        public GraphicsRectangle InnerGuideLine
        {
            get
            {
                return innerGuideLine;
            }
            set
            {
                innerGuideLine = value;
            }
        }

        public Margin OuterMargin
        {
            get
            {
                return outerGuideMargin;
            }
            set
            {
                if (value != null)
                {
                    outerGuideMargin = value;

                    outerGuideLine = null;
                    outerGuideLine = CreateGuideLine(outerGuideMargin);
                    outerGuideLine.MotherROI = this;
                    outerGuideLine1 = CreateGuideLine1(outerGuideMargin);
                    outerGuideLine2 = CreateGuideLine2(outerGuideMargin);
                    outerGuideLine1.MotherROI = this;
                    outerGuideLine2.MotherROI = this;
                }
            }
        }

        public Margin InnerMargin
        {
            get
            {
                return innerGuideMargin;
            }
            set
            {
                if (value != null)
                {
                    innerGuideMargin = value;

                    innerGuideLine = null;
                    innerGuideLine = CreateGuideLine(innerGuideMargin);
                    innerGuideLine.MotherROI = this;
                    innerGuideLine1 = CreateGuideLine1(innerGuideMargin);
                    innerGuideLine2 = CreateGuideLine2(innerGuideMargin);
                    innerGuideLine1.MotherROI = this;
                    innerGuideLine2.MotherROI = this;
                }
            }
        }
        #endregion Properties.
    }

    public class GraphicsTapeLocation : GraphicsRectangleBase
    {
        protected GraphicsRectangle TapeLoation;


        protected GraphicsRectangle outerTape;
        protected GraphicsRectangle innerTape;

        protected double location;
        protected double margin;

        private double posX;
        private double posY;

        private int direction;

        #region Constructors
        public GraphicsTapeLocation(int anDir, double anposX, double anposY, double lineWidth, Color objectColor, double actualScale, double anLoaction, double anMargin)
        {
            this.startPoint = new Point(anposX, anposY);
            posX = anposX;
            posY = anposY;
            location = anLoaction;
            margin = anMargin;
            direction = anDir;
            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.OriginObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            // Guide Line.
            this.graphicsRegionType = GraphicsRegionType.TapeLoaction;
            this.caption = CaptionHelper.TapeLocationCaption;
            

            Paint();
            
        }

        void Paint()
        {
            switch (direction)
            {
                case 0:
                    this.rectangleLeft = posX - 50;
                    this.rectangleTop = posY;
                    this.rectangleRight = posX + 50;
                    this.rectangleBottom = posY + location;
                    break;
                case 1:
                    this.rectangleLeft = posX - 50;
                    this.rectangleTop = posY;
                    this.rectangleRight = posX + 50;
                    this.rectangleBottom = posY + location;
                    break;
                case 2:
                    this.rectangleLeft = posX;
                    this.rectangleTop = posY - 50;
                    this.rectangleRight = posX + location;
                    this.rectangleBottom = posY + 50;
                    break;
                case 3:
                    this.rectangleLeft = posX - location;
                    this.rectangleTop = posY - 50;
                    this.rectangleRight = posX;
                    this.rectangleBottom = posY + 50;
                    break;
            }

            this.LeftProperty = (int)rectangleLeft;
            this.TopProperty = (int)rectangleTop;
            this.WidthProperty = Math.Abs(rectangleRight - rectangleLeft);
            this.HeightProperty = Math.Abs(rectangleBottom - rectangleTop);



            double fLeft = 0, fRight = 0, fTop = 0, fBottom = 0;
            double fLeft1 = 0, fRight1 = 0, fTop1 = 0, fBottom1 = 0;

            switch (direction)
            {
                case 0:
                    fLeft = posX - 50;
                    fRight = posX + 50;
                    fTop = rectangleBottom - margin - 50;
                    fBottom = rectangleBottom - margin;
                    fLeft1 = posX - 50;
                    fRight1 = posX + 50;
                    fTop1 = rectangleBottom + margin;
                    fBottom1 = rectangleBottom + margin + 50;
                    break;
                case 1:
                    fLeft = posX - 50;
                    fRight = posX + 50;
                    fTop = rectangleTop - margin - 50;
                    fBottom = rectangleTop - margin;
                    fLeft1 = posX - 50;
                    fRight1 = posX + 50;
                    fTop1 = rectangleTop + margin;
                    fBottom1 = rectangleTop + margin + 50;
                    break;
                case 2:
                    fLeft = rectangleRight - margin;
                    fRight = rectangleRight - margin - 50;
                    fTop = posY - 50;
                    fBottom = posY + 50;
                    fLeft1 = rectangleRight + margin;
                    fRight1 = rectangleRight + margin + 50;
                    fTop1 = posY - 50;
                    fBottom1 = posY + 50;
                    break;
                case 3:
                    fLeft = rectangleLeft - margin;
                    fRight = rectangleLeft - margin - 50;
                    fTop = posY - 50;
                    fBottom = posY + 50;
                    fLeft1 = rectangleLeft + margin;
                    fRight1 = rectangleLeft + margin + 50;
                    fTop1 = posY - 50;
                    fBottom1 = posY + 50;
                    break;
            }

            this.innerTape = new GraphicsRectangle(Math.Min(fLeft, fRight), Math.Min(fTop, fBottom), Math.Max(fLeft, fRight), Math.Max(fTop, fBottom),
                                         graphicsLineWidth, GraphicsRegionType.TapeLoaction, GraphicsColors.Yellow, graphicsActualScale);
            this.outerTape = new GraphicsRectangle(Math.Min(fLeft1, fRight1), Math.Min(fTop1, fBottom1), Math.Max(fLeft1, fRight1), Math.Max(fTop1, fBottom1),
                                         graphicsLineWidth, GraphicsRegionType.TapeLoaction, GraphicsColors.Yellow, graphicsActualScale);

            innerTape.MotherROI = this;
            outerTape.MotherROI = this;
            RefreshDrawing();
        }

        #endregion Constructors

        #region Overrides
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            drawingContext.DrawRectangle(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                Rectangle);

            if (this.ActualScale >= 0.5)
            {
                drawingContext.DrawText(
                    CreateCaptionString(),
                    new Point(startPoint.X, startPoint.Y - (15 / graphicsActualScale)));
            }

            // 2012-03-30 suoow2 modified. (Line tool을 길이 측정 용도로 사용하고자 함.)
            DashStyle dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);
            Pen dashedPen = new Pen(Brushes.Yellow, ActualLineWidth) { DashStyle = dashStyle };

            drawingContext.DrawRectangle(null, dashedPen, innerTape.Rectangle);
            drawingContext.DrawRectangle(null, dashedPen, outerTape.Rectangle);


            base.Draw(drawingContext);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return this.Rectangle.Contains(point);
        }

        public override void Move(double deltaX, double deltaY, double maxWidth, double maxHeight)
        {
            double farLeft = maxWidth;
            double farTop = maxHeight;
            double farRight = 0;
            double farBottom = 0;

            bool bCanMove = false;
                // farLeft = Math.Min(outerGuideLine.Left, farLeft);
                //farTop = Math.Min(outerGuideLine.Top, farTop);
                //farRight = Math.Max(outerGuideLine.Right, farRight);
                //farBottom = Math.Max(outerGuideLine.Bottom, farBottom);

            farLeft = Math.Min(rectangleLeft, farLeft);
            farTop = Math.Min(rectangleTop, farTop);
            farRight = Math.Max(rectangleRight, farRight);
            farBottom = Math.Max(rectangleBottom, farBottom);

            bCanMove = (farLeft + deltaX > 0) && (farTop + deltaY > 0) && (farRight + deltaX < maxWidth) && (farBottom + deltaY < maxHeight);
            if (bCanMove)
            {
                innerTape.Move(deltaX, deltaY, maxWidth, maxHeight);
                outerTape.Move(deltaX, deltaY, maxWidth, maxHeight);
                base.Move(deltaX, deltaY, maxWidth, maxHeight);
                switch (direction)
                {
                    case 0:
                        PosX = base.Left + (base.Right - base.Left) / 2;
                        PosY = base.Top;
                        break;
                    case 1:
                        PosX = base.Left + (base.Right - base.Left) / 2;
                        PosY = base.Bottom;
                        break;
                    case 2:
                        PosX = base.Left;
                        PosY = base.Top + (base.Bottom - base.Top) / 2;
                        break;
                    case 3:
                        PosX = base.Right;
                        PosY = base.Top + (base.Bottom - base.Top) / 2;
                        break;
                }
               
            }
        }

        /// <summary>
        /// Serialization support
        /// </summary>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsTapeLoaction(this);
        }
        #endregion Overrides

        #region properties

        public int Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
              //  Paint();
            }
        }

        public double PosX
        {
            get
            {
                return posX;
            }
            set
            {
                posX = value;
            }
        }
        public double PosY
        {
            get
            {
                return posY;
            }
            set
            {
                posY = value;
            }
        }

        public double TapeMargin
        {
            get
            {
                return margin;
            }
            set
            {
                margin = value;
                Paint();
            }
        }

        public double Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
               // Paint();
            }
        }
        #endregion
    }
}
