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

using System;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp;

namespace Common
{
    public static class BitmapSourceHelper
    {
        private static int R = 0;
        private static int G = 1;
        private static int B = 2;


        public static byte[] Mono_GetLinePixels(BitmapSource source, int nYPosition)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return null;
                }
                else if (nYPosition == source.PixelWidth - 1 && nYPosition < 0)
                {
                    return null;
                }

                byte[] pixels = new byte[source.PixelWidth];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                source.CopyPixels(new Int32Rect(0, nYPosition, source.PixelWidth, 1), pinnedPixels.AddrOfPinnedObject(), source.PixelWidth, source.PixelWidth);
                pinnedPixels.Free();

                return pixels;
            }
            catch
            {
                Debug.WriteLine("Exception occured in GetLinePixels(BitmapSourceHelper.cs)");
                return null;
            }
        }

        public static byte[,] Color_GetLinePixels(BitmapSource source, int nYPosition, ChannelType channel)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return null;
                }
                else if (nYPosition == source.PixelWidth - 1 && nYPosition < 0)
                {
                    return null;
                }


                byte[,] Color_Line_pixel = new byte[3, source.PixelWidth];


                #region Bgr32 라인 프로파일

                if (source.Format.BitsPerPixel == 32 && channel == ChannelType.Color) // Bgr32
                {

                    int Length = source.PixelWidth * 4;
                    byte[] pixels = new byte[Length];


                    GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                    source.CopyPixels(new Int32Rect(0, nYPosition, source.PixelWidth, 1), pinnedPixels.AddrOfPinnedObject(), Length, Length);
                    pinnedPixels.Free();


                    for (int i = 0; i < Length; i++)
                    {
                        if (i % 4 == 3) { continue; }
                        if (i % 4 == 0) { Color_Line_pixel[2, (int)i / 4] = pixels[i]; }
                        if (i % 4 == 1) { Color_Line_pixel[1, (int)i / 4] = pixels[i]; }
                        if (i % 4 == 2) { Color_Line_pixel[0, (int)i / 4] = pixels[i]; }

                    }

                }

                #endregion

                #region Bgr24 라인 프로파일

                if (source.Format.BitsPerPixel == 24 && channel == ChannelType.Color) // Bgr24
                {
                    int Length = source.PixelWidth * 3;
                    byte[] pixels = new byte[Length];

                    GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                    source.CopyPixels(new Int32Rect(0, nYPosition, source.PixelWidth, 1), pinnedPixels.AddrOfPinnedObject(), Length, Length);
                    pinnedPixels.Free();

                    for (int i = 0; i < Length; i++)
                    {

                        if (i % 3 == 0) { Color_Line_pixel[0, (int)i / 3] = pixels[i]; }
                        if (i % 3 == 1) { Color_Line_pixel[1, (int)i / 3] = pixels[i]; }
                        if (i % 3 == 2) { Color_Line_pixel[2, (int)i / 3] = pixels[i]; }

                    }
                }
                #endregion

                #region Gray8 라인 프로파일

                if (source.Format.BitsPerPixel == 8 && channel != ChannelType.Color) // Gray8
                {
                    int Length = source.PixelWidth;
                    byte[] pixels = new byte[Length];


                    GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                    source.CopyPixels(new Int32Rect(0, nYPosition, source.PixelWidth, 1), pinnedPixels.AddrOfPinnedObject(), Length, Length);
                    pinnedPixels.Free();


                    if (channel == ChannelType.RED) { for (int i = 0; i < Length; i++) { Color_Line_pixel[0, i] = pixels[i]; } }

                    if (channel == ChannelType.GREEN) { for (int i = 0; i < Length; i++) { Color_Line_pixel[1, i] = pixels[i]; } }

                    if (channel == ChannelType.BLUE) { for (int i = 0; i < Length; i++) { Color_Line_pixel[2, i] = pixels[i]; } }

                }

                #endregion


                return Color_Line_pixel;
            }
            catch
            {
                Debug.WriteLine("Exception occured in GetLinePixels(BitmapSourceHelper.cs)");
                return null;
            }
        }

        public static long[,] CalculateHistogramData(BitmapSource[] source)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return new long[source.Length, 256];
                }

                long[,] histogram = new long[source.Length, 256];

                int nWidth = source[0].PixelWidth;
                int nHeight = source[0].PixelHeight;

                byte[] pixels = new byte[nWidth];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                for (int i = 0; i < source.Length; i++)
                {
                    for (int j = 0; j < nHeight; j++)
                    {
                        source[i].CopyPixels(new Int32Rect(0, j, nWidth, 1), pinnedPixels.AddrOfPinnedObject(), nWidth, nWidth);

                        for (int k = 0; k < pixels.Length; k++)
                        {
                            histogram[i, pixels[k]]++;
                        }

                    }
                }

                pinnedPixels.Free();

                return histogram;
            }
            catch
            {
                Debug.WriteLine("Exception occured in CalculateHistogramData(BitmapSourceHelper.cs)");
                return new long[source.Length, 256];
            }
        }

        public static long[,] Color_CalculateHistogramData(BitmapSource source)
        {
            try
            {
                if (source == null)
                {
                    return new long[3, 256];
                }

                if (source.Format.BitsPerPixel != 24)
                {
                    return new long[3, 256];
                }

                long[,] histogram = new long[3, 256];

                int width = source.PixelWidth;
                int height = source.PixelHeight;

                Mat Frame_mat = new Mat();
                Mat[] RGB_Frame_mat = new Mat[3];

                Frame_mat = BitmapSourceConverter.ToMat(source);
                Cv2.Split(Frame_mat, out RGB_Frame_mat);

                BitmapSource[] RGB_Frame_bitmapSource = new BitmapSource[3];


                RGB_Frame_bitmapSource[R] = BitmapSourceConverter.ToBitmapSource(RGB_Frame_mat[0]);
                RGB_Frame_bitmapSource[G] = BitmapSourceConverter.ToBitmapSource(RGB_Frame_mat[1]);
                RGB_Frame_bitmapSource[B] = BitmapSourceConverter.ToBitmapSource(RGB_Frame_mat[2]);



                byte[] pixels = new byte[width];

                GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        RGB_Frame_bitmapSource[i].CopyPixels(new Int32Rect(0, j, width, 1), pinnedPixels.AddrOfPinnedObject(), width, width);

                        for (int k = 0; k < pixels.Length; k++)
                        {
                            histogram[i, pixels[k]]++;
                        }
                    }
                }

                pinnedPixels.Free();

                return histogram;

            }
            catch
            {
                Debug.WriteLine("Exception occured in CalculateHistogramData(BitmapSourceHelper.cs)");
                return new long[3, 256];
            }
        }

        public static long[] Mono_CalculateHistogramData(BitmapSource source)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (source == null)
                {
                    return new long[256];
                }

                if (source.Format.BitsPerPixel != 8)
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



        public static BitmapSource SnapFrameworkElement(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            try
            {
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(new VisualBrush(element), null,
                                                 new System.Windows.Rect(new System.Windows.Point(0, 0), new System.Windows.Point(element.ActualWidth, element.ActualHeight)));
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
                                             new System.Windows.Rect(new System.Windows.Point(0, 0), new System.Windows.Point(element.ActualWidth, element.ActualHeight)));
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
