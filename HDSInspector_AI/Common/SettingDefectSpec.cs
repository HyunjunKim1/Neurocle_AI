using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DefectSpecSettingItem
    {
        public bool Enable { get; set; }

        // Classification 확정 최소 Threshold값
        public double ClassificationThreshold { get; set; }

        // Top1 - Top2 최소 차이값
        public double ClassificationMargin { get; set; }

        public string JudgeMethod { get; set; }

        // Direct 방식일때 최종 판정
        public string DirectJudgement { get; set; }

        // Size / Distance / Difference 기준
        public double ThresholdUm { get; set; }
    }

}
