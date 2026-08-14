using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models.InferenceResult
{
    public class DefectInferenceResult
    {
        public int StripNumber { get; set; }
        public InspectionCameraType CameraType { get; set; }
        public int DefectIndex { get; set; }
        public DefectClass DefectClass { get; set; }
        public string ClassName { get; set; }

        public float ClassificationProbability { get; set; }
        public float ClassificationMargin { get; set; }
        public bool SegmentationExcuted { get; set; }

        public double MeasuredValueUm { get; set; }
        public double SpecValueUm { get; set; }

        public AIJudgement Judgement { get; set; }
        public string JudgementReason { get; set; }

    }
}
