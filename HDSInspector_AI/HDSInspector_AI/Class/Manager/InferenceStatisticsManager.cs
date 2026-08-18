using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{

    /// <summary>   추론 이후 데이터 관리    </summary>
    /// <remarks>   hjkim, 2026-08-18.       </remarks>

    /*
     * 여기서 현 제품, 현 오더번호, 현 Strip 번호, 누적 Prob 관리
     */
    public class InferenceStatisticsManager
    {
        private readonly object _syncLock = new object();

        private string _productName;
        private string _orderNumber;
        private int _totalOK;
        private int _totalNG;
        private int _totalUnknown;

        public event Action<InferenceStatistics> StatisticsChanged;

        public void SetInspectionInfo(InspectionInfo info)
        {
            if (info == null) return;

            lock(_syncLock)
            {
                bool changed = !string.Equals(_productName, info.ProductName, StringComparison.Ordinal) || !string.Equals(_orderNumber, info.OrderNumber, StringComparison.Ordinal);

                if (!changed) return;

                /*
                 * Product 또는 Order가 변경되면 누적 Count Reset 시켜야함. 제품 변경이라는 뜻이니까
                 */

                _productName = info.ProductName;
                _orderNumber = info.OrderNumber;

                _totalOK = 0;
                _totalNG = 0;
                _totalUnknown = 0;
            }

            RaiseEmptyStatistics();
        }

        public void AddStripResult(StripInferenceResult stripResult)
        {
            if (stripResult == null) return;

            InferenceStatistics statistics;

            lock(_syncLock)
            {
                _totalOK += stripResult.OKCount;
                _totalNG += stripResult.NGCount;
                _totalUnknown += stripResult.UnknownCount;

                statistics = new InferenceStatistics
                {
                    ProductName = _productName,
                    OrderNumber = _orderNumber,
                    CurrentStipNumber = stripResult.StripNumber,
                    StripOKCount = stripResult.OKCount,
                    StripNGCount = stripResult.NGCount,
                    StripUnknownCount = stripResult.UnknownCount,

                    TotalOKCount = _totalOK,
                    TotalNGCount = _totalNG,
                    TotalUnknownCount = _totalUnknown
                };
            }

            StatisticsChanged?.Invoke(statistics);
        }

        private void RaiseEmptyStatistics()
        {
            InferenceStatistics statistics = new InferenceStatistics
            {
                ProductName = _productName,
                OrderNumber = _orderNumber
            };

            StatisticsChanged?.Invoke(statistics);
        }
    }
}
