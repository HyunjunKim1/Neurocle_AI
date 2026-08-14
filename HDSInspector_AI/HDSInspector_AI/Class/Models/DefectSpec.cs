using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models
{
    public class DefectSpec
    {
        public InspectionCameraType CameraType { get; set; }

        public DefectClass DefectClass { get; set; }

        // Classification 확정 최소 Threshold값
        public float ClassificationThreshold { get; set; }

        // Top1 - Top2 최소 차이값
        public float ClassificationMargin { get; set; }

        public DefectJudgeMethod JudgeMethod { get; set; }

        // Direct 방식일때 최종 판정
        public AIJudgement DirectJudgement {  get; set; }

        // Size / Distance / Difference 기준
        public double ThresholdUm { get; set; }

        // 추후 Recipe GUI 표시용
        public string Description { get; set; }
    }

}
