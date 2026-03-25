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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common
{
    /// <summary>   Interface for setting.  </summary>
    /// <remarks>   suoow2, 2014-08-24. </remarks>
    public interface ISetting
    {
        bool IsValidate(); // Setting 값이 올바른지를 확인한다.
        bool CheckSave(); // Save가 필요한 경우인지 확인한다.
        void Save();
        void TrySave(); // 입력 값이 올바른 항목만 저장한다.
    }

    /// <summary>   Settings.  </summary>
    public class Settings
    {
        #region Private member variables.
        private readonly static Settings _Instance = new Settings();
        private readonly static CommonPath m_Path = new CommonPath();
        private readonly static XmlSetting m_XmlSetting = new XmlSetting();
        private static SettingGeneral m_General;
        private static SettingSubSystem m_SubSystem;
        private static SettingDevice m_Device;
        private static SettingLog m_Log;
        private static SettingMTS m_MTS;
        //private static SettingMachines m_Machines; 
        #endregion

        /// <summary>   Gets the xml setting. </summary>
        /// <returns>   The xml setting. </returns>
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        /// <summary>   Gets the common path. </summary>
        /// <returns>   The common path. </returns>
        public CommonPath GetCommonPath()
        {
            return m_Path;
        }

        public bool Load()
        {
            String xmlFile = GetCommonPath().GetConfigFileName();

            FileSupport.ForceDirectories(xmlFile);

            if (!m_XmlSetting.Initialize(xmlFile))
                return false;

            General.Load();
            Device.Load();
            SubSystem.Load();
            MTS.Load();
            //Machines.Load();

            return true;
        }

        public void Save()
        {
            General.Save();
            Device.Save();
            SubSystem.Save();
            MTS.Save();
            //Machines.Save();
            
            m_XmlSetting.Flush();
        }

        public static Settings GetSettings()
        {
            return _Instance;
        }

        #region Properties.
        // general setting.
        public SettingGeneral General
        {
            get
            {
                if (m_General == null)
                {
                    m_General = new SettingGeneral(m_XmlSetting);
                }

                return m_General;
            }
            set
            {
                m_General = value;
            }
        }

        // subsystem setting.
        public SettingSubSystem SubSystem
        {
            get
            {
                if (m_SubSystem == null)
                {
                    m_SubSystem = new SettingSubSystem(m_XmlSetting);
                }

                return m_SubSystem;
            }
            set
            {
                m_SubSystem = value;
            }
        }

        // device setting.
        public SettingDevice Device
        {
            get
            {
                if (m_Device == null)
                {
                    m_Device = new SettingDevice(m_XmlSetting);
                }

                return m_Device;
            }
            set
            {
                m_Device = value;
            }
        }

        // log setting.
        public SettingLog Log
        {
            get
            {
                if (m_Log == null)
                {
                    m_Log = new SettingLog(m_XmlSetting);
                }

                return m_Log;
            }
            set
            {
                m_Log = value;
            }
        }

        // mts setting.
        public SettingMTS MTS
        {
            get
            {
                if (m_MTS == null)
                {
                    m_MTS = new SettingMTS(m_XmlSetting);
                }

                return m_MTS;
            }
            set
            {
                m_MTS = value;
            }
        }

        // machines setting.
        //public SettingMachines Machines
        //{
        //    get
        //    {
        //        if (m_Machines == null)
        //        {
        //            m_Machines = new SettingMachines(m_XmlSetting);
        //        }

        //        return m_Machines;
        //    }
        //    set
        //    {
        //        m_Machines = value;
        //    }
        //}
        #endregion
    }
}
