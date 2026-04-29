using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HDSInspector_AI.Class.Interface
{
    public interface IWindowService
    {
        T CreateWindows<T>() where T : Window, new();
        void ShowWindows(Window window, bool asDialog = false);
        void CloseWindows(Window window);

    }
}
