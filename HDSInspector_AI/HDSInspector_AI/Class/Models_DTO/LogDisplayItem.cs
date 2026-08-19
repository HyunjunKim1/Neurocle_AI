using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// MVVM 패턴 기준으로 Model 만드려고 했는데 그냥 너무 구조를 바꿔야하니까
// 그냥 옵저버 패턴을 사용하는거 정도로만 만족하자, 여기선 구현안하고 모델만 구현함
namespace HDSInspector_AI.Class.Models_DTO
{
    /// <summary>   Log Item Model     </summary>
    /// <remarks>   hjkim, 2026-08-06. </remarks>
    public class LogDisplayItem
    {
        public DateTime Time { get; set; }
        public string System { get; set; }
        public SeverityLevel Level { get; set; }
        public string Message { get; set; }
        public string TimeText
        {
            get
            {
                return Time.ToString("HH:mm:ss.fff");
            }
        }

        public string LevelText
        {
            get { return Level.ToString(); }
        }
    }
}
