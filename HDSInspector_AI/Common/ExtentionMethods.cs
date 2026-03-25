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
 * @file  Converters.cs
 * @brief
 *  converter series.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.07.04
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.17 First creation.
 * - 2011.07.04 Added geometry-extention.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Common
{
    /// <summary>   User interface element extensions.  </summary>
    /// <remarks>   suoow, 2014-07-04. </remarks>
    public static class UIElementExtensions
    {
        /// <summary> The empty delegate </summary>
        private static Action EmptyDelegate = delegate() { };

        /// <summary>   A System.Windows.UIElement extension method that refreshes. </summary>
        /// <remarks>   suoow, 2014-07-04. </remarks>
        /// <param name="uiElement">    The user interface element. </param>
        public static void Refresh(this System.Windows.UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }
    }

    /// <summary>   Stream geometry extensions.  </summary>
    /// <remarks>   suoow2, 2014-07-04. </remarks>
    /// <see>       http://stackoverflow.com/questions/2979834/how-to-draw-a-full-ellipse-in-a-streamgeometry-in-wpf </see>
    public static class StreamGeometryExtensions
    {
        // StreamGeometry를 이용하여 원을 그리기 위해 아래의 코드를 사용한다.
        // PathGeometry는 StreamGeometry보다 느리기 때문에 가급적이면 StreamGeometry를 사용하도록 한다.
        // sample usage:
        // using (var ctx = geometry.Open())
        // {
        //     ctx.DrawGeometry(new EllipseGeometry(new Point(100, 100), 10, 10));
        // }

        /// <summary>   A StreamGeometryContext extension method that draw geometry. </summary>
        /// <remarks>   suoow2, 2014-07-04. </remarks>
        /// <param name="ctx">  The ctx to act on. </param>
        /// <param name="geo">  The geo. </param>
        public static void DrawGeometry(this StreamGeometryContext ctx, Geometry geo)
        {
            var pathGeometry = geo as PathGeometry ?? PathGeometry.CreateFromGeometry(geo);
            foreach (var figure in pathGeometry.Figures)
            {
                ctx.DrawFigure(figure);
            }
        }

        /// <summary>   A StreamGeometryContext extension method that draw figure. </summary>
        /// <remarks>   suoow2, 2014-07-04. </remarks>
        /// <param name="ctx">      The ctx to act on. </param>
        /// <param name="figure">   The figure. </param>
        public static void DrawFigure(this StreamGeometryContext ctx, PathFigure figure)
        {
            ctx.BeginFigure(figure.StartPoint, figure.IsFilled, figure.IsClosed);
            foreach (var segment in figure.Segments)
            {
                var lineSegment = segment as LineSegment;
                if (lineSegment != null)
                {
                    ctx.LineTo(lineSegment.Point, lineSegment.IsStroked, lineSegment.IsSmoothJoin);
                    continue;
                }

                var bezierSegment = segment as BezierSegment;
                if (bezierSegment != null)
                {
                    ctx.BezierTo(bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, bezierSegment.IsStroked, bezierSegment.IsSmoothJoin);
                    continue;
                }

                var quadraticSegment = segment as QuadraticBezierSegment;
                if (quadraticSegment != null)
                {
                    ctx.QuadraticBezierTo(quadraticSegment.Point1, quadraticSegment.Point2, quadraticSegment.IsStroked, quadraticSegment.IsSmoothJoin);
                    continue;
                }

                var polyLineSegment = segment as PolyLineSegment;
                if (polyLineSegment != null)
                {
                    ctx.PolyLineTo(polyLineSegment.Points, polyLineSegment.IsStroked, polyLineSegment.IsSmoothJoin);
                    continue;
                }

                var polyBezierSegment = segment as PolyBezierSegment;
                if (polyBezierSegment != null)
                {
                    ctx.PolyBezierTo(polyBezierSegment.Points, polyBezierSegment.IsStroked, polyBezierSegment.IsSmoothJoin);
                    continue;
                }

                var polyQuadraticSegment = segment as PolyQuadraticBezierSegment;
                if (polyQuadraticSegment != null)
                {
                    ctx.PolyQuadraticBezierTo(polyQuadraticSegment.Points, polyQuadraticSegment.IsStroked, polyQuadraticSegment.IsSmoothJoin);
                    continue;
                }

                var arcSegment = segment as ArcSegment;
                if (arcSegment != null)
                {
                    ctx.ArcTo(arcSegment.Point, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, arcSegment.IsStroked, arcSegment.IsSmoothJoin);
                    continue;
                }
            }
        }
    }
}
