using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{
    /// <summary>   불량 판정용 Class    </summary>
    /// <remarks>   hjkim, 2026-08-14.   </remarks>
    
    // 해당 클래스에서는 NRT를 전혀 모르고 있는 상태로 모듈화 시키자
    public class DefectJudgementEngine
    {
        public DefectInferenceResult Judge(ClassificationResult classification, SegmentationResult segmentation, DefectSpec spec)
        {
            DefectInferenceResult result = CreateBaseResult(classification);
            
            // 등록되지 않은 Spec 일 경우
            if(spec == null)
            {
                result.Judgement = AIJudgement.Unknown;
                result.JudgementReason = "Defect spec not found";

                return result;
            }

            /*
             * Classification Confidence 확인
             */
            if(classification.Top1Probability < spec.ClassificationThreshold)
            {
                result.Judgement = AIJudgement.Unknown;
                result.JudgementReason = "Classification confidence low";

                return result;
            }

            /*
             * Top1 / Top2 Margin 값 확인
             */
            if(classification.ProbabilityMargin < spec.ClassificationMargin)
            {
                result.Judgement = AIJudgement.Unknown;
                result.JudgementReason = "Classification margin low";

                return result;
            }

            switch(spec.JudgeMethod)
            {
                case DefectJudgeMethod.Direct:
                    result.Judgement = spec.DirectJudgement;
                    result.JudgementReason = spec.Description;

                    break;

                case DefectJudgeMethod.Size:
                case DefectJudgeMethod.OverflowDistance:
                case DefectJudgeMethod.ReferenceDifference:
                    if(segmentation == null || !segmentation.Success)
                    {
                        result.Judgement = AIJudgement.Unknown;
                        result.JudgementReason = "Measurement failed";
                        break;
                    }
                    result.SegmentationExecuted = true;

                    switch (spec.JudgeMethod)
                    {
                        case DefectJudgeMethod.Size:
                            result.MeasuredValueUm = segmentation.SizeUm;
                            break;

                        case DefectJudgeMethod.OverflowDistance:
                            result.MeasuredValueUm = segmentation.OverflowDistanceUm;
                            break;

                        case DefectJudgeMethod.ReferenceDifference:
                            result.MeasuredValueUm = segmentation.ReferenceDifferenceUm;
                            break;
                    }

                    result.SpecValueUm = spec.ThresholdUm;
                    result.Judgement = result.MeasuredValueUm >= spec.ThresholdUm ? AIJudgement.NG : AIJudgement.OK;
                    result.JudgementReason = $"Measured = {result.MeasuredValueUm:F1}um / Spec = {spec.ThresholdUm:F1}um";
                    break;
            }

            return result;
        }

        private DefectInferenceResult CreateBaseResult(ClassificationResult classification)
        {
            return new DefectInferenceResult
            {
                StripNumber = classification.StripNumber,
                CameraType = classification.CameraType,
                DefectIndex = classification.DefectIndex,
                DefectClass = classification.DefectClass,
                ClassName = classification.ClassName,
                ClassificationProbability = classification.Top1Probability,
                ClassificationMargin = classification.ProbabilityMargin,
                Judgement = AIJudgement.Unknown
            };
        }
    }
}
