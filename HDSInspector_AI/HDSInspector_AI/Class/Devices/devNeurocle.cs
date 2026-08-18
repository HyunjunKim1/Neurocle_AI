using ControlzEx.Behaviors;
using HDSInspector_AI.GUI.Windows.Popup;
using nrt;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Devices
{
    /// <summary>   Easy for use neuro-r functions. </summary>
    /// <remarks>   hjkim, 2026-03-26.              </remarks>
    public class devNeurocle// : IDisposable
    {
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
        public void Dispose()
        {
            //if (_disposed) return;
            //
            //// nrt wrapper가 IDisposable을 지원한다면 여기서 명시적으로 Dispose ㄱㄱ
            //_classificationPredictor = null;
            //_segmentationPredictor = null;
            //_device = null;
            //_disposed = true;
        
        }
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
    }

}
