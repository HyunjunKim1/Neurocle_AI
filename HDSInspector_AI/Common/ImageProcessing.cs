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
 * @file  FileSupport.cs
 * @brief
 *  This class is necessary to load the bitmap image file. 
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.09.26
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.09.26 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace Common
{
    /// <summary>   Image processing helper class.  </summary>
    public static class ImageProcessing
    {
        /// <summary>   Binarizations. </summary>
        /// <remarks>   suoow2, 2014-09-26. </remarks>
        public static ImageSource Binarization(BitmapSource basedImage, int lowerThreshold, int upperThreshold)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (basedImage == null)
                {
                    return null;
                }

                WriteableBitmap writeableBitmapImage = new WriteableBitmap(basedImage);

                int w = writeableBitmapImage.PixelWidth;
                int h = writeableBitmapImage.PixelHeight;
                byte[] pixelData = new byte[w * h];

                writeableBitmapImage.CopyPixels(pixelData, w, 0);

                int nLength = w * h;
                for (int i = 0; i < nLength; ++i)
                {
                    if (pixelData[i] >= upperThreshold)
                    {
                        pixelData[i] = 255;
                    }
                    else if (pixelData[i] < lowerThreshold)
                    {
                        pixelData[i] = 0;
                    }
                }

                writeableBitmapImage.WritePixels(new Int32Rect(0, 0, w, h), pixelData, w, 0);

                return writeableBitmapImage;
            }
            catch
            {
                Debug.WriteLine("Exception occured in Binarization(ImageProcessing.cs)");
                return null;
            }
        }

        /// <summary>   Gets a filter. </summary>
        /// <remarks>   suoow2, 2014-09-26. </remarks>
        public static byte[,] GetFilter(int pixel)
        {
            // Erosion, Dilation 연산에 쓰일 필터를 생성한다.
            byte[,] filter = new byte[pixel * 2 + 1, pixel * 2 + 1];

            int filterSize = pixel * 2 + 1;
            int row = filterSize;
            int column = filterSize;

            int space = pixel;

            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    if (space <= x && x <= filterSize - space - 1)
                    {
                        filter[y, x] = 1;
                    }
                }

                if (space == 0 || y > pixel)
                {
                    space++;
                }
                else
                {
                    space--;
                }
            }
            return filter;
        }

        public static ImageSource Erosion()
        {
            // Not implemented yet.

            return null;
        }

        /// <summary>   Dilations. </summary>
        /// <remarks>   suoow2, 2014-09-26. </remarks>
        public static ImageSource Dilation(BitmapSource basedImage, int pixel)
        {
            try
            {
                // 256 gray format에서만 동작합니다.
                if (basedImage == null)
                {
                    return null;
                }

                int basedImageWidth = basedImage.PixelWidth;
                int basedImageHeight = basedImage.PixelHeight;

                // Set buffer
                WriteableBitmap cloneImage = new WriteableBitmap(basedImage);
                byte[] pixelData = new byte[basedImageWidth * basedImageHeight];
                cloneImage.CopyPixels(pixelData, basedImageWidth, 0);

                int filterSize = pixel * 2 + 1;
                byte[,] filter = GetFilter(filterSize);

                int apertureMinimum = -(filterSize / 2);
                int apertureMaximum = (filterSize / 2);
                for (int outerX = 0; outerX < basedImageWidth; ++outerX)
                {
                    for (int outerY = 0; outerY < basedImageHeight; ++outerY)
                    {
                        byte gvValue = 0;

                        for (int innerX = apertureMinimum; innerX < apertureMaximum; ++innerX)
                        {
                            int tempX = outerX + innerX;
                            if (tempX >= 0 && tempX < basedImageWidth)
                            {
                                for (int innerY = apertureMinimum; innerY < apertureMinimum; ++innerY)
                                {
                                    int tempY = outerY + innerY;
                                    if (tempY >= 0 && tempY < basedImageHeight)
                                    {
                                        byte tempColor = pixelData[tempX + (tempY * basedImageWidth)];
                                        if (tempColor > gvValue)
                                        {
                                            gvValue = tempColor;
                                        }
                                    }
                                }
                            }
                        }

                        pixelData[outerX + (outerY * basedImageWidth)] = gvValue;
                    }
                }

                cloneImage.WritePixels(new Int32Rect(0, 0, basedImageWidth, basedImageHeight), pixelData, basedImageWidth, 0);

                return cloneImage;
            }
            catch
            {
                Debug.WriteLine("Exception occured in Dilation(ImageProcessing.cs)");
                return null;
            }
        }
    }
}
