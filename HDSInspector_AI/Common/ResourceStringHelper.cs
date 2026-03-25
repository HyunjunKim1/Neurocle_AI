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
 * @file  ResourceStringHelper.cs
 * @brief
 *  string resource translator.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.05.11
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.11 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace Common
{
    /// <summary>   Resource string helper.  </summary>
    /// <remarks>   suoow2, 2014-05-11. </remarks>
    public class ResourceStringHelper
    {
        /// <summary> Manager for error message list </summary>
        private static ResourceManager m_ErrorMessageListManager = new ResourceManager("Common.Resource.ErrorMessageList", System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary> Manager for error english message list </summary>
        private static ResourceManager m_ErrorMessageListManager_eng = new ResourceManager("Common.Resource.ErrorMessageList_eng", System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary> Manager for information message list </summary>
        private static ResourceManager m_InformationMessageListManager = new ResourceManager("Common.Resource.InformationMessageList", System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary> Manager for information english message list </summary>
        private static ResourceManager m_InformationMessageListManager_eng = new ResourceManager("Common.Resource.InformationMessageList_eng", System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary>   Gets an error message. </summary>
        /// <remarks>   suoow2, 2014-05-11. </remarks>
        /// <param name="strKey">   The string key. </param>
        /// <returns>   The error message. </returns>
        public static string GetErrorMessage(string strKey ,bool useEng = false)
        {
            try
            {
                string strErrorMessage = (useEng) ? m_ErrorMessageListManager_eng.GetString(strKey) : m_ErrorMessageListManager.GetString(strKey);
                if (!string.IsNullOrEmpty(strErrorMessage))
                {
                    return strErrorMessage;
                }
                else
                {
                    throw new Exception();
                }

            }
            catch
            {
                return "Unknown Error Message.";
            }
        }

        /// <summary>   Gets an information message. </summary>
        /// <remarks>   suoow2, 2014-05-11. </remarks>
        /// <param name="strKey">   The string key. </param>
        /// <returns>   The information message. </returns>
        public static string GetInformationMessage(string strKey, bool useEng = false)
        {            
            try
            {                
                string strInformationMessage = (useEng) ? m_InformationMessageListManager_eng.GetString(strKey)  : m_InformationMessageListManager.GetString(strKey);
                if (!string.IsNullOrEmpty(strInformationMessage))
                {
                    return strInformationMessage;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                return "Unknown Information Message.";
            }
        }
    }
}
