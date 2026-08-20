using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HDSInspector_AI.Class.Models
{
    public class DefectImagePairItem
    {
        public int index { get; set; }
        
        public BitmapSource ReferenceImage { get; set; }
        public BitmapSource DefectImage { get; set; }

        public string IndexText { get { return $"#{index:D2}"; } }
    }
}
