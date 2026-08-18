using HDSInspector_AI.Class.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{
    public class InferenceManager : IDisposable
    {
        private readonly devNeurocle _neurocle;
        private readonly DefectSpecManager _specManager;
        private readonly DefectJudgementEngine _judgeEngine;

        public InferenceManager(DefectSpecManager specManager)
        {
            _specManager = specManager ?? throw new ArgumentNullException(nameof(specManager));
            _judgeEngine = new DefectJudgementEngine();
            _neurocle = new devNeurocle();
        }

        public devNeurocle Neurocle
        {
            get { return _neurocle; }
        }

        public void Dispose()
        {
            _neurocle?.Dispose();
        }

        public void Process(/**/)
        {
            /*
             * 1. Classification
             */

            /*
             * 2. Confidence 확인
             */

            /*
             * 3. Defect Spec 조회
             */

            /*
             * 4. 필요하면 Segmentation
             */

            /*
             * 5. Measurement
             */

            /*
             * 6. 최종 판정
             */

            /*
             * 7. Result Event
             */
        }
    }
}
