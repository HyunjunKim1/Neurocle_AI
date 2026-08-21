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
        private readonly InferenceOutputWriter _outputWriter;

        private readonly object _stripLock = new object();
        private readonly HashSet<int> _processingStrips = new HashSet<int>();

        public event Action<InferenceImageDisplayItem> InferenceImageReady;
        public event Action<StripInferenceResult> StripInferenceCompleted;
        public event Action<StripDefectData> DefectDataReady;

        public bool InspectionEnabled { get; private set; }

        public InferenceManager(DefectSpecManager specManager)
        {
            _specManager = specManager ?? throw new ArgumentNullException(nameof(specManager));
            _judgeEngine = new DefectJudgementEngine();
            _imageCutter = new DefectImageCutter();
            _textParser  = new DefectTextParser();
            _outputWriter = new InferenceOutputWriter();
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

        public void SetInspectionState(bool enabled)
        {
            InspectionEnabled = enabled;

            GLB.AddLog("INFERENCE", enabled ? "Inspection Enabled" : "Inspection Disabled", SeverityLevel.INFO);
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

        private static DefectInferenceResult CreateUnknownResult(NeurocleInferenceInput input, string reason, ClassificationResult classifcation)
        {
            return new DefectInferenceResult
            {
                StripNumber = input.StripNumber,
                CameraType = input.CameraType,
                DefectIndex = input.DefectIndex,
                DefectClass = classifcation != null ? classifcation.DefectClass : DefectClass.Unknown,
                ClassName = classifcation.ClassName,
                ClassificationProbability = classifcation?.Top1Probability ?? 0.0f,
                ClassificationMargin = classifcation?.ProbabilityMargin ?? 0.0f,
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

            // Simulation일 경우 별도로 동작
            if (GLB.Setting.General.Simulation)
                return ProcessDefectSimulation(input);

            if (_neurocle == null || !_neurocle.IsInitialized)
                return CompleteDefect(input, CreateUnknownResult(input, "Neurocle is not initialized"));

            /*
             * 1. Classification
             */
            ClassificationResult classification;
            bool classificationSuccess = _neurocle.Classification(input, out classification);
            if (!classificationSuccess || classification == null || !classification.Success)
                return CompleteDefect(input, CreateUnknownResult(input, _neurocle.LastError));

            if (classification.DefectClass == DefectClass.Unknown)
                return CompleteDefect(input, CreateUnknownResult(input, $"Unknown Class : {classification.ClassName}", classification));
            /*
             * 2. Spec 조회
             */
            DefectSpec spec = _specManager.GetSpec(input.CameraType, classification.DefectClass);
            if (spec == null)
                return CompleteDefect(input, _judgeEngine.Judge(classification, null, null));

            /*
             * 3. Confidence / Margin 검사
             * 
             *    애매하면 Seg도 하지말자
             */
            if (classification.Top1Probability < spec.ClassificationThreshold)
                return CompleteDefect(input, CreateUnknownResult(input, $"Classification confidence is low - {classification.Top1Probability:F3}", classification));

            if (classification.ProbabilityMargin < spec.ClassificationMargin)
                return CompleteDefect(input, CreateUnknownResult(input, $"Classification margin is low - {classification.ProbabilityMargin:F3}", classification));

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
            if(spec.JudgeMethod == DefectJudgeMethod.OverflowDistance)
            {
                DefectInferenceResult result = new DefectInferenceResult
                {
                    StripNumber = input.StripNumber,
                    CameraType = input.CameraType,
                    DefectIndex = input.DefectIndex,
                    DefectClass = classification.DefectClass,
                    ClassName = classification.ClassName,
                    ClassificationProbability = classification.Top1Probability,
                    ClassificationMargin = classification.ProbabilityMargin,
                    Judgement = AIJudgement.Unknown,
                    JudgementReason = "Flash measurement is not implmented yet"
                };

                return CompleteDefect(input, result);
            }
            double resolution = GetResolution(input.CameraType);

            SegmentationResult segmentation;
            bool segmantationSuccess = _neurocle.Segmentation(input, classification.DefectClass, resolution, out segmentation);
            if (!segmantationSuccess || segmentation == null || !segmentation.Success)
                return CompleteDefect(input, CreateUnknownResult(input, _neurocle.LastError ?? segmentation?.ErrorMessage ?? "Segmentation failed", classification));

            /*
             * 6. Spec 판정
             */
            DefectInferenceResult finalResult = _judgeEngine.Judge(classification, segmentation, spec);

            // UI에 띄우기
            return CompleteDefect(input, finalResult);
        }

        private DefectInferenceResult ProcessDefectSimulation(NeurocleInferenceInput input)
        {
            AIJudgement judgement;
            DefectClass defectClass;
            float probability;

            /*
             * Simulation용으로 하나 만들자
             * Index 기준으로 OK / NG / Unknown을 반복적으로 생성
             * 
             * 0 → OK
             * 1 → NG
             * 2 → Unknown
             * 3 → OK
             * .....
             */
            int simulationType = input.DefectIndex % 3;

            switch(simulationType)
            {
                case 0:
                    judgement = AIJudgement.OK;
                    defectClass = DefectClass.Particle;
                    probability = 0.98f;
                    break;

                case 1:
                    judgement = AIJudgement.NG;
                    defectClass = DefectClass.Contaminant;
                    probability = 0.96f;
                    break;

                default:
                    judgement = AIJudgement.Unknown;
                    defectClass = DefectClass.UnderEtching;
                    probability = 0.72f;
                    break;
            }

            DefectInferenceResult result = new DefectInferenceResult
            {
                StripNumber = input.StripNumber,
                CameraType = input.CameraType,
                DefectIndex = input.DefectIndex,
                DefectClass = defectClass,
                ClassName = defectClass.ToString(),
                ClassificationProbability = probability,
                ClassificationMargin = judgement == AIJudgement.Unknown ? 0.05f : 0.80f,
                SegmentationExecuted = false,
                Judgement = judgement,
                JudgementReason = "SIMULATION"
            };

            return CompleteDefect(input, result);
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

        public async Task<StripInferenceResult> ProcessFileSetAsync(DefectImageFileSet fileSet)
        {
            if (fileSet == null) return null;

            if (!InspectionEnabled)
            {
                GLB.AddLog("INFERENCE", $"Inspection disabled. Strip ignored : {fileSet.SequenceNumber:D6}", SeverityLevel.WARN);

                return null;
            }

            lock(_stripLock)
            {
                if (_processingStrips.Contains(fileSet.SequenceNumber))
                {
                    GLB.AddLog("INFERENCE", $"Duplicate strip ignored : {fileSet.SequenceNumber:D6}", SeverityLevel.WARN);

                    return null;
                }
                _processingStrips.Add(fileSet.SequenceNumber);
            }

            StripInferenceResult stripResult = new StripInferenceResult { StripNumber = fileSet.SequenceNumber, Status = StripInferenceStatus.None };
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            try
            {
                /*
                 * 1. Text + Merge PNG를 REF / DEF Pair로 변경
                 */
                StripDefectData defectData = await Task.Run(() => CreateDefectData(fileSet));

                /*
                 * 2. Uc_DefectImage에 전달
                 */
                DefectDataReady?.Invoke(defectData);

                if(defectData.TotalCount == 0)
                {
                    GLB.AddLog("INFERENCE", $"Strip [{fileSet.SequenceNumber:D6}] Vision Defect 없어서 AI 검사 대상 없음.", SeverityLevel.INFO);
                }

                GLB.AddLog("INFERENCE", $"Strip [{fileSet.SequenceNumber}] Pair 준비 완료, Top={defectData.TopPairs.Count}, Bottom={defectData.BottomPairs.Count}, Trans={defectData.TransPairs.Count}", SeverityLevel.INFO);

                /*
                 * 3. 각 카메라 별 추론
                 */

                List<NeurocleInferenceInput> topInputs = CreateCameraInputs(fileSet.SequenceNumber, InspectionCameraType.Top, defectData.TopPairs);
                List<NeurocleInferenceInput> bottomInputs = CreateCameraInputs(fileSet.SequenceNumber, InspectionCameraType.Bottom, defectData.BottomPairs);
                List<NeurocleInferenceInput> transInputs = CreateCameraInputs(fileSet.SequenceNumber, InspectionCameraType.Trans, defectData.TransPairs);

                await ProcessCameraAsync(topInputs, stripResult);
                await ProcessCameraAsync(bottomInputs, stripResult);
                await ProcessCameraAsync(transInputs, stripResult);

                stripResult.Status = StripInferenceStatus.Success;

                string outputError;

                bool outputSuccess = _outputWriter.SaveUnknownResults(fileSet, defectData, stripResult, out outputError);

                if (!outputSuccess)
                {
                    stripResult.Status = StripInferenceStatus.Failed;
                    stripResult.ErrorMessage = outputError;
                    throw new Exception($"Unknown 결과 저장 실패 : {outputError}");
                }

                GLB.AddLog("INFERENCE", $"Strip [{stripResult.StripNumber:D6}] Unknown Result 저장 완료.  {stripResult.UnknownCount}", SeverityLevel.INFO);
            }
            catch(Exception ex)
            {
                stripResult.Status = StripInferenceStatus.Failed;
                stripResult.ErrorMessage = ex.Message;
                GLB.AddLog("INFERENCE", $"Strip [{fileSet.SequenceNumber}] 처리 실패 : {ex.Message}", SeverityLevel.ERROR);
            }
            finally
            {
                stopwatch.Stop();
                stripResult.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

                lock (_stripLock)
                {
                    _processingStrips.Remove(fileSet.SequenceNumber);
                }
            }

            if (stripResult.Status == StripInferenceStatus.Success)
            {
                // DB 저장
                if(GLB.Database != null && GLB.Database.IsInitialized)
                {
                    bool dbSuccess = GLB.Database.SaveInspectionResult(GLB.DefectImage.CurrentInfo, stripResult);

                    if (!dbSuccess)
                    {
                        GLB.AddLog("DATABASE", $"Strip DB 저장 실패 : {GLB.Database.LastError}", SeverityLevel.ERROR);
                    }
                }

                /*
                 * 4. 각 결과값 누적 저장
                 */
                GLB.InferenceStatistics.AddStripResult(stripResult);

                /*
                 * 5. Strip 추론 완료 Event
                 */
                StripInferenceCompleted?.Invoke(stripResult);

                // Main S/W에 추론 완료 통보
                GLB.Client.SendInferenceDone(stripResult.StripNumber);
                GLB.AddLog("INFERENCE", $"Strip [{stripResult.StripNumber}] 완료. Total={stripResult.TotalCount}, OK={stripResult.OKCount}, NG={stripResult.NGCount}, Unknown={stripResult.UnknownCount}, Time={stripResult.ProcessingTimeMs}", SeverityLevel.INFO);
            }

            return stripResult;
        }

        private async Task ProcessCameraAsync(IList<NeurocleInferenceInput> inputs, StripInferenceResult stripResult)
        {
            if(inputs == null || inputs.Count == 0) return;

            foreach(NeurocleInferenceInput input in inputs)
            {
                if (input == null) continue;

                DefectInferenceResult result = await Task.Run(() => ProcessDefect(input));

                if (result == null)
                {
                    result = CreateUnknownResult(input, "Inference result is null");

                    RaiseInferenceImage(input, result);
                }

                stripResult.Results.Add(result);  
            }
        }

        private List<NeurocleInferenceInput> CreateCameraInputs(int stripNumber, InspectionCameraType cameraType, IList<DefectImagePairItem> pairItems)
        {
            List<NeurocleInferenceInput> inputs = new List<NeurocleInferenceInput>();
            if (pairItems == null || pairItems.Count == 0)
                return inputs;

            foreach(DefectImagePairItem pair in pairItems)
            {
                if(pair == null) continue;
                inputs.Add(new NeurocleInferenceInput
                {
                    StripNumber = stripNumber,
                    CameraType = cameraType,
                    DefectIndex = pair.index,
                    ReferenceImage = pair.ReferenceImage,
                    DefectImage = pair.DefectImage
                });
            }

            return inputs;
        }
    }
}
