using Common;
using ControlzEx.Behaviors;
using HDSInspector_AI.Class.GlobalFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{
    public enum SEQUENCE_TYPE
    {
        NONE,

        CLASSIFICATION,
        SEGMENTATION,

        // etc...
    }
    public class SequenceManager
    {
        enum SUB_SEQUENCE_RESULT
        {
            ING,
            SUCCESS,
            FAIL
        }

        CustomThread _threadSequence;

        int _mainSequenceStep       = 0;
        int _oldMainSequenceStep    = 0;
        int _subSequenceStep        = 0;

        SEQUENCE_TYPE _mainSequenceType = SEQUENCE_TYPE.NONE;

        public SEQUENCE_TYPE GetMainSequenceType()
        {
            return _mainSequenceType;
        }

        public void SetMainSequenceType(SEQUENCE_TYPE type)
        {
            _mainSequenceType = type;
        }

        public SequenceManager()
        {
            _threadSequence = new CustomThread(10, MainSequence);
            _threadSequence.Start();
        }

        public void Dispose()
        {
            _threadSequence.Stop();
            _threadSequence.WaitStopThread();
        }
        private void Logging(string Msg, SeverityLevel lvl)
        {
            string NowProcess = "Sequence";

            GLB.AddLog(NowProcess, Msg, lvl);
        }

        public void Start(SEQUENCE_TYPE type)
        {
            _mainSequenceStep = 0;
            SetMainSequenceType(type);
        }

        public void Stop()
        {
            SetMainSequenceType(SEQUENCE_TYPE.NONE);
            _mainSequenceStep = 0;
        }

        private void MainSequence()
        {
            if (_mainSequenceStep != _oldMainSequenceStep)
            {
                _oldMainSequenceStep = _mainSequenceStep;
                _subSequenceStep = 0;
            }

            try
            {
                switch(_mainSequenceType)
                {
                    case SEQUENCE_TYPE.NONE:
                        break;

                    case SEQUENCE_TYPE.CLASSIFICATION:
                        break;

                    case SEQUENCE_TYPE.SEGMENTATION:
                        break;
                }
            }
            catch(Exception ex)
            {
                Stop();
                Logging($@"Error - {ex.Message}", SeverityLevel.ERROR);
                Logging($@"StackTrace - {Environment.NewLine + ex.StackTrace}", SeverityLevel.ERROR);
            }
        }
    }
}
