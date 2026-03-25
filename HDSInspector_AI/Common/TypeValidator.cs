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
 * @file  TypeValidation.cs
 * @brief
 *  This class is necessary to validating default type value. 
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.08.31
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.07.29 First creation.
 * - 2011.08.31 Added IsNumeric method.
 */

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Common
{
    /// <summary>   Type validation.  </summary>
    /// <remarks>   suoow2, 2014-07-29. </remarks>
    public static class TypeValidator
    {
        public static bool ValidateDouble(string strValue, out double result)
        {
            return Double.TryParse(strValue, out result);
        }

        public static bool ValidateDouble(string strValue, double fMinValue, double fMaxValue, out double result)
        {
            if (ValidateDouble(strValue, out result))
            {
                if (fMinValue <= result && result <= fMaxValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool ValidateInteger(string strValue, out int result)
        {
            return Int32.TryParse(strValue, out result);
        }

        public static bool ValidateInteger(string strValue, int nMinValue, int nMaxValue, out int result)
        {
            if (ValidateInteger(strValue, out result))
            {
                if (nMinValue <= result && result <= nMaxValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // 숫자로 표현 가능한지 확인.
        public static bool IsNumeric(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                return false;
            }
            else
            {
                Regex numericPattern = new Regex(@"^(\+|-)?\d+$");

                return numericPattern.IsMatch(strValue);
            }
        }

        // Decimal 값으로 변환 가능한지 확인.
        public static bool IsDecimal(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                return false;
            }
            else
            {
                Regex numericPattern = new Regex(@"^[-+]?\d*\.?\d*$");

                return numericPattern.IsMatch(strValue);
            }
        }
    }

    /// <summary>   Value validator.  </summary>
    /// <remarks>   suoow2, 2012-02-10. </remarks>
    public static class ValueValidator
    {
        public static bool IsValidTextBoxValue(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {                
                MessageBox.Show("Please enter a value.", "Information");
                textBox.Focus();
                return false;
            }
            return true;
        }
    }
}
