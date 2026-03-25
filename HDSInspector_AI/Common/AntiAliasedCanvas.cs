/*********************************************************************************
 * Copyright(c) 2011,2012,2013 by Samsung Techwin.
 * 
 * This software is copyrighted by, and is the sole property of Samsung Techwin.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Samsung Techwin. This software may only be used in accordance with
 * the corresponding license agreement. Any unauthorized use, duplication, 
 * transmission, distribution, or disclosure of this software is expressly 
 * forbidden.
 *
 * This Copyright notice may not be removed or modified without prior written
 * consent of Samsung Techwin.Samsung Techwin reserves the right to modify this 
 * software without notice.
 *
 * Samsung Techwin.
 * KOREA 
 * http://www.samsungtechwin.co.kr
 *********************************************************************************/
/**
 * @file  AntiAliasedCanvas.cs
 * @brief
 *  WPF에서 제공되는 Canvas, InkCanvas 컨트롤의 렌더링 방식은 Anti-Aliasing을 Default로 적용하고 있습니다.
 *  하지만 IS에서 실제 비전 검사를 수행하는데 있어서 기준 이미지에 Anti-Aliasing이 적용된 경우 정확한 연산을 수행할 수 없습니다.
 *  따라서 본 파일은 Anti-Aliasing이 제거된 Canvas, InkCanvas를 제공하기 위해 선언되었습니다.
 * 
 * @author : Minseok Hwang <h.min-suck@samsung.com>
 * @date : 2011.10.01
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.10.01 First creation.
 */

using System;
using System.Windows.Media;

namespace Common
{
    // AntiAliasedInkCanvas : Anti-Aliasing이 제거된 InkCanvas 버전입니다.
    // AntiAliasedCanvas : Anti-Aliasing이 제거된 Canvas 버전입니다.
    
    /// <summary>   Anti aliased ink canvas.  </summary>
    /// <remarks>   Minseok, Hwang, 2011-10-01. </remarks>
    public class AntiAliasedInkCanvas : System.Windows.Controls.InkCanvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            this.VisualBitmapScalingMode = System.Windows.Media.BitmapScalingMode.NearestNeighbor;
            base.OnRender(dc);
        }
    }

    /// <summary>   Anti aliased canvas.  </summary>
    /// <remarks>   Minseok, Hwang, 2011-10-01. </remarks>
    public class AntiAliasedCanvas : System.Windows.Controls.Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            this.VisualBitmapScalingMode = System.Windows.Media.BitmapScalingMode.NearestNeighbor;
            base.OnRender(dc);
        }
    }
}
