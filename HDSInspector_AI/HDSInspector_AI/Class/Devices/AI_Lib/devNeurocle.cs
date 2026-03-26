using ControlzEx.Behaviors;
using HDSInspector_AI.GUI.Windows.Popup;
using nrt;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Devices.AI_Lib
{
    /// <summary>   Easy for use neuro-r functions. </summary>
    /// <remarks>   hjkim, 2026-03-26.              </remarks>
    public class devNeurocle
    {
        // GPU
        nrt.Device dev;

        private nrt.Status _status;
        private string _modelPath = "~~.net";
        private string _predictorPath = "~~.nrpd";
        static List<string> infFiles = new List<string>();

        public nrt.Status Status    { get { return _status; } set { _status = value; } }
        public string ModelPath     { get { return _modelPath; } set { _modelPath = value; } }
        public string PredictorPath { get { return _predictorPath; } set { _predictorPath = value; } }

        public devNeurocle() { dev = nrt.Device.get_gpu_device(0); }

        
        #region Classification
        /// <summary>
        /// Classification result output
        /// </summary>
        /// <param name="res">  0 : Success, 1 : Invalid value, 2 : error_system, 3 : error_unknown. </param>
        /// <param name="pred"> 예측된 데이터 저장하고 있는 변수. </param>
        public void ClassificationResult(nrt.Result res, nrt.Predictor pred)
        {
            for(int i = 0; i < (int)res.classes.get_count(); i++)
            {
                nrt.Class cla = res.classes.get(i);
                float prob = res.probs.get(i, cla.idx);
                Console.Write($"File name : {infFiles[i]} ");
                Console.WriteLine($"- Class: {pred.get_class_name(cla.idx)}, Prob : {prob}");

                /*
                // For debug.
                // 이미지 볼 수 있음.
                if (!res.cams.empty())
                {
                    nrt.CAM nrtCam = res.cams.get(i);
                    var camImg = new Mat(nrtCam.get_height(), nrtCam.get_width(), MatType.CV_8UC3, nrtCam.get_data_ptr());
                    Cv2.ImShow("cam", camImg);
                    Cv2.WaitKey(0);
                }
                */
            }
        }

        /// <summary>   inference for classification.   </summary>
        /// <remarks>   hjkim, 2026-03-26.              </remarks>
        public void Inference_Classification()
        {
            /* 
             * Predictor는 '.net' 파일 또는 '.nrpd' 파일을 사용함
             * CPU 환경일 경우, device_idx = -1 설정
             * GPU 환경일 경우, device_idx = [0, num of device] 설정
             * '.nrpd' 파일이 추론 속도가 조금 더 빠르다고함.
             */

            // ============ Step 1 ============ //
            // ========= Device 설정 부 ======= //
            nrt.Predictor predictor;

            Console.WriteLine("Optimizing the Predictor for the Model and Device... It may take a few minutes.");
            predictor = new nrt.Predictor(_modelPath, nrt.Model.MODELIO_OUT_CAM, dev.id, batch_size:64, fp16_flag:false , threshold_flag:false, nrt.DevType.DEVICE_CUDA_GPU);
            
            // Predictor에 최적화된 정보 저장, 동일 환경일땐 재사용 가능
            if (dev.id >= 0 && predictor.get_device_type() == ((int)nrt.DevType.DEVICE_CUDA_GPU) && predictor.get_status() == nrt.Status.STATUS_SUCCESS)
            {
                // 최적화된 predictor 저장
                _status = predictor.save_predictor(_predictorPath);
                if (_status != nrt.Status.STATUS_SUCCESS)
                {
                    Console.WriteLine("Predictor save failed. : " + nrt.nrt.get_last_error_msg());
                    throw new Exception("Predictor save failed");
                }
            }

            if(predictor.get_status() != Status.STATUS_SUCCESS)
            {
                Console.WriteLine("Predictor initialization failed. : " + nrt.nrt.get_last_error_msg());
                WarningMessageBox warningMessageBox = new WarningMessageBox($@"Predictor initialization failed. : + {nrt.nrt.get_last_error_msg()}");
            }

            // 중요 작업 시간 소요가 큰 지점마다 BusyIndicator 또는 Progress등을 이용해서 시간소요를 알려주는게 필요할듯.
            // ============ Step 2 ============ //
            // ====== 이미지 Predict 부 ======= //
            nrt.Input inputs = new nrt.Input();
            int batchSize = predictor.get_batch_size();
            int curBatch = 0;
            int imageChannels = 3; // RGB 
            string exts = ".png";

            /*
             * 이부분 뉴로클이랑 어떤방식으로 할지 정해야할듯.
             * ☆★☆★※ 중요 ※☆★☆★
             */
        }

        #endregion

        #region Segmentation

        #endregion
    }
}
