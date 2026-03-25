/*********************************************************************************
 * Copyright(c) 2015 by Haesung DS.
 * 
 * This software is copyrighted by, and is the sole property of Haesung DS.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Haesung DS. This software may only be used in accordance with
 * the corresponding license agreement. Any unauthorized use, duplication, 
 * transmission, distribution, or disclosure of this software is expressly 
 * forbidden.
 *
 * This Copyright notice may not be removed or modified without prior written
 * consent of Haesung DS reserves the right to modify this 
 * software without notice.
 *
 * Haesung DS.
 * KOREA 
 * http://www.HaesungDS.com
 *********************************************************************************/
/**
 * @file  Converters.cs
 * @brief
 *  converter series.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.05.24
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.17 First creation.
 * - 2011.05.24 Add ColorToBrushConverter
 */

using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace Common
{
    /// <summary>   Double to integer converter.  </summary>
    /// <remarks>   suoow2, 2014-05-24. </remarks>
    [ValueConversion(typeof(double), typeof(int))]
    public class DoubleToIntegerConverter : IValueConverter
    {
        private int m_nMinValue = Int32.MinValue;
        private int m_nMaxValue = Int32.MaxValue;

        public int m_nMin
        {
            get
            {
                return m_nMinValue;
            }
            set
            {
                m_nMinValue = value;
            }
        }

        public int m_nMax
        {
            get
            {
                return m_nMaxValue;
            }
            set
            {
                m_nMaxValue = value;
            }
        }

        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)Math.Round((double)value);

            if (intValue < m_nMinValue)
            {
                return m_nMinValue;
            }
            if (intValue > m_nMaxValue)
            {
                return m_nMaxValue;
            }

            return intValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }

    /// <summary>   Double to decimal converter.  </summary>
    /// <remarks>   suoow2, 2014-05-24. </remarks>
    [ValueConversion(typeof(double), typeof(decimal))]
    public class DoubleToDecimalConverter : IValueConverter
    {
        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal decimalValue = new Decimal((double)value);

            if (parameter != null)
            {
                decimalValue = Decimal.Round(decimalValue, Int32.Parse(parameter as string, CultureInfo.InvariantCulture));
            }

            return decimalValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Decimal.ToDouble((decimal)value);
        }
        #endregion
    }

    /// <summary>   Color to brush converter.  </summary>
    /// <remarks>   suoow2, 2014-05-24. </remarks>
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color)value;

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotSupportedException(this.GetType().Name);

        }
    }
}
