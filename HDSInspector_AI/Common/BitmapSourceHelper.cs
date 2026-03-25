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
 * @file  BitmapSourceHelper.cs
 * @brief
 *  BitmapSource helper class.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.08.24
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.08.24 First creation.
 */

using System;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace Common
{
    /// <summary>   Bitmap source helper.  </summary>
    /// <remarks>   suoow2, 2014-08-24. </remarks>
    public static class BitmapSourceHelper
    {
        public static byte[] GetPixels(BitmapSource source)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return null;
                }

                byte[] pixels = new byte[source.PixelWidth * source.PixelHeight];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                source.CopyPixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), pinnedPixels.AddrOfPinnedObject(),
                                  source.PixelWidth * source.PixelHeight, source.PixelWidth);
                pinnedPixels.Free();

                return pixels;
            }
            catch
            {
                Debug.WriteLine("Exception occured in GetPixels(BitmapSourceHelper.cs)");
                return null;
            }
        }

        public static byte[] GetLinePixels(BitmapSource source, int nYPosition)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return null;
                }
                else if (nYPosition == source.PixelWidth - 1)
                {
                    return null;
                }

                byte[] pixels = new byte[source.PixelWidth];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                
                source.CopyPixels(new Int32Rect(0, nYPosition, source.PixelWidth, 1), pinnedPixels.AddrOfPinnedObject(),
                                  source.PixelWidth, source.PixelWidth);
                pinnedPixels.Free();

                return pixels;
            }
            catch
            {
                Debug.WriteLine("Exception occured in GetLinePixels(BitmapSourceHelper.cs)");
                return null;
            }
        }

        public static long[] CalculateHistogramData(BitmapSource source)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return new long[256];
                }

                long[] histogram = new long[256];

                byte[] pixels = new byte[source.PixelWidth];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                int nHeight = source.PixelHeight;
                for (int i = 0; i < nHeight; i++)
                {
                    source.CopyPixels(new Int32Rect(0, i, source.PixelWidth, 1),
                                                   pinnedPixels.AddrOfPinnedObject(),
                                                   source.PixelWidth, source.PixelWidth);

                    foreach (byte data in pixels)
                    {
                        histogram[data]++;
                    }
                }

                pinnedPixels.Free();

                return histogram;
            }
            catch
            {
                Debug.WriteLine("Exception occured in CalculateHistogramData(BitmapSourceHelper.cs)");
                return new long[256];
            }
        }

        public static BitmapSource SnapFrameworkElement(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            try
            {
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(new VisualBrush(element), null,
                                                 new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(element.ActualWidth, element.ActualHeight)));
                }
            }
            catch 
            {
                Debug.WriteLine("Exception occured in SnapFrameworkElement()");
                return null;
            }

            try
            {
                // RenderTargetBitmap의 PixelFormat은 Pbgra32만 지정가능하다.
                RenderTargetBitmap target = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                target.Render(drawingVisual);

                return target;
            }
            catch
            {
                Debug.WriteLine("Exception occured in ConverterBitmapImage(BitmapSourceHelper.cs");
                return null;
            }
        }

        public static BitmapSource ConverterBitmapImage(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(new VisualBrush(element), null,
                                             new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(element.ActualWidth, element.ActualHeight)));
            }

            try
            {
                // RenderTargetBitmap의 PixelFormat은 Pbgra32만 지정가능하다.
                RenderTargetBitmap target = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                target.Render(drawingVisual);

                // 실제 사용하는 포맷인 Indexed8로 변환하여 반환하도록 한다.
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(target, PixelFormats.Indexed8, BitmapPalettes.BlackAndWhite, 0);
                return formatConvertedBitmap;
            }
            catch
            {
                Debug.WriteLine("Exception occured in ConverterBitmapImage(BitmapSourceHelper.cs");
                return null;
            }
        }

        public static BitmapSource CloneBitmapSource(BitmapSource source)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source != null)
                {
                    byte[] pixels = new byte[source.PixelWidth * source.PixelHeight];
                    source.CopyPixels(pixels, source.PixelWidth, 0);

                    return BitmapSource.Create(source.PixelWidth, source.PixelHeight, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256, pixels, source.PixelWidth);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return source;
            }
        }
    }
}
