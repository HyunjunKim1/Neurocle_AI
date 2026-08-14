using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models.InferenceResult
{
    // Strip 한장에 대한 전체 AI 결과 데이터
    public class StripInferenceResult
    {
        public int StripNumber { get; set; }
        public long ProcessingTimeMs { get; set; }
        public List<DefectInferenceResult> Results { get; set; }
        public StripInferenceResult()
        {
            Results = new List<DefectInferenceResult>();
        }

        public int TotalCount => Results.Count;
        public int OKCount      => Results.Count(x => x.Judgement == AIJudgement.OK);
        public int NGCount      => Results.Count(x => x.Judgement == AIJudgement.NG);
        public int UnknownCount => Results.Count(x => x.Judgement == AIJudgement.Unknown);

        // Strip 전체 판정
        // NG > Unknown > OK
        public AIJudgement OverallJudgement
        {
            get
            {
                if (NGCount > 0) return AIJudgement.NG;
                if (UnknownCount > 0) return AIJudgement.Unknown;
                return AIJudgement.OK;
            }
        }

    }
}
