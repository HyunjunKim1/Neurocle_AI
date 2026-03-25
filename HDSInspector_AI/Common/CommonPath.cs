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
 * @file  CommonPath.cs
 * @brief
 *  It contains informations of current application. 
 * 
 * @author : suoow <suoow.yeo@haesung.net>
 * @date : 2011.08.01
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.04.04 First creation.
 */
using System;
using System.IO;

namespace Common
{
    /// <summary>   Common path.  </summary>
    /// <remarks>   Sungbok, Hong. 2014-08-01. </remarks>
    public class CommonPath
    {
        private string m_AppPath;

        public string GetAppPath()
        {
            if (string.IsNullOrEmpty(m_AppPath))
            {
                m_AppPath = Directory.GetCurrentDirectory();
            }

            return m_AppPath;
        }

        public string GetConfigFileName()
        {
            return GetConfigPath() + "\\Settings.xml";
        }

        public string GetConfigPath()
        {
            string ConfigPath = GetAppPath() + "\\..\\config";

            FileSupport fs = new FileSupport();
            if (!fs.IsDirectory(ConfigPath))
            {
                ConfigPath = GetAppPath() + "\\config";
            }

            return ConfigPath;
        }
    }
}
