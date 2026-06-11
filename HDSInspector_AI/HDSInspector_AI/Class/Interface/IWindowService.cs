using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HDSInspector_AI.Class.Manager.WindowManager;

namespace HDSInspector_AI.Class.Interface
{
    public interface IWindowService
    {
        void CreateWindows(WINDOW_NAME name);

    }
}
