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
 * @file  SettingMTS.cs
 * @brief
 *  Setting about Offline teaching SW.
 * 
 * @author : suoow <suoow.yeo@haesung.net>
 * @date : 2012.03.30
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2012.03.30 First creation.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class SettingMTS
    {
        private readonly XmlSetting m_XmlSetting;
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        public bool OffLineMode { get; set; }
        public string ModelPath { get; set; }
        public string VisionLocation { get; set; }
        public string Vision1ImagePath { get; set; }
        public string Vision2ImagePath { get; set; }
        public string Vision3ImagePath { get; set; }
        public int LastSelectedMachine { get; set; }
        public int LastSelectedGroup { get; set; }
        public int LastSelectedModel { get; set; }

        public SettingMTS(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
            {
                aXmlSetting = new XmlSetting();
                Load();
            }

            m_XmlSetting = aXmlSetting;
        }

        public void Load()
        {
            OffLineMode = m_XmlSetting.GetSettingBool("MTS", "OffLineMode", "0");
            ModelPath = m_XmlSetting.GetSettingString("MTS", "ModelPath", @"C:\ModelPath");
            VisionLocation = m_XmlSetting.GetSettingString("MTS/VISION", "VisionLocation", @"C:\IS\IS.exe");
            Vision1ImagePath = m_XmlSetting.GetSettingString("MTS/VISION", "Vision1ImagePath", @"C:\IS\IS-1\GrabImage_00.bmp");
            Vision2ImagePath = m_XmlSetting.GetSettingString("MTS/VISION", "Vision2ImagePath", @"C:\IS\IS-2\GrabImage_00.bmp");
            Vision3ImagePath = m_XmlSetting.GetSettingString("MTS/VISION", "Vision3ImagePath", @"C:\IS\IS-3\GrabImage_00.bmp");
            LastSelectedMachine = m_XmlSetting.GetSettingInt("MTS", "LastSelectedMachine", "0");
            LastSelectedGroup = m_XmlSetting.GetSettingInt("MTS", "LastSelectedGroup", "0");
            LastSelectedModel = m_XmlSetting.GetSettingInt("MTS", "LastSelectedModel", "0");
        }

        public void Save()
        {
            m_XmlSetting.SetSettingBool("MTS", "OffLineMode", OffLineMode);
            m_XmlSetting.SetSettingString("MTS", "ModelPath", ModelPath);
            m_XmlSetting.SetSettingString("MTS/VISION", "VisionLocation", VisionLocation);
            m_XmlSetting.SetSettingString("MTS/VISION", "Vision1ImagePath", Vision1ImagePath);
            m_XmlSetting.SetSettingString("MTS/VISION", "Vision2ImagePath", Vision2ImagePath);
            m_XmlSetting.SetSettingString("MTS/VISION", "Vision3ImagePath", Vision3ImagePath);
            m_XmlSetting.SetSettingInt("MTS", "LastSelectedMachine", LastSelectedMachine);
            m_XmlSetting.SetSettingInt("MTS", "LastSelectedGroup", LastSelectedGroup);
            m_XmlSetting.SetSettingInt("MTS", "LastSelectedModel", LastSelectedModel);
        }

        // XML 파일로부터 장비 이름에 해당되는 IP를 찾아 반환한다.
        public string TryLoadMachineIP(string aszMachineName)
        {
            if (string.IsNullOrEmpty(aszMachineName))
            {
                return "127.0.0.1";
            }
            else
            {
                return m_XmlSetting.GetSettingString(string.Format("MTS/{0}", aszMachineName), "IP", "127.0.0.1");
            }
        }

        // XML 파일에 장비의 IP를 기록한다.
        public void TrySaveMachineIP(string aszMachineName, string aszIPAddress)
        {
            if (!string.IsNullOrEmpty(aszMachineName))
            {
                m_XmlSetting.SetSettingString(string.Format("MTS/{0}", aszMachineName), "IP", aszIPAddress);
            }
        }
    }
}
