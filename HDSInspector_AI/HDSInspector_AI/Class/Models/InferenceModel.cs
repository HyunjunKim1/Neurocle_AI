using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HDSInspector_AI.Class.Models
{
    public class NeurocleInferenceInput
    {
        public int StripNumber { get; set; }
        public InspectionCameraType CameraType { get; set; }
        public int DefectIndex { get; set; }

        public BitmapSource ReferenceImage { get; set; }
        public BitmapSource DefectImage { get; set; }
    }

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


    public class SegmentationResult
    {
        public int StripNumber { get; set; }

        public InspectionCameraType CameraType { get; set; }

        public int DefectIndex { get; set; }

        public DefectClass DefectClass { get; set; }

        public int ClassIndex { get; set; }

        public float Probability { get; set; }

        // Blob Bounding Box
        public int X { get; set; }
        public int Y { get; set; }

        public int WidthPixel { get; set; }
        public int HeightPixel { get; set; }

        public ulong AreaPixel { get; set; }

        public double WidthUm { get; set; }
        public double HeightUm { get; set; }
        public double SizeUm { get; set; }

        public double OverflowDistanceUm { get; set; }
        public double ReferenceDifferenceUm { get; set; }

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }


    public class DefectInferenceResult
    {
        public int StripNumber { get; set; }
        public InspectionCameraType CameraType { get; set; }
        public int DefectIndex { get; set; }
        public DefectClass DefectClass { get; set; }
        public string ClassName { get; set; }

        public float ClassificationProbability { get; set; }
        public float ClassificationMargin { get; set; }
        public bool SegmentationExecuted { get; set; }

        public double MeasuredValueUm { get; set; }
        public double SpecValueUm { get; set; }

        public AIJudgement Judgement { get; set; }
        public string JudgementReason { get; set; }

    }

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
        public int OKCount => Results.Count(x => x.Judgement == AIJudgement.OK);
        public int NGCount => Results.Count(x => x.Judgement == AIJudgement.NG);
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

    public class InferenceImageDisplayItem
    {
        public int StripNumber { get; set; }
        public InspectionCameraType CameraType { get; set; }
        public int DefectIndex { get; set; }
        public DefectClass DefectClass { get; set; }
        public string ClassName { get; set; }
        public AIJudgement Judgement { get; set; }
        public float Probability { get; set; }
        public double MeasuredValueUm { get; set; }
        public BitmapSource DefectImage { get; set; }
        public string IndexTest => $"#{DefectIndex:D2}";
        public string ProbabilityText => $"{Probability * 100.0:F1}%";
        public string ResultText => Judgement.ToString();

        public string ClassText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ClassName))
                    return ClassName;

                return DefectClass.ToString();
            }
        }
    }

    public class InferenceStatistics
    {
        public string ProductName { get; set; }
        public string OrderNumber { get; set; }
        public int CurrentStipNumber { get; set; }

        public int StripOKCount { get; set; }
        public int StripNGCount { get; set; }
        public int StripUnknownCount { get; set; }
        public int TotalOKCount { get; set; }
        public int TotalNGCount { get; set; }
        public int TotalUnknownCount { get; set; }

        public int TotalCount => TotalOKCount + TotalNGCount + TotalUnknownCount;
        public int StripTotalCount => StripOKCount + StripNGCount + StripUnknownCount;
    }
}
