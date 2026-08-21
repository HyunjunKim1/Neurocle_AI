using HDSInspector_AI.Class.Models;
using HDSInspector_AI.GUI.Windows.Popup;
using nrt;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Devices
{
    /// <summary>   Easy for use neuro-r functions. </summary>
    /// <remarks>   hjkim, 2026-03-26.              </remarks>
    public class devNeurocle : IDisposable
    {
        #region Old
        //// GPU
        //private Device _device;
        //
        //private Predictor _classificationPredictor;
        //private Predictor _segmentationPredictor;
        //
        //private bool _initialized;
        //private bool _disposed;
        //
        //public bool Initialized
        //{
        //    get { return _initialized; }
        //}
        //
        //public string LastError
        //{
        //    get;
        //    private set;
        //}
        //
        //public int DeviceIndex
        //{ 
        //    get; 
        //    private set; 
        //}
        //
        //private nrt.Status _status;
        //private string _modelPath = "~~.net";
        //private string _predictorPath = "~~.nrpd";
        //static List<string> clf_infFiles = new List<string>();
        //static List<string> seg_infFiles = new List<string>();
        //
        //public nrt.Status Status    { get { return _status; } set { _status = value; } }
        //public string ModelPath     { get { return _modelPath; } set { _modelPath = value; } }
        //public string PredictorPath { get { return _predictorPath; } set { _predictorPath = value; } }
        //
        //public devNeurocle(int deviceIdx = 0) { DeviceIndex = deviceIdx; }
        //
        //
        //#region Classification
        //
        //public bool InitializeClassification(string modelPath, string predictorPath, int batchSize = 64, bool useFp16 = false)
        //{
        //    LastError = null;
        //
        //    try
        //    {
        //        if(string.IsNullOrWhiteSpace(modelPath))
        //        {
        //            LastError = "Classification Model Path가 비어있습니다.";
        //
        //            return false;
        //        }
        //
        //        // GPU Device는 초기화때 한번만 획득하자
        //        _device = Device.get_gpu_device(DeviceIndex);
        //
        //        // 일단 Predictor 파일을 재사용 할수 있다면..
        //        // 향후 .nrpd 직접 Load하는 방식으로 분기하면 좋을듯
        //        // 현재 API 확인 범위에서는 기존 Model 생성 방식을유지하자
        //        Predictor predictor = new Predictor(modelPath, Model.MODELIO_OUT_CAM, _device.id, batch_size: batchSize, fp16_flag: useFp16, threshold_flag: false, DevType.DEVICE_CUDA_GPU);
        //        
        //        if(predictor.get_status() != Status.STATUS_SUCCESS)
        //        {
        //            LastError = $"Predictor 초기화 실패 : {nrt.nrt.get_last_error_msg()}";
        //            return false;
        //        }
        //        _classificationPredictor = predictor;
        //
        //        // Predictor 저장은 매 추론마다 하지 ㅇ낳고 Initialize 시점에 한번만함.
        //        if (!string.IsNullOrWhiteSpace(predictorPath))
        //        {
        //            Status saveStatus = predictor.save_predictor(predictorPath);
        //
        //            if(saveStatus != Status.STATUS_SUCCESS)
        //                // 저장실패가 추론 불가와 동일한건 아니라 Predictor 자체는 유지할 수도 있음
        //                LastError = $"Predictor 저장 실패 : {nrt.nrt.get_last_error_msg()}"; 
        //
        //        }
        //        _initialized = true;
        //
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        LastError = $"Neurocle 초기화 오류 : {ex.Message}";
        //        _initialized = false;
        //    }
        //}
        //
        //public void Dispose()
        //{
        //    if (_disposed) return;
        //    
        //    // nrt wrapper가 IDisposable을 지원한다면 여기서 명시적으로 Dispose ㄱㄱ
        //    _classificationPredictor = null;
        //    _segmentationPredictor = null;
        //    _device = null;
        //    _disposed = true;
        //
        //}
        //
        ///// <summary>
        ///// Classification result output
        ///// </summary>
        ///// <param name="res">  0 : Success, 1 : Invalid value, 2 : error_system, 3 : error_unknown. </param>
        ///// <param name="pred"> 예측된 데이터 저장하고 있는 변수. </param>
        //public void ClassificationResult(nrt.Result res, nrt.Predictor pred)
        //{
        //    for(int i = 0; i < (int)res.classes.get_count(); i++)
        //    {
        //        nrt.Class cla = res.classes.get(i);
        //        float prob = res.probs.get(i, cla.idx);
        //        Console.Write($"File name : {clf_infFiles[i]} ");
        //        Console.WriteLine($"- Class: {pred.get_class_name(cla.idx)}, Prob : {prob}");
        //
        //        /*
        //        // For debug.
        //        // 이미지 볼 수 있음.
        //        if (!res.cams.empty())
        //        {
        //            nrt.CAM nrtCam = res.cams.get(i);
        //            var camImg = new Mat(nrtCam.get_height(), nrtCam.get_width(), MatType.CV_8UC3, nrtCam.get_data_ptr());
        //            Cv2.ImShow("cam", camImg);
        //            Cv2.WaitKey(0);
        //        }
        //        */
        //    }
        //}
        //
        ///// <summary>   inference for classification.   </summary>
        ///// <remarks>   hjkim, 2026-03-26.              </remarks>
        //public void Inference_Classification()
        //{
        //    /* 
        //     * Predictor는 '.net' 파일 또는 '.nrpd' 파일을 사용함
        //     * CPU 환경일 경우, device_idx = -1 설정
        //     * GPU 환경일 경우, device_idx = [0, num of device] 설정
        //     * '.nrpd' 파일이 추론 속도가 조금 더 빠르다고함.
        //     */
        //
        //    // ============ Step 1 ============ //
        //    // ========= Device 설정 부 ======= //
        //    nrt.Predictor predictor;
        //    GLB.AddLog("Neurocle", "Optimizing the Predictor for the Model and Device... It may take a few minutes.", Common.SeverityLevel.INFO);
        //
        //    predictor = new nrt.Predictor(_modelPath, nrt.Model.MODELIO_OUT_CAM, dev.id, batch_size:64, fp16_flag:false , threshold_flag:false, nrt.DevType.DEVICE_CUDA_GPU);
        //    
        //    // Predictor에 최적화된 정보 저장, 동일 환경일땐 재사용 가능
        //    if (dev.id >= 0 && predictor.get_device_type() == ((int)nrt.DevType.DEVICE_CUDA_GPU) && predictor.get_status() == nrt.Status.STATUS_SUCCESS)
        //    {
        //        // 최적화된 predictor 저장
        //        _status = predictor.save_predictor(_predictorPath);
        //        if (_status != nrt.Status.STATUS_SUCCESS)
        //        {
        //            Console.WriteLine("Predictor save failed. : " + nrt.nrt.get_last_error_msg());
        //            throw new Exception("Predictor save failed");
        //        }
        //    }
        //
        //    if(predictor.get_status() != Status.STATUS_SUCCESS)
        //    {
        //        Console.WriteLine("Predictor initialization failed. : " + nrt.nrt.get_last_error_msg());
        //        WarningMessageBox warningMessageBox = new WarningMessageBox($@"Predictor initialization failed. : + {nrt.nrt.get_last_error_msg()}");
        //    }
        //
        //    // 중요 작업 시간 소요가 큰 지점마다 BusyIndicator 또는 Progress등을 이용해서 시간소요를 알려주는게 필요할듯.
        //    // ============ Step 2 ============ //
        //    // ====== 이미지 Predict 부 ======= //
        //    nrt.Input inputs = new nrt.Input();
        //    int batchSize = predictor.get_batch_size();
        //    int curBatch = 0;
        //    int imageChannels = 3; // RGB 
        //    string exts = ".png";
        //
        //    /*
        //     * 이부분 뉴로클이랑 어떤방식으로 할지 정해야할듯.
        //     * ☆★☆★※ 중요 ※☆★☆★
        //     */
        //}
        //
        //#endregion
        //
        //#region Segmentation
        ///// <summary>
        ///// Segmentation result output
        ///// </summary>
        ///// <param name="res">  0 : Success, 1 : Invalid value, 2 : error_system, 3 : error_unknown. </param>
        ///// <param name="pred"> 예측된 데이터 저장하고 있는 변수. </param>
        //public void SegmentationResult(nrt.Result res, nrt.Predictor pred)
        //{
        //    List<bool> isDetected = Enumerable.Repeat(false, seg_infFiles.Count).ToList();
        //
        //    for (int i = 0; i < (int)res.blobs.get_count(); i++)
        //    {
        //        nrt.Blob blob = res.blobs.get(i);
        //        int batchIdx = blob.batch_idx;
        //        int clsIdx = blob.class_idx;
        //        float prob = blob.prob;
        //        isDetected[batchIdx] = true;
        //
        //        // Test용 Console log
        //        Console.WriteLine($"{batchIdx} th Image");
        //        Console.WriteLine($"[{i}]th blob Class : {pred.get_class_name(clsIdx)}, Prob : {prob}");
        //        Console.WriteLine($"Class number: {blob.class_idx}");
        //        Console.WriteLine($"Bounding left top X: {blob.rect.x}");
        //        Console.WriteLine($"Bounding box center Y: {blob.rect.y}");
        //        Console.WriteLine($"Bounding box width: {blob.rect.width}");
        //        Console.WriteLine($"Bounding box height: {blob.rect.height}");
        //        Console.WriteLine($"Bounding box area: {blob.area}");
        //        Console.WriteLine($"Bounding box gray: {blob.gray}");
        //    }
        //    for (int i = 0; i < isDetected.Count; i++)
        //        if (isDetected[i] != true) Console.WriteLine($"File name : {seg_infFiles[i]} - There are no Objects in this image.");
        //}
        //
        ///// <summary>
        ///// Get image patch size.
        ///// </summary>
        ///// <param name="imgPath">  path of image</param>
        ///// <param name="model"> model </param>
        //public int GetPatchSize(string imgPath, nrt.Model model)
        //{
        //    int batchUnit = 1;
        //    if (model.is_patch_mode(0))
        //    {
        //        nrt.Shape modelInputShape = model.get_input_shape(0);
        //        nrt.InterpolationType resizeMethod = model.get_InterpolationType(0);
        //        float scaleFactor = model.get_scale_factor();
        //
        //        nrt.NDBuffer ndImage = nrt.NDBuffer.load_image(imgPath);
        //        nrt.NDBuffer resizedNdImage = new nrt.NDBuffer();
        //        _status = nrt.nrt.resize(ndImage, resizedNdImage, scaleFactor, resizeMethod);
        //        if (_status != nrt.Status.STATUS_SUCCESS)
        //        {
        //            Console.WriteLine("Resize failed.  : " + nrt.nrt.get_last_error_msg());
        //            throw new Exception("Resize failed");
        //        }
        //
        //        nrt.NDBuffer imagePatchBuff = new nrt.NDBuffer();
        //        nrt.NDBuffer patchInfo = new nrt.NDBuffer();
        //        _status = nrt.nrt.extract_patches_to_target_shape(resizedNdImage, modelInputShape, imagePatchBuff, patchInfo);
        //        if (_status != nrt.Status.STATUS_SUCCESS)
        //        {
        //            Console.WriteLine("Extract patches failed.  : " + nrt.nrt.get_last_error_msg());
        //            throw new Exception("Extract patches failed");
        //        }
        //        nrt.Shape patchShape = patchInfo.get_shape();
        //        batchUnit = patchShape.get_dim(0);
        //    }
        //    return batchUnit;
        //}
        //
        ///// <summary>   inference for segmentation.     </summary>
        ///// <remarks>   hjkim, 2026-03-26.              </remarks>
        //public void Inference_Segmentation()
        //{
        //
        //}
        //#endregion
        #endregion

        /*
         * << devNeurocle 수행 역할 >>
         *  1. GPU Device 초기화
         *  2. 상부 CLF predictor, SEG predictor
         *  3. 하부 CLF predictor, SEG predictor 
         *  4. 투과 CLF predictor, SEG predictor
         *  
         *  여기선 OK/NG 판단 안함
         *  Spec 비교 안함
         *  단순히 NRT 실행과 AI 결과만 반환시킴
         */

        /*
         *  INI → Config → GPU → 상부 / 하부 / 투과 Predictor → 프로그램 종료하기 전까지 재사용 하는 Sequence
         */
        private readonly object _inferenceLock = new object();
        private readonly int _deviceIndex;
        private Device _device;

        // CLF
        private Predictor _topClassifier;
        private Predictor _bottomClassifier;
        private Predictor _transClassifier;

        // SEG
        private Predictor _topSegmenter;
        private Predictor _bottomSegmenter;
        private Predictor _transSegmenter;

        private bool _disposed;

        public bool IsInitialized { get; private set; }
        public string LastError { get; private set; }

        public devNeurocle(int deviceIndex)
        {
            _deviceIndex = deviceIndex;
        }

        #region Initialize

        public bool Initialize(IEnumerable<NeurocleModelConfig> configs)
        {
            LastError = null;

            try
            {
                ReleaseAll();

                _device = Device.get_gpu_device(_deviceIndex);

                if (_device == null)
                {
                    LastError = "GPU Device 초기화 실패.";

                    return false;
                }
                if (configs == null)
                {
                    LastError = "Neurocle Model Config가 없습니다.";

                    return false;
                }

                foreach (NeurocleModelConfig config in configs)
                {
                    if (config == null) continue;

                    Predictor classifier = CreatePredictor(config.ClassificationModelPath, config.ClassificationPredictorPath, config.ClassificationBatchSize, config.UseFP16, Model.MODELIO_OUT_CAM);
                    Predictor segmenter = CreatePredictor(config.SegmentationModelPath, config.SegmentationPredictorPath, config.SegmentationBatchSize, config.UseFP16, Model.MODELIO_OUT_PRED);

                    SetClassifier(config.CameraType, classifier);
                    SetSegmenter(config.CameraType, segmenter);
                }

                if (_topClassifier == null || _bottomClassifier == null || _transClassifier == null)
                {
                    LastError = "Classification Predictor가 모두 준비되지 않았습니다.";

                    ReleaseAll();

                    return false;
                }

                IsInitialized = true;

                return true;
            }
            catch (Exception ex)
            {
                LastError = $"Neurocle 초기화 오류 : {ex.Message}";

                return false;
            }
        }

        private Predictor CreatePredictor(string modelPath, string predictorPath, int batchSize, bool useFP16, int outputType)
        {
            Predictor predictor = null;

            // 기존 최적화 Predictor 우선으로 함
            if (!string.IsNullOrWhiteSpace(predictorPath) && File.Exists(predictorPath))
            {
                predictor = new Predictor(_device.id, predictorPath, false);


                if (predictor.get_status() == Status.STATUS_SUCCESS) return predictor;

                predictor.Dispose();
                predictor = null;
            }

            // Predictor가 없으면 .net으로 대체
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath)) { return null; }

            predictor = new Predictor(modelPath, outputType, _device.id, batch_size: batchSize, fp16_flag: useFP16, threshold_flag: false, DevType.DEVICE_CUDA_GPU);

            if (predictor.get_status() != Status.STATUS_SUCCESS)
            {
                string error = nrt.nrt.get_last_error_msg();

                predictor.Dispose();
                throw new Exception(error);
            }

            // 최초 생성된 Predictor 저장
            if (!string.IsNullOrWhiteSpace(predictorPath))
            {
                string directory = Path.GetDirectoryName(predictorPath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                predictor.save_predictor(predictorPath);
            }

            return predictor;
        }
        #endregion

        #region Predictor

        private void SetClassifier(InspectionCameraType cameraType, Predictor predictor)
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    _topClassifier = predictor;
                    break;

                case InspectionCameraType.Bottom:
                    _bottomClassifier = predictor;
                    break;

                case InspectionCameraType.Trans:
                    _transClassifier = predictor;
                    break;
            }
        }

        private void SetSegmenter(InspectionCameraType cameraType, Predictor predictor)
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    _topSegmenter = predictor;
                    break;

                case InspectionCameraType.Bottom:
                    _bottomSegmenter = predictor;
                    break;

                case InspectionCameraType.Trans:
                    _transSegmenter = predictor;
                    break;
            }
        }

        private Predictor GetClassifier(InspectionCameraType cameraType)
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    return _topClassifier;

                case InspectionCameraType.Bottom:
                    return _bottomClassifier;

                case InspectionCameraType.Trans:
                    return _transClassifier;

                default:
                    return null;
            }
        }

        private Predictor GetSegmenter(InspectionCameraType cameraType) 
        {
            switch (cameraType)
            {
                case InspectionCameraType.Top:
                    return _topSegmenter;

                case InspectionCameraType.Bottom:
                    return _bottomSegmenter;

                case InspectionCameraType.Trans:
                    return _transSegmenter;

                default:
                    return null;
            }
        }
        #endregion

        #region Classification

        /// <summary>
        /// Classification 수행
        /// 260818_hjkim
        /// </summary>
        /// <param name="inferenceInput"></param>
        /// <param name="classificationResult"></param>
        /// <returns></returns>
        public bool Classification(NeurocleInferenceInput inferenceInput, out ClassificationResult classificationResult)
        {
            classificationResult = null;
            LastError = null;

            if(!IsInitialized)
            {
                LastError = "Neurocle이 초기화 되지 않았습니다.";
                return false;
            }

            if(inferenceInput == null || inferenceInput.DefectImage == null)
            {
                LastError = "Classification Input Image가 없습니다.";

                return false;
            }

            Predictor predictor = GetClassifier(inferenceInput.CameraType);

            if(predictor == null)
            {
                LastError = $"Classification Predictor가 없습니다 : {inferenceInput.CameraType}";

                return false;
            }

            lock (_inferenceLock)
            {
                string tempPath = null;
                Input input = null;
                Result result = null;

                try
                {
                    /*
                     * 우선 툴 개발 단계에서는 
                     * BitmapSource → Temp PNG → NRT Input 이렇게하자.
                     * 
                     * 나중에 Memory direct Input으로 변경해야함. 
                     * 뭐 이런거 테스트도 Neuro-R을 사고 난 뒤에 해야겠지만
                     */

                    tempPath = SaveTemporaryImage(inferenceInput.DefectImage);

                    input = new Input(tempPath, 3);
                    result = predictor.predict(input);

                    if (result == null || result.get_status() != Status.STATUS_SUCCESS)
                    {
                        LastError = $"Classification Predict 실패 : {nrt.nrt.get_last_error_msg()}";

                        return false;
                    }

                    if (result.classes == null || result.classes.get_count() <= 0)
                    {
                        LastError = "Classification 결과가 없습니다.";

                        return false;
                    }

                    nrt.Class topClass = result.classes.get(0);
                    int top1ClassIndex = topClass.idx;
                    float top1Probability = result.probs.get(0, top1ClassIndex);
                    int classCount = predictor.get_num_classes();
                    float top2Probability = 0.0f;

                    for (int classIndex = 0; classIndex < classCount; classIndex++)
                    {
                        if (classIndex == top1ClassIndex) continue;
                        float probability = result.probs.get(0, classIndex);

                        if (probability > top2Probability)
                            top2Probability = probability;
                    }
                    string className = predictor.get_class_name(top1ClassIndex);

                    classificationResult = new ClassificationResult
                    {
                        StripNumber = inferenceInput.StripNumber,
                        CameraType = inferenceInput.CameraType,
                        DefectIndex = inferenceInput.DefectIndex,
                        ClassIndex = top1ClassIndex,
                        ClassName = className,
                        DefectClass = ConvertDefectClass(className),
                        Top1Probability = top1Probability,
                        Top2Probability = top2Probability,
                        Success = true
                    };

                    return true;
                }
                catch (Exception ex)
                {
                    LastError = $"Classification Exception : {ex.Message}";

                    return false;
                }
                finally
                {
                    result?.Dispose();
                    input?.Dispose();
                    DeleteTemporaryFile(tempPath);
                }
            }
        }

        private static DefectClass ConvertDefectClass(string className)
        {
            if (string.IsNullOrWhiteSpace(className)) return DefectClass.Unknown;

            string name = className.Trim().ToLowerInvariant();

            switch(name)
            {
                case "particle":
                    return DefectClass.Particle;

                case "contaminant":
                case "오염":
                    return DefectClass.Contaminant;

                case "contaminantag":
                case "오염ag":
                    return DefectClass.ContaminantAg;

                case "underetching":
                case "미성형":
                    return DefectClass.UnderEtching;

                case "flash":
                    return DefectClass.Flash;

                case "void":
                case "미도금":
                    return DefectClass.Void;

                case "punch":
                case "천공":
                    return DefectClass.Punch;

                default:
                    return DefectClass.Unknown;

            }
        }

        #endregion

        #region Segmentation

        /// <summary>
        /// Segmentation 동작 구현
        /// 260818_hjkim
        /// </summary>
        /// <param name="inferenceInput"></param>
        /// <param name="defectClass"></param>
        /// <param name="resolutionUmPerPixel"></param>
        /// <param name="segmentationResult"></param>
        /// <returns></returns>
        public bool Segmentation(NeurocleInferenceInput inferenceInput, DefectClass defectClass, double resolutionUmPerPixel, out SegmentationResult segmentationResult)
        {
            segmentationResult = null;
            LastError = null;

            if (!IsInitialized)
            {
                LastError = "Neurocle이 초기화 되지 않았습니다.";

                return false;
            }
            if(inferenceInput == null || inferenceInput.DefectImage == null)
            {
                LastError = $"Segmentation Input Image가 없습니다.";

                return false;
            }

            Predictor predictor = GetSegmenter(inferenceInput.CameraType);

            if (predictor == null)
            {
                LastError = $"Segmentation Predictor가 없습니다 : {inferenceInput.CameraType}";

                return false;
            }

            lock (_inferenceLock)
            {
                string tempPath = null;
                Input input = null;
                Result result = null;

                try
                {
                    tempPath = SaveTemporaryImage(inferenceInput.DefectImage);
                    input = new Input(tempPath, 3);
                    result = predictor.predict(input);

                    if (result == null || result.get_status() != Status.STATUS_SUCCESS)
                    {
                        LastError = $"Segmentation Predict 실패 : {nrt.nrt.get_last_error_msg()}";

                        return false;
                    }

                    if (result.blobs == null || result.blobs.get_count() <= 0)
                    {
                        LastError = $"Segmentation Blob이 없습니다.";

                        return false;
                    }

                    // 일단 가장 큰 Blob을 선택하자
                    Blob largestBlob = null;
                    ulong largestArea = 0;

                    int blobCount = (int)result.blobs.get_count();

                    for (int i = 0; i < blobCount; i++)
                    {
                        Blob blob = result.blobs.get(i);

                        if (largestBlob == null || blob.area > largestArea)
                        {
                            largestBlob = blob;
                            largestArea = blob.area;
                        }
                    }

                    if(largestBlob == null)
                    {
                        LastError = "Valid blob이 없습니다.";
                        return false;
                    }

                    double widthUm = largestBlob.rect.width * resolutionUmPerPixel;
                    double heightUm = largestBlob.rect.height * resolutionUmPerPixel;
                    double sizeUm = Math.Max(widthUm, heightUm);

                    segmentationResult = new SegmentationResult
                    {
                        StripNumber = inferenceInput.StripNumber,
                        CameraType = inferenceInput.CameraType,
                        DefectIndex = inferenceInput.DefectIndex,
                        DefectClass = defectClass,
                        ClassIndex = largestBlob.class_idx,
                        Probability = largestBlob.prob,
                        X = largestBlob.rect.x,
                        Y = largestBlob.rect.y,
                        WidthPixel = largestBlob.rect.width,
                        HeightPixel = largestBlob.rect.height,
                        AreaPixel = largestBlob.area,
                        WidthUm = widthUm,
                        HeightUm = heightUm,
                        SizeUm = sizeUm,
                        Success = true
                    };

                    return true;
                }
                catch (Exception ex)
                {
                    LastError = $"Segmentation Exception : {ex.Message}";

                    return false;
                }
                finally
                {
                    result?.Dispose();
                    input?.Dispose();
                    DeleteTemporaryFile(tempPath);
                }
            }

        }

        #endregion

        #region Image Helper.

        private static string SaveTemporaryImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            if(image == null) throw new ArgumentNullException(nameof(image));

            string directory = Path.Combine(Path.GetTempPath(), "HDSInspector_AI", "Neurocle");
            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".png");

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(stream);
            }

            return filePath;
        }

        private static void DeleteTemporaryFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }

        #endregion

        #region Release

        private void ReleaseAll()
        {
            ReleasePredictor(ref _topClassifier);
            ReleasePredictor(ref _bottomClassifier);
            ReleasePredictor(ref _transClassifier);

            ReleasePredictor(ref _topSegmenter);
            ReleasePredictor(ref _bottomSegmenter);
            ReleasePredictor(ref _transSegmenter);

            _device = null;

            IsInitialized = false;
        }

        private static void ReleasePredictor(ref Predictor predictor) 
        {
            if (predictor == null) return;

            try
            {
                predictor.destroy();
            }
            catch { }

            try
            {
                predictor.Dispose();
            }
            catch { }

            predictor = null;
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock(_inferenceLock)
            {
                ReleaseAll();

                _disposed = true;
            }
        }

        #endregion
    }

}
