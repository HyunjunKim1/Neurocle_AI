using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.GlobalFunction
{
    #region define enums
    /// <summary>
    /// Severity Levels 3 Tier
    /// 
    /// SEV1 = Critical
    /// SEV2 = Moderate
    /// SEV3 = Information
    /// 
    /// </summary>
    public enum E_SERVERITY_LEVELS
    {
        Critical,
        Moderate,
        Information
    }
    public enum E_GRAB_STATUS
    {
        GrabReady,
        FrameDone,
        FrameComplete,
        Error
    }

    #endregion

    public class GlobalFunction : IDisposable
    {
        private static readonly Lazy<GlobalFunction> _instance = new Lazy<GlobalFunction>();
        public static GlobalFunction GLB => _instance.Value;

        CustomLog _clsCustomLog = new CustomLog();

        public GlobalFunction()
        {

        }

        public void Dispose()
        {
            _clsCustomLog.Dispose();
        }

        #region Global Functions
        public void AddLog(E_SERVERITY_LEVELS level, string log)
        {
            switch(level)
            {
                case E_SERVERITY_LEVELS.Critical:
                    log = $"[C][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
                case E_SERVERITY_LEVELS.Moderate:
                    log = $"[M][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
                case E_SERVERITY_LEVELS.Information:
                    log = $"[I][{DateTime.Now:HH:mm:ss:fff}] {log}"; // [19:23:34:212] Blah, blah, blah.
                    break;
            }
            _clsCustomLog?.AddLog(log);
        }
        #endregion
    }
}
