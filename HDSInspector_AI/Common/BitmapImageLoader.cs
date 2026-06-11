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
 * @file  BitmapImageLoader.cs
 * @brief
 *  This class is necessary to load the bitmap image file. 
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.08.15
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.04.04 First creation.
 * - 2011.08.15 Added LoadBitmapAsIndexed8()
 */

using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace Common
{
    /// <summary>   Bitmap image loader.  </summary>
    /// <remarks>   suoow2, 2014-04-04. </remarks>
    public static class BitmapImageLoader
    {
        // BitmapCacheOption ; BitmapCacheOption.OnDemand(default)
        // BitmapCreateOptions ; BitmapCreateOptions.DelayCreation(default)

        public static BitmapImage LoadBitmap(Uri uriSource, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.OnDemand, BitmapCreateOptions bitmapCreateOption = BitmapCreateOptions.DelayCreation)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = bitmapCacheOption;
                bitmapImage.CreateOptions = bitmapCreateOption;
                bitmapImage.UriSource = uriSource;
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadBitmap(BitmapImageLoader.cs)");
                return null;
            }
        }

        // Indexed8 Format(1픽셀당 8비트)으로 BitmapImage를 반환합니다.
        public static ImageSource LoadBitmapAsIndexed8(Uri uriSource, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.OnDemand, BitmapCreateOptions bitmapCreateOption = BitmapCreateOptions.DelayCreation)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = bitmapCacheOption;
                bitmapImage.CreateOptions = bitmapCreateOption;
                bitmapImage.UriSource = uriSource;
                bitmapImage.EndInit();

                if (bitmapImage.Format != PixelFormats.Indexed8)
                {
                    return new FormatConvertedBitmap(bitmapImage, PixelFormats.Indexed8, null, 0);
                }
                else
                {
                    return bitmapImage;
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadBitmapAsIndexed8(BitmapImageLoader.cs)");
                return null;
            }
        }

        // 요청된 decodePixelWidth, decodePixelHeight로 BitmapImage를 반환합니다.
        public static ImageSource LoadBitmapByDecodePixel(Uri uriSource, int decodePixelWidth, int decodePixelHeight)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                if (decodePixelWidth > 0 && decodePixelHeight > 0)
                {
                    bitmapImage.DecodePixelWidth = decodePixelWidth;
                    bitmapImage.DecodePixelHeight = decodePixelHeight;
                }
                bitmapImage.UriSource = uriSource;
                bitmapImage.EndInit();
                                
                return bitmapImage;
            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadBitmapByDecodePixel(BitmapImageLoader.cs)");
                return null;
            }
        }

        // Uri로부터 Cached된 ImageSource를 반환합니다. (자유롭게 쓰고 지울 수 있다.)
        public static ImageSource LoadCachedBitmapImage(Uri aUriSource)
        {
            try
            {
                BitmapSource cachedBitmapSource = Algo.LoadImage(aUriSource.LocalPath);

                //BitmapSource bitmapImage = LoadBitmapAsIndexed8(aUriSource) as BitmapSource;
                //
                //byte[] pixels = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight];
                //bitmapImage.CopyPixels(pixels, bitmapImage.PixelWidth, 0);
                //
                //BitmapSource cachedBitmapSource = BitmapSource.Create(bitmapImage.PixelWidth, bitmapImage.PixelHeight, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256, pixels, bitmapImage.PixelWidth);
                return cachedBitmapSource;
            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadClonedBitmapByDecodePixel(BitmapImageLoader.cs)");
                return null;
            }
        }

        public static BitmapSource LoadCachedPngImage(string aUriSource)
        {
            try
            {

                System.IO.FileStream stream = new System.IO.FileStream(aUriSource, System.IO.FileMode.Open);
                PngBitmapDecoder pngBitmapDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                BitmapSource stripImage = pngBitmapDecoder.Frames[0].Clone();

                stream.Close();
                return stripImage;

            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadClonedBitmapByDecodePixel(BitmapImageLoader.cs)");
                return null;
            }
        }

        public static BitmapSource LoadCachedPngImage(Uri aUriSource)
        {
            try
            {
                PngBitmapDecoder pngBitmapDecoder = new PngBitmapDecoder(aUriSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                
                BitmapSource stripImage = pngBitmapDecoder.Frames[0].Clone();

                //pngBitmapDecoder = null;
                //GC.Collect();
                return stripImage;

            }
            catch
            {
                Debug.WriteLine("Exception occured in LoadClonedBitmapByDecodePixel(BitmapImageLoader.cs)");
                return null;
            }
        }
    }
}
