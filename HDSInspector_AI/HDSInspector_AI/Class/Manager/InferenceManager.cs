using Common;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.GlobalFunctions;
using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{

    /// <summary>   추론관련 Process 및 Sequence 관리    </summary>
    /// <remarks>   hjkim, 2026-08-13.                   </remarks>

    public class InferenceManager : IDisposable
    {
        private devNeurocle _neurocle;
        private readonly DefectSpecManager _specManager;
        private readonly DefectJudgementEngine _judgeEngine;
        private readonly DefectImageCutter _imageCutter;
        private readonly DefectTextParser _textParser;

        public event Action<InferenceImageDisplayItem> InferenceImageReady;
        public event Action<StripInferenceResult> StripInferenceCompleted;
        public event Action<StripDefectData> DefectDataReady;

        public InferenceManager(DefectSpecManager specManager)
        {
            _specManager = specManager ?? throw new ArgumentNullException(nameof(specManager));
            _judgeEngine = new DefectJudgementEngine();
            _imageCutter = new DefectImageCutter();
            _textParser  = new DefectTextParser();
        }

        public devNeurocle Neurocle
        {
            get { return _neurocle; }
        }

        public void Dispose()
        {
            _neurocle?.Dispose();
        }

        public bool InitializeNeurocle()
        {
            if(_neurocle != null)
            {
                _neurocle.Dispose();
                _neurocle = null;
            }

            _neurocle = new devNeurocle(GLB.Setting.Neurocle.GpuIndex);

            List<NeurocleModelConfig> configs = new List<NeurocleModelConfig>
            {
                CreateModelConfig(InspectionCameraType.Top, GLB.Setting.Neurocle.Top),
                CreateModelConfig(InspectionCameraType.Bottom, GLB.Setting.Neurocle.Bottom),
                CreateModelConfig(InspectionCameraType.Trans, GLB.Setting.Neurocle.Trans)
            };

            return _neurocle.Initialize(configs);
        }

        private NeurocleModelConfig CreateModelConfig(InspectionCameraType cameraType, NeurocleCameraSetting setting)
        {
            return new NeurocleModelConfig
            {
                CameraType = cameraType,
                ClassificationModelPath = setting.ClassificationModelPath,
                ClassificationPredictorPath = setting.ClassificationPredictorPath,
                ClassificationBatchSize = setting.ClassificationBatchSize,

                SegmentationModelPath = setting.SegmentationModelPath,
                SegmentationPredictorPath = setting.SegmentationPredictorPath,
                SegmentationBatchSize = setting.SegmentationBatchSize,

                UseFP16 = setting.UseFP16
            };
        }

        private double GetResolution(InspectionCameraType cameraType)
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    return GLB.Setting.Inference.TopResolutionUmPerPixel;

                case InspectionCameraType.Bottom:
                    return GLB.Setting.Inference.BottomResolutionUmPerPixel;

                case InspectionCameraType.Trans:
                    return GLB.Setting.Inference.TransResolutionUmPerPixel;

                default:
                    return 0.0;
            }
        }

        private static DefectInferenceResult CreateUnknownResult(NeurocleInferenceInput input, string reason)
        {
            return new DefectInferenceResult
            {
                StripNumber = input.StripNumber,
                CameraType = input.CameraType,
                DefectIndex = input.DefectIndex,
                DefectClass = DefectClass.Unknown,
                Judgement = AIJudgement.Unknown,
                JudgementReason = reason
            };
        }

        /* 
         *  260818_hjkim
         *  << ProcessDefect 순서 >>
         * 
         *  Defect Pair 1개
         *         ↓
         *  NeurocleInference Input
         *         ↓
         *  Classification
         *         │
         *        조건 - 실패 → Unknown
         *         ↓
         *  DefectClass Top1, Top2
         *         ↓
         *  DefectSpecManager
         *        조건 - Confidence 낮음 → Unknown
         *        조건 - Margin 낮음 → Unknown
         *    Direct인 경우
         *        Particle - 양품
         *        Void = NG
         *        천공 = NG
         *    Size 측정의 경우
         *        SEG 해서 측정
         *         ↓
         *  DefectJudgementEngine : 여기서 바로 판정함      
         */
        public DefectInferenceResult ProcessDefect(NeurocleInferenceInput input)
        {
            if(input == null)
            {
                return new DefectInferenceResult
                {
                    Judgement = AIJudgement.Unknown,
                    JudgementReason = "Inference Input is null"
                };
            }

            if (_neurocle == null || !_neurocle.IsInitialized)
                return CompleteDefect(input, CreateUnknownResult(input, "Neurocle is not initialized"));

            /*
             * 1. Classification
             */
            ClassificationResult classification;
            bool classificationSuccess = _neurocle.Classification(input, out classification);
            if (!classificationSuccess || classification == null)
                return CompleteDefect(input, CreateUnknownResult(input, _neurocle.LastError));

            /*
             * 2. Spec 조회
             */
            DefectSpec spec = _specManager.GetSpec(input.CameraType, classification.DefectClass);
            if (spec == null)
            {
                DefectInferenceResult result = _judgeEngine.Judge(classification, null, null);

                return CompleteDefect(input, result);
            }

            /*
             * 3. Confidence / Margin 검사
             * 
             *    애매하면 Seg도 하지말자
             */
            if (classification.Top1Probability < spec.ClassificationThreshold || classification.ProbabilityMargin < spec.ClassificationMargin)
            {
                DefectInferenceResult result = _judgeEngine.Judge(classification, null, spec);

                return CompleteDefect(input, result);
            }

            /*
             * 4. Direct 판정 (particle / void / punch)
             *    얘네들은 Spec이고 자시고 있기만 해도 바로 불량임
             */
            if (spec.JudgeMethod == DefectJudgeMethod.Direct)
            {
                DefectInferenceResult result = _judgeEngine.Judge(classification, null, spec);

                return CompleteDefect(input, result);
            }

            /*
             * 5. Measurement 필요한 경우 (Seg로 할지 rule로 할지 하다가 전부 AI, Seg로하자 그냥)
             */
            double resolution = GetResolution(input.CameraType);

            SegmentationResult segmentation;
            bool segmantationSuccess = _neurocle.Segmentation(input, classification.DefectClass, resolution, out segmentation);
            if (!segmantationSuccess || segmentation == null)
            {
                segmentation = new SegmentationResult
                {
                    StripNumber = input.StripNumber,
                    CameraType = input.CameraType,
                    DefectIndex = input.DefectIndex,
                    DefectClass = classification.DefectClass,
                    Success = false,
                    ErrorMessage = _neurocle.LastError
                };
            }

            /*
             * 6. Spec 판정
             */
            DefectInferenceResult finalResult = _judgeEngine.Judge(classification, segmentation, spec);

            // UI에 띄우기
            return CompleteDefect(input, finalResult);
        }

        private DefectInferenceResult CompleteDefect(NeurocleInferenceInput input, DefectInferenceResult result)
        {
            RaiseInferenceImage(input, result);

            return result;
        }

        private void RaiseInferenceImage(NeurocleInferenceInput input, DefectInferenceResult result)
        {
            if (input == null || result == null)
                return;

            InferenceImageDisplayItem item = new InferenceImageDisplayItem
            {
                StripNumber = result.StripNumber,
                CameraType = result.CameraType,
                DefectIndex = result.DefectIndex,
                DefectClass = result.DefectClass,
                ClassName = result.ClassName,
                Judgement = result.Judgement,
                Probability = result.ClassificationProbability,
                MeasuredValueUm = result.MeasuredValueUm,
                DefectImage = input.DefectImage
            };

            InferenceImageReady?.Invoke(item);
        }

        private StripDefectData CreateDefectData(DefectImageFileSet fileSet)
        {
            StripDefectData data = new StripDefectData
            {
                StripNumber = fileSet.SequenceNumber
            };

            // Top
            if (fileSet.HasTopImage && fileSet.HasTopText)
                data.TopPairs = LoadCameraPairs(fileSet.TopImagePath, fileSet.TopTextPath, "TOP");

            // Bottom
            if (fileSet.HasBottomImage && fileSet.HasBottomText)
                data.BottomPairs = LoadCameraPairs(fileSet.BottomImagePath, fileSet.BottomTextPath, "BOTTOM");

            // Trans
            if (fileSet.HasTransImage && fileSet.HasTransText)
                data.TransPairs = LoadCameraPairs(fileSet.TransImagePath, fileSet.TransTextPath, "TRANS");

            return data;
        }

        private List<DefectImagePairItem> LoadCameraPairs(string imagePath, string textPath, string cameraName)
        {
            List<DefectImagePairItem> pairItems = new List<DefectImagePairItem>();

            // Text 불량 개수
            int defectCount;
            bool textSuccess = _textParser.TryGetDefectCount(textPath, out defectCount);

            if(!textSuccess)
            {
                GLB.AddLog("INFERENCE", $"{cameraName} Text 읽기 실패 : {textPath}", SeverityLevel.ERROR);

                return pairItems;
            }

            if (defectCount <= 0)
                return pairItems;

            // Merge PNG
            BitmapSource mergedImage = LoadBitmapWithoutLock(imagePath);

            bool cuttingSuccess = _imageCutter.CuttingImage(mergedImage, defectCount, out pairItems);
            if (!cuttingSuccess)
            {
                GLB.AddLog("INFERENCE", $"{cameraName} Cutting 실패 : {imagePath}", SeverityLevel.ERROR);

                return pairItems;
            }

            return pairItems;
        }

        private static BitmapSource LoadBitmapWithoutLock(string imagePath)
        {
            using(FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        public async Task ProcessFileSetAsync(DefectImageFileSet fileSet)
        {
            if (fileSet == null) return;

            try
            {
                StripDefectData defectData = await Task.Run(() => CreateDefectData(fileSet));

                // Uc_DefectImage에 전달해야함.
                DefectDataReady?.Invoke(defectData);
                GLB.AddLog("INFERENCE", $"Strip [{fileSet.SequenceNumber}] Pair 준비 완료, Top={defectData.TopPairs.Count}, Bottom={defectData.BottomPairs.Count}, Trans={defectData.TransPairs.Count}", SeverityLevel.INFO);
            }
            catch(Exception ex)
            {
                GLB.AddLog("INFERENCE", $"Defect Data 생성 실패. {ex.Message}", SeverityLevel.ERROR);
            }
        }
    }
}
