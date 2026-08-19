using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models_DTO
{
    public class NeurocleModelConfig
    {
        public InspectionCameraType CameraType { get; set; }
        public string ClassificationModelPath { get; set; }
        public string ClassificationPredictorPath { get; set; }
        public int ClassificationBatchSize { get; set; }
        public string SegmentationModelPath { get; set; }
        public string SegmentationPredictorPath { get; set; }
        public int SegmentationBatchSize { get; set; }
        public bool UseFP16 { get; set; }
    }
}
