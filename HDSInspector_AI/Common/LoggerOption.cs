/*********************************************************************************
 * Copyright(c) 2011,2012,2013 by Samsung Techwin.
 * 
 * This software is copyrighted by, and is the sole property of Samsung Techwin.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Samsung Techwin. This software may only be used in accordance with
 * the corresponding license agreement. Any unauthorized use, duplication, 
 * transmission, distribution, or disclosure of this software is expressly 
 * forbidden.
 *
 * This Copyright notice may not be removed or modified without prior written
 * consent of Samsung Techwin.Samsung Techwin reserves the right to modify this 
 * software without notice.
 *
 * Samsung Techwin.
 * KOREA 
 * http://www.samsungtechwin.co.kr
 *********************************************************************************/
/**
 * @file  LoggerOption.cs
 * @brief
 *  it contains option of logger module.
 * 
 * @author : Minseok Hwang <h.min-suck@samsung.com>
 * @date : 2011.05.11
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.09 First creation.
 * - 2011.05.11 add clean log function. (by keep date.)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/**
 * @brief PCS.
 * @author Minseok Hwang
 * @date 2011.05.02
*/
namespace Common
{
    /** 
     * @brief option of logger module.
     * @author Minseok Hwang
     * @date 2011.05.09
     */
    public class LoggerOption
    {
        #region LoggerOptions's member variables.
        /**
         * @var m_bLocalSave
         * @brief bool value of local save log option.
        */
        private bool m_bLocalSave = false;
        public bool LocalSave
        {
            get { return m_bLocalSave;}
            set { m_bLocalSave = value;}
        }

        /**
         * @var m_bRemoteLog
         * @brief bool value of remote log option.
        */
        public bool m_bRemoteLog
        {
            get;
            set;
        }

        /**
         * @var m_nKeepDate
         * @brief keeping day of log files.
        */
        public long m_nKeepDate
        {
            get;
            set;
        }

        /**
         * @var m_enumLocalSaveLevel
         * @brief minimum save severity log level.
        */
        public SeverityLevel m_enumLocalSaveLevel;

        /**
         * @var m_enumUIDisplayLevel
         * @brief minimum display severity log level.
        */
        public SeverityLevel m_enumUIDisplayLevel;

        /**
         * @var m_strStartUpPath
         * @brief start up path.
        */
        private static readonly string m_strStartUpPath = Directory.GetCurrentDirectory();

        /**
         * @var m_strDirectoryName
         * @brief log save path.
        */
        private static string m_strDirectoryName = "Log";
        #endregion

        /**
         * @fn LoggerOption()
         * @brief default constructor.
         */
        public LoggerOption()
        {
            SetDirectoryName();
            LoadLogOptions();
        }

        /**
         * @fn SetDirectoryName()
         * @brief set directory name of saving logs.
         */
        private void SetDirectoryName()
        {
            m_strDirectoryName = Path.Combine(m_strStartUpPath, m_strDirectoryName);
        }

        /**
         * @fn LoadLogOptions()
         * @brief load log options from the configure file.
         * @todo define log configure file and load from it.
         */
        private void LoadLogOptions()
        {
            m_bLocalSave = true;
            m_bRemoteLog = false;
            m_nKeepDate = 30;

            m_enumLocalSaveLevel = SeverityLevel.DEBUG;
            m_enumUIDisplayLevel = SeverityLevel.DEBUG;
        }

        /**
         * @fn CleanLog()
         * @brief clean log by keep date.
         * @return delete log file counts.
         */
        public int CleanLog()
        {
            if (Directory.Exists(m_strDirectoryName) == false)
            {
                Directory.CreateDirectory(m_strDirectoryName);
            }

            string[] files = Directory.GetFiles(m_strDirectoryName, "*.log");

            int nDeleteCount = 0;
            foreach (string file in files)
            {
                DateTime dateTime = File.GetLastWriteTime(file);

                try
                {
                    int nDays = (DateTime.Now.Year - dateTime.Year) * 365;
                    nDays += DateTime.Now.DayOfYear - dateTime.DayOfYear;

                    if (nDays > m_nKeepDate)
                    {
                        File.Delete(file);
                        nDeleteCount++;
                    }
                }
                catch
                {

                }
            }

            return nDeleteCount;
        }
    }
}
