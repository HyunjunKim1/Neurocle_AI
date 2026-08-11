using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TEST_MAIN_SERVER
{
    internal class WhileThread
    {
        Task _task;

        public delegate void UserWhileFunction();
        private UserWhileFunction _whileFunction;

        public delegate void DelStopCompleted();

        bool _stopTask = false;
        bool _startTask = false;
        int _cycle;

        public WhileThread(int cycle_ms, UserWhileFunction UserFunc)
        {
            _cycle = cycle_ms / 10;
            _whileFunction = UserFunc;
            _task = Task.Run(() => Tasking());
        }

        ~WhileThread()
        {
            Stop();
        }

        private void Tasking()
        {
            int cnt = _cycle;
            while (_stopTask == false)
            {
                if (_startTask == false || cnt++ < _cycle)
                {
                    Thread.Sleep(10);
                    continue;
                }

                cnt = 0;
                _whileFunction();
            }
        }

        public void Start() { _startTask = true; }
        public void Pause() { _startTask = false; }
        public void Stop() { _stopTask = true; }

        public void SafeStop(DelStopCompleted delStopCompleted)
        {
            _task.GetAwaiter().OnCompleted(new Action(delStopCompleted));
            _stopTask = true;
        }

        public void WaitStopThread()
        {
            _task.Wait();
        }
    }
}
