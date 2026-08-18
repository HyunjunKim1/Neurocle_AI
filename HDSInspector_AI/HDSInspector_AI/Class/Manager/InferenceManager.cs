using Common;
using HDSInspector_AI.Class.Devices;
using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{
    public class InferenceManager : IDisposable
    {
        private devNeurocle _neurocle;
        private readonly DefectSpecManager _specManager;
        private readonly DefectJudgementEngine _judgeEngine;

        public InferenceManager(DefectSpecManager specManager)
        {
            _specManager = specManager ?? throw new ArgumentNullException(nameof(specManager));
            _judgeEngine = new DefectJudgementEngine();
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

        public void Process(/**/)
        {
            /*
             * 1. Classification
             */

            /*
             * 2. Confidence 확인
             */

            /*
             * 3. Defect Spec 조회
             */

            /*
             * 4. 필요하면 Segmentation
             */

            /*
             * 5. Measurement
             */

            /*
             * 6. 최종 판정
             */

            /*
             * 7. Result Event
             */
        }
    }
}
