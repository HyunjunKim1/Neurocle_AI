using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models.InferenceResult
{
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
}
