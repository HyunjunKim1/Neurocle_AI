using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models
{
    /// <summary>   Defect Item Model     </summary>
    /// <remarks>   hjkim, 2026-08-07.    </remarks>
    
    // 동일한 일련번호를 가지는 상/하/투 이미지 세트
    public class DefectImageFileSet
    {
        // [000001] → 이거를 1번으로
        // 경모말로는 이게 PLC 스캔 순번이라던데 일단 순차인덱싱용
        public int SequenceNumber { get; set; }

        //실제 System 폴더
        public string SystemDirectory { get; set; }

        /*
         * 이거 Verify에서 가져가는 이미지들은 하나의 폴더에 상부 하부 투과 다 들어있음
         * 근데 각각 경로를 나눠놓은 이유는 혹시나 추론 결과를 각기 다른 경로로 저장할 가능성이 있어서
         * 상부, 하부, 투과 이미지 경로를 각각 만듦
         */
        // 9011 상부
        public string TopImagePath { get; set; }
        public string TopTextPath { get; set; }

        // 9021 하부
        public string BottomImagePath { get; set; }
        public string BottomTextPath { get; set; }

        // 9031 투과
        public string TransImagePath { get; set; }
        public string TransTextPath { get; set; }

        public DateTime LastWriteTime { get; set; }

        public bool HasTopImage => !string.IsNullOrWhiteSpace(TopImagePath) && File.Exists(TopImagePath);
        public bool HasBottomImage => !string.IsNullOrWhiteSpace(BottomImagePath) && File.Exists(BottomImagePath);
        public bool HasTransImage => !string.IsNullOrWhiteSpace(TransImagePath) && File.Exists(TransImagePath);

        public bool HasAnyImage => HasTopImage || HasBottomImage || HasTransImage;
    }
}
