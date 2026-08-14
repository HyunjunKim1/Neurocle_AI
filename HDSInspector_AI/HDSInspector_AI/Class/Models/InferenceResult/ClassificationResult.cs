using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models.InferenceResult
{
    public class ClassificationResult
    {
        public int StripNumber { get; set; }
        public InspectionCameraType CameraType { get; set; }

        public int DefectIndex { get; set; }

        public DefectClass DefectClass { get; set; }

        public int ClassIndex { get; set; }

        public string ClassName { get; set; }

        // 가장 높은 Class 확률
        public float Top1Probability { get; set; }

        // 두번째 Class 확률
        public float Top2Probability { get; set; }

        public float ProbabilityMargin
        { 
            get
            {
                return Top1Probability - Top2Probability;
            }
        }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}
