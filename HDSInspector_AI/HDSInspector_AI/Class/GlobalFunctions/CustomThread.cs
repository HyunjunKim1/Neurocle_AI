using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.GlobalFunctions
{
    /// <summary>
    /// ## 260312_hjkim ##
    /// while을 이용한 Thread 관리
    /// </summary>
    class CustomThread
    {
        Task _task;

        public delegate void UserWhileThreadFunction();
        private UserWhileThreadFunction _whileFunction;

        bool _bStopTask = false;
        bool _bStartTask = false;
        int _cycle;

        public CustomThread(int cycle_ms, UserWhileThreadFunction UserFunc)
        {
            _cycle = cycle_ms / 10;
            _whileFunction = UserFunc;
            _task = Task.Run(() => Tasking());
        }

        ~CustomThread()
        {
            Stop();
        }

        private void Tasking()
        {
            int cnt = _cycle;
            while (_bStopTask == false)
            {
                if (_bStartTask == false)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (cnt++ < _cycle)
                {
                    Thread.Sleep(10);
                    continue;
                }

                cnt = 0;
                _whileFunction();
            }
        }

        public void Start() { _bStartTask = true; }
        public void Pause() { _bStartTask = false; }
        public void Stop() { _bStopTask = true; }

        public void WaitStopThread()
        {
            return;
        }
    }
}
