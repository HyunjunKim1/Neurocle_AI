// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Globalization;
using System.Windows.Data;

// Commented by suoow2.

namespace Common.Drawing
{
    // For enable/disabling toggle button group.
    [ValueConversion(typeof(ToolType), typeof(string))]
    public class ToolTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = Enum.GetName(typeof(ToolType), value);

            return (name == (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotSupportedException(this.GetType().Name + Properties.Settings.Default.ConvertBackNotSupported);
        }
    }
}
