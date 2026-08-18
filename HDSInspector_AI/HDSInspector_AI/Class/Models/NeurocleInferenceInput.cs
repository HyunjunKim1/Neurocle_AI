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
}
