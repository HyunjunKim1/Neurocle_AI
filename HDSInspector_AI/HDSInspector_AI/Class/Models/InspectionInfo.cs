using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models
{
    /// <summary>   Inspection Item Model     </summary>
    /// <remarks>   hjkim, 2026-08-07.        </remarks>
    
    // Main에서 받아올 데이터들, 이거 Verify 이미지 경로 탐색할떄 쓸거임 
    public class InspectionInfo
    {
        public string DeviceName { get; set; }
        public string ProductName { get; set; }
        public string OrderNumber { get; set; }
        public bool IsValid
        {
            get
            {
                return
                    IsValidPathSegment(DeviceName) &&
                    IsValidPathSegment(ProductName) &&
                    IsValidPathSegment(OrderNumber);
            }
        }

        private static bool IsValidPathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return value.LastIndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
