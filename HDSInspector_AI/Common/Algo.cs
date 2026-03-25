using System;
using System.Drawing;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Media.Effects;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using System.Windows.Markup;

namespace Common
{
    /// <summary>
    /// Modified by hjkim 26.03.25
    /// </summary>
    /// <param name="hObject"></param>
    /// <returns></returns>
    
    public static class BitmapHelper
    {
        // Bitmap : C# style.
        // BitmapSource : WPF style.

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Bitmap BitmapSource2Bitmap(BitmapSource aBitmapSource)
        {
            if (aBitmapSource == null) return null;

            Bitmap bitmap;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(aBitmapSource));
                bitmapEncoder.Save(memoryStream);
                bitmap = new Bitmap(memoryStream);
            }

            return bitmap;
        }
        public static Mat BitmapSourceToCVImage(BitmapSource bs)
        {
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder benc = new BmpBitmapEncoder();
            benc.Frames.Add(BitmapFrame.Create(bs));
            benc.Save(outStream);
            return BitmapConverter.ToMat(new System.Drawing.Bitmap(outStream));
        }
        public static BitmapSource ConvertBitmapToBS(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Gray8, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            GC.Collect();
            return bitmapSource;
        }
    }

    public class Algo
    {
        private OpenCvSharp.Mat m_imgEntireImage = new OpenCvSharp.Mat();
        private OpenCvSharp.Mat m_imgBufferImage = new OpenCvSharp.Mat();
        private OpenCvSharp.Mat m_imgTemplateImage = new OpenCvSharp.Mat();
        private OpenCvSharp.Mat m_imgGerberContours = new OpenCvSharp.Mat();
        private Mat m_imgYIQ = new Mat();
        OpenCvSharp.Rect m_rcImageROI;

        private double m_fMaxScore = 0;

        private System.Drawing.Point m_ptMaxPos = new System.Drawing.Point();
        private System.Drawing.Point m_ptOffset = new System.Drawing.Point();

        #region Property (MaxScore, Offset)
        public double MaxScore
        {
            get
            {
                return m_fMaxScore;
            }
            set
            {
                m_fMaxScore = value;
            }
        }

        public System.Drawing.Point Offset
        {
            get
            {
                return m_ptOffset;
            }
            set
            {
                m_ptOffset = value;
            }
        }
        #endregion

        public void XOffsetSetZero()
        {
            m_ptOffset.X = 0;
        }

        public void YOffsetSetZero()
        {
            m_ptOffset.Y = 0;
        }

        // Modified by hjkim - 26.03.25
        public bool SetImage(BitmapSource aBitmap)
        {
            if (aBitmap == null)
                return false;
            try
            {
                if (m_imgEntireImage != null)
                    m_imgEntireImage.Release();

                /*
                int size = aBitmap.PixelHeight * aBitmap.PixelWidth;
                byte[] bytes = new byte[size];
                aBitmap.CopyPixels(bytes, aBitmap.PixelWidth, 0);
                m_imgEntireImage = new Mat(aBitmap.PixelHeight, aBitmap.PixelWidth, MatType.CV_8UC1, bytes);
                //m_imgEntireImage = OpenCvSharp.Extensions.BitmapSourceConverter.ToMat(aBitmap);
                m_imgBufferImage = m_imgEntireImage.Clone();
                */

                // BitmapSourceConverter 사용 , 260325_hjkim
                // BitmapSource → Mat 변환
                m_imgEntireImage = BitmapSourceConverter.ToMat(aBitmap);
                m_imgBufferImage = m_imgEntireImage.Clone();

                return (m_imgEntireImage != null) && (m_imgBufferImage != null);
            }
            catch
            {
                return false;
            }
        }
        public unsafe byte[] GetAlignImage(Rectangle aROI)//, ref int w, ref int h)
        {
            Mat tmp = new Mat(aROI.Height, aROI.Width, MatType.CV_8UC1);
            m_imgBufferImage.SubMat(RectangleToRect(aROI)).CopyTo(tmp);

            int size = tmp.Rows * tmp.Cols * tmp.ElemSize();

            byte* bytes = (byte*)tmp.DataPointer;
            byte[] b = new byte[tmp.Rows * tmp.Cols];
            Marshal.Copy((IntPtr)bytes, b, 0, b.Length);

            return b;
        }

        // Modified by hjkim - 26.03.25
        public bool SetImage(byte[] data, int width, int height)
        {
            if (data == null)
                return false;
            try
            {
                if (m_imgEntireImage != null)
                    m_imgEntireImage.Release();
                
                m_imgEntireImage = new Mat(height, width, MatType.CV_8UC1);
                m_imgEntireImage.SetArray(data);
                m_imgBufferImage = m_imgEntireImage.Clone();

                return (m_imgEntireImage != null) && (m_imgBufferImage != null);
            }
            catch
            {
                return false;
            }
        }
        public bool SetImageROI(Rectangle aROI)
        {
            if ((m_imgEntireImage == null) || (m_imgBufferImage == null))
                return false;

            m_rcImageROI = RectangleToRect(aROI);

            return true;
        }

        public bool SetTemplateImage(Rectangle aROI)
        {
            if (m_imgBufferImage == null)
                return false;

            if (!SetImageROI(aROI))
                return false;
            
            Mat roi = m_imgBufferImage.SubMat(m_rcImageROI);
            roi.CopyTo(m_imgTemplateImage);

            return true;
        }
        public OpenCvSharp.Rect RectangleToRect(Rectangle rtg)
        {
            OpenCvSharp.Rect rt = new OpenCvSharp.Rect(rtg.X, rtg.Y, rtg.Width, rtg.Height);
            return rt;
        }
        public System.Drawing.Point OpencvSharpPointToDP(OpenCvSharp.Point opt)
        {
            System.Drawing.Point pt = new System.Drawing.Point(opt.X, opt.Y);
            return pt;
        }
        // Template Image를 기준으로 Matching 작업을 수행한다.
        public bool SearchTemplateImage(System.Drawing.Point aptPosition /* Search 시작 좌표 */,
                                        System.Drawing.Point aptSearchMargin /* Search Margin */,
                                        double afMinCorr /* 최소 허용 일치율 */,
                                        System.Drawing.Point aptOffset = new System.Drawing.Point() /* Offset, 기본값 (0,0) */,
                                        bool UsePadding = false)
        {
            Rectangle rectSearchROI = new Rectangle(aptPosition.X - aptSearchMargin.X, aptPosition.Y - aptSearchMargin.Y, 
                                                    m_imgTemplateImage.Width + aptSearchMargin.X * 2, m_imgTemplateImage.Height + aptSearchMargin.Y * 2);

            // modify - hjkim_250702 : 인라인 검사용 Matching
            if(UsePadding)
                return SearchTemplateImageWithPadding(RectangleToRect(rectSearchROI), afMinCorr, UsePadding, aptOffset);
            else
                return SearchTemplateImage(RectangleToRect(rectSearchROI), afMinCorr, UsePadding, aptOffset);
        }

        public bool SearchTemplateImageWithPadding(OpenCvSharp.Rect aRectSearchROI, double afMinCorr, bool UsePadding, System.Drawing.Point aptOffset = new System.Drawing.Point())
        {
            Mat imgResult = null;
            Mat paddedImage = new Mat();
            
            // 패딩 크기 (고정값 or margin과 동일하게 설정)
            int paddingY = 50;

            try
            {
                // ROI 영역에 Offset 반영
                aRectSearchROI.X += aptOffset.X;
                aRectSearchROI.Y += aptOffset.Y;

                // 패딩처리
                Cv2.CopyMakeBorder(m_imgBufferImage, paddedImage, paddingY, paddingY, 0, 0, BorderTypes.Replicate);

                OpenCvSharp.Rect paddedROI = new OpenCvSharp.Rect(aRectSearchROI.X,
                                                                  aRectSearchROI.Y + paddingY, 
                                                                  aRectSearchROI.Width, aRectSearchROI.Height);

                if (paddedROI.X < 0 || paddedROI.Y < 0 || paddedROI.Right > paddedImage.Width || paddedROI.Bottom > paddedImage.Height)
                    return false;

                OpenCvSharp.Size resultSize = new OpenCvSharp.Size(paddedROI.Width - m_imgTemplateImage.Width + 1,
                                                                   paddedROI.Height - m_imgTemplateImage.Height + 1);
                if (resultSize.Width <= 0 || resultSize.Height <= 0)
                    return false;

                imgResult = new Mat(resultSize, MatType.CV_32FC1);

                paddedImage.SubMat(paddedROI).SaveImage("d:\\test4.png");
                Cv2.MatchTemplate(paddedImage.SubMat(paddedROI), m_imgTemplateImage, imgResult, TemplateMatchModes.CCoeffNormed);
                
                OpenCvSharp.Point ptMinLocation;
                OpenCvSharp.Point ptMaxLocation;
                Cv2.MinMaxLoc(imgResult, out double fMinVal, out double fMaxVal, out ptMinLocation, out ptMaxLocation);

                var roi = paddedImage.SubMat(paddedROI);

                // 2. 가장 잘 맞는 위치 (ROI 내부 좌표)
                var matchTopLeft = ptMaxLocation;

                // 3. 템플릿 이미지 크기
                int w = m_imgTemplateImage.Width;
                int h = m_imgTemplateImage.Height;

                // 4. ROI 내부에서 매칭 영역 잘라내기
                var matchRegion = roi.SubMat(new OpenCvSharp.Rect(matchTopLeft.X, matchTopLeft.Y, w, h));

                matchRegion.SaveImage("d:\\test5.png");

                m_fMaxScore = fMaxVal;
                m_ptMaxPos = OpencvSharpPointToDP(ptMaxLocation);

                if (m_ptMaxPos.X <= 0) m_ptMaxPos.X = 1;
                if (m_ptMaxPos.Y <= 0) m_ptMaxPos.Y = 1;
                if (m_ptMaxPos.X >= imgResult.Width - 1) ptMaxLocation.X = imgResult.Width - 2;
                if (m_ptMaxPos.Y >= imgResult.Height - 1) ptMaxLocation.Y = imgResult.Height - 2;

                int matchX = paddedROI.X + ptMaxLocation.X;
                int matchY = paddedROI.Y + ptMaxLocation.Y - paddingY;

                var matchFromOrigin = m_imgBufferImage.SubMat(new OpenCvSharp.Rect(matchX, matchY, w,h)).SaveImage("d:\\test5.png");

                //m_imgEntireImage.ROI = rectOldROI;
                m_ptOffset.X = matchX - aRectSearchROI.X - (imgResult.Width / 2) + aptOffset.X;
                m_ptOffset.Y = matchY - aRectSearchROI.Y - (imgResult.Height / 2) + aptOffset.Y;


                return (m_fMaxScore >= 0.5);
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                // Resource 반환.
                imgResult?.Release();
                imgResult?.Dispose();
                imgResult = null;

                paddedImage?.Release();
                paddedImage?.Dispose();
                paddedImage = null;
            }
        }

        public bool SearchTemplateImage(OpenCvSharp.Rect aRectSearchROI, double afMinCorr, bool UsePadding, System.Drawing.Point aptOffset = new System.Drawing.Point())
        {
            Mat imgResult = new Mat();
            try
            {
                aRectSearchROI.X += aptOffset.X;
                aRectSearchROI.Y += aptOffset.Y;
                OpenCvSharp.Size nResultSize = new OpenCvSharp.Size(aRectSearchROI.Width - m_imgTemplateImage.Width + 1,
                                                                    aRectSearchROI.Height - m_imgTemplateImage.Height + 1);

                imgResult = new Mat(nResultSize, MatType.CV_32FC1);
                m_imgBufferImage.SubMat(aRectSearchROI).SaveImage("d:\\test4.png");
                Cv2.MatchTemplate(m_imgBufferImage.SubMat(aRectSearchROI), m_imgTemplateImage, imgResult, TemplateMatchModes.CCoeffNormed);

                double fMinVal;
                double fMaxVal;
                OpenCvSharp.Point ptMinLocation;
                OpenCvSharp.Point ptMaxLocation;
                Cv2.MinMaxLoc(imgResult, out fMinVal, out fMaxVal, out ptMinLocation, out ptMaxLocation);

                m_fMaxScore = fMaxVal;
                m_ptMaxPos = OpencvSharpPointToDP(ptMaxLocation);

                if (m_ptMaxPos.X <= 0) m_ptMaxPos.X = 1;
                if (m_ptMaxPos.Y <= 0) m_ptMaxPos.Y = 1;
                if (m_ptMaxPos.X >= imgResult.Width - 1) m_ptMaxPos.X = imgResult.Width - 2;
                if (m_ptMaxPos.Y >= imgResult.Height - 1) m_ptMaxPos.Y = imgResult.Height - 2;

                //m_imgEntireImage.ROI = rectOldROI;
                m_ptOffset.X = m_ptMaxPos.X - nResultSize.Width / 2 + aptOffset.X;
                m_ptOffset.Y = m_ptMaxPos.Y - nResultSize.Height / 2 + aptOffset.Y;

                // Resource 반환.
                imgResult.Dispose();
                imgResult = null;

                return (m_fMaxScore >= 0.5);
            }
            catch (Exception ex)
            {
                if (!imgResult.Empty())
                {
                    imgResult.Release();
                }
                return false;
            }
        }


        public void FindPick()
        {
            if (m_imgBufferImage != null)
            {
                int nVertCenter = m_imgBufferImage.Width / 2;
                Mat VertLine = m_imgBufferImage.Col(nVertCenter);
            }
        }
        // Y축 기준의 Profile을 추출한다.
        public float[] GetVerticalProfile(int anStartX, int anEndX, int anStartY, int anEndY)
        {
            try
            {
                if (m_imgBufferImage == null)
                    return null;

                int nHeight = anEndY - anStartY + 1;
                float[] arrVerticalProfile = new float[nHeight];

                if (nHeight < 1 || nHeight > m_imgBufferImage.Height ||
                    anStartX < 0 || anStartX < 0 ||
                    anEndX < anStartX || anEndY < anStartY ||
                    anStartX > m_imgBufferImage.Width ||
                    anStartY > m_imgBufferImage.Height ||
                    anEndX > m_imgBufferImage.Width ||
                    anEndY > m_imgBufferImage.Height)
                    return null;
                Rectangle rectROI = new Rectangle(anStartX, anStartY, anEndX - anStartX, 1);
                Mat imgLine = new Mat(anEndX - anStartX, 1, MatType.CV_8UC1);
                for (int y = anStartY; y <= anEndY; y++)
                {
                    rectROI.Y = y;
                    m_imgBufferImage.SubMat(RectangleToRect(rectROI)).CopyTo(imgLine);
                    Scalar sum = imgLine.Sum();
                    arrVerticalProfile[y - anStartY] = (float)sum.Val0 / (anEndX - anStartX);
                }

                return arrVerticalProfile;
            }
            catch
            {
                return null;
            }
        }

        // X축 기준의 Profile을 추출한다.
        public float[] GetHorizontalProfile(int anStartX, int anEndX, int anStartY, int anEndY)
        {
            try
            {
                if (m_imgBufferImage == null)
                    return null;

                int nWidth = anEndX - anStartX + 1;
                float[] arrHorizontalProfile = new float[nWidth];

                if (nWidth < 1 || nWidth > m_imgBufferImage.Width || anStartX < 0 || anStartX < 0 ||
                    anEndX < anStartX || anEndY < anStartY || anStartX > m_imgBufferImage.Width ||
                    anStartY > m_imgBufferImage.Height || anEndX > m_imgBufferImage.Width || anEndY > m_imgBufferImage.Height)
                    return null;

                Rectangle rectROI = new Rectangle(anStartX, anStartY, 1, anEndY - anStartY);
                Mat imgLine = new Mat(1, anEndY - anStartY, MatType.CV_8UC1);
                for (int x = anStartX; x <= anEndX; x++)
                {
                    rectROI.X = x;
                    m_imgBufferImage.SubMat(RectangleToRect(rectROI)).CopyTo(imgLine);
                    Scalar sum = imgLine.Sum();
                    arrHorizontalProfile[x - anStartX] = (float)sum.Val0;// / (anEndX - anStartX);
                }
                return arrHorizontalProfile;
            }
            catch
            {
                return null;
            }
        }
        public void DoProcessing(int anLowerThreshold, int anUpperThreshold, int anErodeIteration, int anDilateIteration)
        {
            m_imgEntireImage.CopyTo(m_imgBufferImage);
            //Mat tmp = m_imgBufferImage.SubMat(m_rcImageROI);
            Cv2.InRange(m_imgBufferImage.SubMat(m_rcImageROI), new Scalar(anLowerThreshold), new Scalar(anUpperThreshold), m_imgBufferImage.SubMat(m_rcImageROI));
            if (anErodeIteration > 0)
                Cv2.Erode(m_imgBufferImage.SubMat(m_rcImageROI), m_imgBufferImage.SubMat(m_rcImageROI), new Mat(), null, anErodeIteration);
            if (anDilateIteration > 0)
                Cv2.Dilate(m_imgBufferImage.SubMat(m_rcImageROI), m_imgBufferImage.SubMat(m_rcImageROI), new Mat(), null, anDilateIteration);

        }

        public static BitmapSource GetIndexed8BitmapSource(byte[] pixels, int pixelWidth, int pixelHeight)
        {
            if (pixels == null)
                return null;

            System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Indexed8;
            BitmapPalette palette = BitmapPalettes.Gray256;
            BitmapSource bitmapSource = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, pf, palette, pixels, pixelWidth);
            
            if (bitmapSource != null)
            {
                return bitmapSource;
            }
            else
            {
                return null;
            }
        }
        public BitmapSource GetImage()
        {
            int size = m_imgBufferImage.Rows * m_imgBufferImage.Cols * m_imgBufferImage.ElemSize();
            byte[] bytes = new byte[size];
            Marshal.Copy(m_imgBufferImage.Data, bytes, 0, size);
            return GetIndexed8BitmapSource(bytes, m_imgBufferImage.Cols, m_imgBufferImage.Rows) ;// OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(m_imgBufferImage);
        }
        public static BitmapSource LoadImage(string filepath)
        {
            return BitmapSourceConverter.ToBitmapSource(Cv2.ImRead(filepath, ImreadModes.Grayscale));
        }
        public bool SaveBS(string aszfilepath, BitmapSource bs)
        {
            try
            {
                BitmapHelper.BitmapSourceToCVImage(bs).ImWrite(aszfilepath);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #region by min algo
        private int MaxEntropy(int[] data)
        {
            // Implements Kapur-Sahoo-Wong (Maximum Entropy) thresholding method
            // Kapur J.N., Sahoo P.K., and Wong A.K.C. (1985) "A New Method for
            // Gray-Level Picture Thresholding Using the Entropy of the Histogram"
            // Graphical Models and Image Processing, 29(3): 273-285
            // M. Emre Celebi
            // 06.15.2007
            // Ported to ImageJ plugin by G.Landini from E Celebi's fourier_0.8 routines
            int threshold = -1;
            int ih, it;
            int first_bin;
            int last_bin;
            double tot_ent;  /* total entropy */
            double max_ent;  /* max entropy */
            double ent_back; /* entropy of the background pixels at a given threshold */
            double ent_obj;  /* entropy of the object pixels at a given threshold */
            double[] norm_histo = new double[256]; /* normalized histogram */
            double[] P1 = new double[256]; /* cumulative normalized histogram */
            double[] P2 = new double[256];

            double total = 0;
            for (ih = 0; ih < 256; ih++)
                total += data[ih];

            for (ih = 0; ih < 256; ih++)
                norm_histo[ih] = data[ih] / total;

            P1[0] = norm_histo[0];
            P2[0] = 1.0 - P1[0];
            for (ih = 1; ih < 256; ih++)
            {
                P1[ih] = P1[ih - 1] + norm_histo[ih];
                P2[ih] = 1.0 - P1[ih];
            }

            /* Determine the first non-zero bin */
            first_bin = 0;
            for (ih = 0; ih < 256; ih++)
            {
                if (!(Math.Abs(P1[ih]) < 2.220446049250313E-16))
                {
                    first_bin = ih;
                    break;
                }
            }

            /* Determine the last non-zero bin */
            last_bin = 255;
            for (ih = 255; ih >= first_bin; ih--)
            {
                if (!(Math.Abs(P2[ih]) < 2.220446049250313E-16))
                {
                    last_bin = ih;
                    break;
                }
            }

            // Calculate the total entropy each gray-level
            // and find the threshold that maximizes it 
            max_ent = Double.MinValue;

            for (it = first_bin; it <= last_bin; it++)
            {
                /* Entropy of the background pixels */
                ent_back = 0.0;
                for (ih = 0; ih <= it; ih++)
                {
                    if (data[ih] != 0)
                    {
                        ent_back -= (norm_histo[ih] / P1[it]) * Math.Log(norm_histo[ih] / P1[it]);
                    }
                }

                /* Entropy of the object pixels */
                ent_obj = 0.0;
                for (ih = it + 1; ih < 256; ih++)
                {
                    if (data[ih] != 0)
                    {
                        ent_obj -= (norm_histo[ih] / P2[it]) * Math.Log(norm_histo[ih] / P2[it]);
                    }
                }

                /* Total entropy */
                tot_ent = ent_back + ent_obj;

                // IJ.log(""+max_ent+"  "+tot_ent);
                if (max_ent < tot_ent)
                {
                    max_ent = tot_ent;
                    threshold = it;
                }
            }
            return threshold;
        }
        private int roofs(ref Mat roof, int x, int y, double th, int dir, int orgx, int orgy, int width, int height)
        {
            if (x <= 0 || y <= 0 || x >= width || y >= height)
                return 0;

            switch (dir)
            {
                case 1:
                    if (roof.At<float>(y, x + 1) > th && (y != orgy || x + 1 != orgx))
                    {
                        roof.Set<float>(y, x + 1, 0);
                        //roof.At<char>(y, x + 1) = 0;
                        roofs(ref roof, x + 1, y, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 7, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y, th, 8, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 2:
                    if (roof.At<float>(y + 1, x + 1) > th && (y + 1 != orgy || x + 1 != orgx))
                    {
                        roof.Set<float>(y + 1, x + 1, 0);
                        //roof.At<char>(y + 1, x + 1) = 0;
                        roofs(ref roof, x + 1, y + 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 7, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y + 1, th, 8, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 3:
                    if (roof.At<float>(y + 1, x) > th && (y + 1 != orgy || x != orgx))
                    {
                        roof.Set<float>(y + 1, x, 0);
                        //roof.At<char>(y + 1, x) = 0;
                        roofs(ref roof, x, y + 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 7, orgx, orgy, width, height);
                        roofs(ref roof, x, y + 1, th, 8, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 4:
                    if (roof.At<float>(y + 1, x - 1) > th && (y + 1 != orgy || x - 1 != orgx))
                    {
                        roof.Set<float>(y + 1, x - 1, 0);
                        //roof.At<float>(y + 1, x - 1) = 0;
                        roofs(ref roof, x - 1, y + 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 7, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y + 1, th, 8, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 5:
                    if (roof.At<float>(y, x - 1) > th && (y != orgy || x - 1 != orgx))
                    {
                        roof.Set<float>(y, x - 1, 0);
                        //roof.At<float>(y, x - 1) = 0;
                        roofs(ref roof, x - 1, y, th, 8, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y, th, 7, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 6:
                    if (roof.At<float>(y - 1, x - 1) > th && (y - 1 != orgy || x - 1 != orgx))
                    {
                        roof.Set<float>(y - 1, x - 1, 0);
                        //roof.At<float>(y - 1, x - 1) = 0;
                        roofs(ref roof, x - 1, y - 1, th, 8, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x - 1, y - 1, th, 7, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 7:
                    if (roof.At<float>(y - 1, x) > th && (y - 1 != orgy || x != orgx))
                    {
                        roof.Set<float>(y + 1, x, 0);
                        //roof.At<float>(y - 1, x) = 0;
                        roofs(ref roof, x, y - 1, th, 8, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x, y - 1, th, 7, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
                case 8:
                    if (roof.At<float>(y - 1, x + 1) > th && (y - 1 != orgy || x + 1 != orgx))
                    {
                        roof.Set<float>(y - 1, x + 1, 0);
                        //roof.At<float>(y - 1, x + 1) = 0;
                        roofs(ref roof, x + 1, y - 1, th, 8, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 1, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 2, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 3, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 4, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 5, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 6, orgx, orgy, width, height);
                        roofs(ref roof, x + 1, y - 1, th, 7, orgx, orgy, width, height);
                    }
                    else
                        return 0;
                    break;
            }
            return 0;
        }
        //getgradient
        private Mat GetGradient(Mat src)
        {
            Mat grad_x = new Mat(), grad_y = new Mat();
            Mat abs_grad_x = new Mat(), abs_grad_y = new Mat();

            int scale = 1;
            int delta = 0;
            int ddepth = (int)MatType.CV_32FC1;

            // Calculate the x and y gradients using Sobel operator
            Cv2.Sobel(src, grad_x, ddepth, 1, 0, 3, scale, delta, BorderTypes.Default);
            Cv2.ConvertScaleAbs(grad_x, abs_grad_x);

            Cv2.Sobel(src, grad_y, ddepth, 0, 1, 3, scale, delta, BorderTypes.Default);
            Cv2.ConvertScaleAbs(grad_y, abs_grad_y);

            // Combine the two gradients
            Mat grad = new Mat();
            Cv2.AddWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 0, grad);

            return grad;
        }
        public List<List<System.Drawing.Point>> SearchTemplateBasedImage(double afMinCorr, int cols, int rows)
        {
            List<List<System.Drawing.Point>> tmpPoint = new List<List<System.Drawing.Point>>();
            List<System.Drawing.Point> tmpPointrows = new List<System.Drawing.Point>();
            try
            {
                int nunitcnt = 0;
                int ncheckcnt = 0;
                System.Drawing.Size nResultSize = new System.Drawing.Size(m_imgBufferImage.Width - m_imgTemplateImage.Width + 1, m_imgBufferImage.Height - m_imgTemplateImage.Height + 1);
                Mat imgResult = new Mat(nResultSize.Height, nResultSize.Width, MatType.CV_32FC1);
                Mat grad1 = new Mat(), grad2 = new Mat();
                //grad1 = GetGradient(m_imgBufferImage);
                //grad2 = GetGradient(m_imgTemplateImage);
                Cv2.MatchTemplate(m_imgBufferImage, m_imgTemplateImage, imgResult, TemplateMatchModes.CCoeffNormed);
                Cv2.Normalize(imgResult, imgResult, 0, 0.8, NormTypes.MinMax);

                //Cv2.ImWrite("E:\\algotest\\match\\imgResult.bmp", imgResult * 255);
                for (int i = 0; i < imgResult.Height; i++)
                {
                    for (int j = 0; j < imgResult.Width; j++)
                    {
                        if (imgResult.At<float>(i, j) > afMinCorr)
                        {
                            tmpPointrows.Add(new System.Drawing.Point(j, i));
                            nunitcnt++;

                            System.Threading.Tasks.Parallel.For(1, 9, (z) =>
                            {
                                roofs(ref imgResult, j, i, afMinCorr, z, j, i, imgResult.Width, imgResult.Height);
                            });
                            j += m_imgTemplateImage.Width / 3;
                        }
                        else
                            imgResult.Set<float>(i, j, 0);

                        if (nunitcnt % cols == 0 && nunitcnt != ncheckcnt)
                        {
                            tmpPointrows = tmpPointrows.OrderBy(p => p.X).ToList();
                            //deep copy 필요
                            tmpPoint.Add(tmpPointrows.ConvertAll(o => new System.Drawing.Point(o.X, o.Y)));
                            tmpPointrows.Clear();
                            ncheckcnt = nunitcnt;
                            i += m_imgTemplateImage.Height / 3;
                        }
                    }
                }

                //sort x , y 순
                //Cv2.ImWrite("E:\\algotest\\match\\savemp.bmp", imgResult * 255);
            }
            catch (Exception e)
            {

            }
            return tmpPoint;
        }

        // Modified by hjkim - 26.03.25
        public System.Drawing.Point FindCenterOfSectionImage(BitmapSource bsrc)
        {
            System.Drawing.Point center = new System.Drawing.Point();

            int size = bsrc.PixelHeight * bsrc.PixelWidth;
            byte[] bytes = new byte[size];
            bsrc.CopyPixels(bytes, bsrc.PixelWidth, 0);
            Mat tmp = new Mat(bsrc.PixelHeight, bsrc.PixelWidth, MatType.CV_8UC1);
            tmp.SetArray(bytes);

            int[] ints = bytes.Select(x => (int)x).ToArray();
            int nentropythresh = MaxEntropy(ints);

            Mat edges = new Mat(tmp.Size(), MatType.CV_8UC1);
            //Mat mask = new Mat(tmp.Size(), MatType.CV_8UC1);
            //mask = tmp.Threshold(35, 255, ThresholdTypes.Binary);
            edges = tmp.Threshold(nentropythresh, 255, ThresholdTypes.Binary);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchyIndexes;
            Cv2.FindContours(
                edges,
                out contours,
                out hierarchyIndexes,
                RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            double area = 0;
            for (int i = 0; i < contours.Length; i++)
            {
                Moments moms = Cv2.Moments(contours[i]);
                if (area < moms.M00)
                {
                    area = moms.M00;
                    center.X = (int)(moms.M10 / moms.M00);
                    center.Y = (int)(moms.M01 / moms.M00);
                }
            }
            return center;
        }
        public BitmapSource GetSectionImage(List<List<System.Windows.Point>> lst, int rows, int cols)
        {
            List<List<OpenCvSharp.Point>> tmppoint = new List<List<OpenCvSharp.Point>>();
            for (int i = 0; i < lst.Count; i++)
            {
                tmppoint.Add(new List<OpenCvSharp.Point>());
                for (int j = 0; j < lst[i].Count; j++)
                    tmppoint[i].Add(new OpenCvSharp.Point(lst[i][j].X, lst[i][j].Y));
            }

            Mat tmpgbr = new Mat(rows, cols, MatType.CV_8UC1);
            tmpgbr.SetTo(255);
            for (int i = 0; i < tmppoint.Count; i++)
                Cv2.DrawContours(tmpgbr, tmppoint, i, new Scalar(0), -1);
            //Cv2.ImWrite("E:\\testgerber.bmp", tmpgbr);
            return BitmapSourceConverter.ToBitmapSource(tmpgbr);
        }

        // Modified by hjkim - 26.03.25
        public int WhichGerberisCollect(BitmapSource bsrc, List<List<System.Windows.Point>> lst, List<List<System.Windows.Point>> lst2, int rows, int cols)
        {
            int size = bsrc.PixelHeight * bsrc.PixelWidth;
            byte[] bytes = new byte[size];
            bsrc.CopyPixels(bytes, bsrc.PixelWidth, 0);
            Mat refimg = new Mat(bsrc.PixelHeight, bsrc.PixelWidth, MatType.CV_8UC1);
            refimg.SetArray(bytes);
            refimg = refimg.Threshold(55, 255, ThresholdTypes.Binary);

            //여기서 한번 더 ListToContour의 이미지 생성한 다음에 Template Match 하면 시작점이나오니까 거기서 template image의 중심점 더하면 센터가 된다.
            #region Create contour Image
            List<List<OpenCvSharp.Point>> tmppoint = new List<List<OpenCvSharp.Point>>();
            List<List<OpenCvSharp.Point>> tmppoint2 = new List<List<OpenCvSharp.Point>>();

            for (int i = 0; i < lst.Count; i++)
            {
                tmppoint.Add(new List<OpenCvSharp.Point>());
                for (int j = 0; j < lst[i].Count; j++)
                    tmppoint[i].Add(new OpenCvSharp.Point(lst[i][j].X, lst[i][j].Y));
            }

            Mat tmpgbr = new Mat(rows, cols, MatType.CV_8UC1);
            Mat tmpgbr2 = new Mat(rows, cols, MatType.CV_8UC1);

            tmpgbr.SetTo(255);
            for (int i = 0; i < tmppoint.Count; i++)
                Cv2.DrawContours(tmpgbr, tmppoint, i, new Scalar(0), -1);

            //만약 유닛2가 있다며언?
            if (lst2 != null)
            {
                for (int i = 0; i < lst2.Count; i++)
                {
                    tmppoint2.Add(new List<OpenCvSharp.Point>());
                    for (int j = 0; j < lst2[i].Count; j++)
                        tmppoint2[i].Add(new OpenCvSharp.Point(lst2[i][j].X, lst2[i][j].Y));
                }


                tmpgbr2.SetTo(255);
                for (int i = 0; i < tmppoint2.Count; i++)
                    Cv2.DrawContours(tmpgbr2, tmppoint2, i, new Scalar(0), -1);
            }
            #endregion


            OpenCvSharp.Size nResultSize = new OpenCvSharp.Size(refimg.Width - tmpgbr.Width + 1,
                                                                    refimg.Height - tmpgbr.Height + 1);

            Mat imgResult = new Mat(nResultSize, MatType.CV_32FC1);
            Mat imgResult2 = new Mat(nResultSize, MatType.CV_32FC1);
            Cv2.MatchTemplate(refimg, tmpgbr, imgResult, TemplateMatchModes.CCoeffNormed);

            //Cv2.ImWrite("E:\\refimg.bmp", refimg);
            //Cv2.ImWrite("E:\\tmpgbr.bmp", tmpgbr);
            double fMinVal, fMinVal2 = 1.0;
            double fMaxVal, fMaxVal2 = 0.0;
            OpenCvSharp.Point ptMinLocation, ptMinLocation2 = new OpenCvSharp.Point(0, 0);
            OpenCvSharp.Point ptMaxLocation, ptMaxLocation2 = new OpenCvSharp.Point(0, 0);
            Cv2.MinMaxLoc(imgResult, out fMinVal, out fMaxVal, out ptMinLocation, out ptMaxLocation);

            if (lst2 != null)
            {
                Cv2.MatchTemplate(refimg, tmpgbr2, imgResult2, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(imgResult2, out fMinVal2, out fMaxVal2, out ptMinLocation2, out ptMaxLocation2);
            }
            if (fMaxVal > fMaxVal2)
                return 1;
            else
                return 2;

        }

        // Modified by hjkim - 26.03.25
        public BitmapSource FindCenterOfSectionImageWithContour(int workType, BitmapSource bsrc, List<List<System.Windows.Point>> lst, int rows, int cols, out System.Drawing.Point ct)
        {
            System.Drawing.Point center = new System.Drawing.Point();
            int size = bsrc.PixelHeight * bsrc.PixelWidth;
            byte[] bytes = new byte[size];
            bsrc.CopyPixels(bytes, bsrc.PixelWidth, 0);
            Mat refimg = new Mat(bsrc.PixelHeight, bsrc.PixelWidth, MatType.CV_8UC1);
            refimg.SetArray(bytes);

            refimg = refimg.Threshold(55, 255, ThresholdTypes.Binary);
            //여기서 한번 더 ListToContour의 이미지 생성한 다음에 Template Match 하면 시작점이나오니까 거기서 template image의 중심점 더하면 센터가 된다.

            #region Create contour Image
            List<List<OpenCvSharp.Point>> tmppoint = new List<List<OpenCvSharp.Point>>();

            for (int i = 0; i < lst.Count; i++)
            {
                tmppoint.Add(new List<OpenCvSharp.Point>());
                for (int j = 0; j < lst[i].Count; j++)
                    tmppoint[i].Add(new OpenCvSharp.Point(lst[i][j].X, lst[i][j].Y));
            }

            Mat tmpgbr = new Mat(rows, cols, MatType.CV_8UC1);

            tmpgbr.SetTo(255);
            for (int i = 0; i < tmppoint.Count; i++)
                Cv2.DrawContours(tmpgbr, tmppoint, i, new Scalar(0), -1);
            #endregion

            OpenCvSharp.Size nResultSize = new OpenCvSharp.Size(refimg.Width - tmpgbr.Width + 1,
                                                                    refimg.Height - tmpgbr.Height + 1);

            Mat imgResult = new Mat(nResultSize, MatType.CV_32FC1);
            Cv2.MatchTemplate(refimg, tmpgbr, imgResult, TemplateMatchModes.CCoeffNormed);

            //Cv2.ImWrite("E:\\refimg.bmp", refimg);
            //Cv2.ImWrite("E:\\tmpgbr.bmp", tmpgbr);
            double fMinVal;
            double fMaxVal;
            OpenCvSharp.Point ptMinLocation;
            OpenCvSharp.Point ptMaxLocation;
            Cv2.MinMaxLoc(imgResult, out fMinVal, out fMaxVal, out ptMinLocation, out ptMaxLocation);

            if (fMaxVal > 0.8)
            {
                center.X = ptMaxLocation.X + tmpgbr.Width / 2;
                center.Y = ptMaxLocation.Y + tmpgbr.Height / 2;
                ct = center;// OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(tmpgbr);
            }
            else
            {
                ct = new System.Drawing.Point(0, 0);// OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(tmpgbr);
            }
            return BitmapSourceConverter.ToBitmapSource(tmpgbr);
        }
        public BitmapSource ListToContour(List<List<System.Windows.Point>> lst, int rows, int cols)
        {
            List<List<OpenCvSharp.Point>> tmppoint = new List<List<OpenCvSharp.Point>>();
            for (int i = 0; i < lst.Count; i++)
            {
                tmppoint.Add(new List<OpenCvSharp.Point>());
                for (int j = 0; j < lst[i].Count; j++)
                    tmppoint[i].Add(new OpenCvSharp.Point(lst[i][j].X, lst[i][j].Y));
            }
            m_imgGerberContours = new Mat(rows, cols, MatType.CV_8UC1);
            m_imgGerberContours.SetTo(0);
            for (int i = 0; i < tmppoint.Count; i++)
                Cv2.DrawContours(m_imgGerberContours, tmppoint, i, new Scalar(255), -1);
            return BitmapSourceConverter.ToBitmapSource(m_imgGerberContours);
        }
        public double CompareGerberANDSectionImage(BitmapSource refsection)
        {
            if (refsection.Height != m_imgGerberContours.Height || refsection.Width != m_imgGerberContours.Width)
                return 0.0;

            Mat refMat = new Mat((int)refsection.Height, (int)refsection.Width, MatType.CV_8UC1);
            Mat OutMat = new Mat((int)refsection.Height, (int)refsection.Width, MatType.CV_8UC1);
            refMat = BitmapSourceConverter.ToMat(refsection);
            refMat = refMat.Threshold(105, 255, ThresholdTypes.Binary);
            Mat element1 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            refMat.Erode(element1);
            refMat.Dilate(element1);
            //Cv2.ImWrite("E:\\refMat.bmp", refMat);
            Cv2.BitwiseAnd(m_imgGerberContours, refMat, OutMat);
            //Cv2.ImWrite("E:\\m_imgGerberContours.bmp", m_imgGerberContours);
            int ngerber = Cv2.CountNonZero(m_imgGerberContours);
            int nout = Cv2.CountNonZero(OutMat);
            double percent = (1.0 - ((double)nout / (double)ngerber)) * 100.0;
            //Cv2.ImWrite("E:\\OutMat" + percent.ToString() +".bmp", OutMat);
            return percent;
        }
        #endregion
        public bool Save(String aszFilePath)
        {
            m_imgBufferImage.ImWrite(aszFilePath);

            return true;
        }
    }
}
