using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HDSInspector_AI.Class.Models_DTO
{
    public class DefectImagePairItem
    {
        public int index { get; set; }
        
        public ImageSource ReferenceImage { get; set; }
        public ImageSource DefectImage { get; set; }

        public string IndexText { get { return $"#{index:D2}"; } }
    }
}
