using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace HDSInspector_AI.Class.GlobalFunctions
{
    public class ImageProcessing
    {
        /// <summary>
        /// R, G, B 이미지 병합하여 컬러로 저장
        /// </summary>
        /// <param name="redChannel"></param>
        /// <param name="greenChannel"></param>
        /// <param name="blueChannel"></param>
        /// <returns></returns>
        /// <remarks>   hjkim,  26.04.13    </remarks>
        public bool ImageMerge(string redChannel, string greenChannel, string blueChannel)
        {
            if (!File.Exists(redChannel) || !File.Exists(greenChannel) || !File.Exists(blueChannel))
                return false;

            string outputDir = Path.GetDirectoryName("D:\\ftp\\Images\\Detection_AI\\SourceImages");

            if(!Directory.Exists(outputDir) && !string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            Mat red = null, green = null, blue = null, merged = null;

            try
            {
                red = Cv2.ImRead(redChannel, ImreadModes.Grayscale);
                green = Cv2.ImRead(greenChannel, ImreadModes.Grayscale);
                blue = Cv2.ImRead(blueChannel, ImreadModes.Grayscale);

                if (red.Empty() || green.Empty() || blue.Empty())
                    return false;

                var size = red.Size();
                if (green.Size() != size || blue.Size() != size)
                    throw new InvalidOperationException("모든 채널의 해상도가 일치해야 함");

                merged = new Mat();
                Cv2.Merge(new[] { blue, green, red }, merged);
                merged.SaveImage("D:\\ftp\\Images\\Detection_AI\\SourceImages\\Merged.bmp");
                return true;
            }
            catch(Exception ex)
            {

            }

            return true;
        }

       

        public List<Rect> NonMaxSuppression(List<Rect> boxes, float overlapThresh, Mat MatchResult)
        {
            if (boxes.Count == 0) return new List<Rect>();

            var boxesArray = boxes.ToArray();
            List<Rect> pick = new List<Rect>();

            int[] x1 = boxesArray.Select(box => box.Left).ToArray();
            int[] y1 = boxesArray.Select(box => box.Top).ToArray();
            int[] x2 = boxesArray.Select(box => box.Right).ToArray();
            int[] y2 = boxesArray.Select(box => box.Bottom).ToArray();
            double[] area = boxesArray.Select(box => (double)(box.Width + 1) * (box.Height + 1)).ToArray();
            float[] matchValues = boxesArray.Select(box => MatchResult.At<float>(box.Top, box.Left)).ToArray();

            // 좌표 정렬로 비교 범위 줄이기
            int[] idxs = Enumerable.Range(0, boxesArray.Length).OrderBy(i => x1[i]).ToArray();

            // 병렬 처리 가능한 버전
            Parallel.ForEach(idxs, (i, state) =>
            {
                List<int> suppress = new List<int>();
                int maxMatchIndex = i;
                //Console.WriteLine($"Parallel : {i}");

                for (int j = 0; j < idxs.Length; j++)
                {
                    if (i == j) continue;

                    // 빠른 체크로 비교 스킵
                    if (x2[i] < x1[j] || x2[j] < x1[i] || y2[i] < y1[j] || y2[j] < y1[i])
                        continue;

                    // IoU 계산
                    int xx1 = Math.Max(x1[i], x1[j]);
                    int yy1 = Math.Max(y1[i], y1[j]);
                    int xx2 = Math.Min(x2[i], x2[j]);
                    int yy2 = Math.Min(y2[i], y2[j]);
                    int w = Math.Max(0, xx2 - xx1 + 1);
                    int h = Math.Max(0, yy2 - yy1 + 1);
                    double interArea = w * h;
                    double overlap = interArea / (area[j] + area[i] - interArea);

                    if (overlap > overlapThresh)
                    {
                        suppress.Add(j);
                        if (matchValues[j] > matchValues[maxMatchIndex])
                        {
                            maxMatchIndex = j;
                        }
                    }
                }

                // 최적의 박스 선택
                lock (pick)
                {
                    if (!pick.Contains(boxesArray[maxMatchIndex]))
                        pick.Add(boxesArray[maxMatchIndex]);
                }
            });

            return pick;
        }

        public List<Rect> MatchTemplateMulti(
            string imagePath,
            string templatePath,
            double threshold = 0.7,
            double nmsIoU = 0.3)
        {
            Mat imgSrc = Cv2.ImRead(imagePath, ImreadModes.Color);
            Mat tmplSrc = Cv2.ImRead(templatePath, ImreadModes.Color);

            if (imgSrc.Empty() || tmplSrc.Empty())
                throw new Exception("이미지를 로드할 수 없습니다.");

            // 1.8G 이미지 그대로 매칭하면 진짜 엄청 느린데다가 간헐적 VS에서 지원되는 배열 범위 밖으로 벗어남
            Cv2.Resize(imgSrc, imgSrc, new OpenCvSharp.Size(imgSrc.Cols / 4, imgSrc.Rows / 4));
            Cv2.Resize(tmplSrc, tmplSrc, new OpenCvSharp.Size(tmplSrc.Cols / 4, tmplSrc.Rows / 4));

            // 그레이스케일 변환
            Mat imgGray = new Mat();
            Mat tmplGray = new Mat();
            Cv2.CvtColor(imgSrc, imgGray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(tmplSrc, tmplGray, ColorConversionCodes.BGR2GRAY);

            int h = tmplGray.Rows;
            int w = tmplGray.Cols;

            // 매칭 수행
            Mat result = new Mat();
            Cv2.MatchTemplate(imgGray, tmplGray, result, TemplateMatchModes.CCorrNormed);

            // 임계값 기준 필터링
            ConcurrentBag<OpenCvSharp.Point> locations = new ConcurrentBag<Point>();
            Parallel.For(0, result.Rows, y =>
            {
                for (int x = 0; x < result.Cols; x++)
                {
                    if (result.At<float>(y, x) >= 0.95)
                        locations.Add(new Point(x, y));
                }
            });

            // 박스 및 점수 수집
            var boxes = new List<Rect>();

            foreach (var pt in locations)
                boxes.Add(new Rect(pt.X, pt.Y, tmplGray.Cols, tmplGray.Rows));

            // NMS 적용
            boxes = NonMaxSuppression(boxes, 0.2f, result);

            var scaledBoxes = new List<Rect>();
            foreach (var box in boxes)
            {
                scaledBoxes.Add(new Rect(
                    box.X * 4,
                    box.Y * 4,
                    box.Width * 4,
                    box.Height * 4
                ));
            }

            imgSrc?.Dispose();
            imgGray?.Dispose();
            tmplSrc?.Dispose();
            tmplGray?.Dispose();
            result?.Dispose();

            return scaledBoxes;
        }

        public List<string> ExtractUnits(
            string imagePath,
            string templatePath,
            string outputDir = "D:\\ftp\\Images\\Detection_AI\\SourceImages\\matched_units",
            double threshold = 0.7,
            double nmsIoU = 0.2,
            bool verbose = true)
        {
            var boxes = MatchTemplateMulti(imagePath, templatePath, threshold, nmsIoU);

            if (boxes.Count == 0)
            {
                if (verbose) Console.WriteLine("No matching units were found.");
                return new List<string>();
            }

            if (verbose)
                Console.WriteLine($"Detected {boxes.Count} unit(s). Extracting...");

            // 출력 디렉터리 생성
            System.IO.Directory.CreateDirectory(outputDir);

            var savedFiles = new List<string>();
            string baseName = System.IO.Path.GetFileNameWithoutExtension(imagePath);

            Mat src = Cv2.ImRead(imagePath, ImreadModes.Color);
            List<Mat> cropImages = new List<Mat>();

            for (int i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                int x = box.X;
                int y = box.Y;
                int width = box.Width;
                int height = box.Height;

                // 클리핑
                int x1 = Math.Max(0, x);
                int y1 = Math.Max(0, y);
                int x2 = Math.Min(src.Cols, x + width);
                int y2 = Math.Min(src.Rows, y + height);

                int cropWidth = x2 - x1;
                int cropHeight = y2 - y1;

                if (cropWidth <= 0 || cropHeight <= 0) continue;

                Mat crop = new Mat(src, new OpenCvSharp.Rect(x1, y1, cropWidth, cropHeight));
                //string outputPath = System.IO.Path.Combine(outputDir, $"{baseName}_unit_{i + 1:000}.png");
                //Cv2.ImWrite(outputPath, crop);
                //savedFiles.Add(outputPath);

                cropImages.Add(crop.Clone());

                crop?.Dispose();
            }

            src?.Dispose();

            if (verbose)
                Console.WriteLine($"[+] Extraction finished. {savedFiles.Count} files saved to '{outputDir}'.");

            return savedFiles;

        }
        public static BitmapSource ApplyErosion(BitmapSource bitmapSource)
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장

            Mat dst = new Mat(); //결과 이미지를 dst 변수에 저장

            Mat kernel = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new OpenCvSharp.Size(3, 3));

            //erosion 필터 생성
            
            Cv2.Erode(src, dst, kernel); //erosion 수행

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환
        }

        public static BitmapSource ApplyCanny(BitmapSource bitmapSource)
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장

            Mat dst = new Mat(); //결과 이미지를 dst 변수에 저장

            Cv2.Canny(src, dst, 50, 150, 3, true);//CannyEdge 수행(입력,출력,하위 임계, 상위 임계, 소벨 마스크 크기, gradient)

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환
        }

        public static BitmapSource ApplySobel(BitmapSource bitmapSource)
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장
            Mat gradX = new Mat();
            Mat gradY = new Mat();
            Mat absGradX = new Mat();
            Mat absGradY = new Mat();
            Mat dst = new Mat(); //결과 이미지를 dst 변수에 저장

            // 2. 가로(X) 방향 소벨 에지 검출
            Cv2.Sobel(src, gradX, MatType.CV_16S, 1, 0, 3); //(입력, x결과 저장, _,x방향 미분, y방향 미분, 커널 크기)
            Cv2.ConvertScaleAbs(gradX, absGradX);

            // 3. 세로(Y) 방향 소벨 에지 검출
            Cv2.Sobel(src, gradY, MatType.CV_16S, 0, 1, 3);
            Cv2.ConvertScaleAbs(gradY, absGradY);

            // 4. X방향과 Y방향 에지 이미지 합성 (가중치 0.5)
            Cv2.AddWeighted(absGradX, 0.5, absGradY, 0.5, 0, dst);

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환
        }


        public static BitmapSource ApplyContrast(BitmapSource bitmapSource)
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장

            Mat dst = new Mat(); //결과 이미지를 dst 변수에 저장

            Cv2.Normalize(src, dst, 0, 255, NormTypes.MinMax);

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환

        }

        public static BitmapSource ApplyDilation(BitmapSource bitmapSource)
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장

            Mat dst = new Mat(); //결과 이미지를 dst 변수에 저장

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));

            Cv2.Dilate(src, dst, kernel, iterations: 1);//팽창 연산 수행

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환

        }

        public static BitmapSource ApplyClahe(BitmapSource bitmapSource) //컬러 이미지
        {
            Mat src = bitmapSource.ToMat(); //opencv에서 이미지 데이터 저장하려면 Mat 클래스 사용. 여기서는 원본 이미지를 src에 저장

            Mat lab = new Mat(); //결과 이미지를 dst 변수에 저장

            Cv2.CvtColor(src, lab, ColorConversionCodes.BGR2Lab);

            Mat[] labPlanes = Cv2.Split(lab);
            Mat lChannel = labPlanes[0];

            CLAHE clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new OpenCvSharp.Size(8, 8));

            Mat dstL = new Mat();
            clahe.Apply(lChannel, dstL);

            // 6. 처리된 밝기 채널을 원래의 A, B 채널과 병합
            dstL.CopyTo(labPlanes[0]);
            Cv2.Merge(labPlanes, lab);

            Mat dst = new Mat();
            Cv2.CvtColor(lab, dst, ColorConversionCodes.Lab2BGR);

            return dst.ToBitmapSource(); //결과 이미지를 다시 원본 형태로 변환하여 반환

        }

        public static BitmapSource ApplyResize(BitmapSource bitmapSource)
        {
            double width = bitmapSource.PixelWidth;
            double height = bitmapSource.PixelHeight;

            double newWidth = width /4;
            double newHeight = height/4;

            int newWidthInt = (int)Math.Round(newWidth);
            int newHeightInt = (int)Math.Round(newHeight);

            var resizedBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(newWidthInt / width, newHeightInt / height));
            return resizedBitmap;
        }
        public static BitmapSource ApplyExtract(BitmapSource bitmapSource, int threshold = 40) //threshold값 임의 설정
        //minboundaryX, maxboundaryX는 각각 축소된 이미지 기준으로 설정됨
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = (width * bitmapSource.Format.BitsPerPixel + 7) / 8;
            int bytesPerPixel = bitmapSource.Format.BitsPerPixel / 8;
            int minBoundaryX = width / 9; //임의 설정
            int maxBoundaryX = (int)(width / 1.1); //임의 설정

            byte[] pixelBuffer = new byte[height * stride];
            bitmapSource.CopyPixels(pixelBuffer, stride, 0);

            // 전체 라인 스캔 후 최솟값으로 시작점, 끝점 설정
            int sampleCount = height;
            var startXList = new List<int>();
            var endXList = new List<int>();

            for (int i = 1; i <= sampleCount; i++)
            {
                int scanY = height * i / (sampleCount + 1);
                int baseOffset = scanY * stride;

                byte[] lineGV = new byte[width];
                for (int x = 0; x < width; x++)
                {
                    int offset = baseOffset + x * bytesPerPixel;
                    lineGV[x] = (byte)(
                        pixelBuffer[offset + 2] * 0.299 +
                        pixelBuffer[offset + 1] * 0.587 +
                        pixelBuffer[offset + 0] * 0.114);
                }

                // 왼쪽 → 오른쪽 : 최소 시작점 이후로 탐색
                for (int x = minBoundaryX; x < width; x++)
                {
                    if (Math.Abs(lineGV[x] - lineGV[x - 1]) >= threshold)
                    {
                        startXList.Add(x);
                        break;
                    }
                }

                // 오른쪽 → 왼쪽 : 제품 종료 X
                for (int x = maxBoundaryX; x > 0; x--)
                {
                    if (Math.Abs(lineGV[x] - lineGV[x - 1]) >= threshold)
                    {
                        endXList.Add(x);
                        break;
                    }
                }
            }

            if (startXList.Count == 0 || endXList.Count == 0)
                return bitmapSource;

            // 최소값, 최댓값 사용
            int startX = startXList.Min();
            int endX = endXList.Max();

            // 같은 방식으로 startY/endY 결정(색상 균일해서 최소 최대만 바로 추출)
            var startYList = new List<int>();
            var endYList = new List<int>();

            for (int i = 1; i <= sampleCount; i++)
            {
                int scanX = width * i / (sampleCount + 1);

                byte[] colGV = new byte[height];
                for (int y = 0; y < height; y++)
                {
                    int offset = y * stride + scanX * bytesPerPixel;
                    colGV[y] = (byte)(
                        pixelBuffer[offset + 2] * 0.299 +
                        pixelBuffer[offset + 1] * 0.587 +
                        pixelBuffer[offset + 0] * 0.114);
                }

                // 위 → 아래 : 제품 시작 Y
                for (int y = 1; y < height; y++)
                {
                    if (Math.Abs(colGV[y] - colGV[y - 1]) >= threshold)
                    {
                        startYList.Add(y);
                        break;
                    }
                }

                // 아래 → 위 : 제품 종료 Y
                for (int y = height - 1; y > 0; y--)
                {
                    if (Math.Abs(colGV[y] - colGV[y - 1]) >= threshold)
                    {
                        endYList.Add(y);
                        break;
                    }
                }
            }

            if (startYList.Count == 0 || endYList.Count == 0)
                return bitmapSource;

            int startY = startYList.Min();
            int endY = endYList.Max();

            if (endX <= startX || endY <= startY)
                return bitmapSource;

            var cropRect = new Int32Rect(startX, startY, endX - startX, endY - startY);
            return new CroppedBitmap(bitmapSource, cropRect);
        }

        ///public static BitmapSource ApplyColormode(BitmapSource bitmapSource)
        ///{
        ///    _ = new BitmapImage();
        ///    bitmap.BeginInit();
        ///
        ///
        ///}
    }
}
