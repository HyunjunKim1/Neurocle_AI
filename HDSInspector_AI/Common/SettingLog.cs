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
 * @file  SettingOption.cs
 * @brief
 *  Setting about Option node in Settings.xml file
 * 
 * @author : suoow <suoow.yeo@haesung.net>
 * @date : 2011.05.25
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.25 First creation.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Common
{
    public class SettingLog
    {
        #region Private member variables.
        private XmlSetting m_XmlSetting = null;

        private string m_key = "Log";
        private static string m_strLogPath = "log";
        private static readonly string m_strStartUpPath = ((DirectoryInfo)Directory.GetParent(Directory.GetCurrentDirectory())).FullName;

        private long m_LocalSave = 1;
        private long m_RemoteSave;
        private long m_keepDate = 30;
        private long m_LocalSaveLevel;
        private long m_UIDisplayLevel;
        #endregion

        #region Constructor.
        /// <summary>   Initializes a new instance of the SettingLog class. </summary>
        /// <remarks>   suoow, 2014-05-25. </remarks>
        /// <param name="aXmlSetting">  The xml setting. </param>
        public SettingLog(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
            {
                aXmlSetting = new XmlSetting();
            }

            m_XmlSetting = aXmlSetting;

            SetDirectoryName();
        }
        #endregion

        /// <summary>   Sets the directory name. </summary>
        /// <remarks>   suoow2, 2014-05-25. </remarks>
        private static void SetDirectoryName()
        {
            m_strLogPath = Path.Combine(m_strStartUpPath, m_strLogPath);
        }

        /// <summary>   Gets the xml setting. </summary>
        /// <remarks>   suoow, 2014-05-25. </remarks>
        /// <returns>   The xml setting. </returns>
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        /// <summary>   Gets the clean log. </summary>
        /// <remarks>   suoow2, 2014-05-25. </remarks>
        /// <returns>   Delete count </returns>
        public int CleanLog()
        {
            if (Directory.Exists(m_strLogPath) == false)
            {
                Directory.CreateDirectory(m_strLogPath);
            }

            string[] files = Directory.GetFiles(m_strLogPath, "*.log");

            int nDeleteCount = 0;
            foreach (string file in files)
            {
                DateTime dateTime = File.GetLastWriteTime(file);

                try
                {
                    int nDays = (DateTime.Now.Year - dateTime.Year) * 365;
                    nDays += DateTime.Now.DayOfYear - dateTime.DayOfYear;

                    if (nDays > KeepDate)
                    {
                        File.Delete(file);
                        nDeleteCount++;
                    }
                }
                catch
                {
                    Debug.WriteLine("Exception occured in CleanLog(SettingLog.cs)");
                }
            }

            return nDeleteCount;
        }

        #region Properties.
        public long LocalSave
        {
            get 
            {
                return m_LocalSave = m_XmlSetting.GetSettingLong(m_key, "LocalSave", "1"); 
            }
            set 
            { 
                m_LocalSave = value; 
            }
        }

        public long RemoteSave
        {
            get 
            { 
                return m_RemoteSave = m_XmlSetting.GetSettingLong(m_key, "RemoteSave", "0");
            }
            set 
            { 
                m_RemoteSave = value; 
            }
        }

        public long KeepDate
        {
            get
            {
                return m_keepDate = m_XmlSetting.GetSettingLong(m_key, "KeepDate", "30");
            }
            set 
            { 
                m_keepDate = value; 
            }
        }

        public long LocalSaveLevel
        {
            get
            {
                return m_LocalSaveLevel = m_XmlSetting.GetSettingLong(m_key, "LocalSaveLevel", "0");
            }
            set 
            { 
                m_LocalSaveLevel = value; 
            }
        }

        public long UIDisplayLevel
        {
            get
            {
                return m_UIDisplayLevel = m_XmlSetting.GetSettingLong(m_key, "UIDisplayLevel", "0");
            }
            set 
            { 
                m_UIDisplayLevel = value; 
            }
        }
        #endregion
    }
}
