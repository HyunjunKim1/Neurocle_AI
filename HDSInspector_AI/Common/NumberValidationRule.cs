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
 * @file  NumberValidationRule.cs
 * @brief
 *  number input validator.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.05.20
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.20 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

/**
 * @brief Common
 * @author suoow2
 * @date 2011.05.02
*/
namespace Common
{
    /** 
     * @brief number input validator. it used by textbox control.
     * @author suoow2
     * @date 2011.05.20
     */
    public class NumberValidationRule : ValidationRule
    {
        /**
         * @fn Validate(object value, System.Globalization.CultureInfo cultureInfo)
         * @brief when it throws false the value is not number.
         * @return validation result
         * @see ValidationRule
         */
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int integerValue;
            if(!int.TryParse((string)value, out integerValue))
            {
                return new ValidationResult(false, null);
            }

            return ValidationResult.ValidResult;
        }
    }
}
