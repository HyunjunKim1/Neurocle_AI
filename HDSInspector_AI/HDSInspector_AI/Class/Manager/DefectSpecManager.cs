using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{
    public class DefectSpecManager
    {
        private readonly Dictionary<string, DefectSpec> _specs;

        public DefectSpecManager()
        {
            _specs = new Dictionary<string, DefectSpec>();

            InitializeDefaultSpecs();
        }

        private string GetKey(InspectionCameraType cameraType, DefectClass defectClass)
        {
            return $"{cameraType}_{defectClass}";
        }

        private void Add(DefectSpec spec)
        {
            _specs[GetKey(spec.CameraType, spec.DefectClass)] = spec;
        }

        public DefectSpec GetSpec(InspectionCameraType cameraType, DefectClass defectClass)
        {
            DefectSpec spec;

            if(_specs.TryGetValue(GetKey(cameraType, defectClass), out spec)) { return spec; }

            return null;
        }

        // Spec 관리임. 이거 나중에 Ini 파일에서 읽어오도록 수정필요함
        /* 수정필요 */
        private void InitializeDefaultSpecs() 
        {

            // Top
            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Top,
                DefectClass = DefectClass.Particle,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                DirectJudgement = AIJudgement.OK,
                Description = "상부 Particle 양품 처리"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Top,
                DefectClass = DefectClass.UnderEtching,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                ThresholdUm = 40.0,
                Description = "상부 미성형 40um 이상 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Top,
                DefectClass = DefectClass.Flash,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.OverFlowDistance,
                ThresholdUm = 100.0,
                Description = "Ag 도금 영역 기준 100um 이상 Flash NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Top,
                DefectClass = DefectClass.Void,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                Description = "미도금 존재 시 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Top,
                DefectClass = DefectClass.Deformation,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.ReferenceDiffer,
                ThresholdUm = 20.0,
                Description = "REF 대비 20um 이상 변형 NG"
            });

            // Bottom
            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Bottom,
                DefectClass = DefectClass.Particle,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                DirectJudgement = AIJudgement.OK,
                Description = "하부 부유이물 양품 처리"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Bottom,
                DefectClass = DefectClass.Contamination,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Size,
                ThresholdUm = 200.0,
                Description = "하부 오염 200um 이상 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Bottom,
                DefectClass = DefectClass.UnderEtching,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Size,
                ThresholdUm = 40.0,
                Description = "하부 미성형 40um 이상 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Bottom,
                DefectClass = DefectClass.Deformation,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.ReferenceDiffer,
                ThresholdUm = 20,
                Description = "하부 변형 20um 이상 NG"
            });


            // Trans
            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Trans,
                DefectClass = DefectClass.Particle,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                DirectJudgement = AIJudgement.OK,
                Description = "투과 부유이물 양품 처리"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Trans,
                DefectClass = DefectClass.UnderEtching,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Size,
                ThresholdUm = 40.0,
                Description = "투과 미성형 40um 이상 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Trans,
                DefectClass = DefectClass.Deformation,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.ReferenceDiffer,
                ThresholdUm = 20.0,
                Description = "투과 변형 20um 이상 NG"
            });

            Add(new DefectSpec
            {
                CameraType = InspectionCameraType.Trans,
                DefectClass = DefectClass.Punch,
                ClassificationThreshold = 0.9f,
                ClassificationMargin = 0.2f,
                JudgeMethod = DefectJudgeMethod.Direct,
                DirectJudgement = AIJudgement.NG,
                Description = "투과 천공 존재 시 NG"
            });
        }
    }
}
