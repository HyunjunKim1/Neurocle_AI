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
 * @author : Cheol Min <suoow.yeo@haesung.net>
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

namespace Common
{
    /// <summary>   Setting sub system.  </summary>
    /// <remarks>   suoow, 2014-05-25. </remarks>
    public class SettingSubSystem
    {
        private readonly XmlSetting m_XmlSetting;
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        private const int KEYPADCOUNT = 11;
        private const int MAX_CONNECTION = 50;

        public SettingSubSystem(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
            {
                aXmlSetting = new XmlSetting();
                Load();
            }

            m_XmlSetting = aXmlSetting;
        }

        #region Properties.
        //POP
        public bool     UsePOP { get; set; }
        public string   POPIP { get; set; }
        public string POP_BKIP { get; set; }
        public string   POPSite { get; set; }
        public int      POPPort { get; set; }

        // Database
        public String UseDB { get; set; }
        public String DBIP { get; set; }
        public String DBPort { get; set; }

        // RVS
        public String UseRVS { get; set; }
        public String RVSIP { get; set; }
        public String RVSPort { get; set; }

        // IS Type.
        public int ISType { get; set; }
        public int ISType2 { get; set; }
        public int CameraWidth { get; set; }
        public int CameraWidth2 { get; set; }
        public int CameraHeight { get; set; }
        public String DeviceName { get; set; }
        public String DeviceName2 { get; set; }
        public String CamFile { get; set; }
        public String CamFile2 { get; set; }
        public int TestID { get; set; }


        // File Server.
        public String FileServerIP { get; set; }
        public String FileServerPort { get; set; }

        // RVS Setting.
        public int ConnectionCount { get; set; }
        public int InnerResolution { get; set; }
        public int CenterResolution { get; set; }
        public int OuterResolution { get; set; }
        public string LoadNumber { get; set; }

        #region Equip Name
        private string[] m_arrEquipName = new string[MAX_CONNECTION];

        public void SetEquipName(int anIndex, string anValue)
        {
            m_arrEquipName = EquipName;
            m_arrEquipName[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "EquipName" + (anIndex + 1).ToString("D2"), m_arrEquipName[anIndex]);
        }
        public string[] EquipName
        {
            get
            {
                return m_arrEquipName;
            }
            set
            {
                m_arrEquipName = value;
                for (int i = 0; i < m_arrEquipName.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "EquipName" + (i + 1).ToString("D2"), m_arrEquipName[i]);
            }
        }
        #endregion

        #region Equip IP
        private string[] m_arrEquipIP = new string[MAX_CONNECTION];

        public void SetEquipIP(int anIndex, string anValue)
        {
            m_arrEquipIP = EquipIP;
            m_arrEquipIP[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "EquipIP" + (anIndex + 1).ToString("D2"), m_arrEquipIP[anIndex]);
        }
        public string[] EquipIP
        {
            get
            {
                return m_arrEquipIP;
            }
            set
            {
                m_arrEquipIP = value;
                for (int i = 0; i < m_arrEquipIP.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "EquipIP" + (i + 1).ToString("D2"), m_arrEquipIP[i]);
            }
        }
        #endregion

        #region KeyPad Name
        private string[] m_arrKeyPadName = new string[KEYPADCOUNT];

        public void SetKeyPadName(int anIndex, string anValue)
        {
            m_arrKeyPadName = KeyPadName;
            m_arrKeyPadName[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "KeyPadName" + (anIndex + 1).ToString("D2"), m_arrKeyPadName[anIndex]);
        }
        public string[] KeyPadName
        {
            get
            {
                return m_arrKeyPadName;
            }
            set
            {
                m_arrKeyPadName = value;
                for (int i = 0; i < m_arrKeyPadName.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "KeyPadName" + (i + 1).ToString("D2"), m_arrKeyPadName[i]);
            }
        }
        #endregion

        #region DefectCode
        private string[] m_arrDefectCode = new string[KEYPADCOUNT+1];           // +1은 AutoNG 땜에 가상으로 하나 추가함

        public void SetDefectCode(int anIndex, string anValue)
        {
            m_arrDefectCode = DefectCode;
            m_arrDefectCode[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "DefectCode" + (anIndex + 1).ToString("D2"), m_arrDefectCode[anIndex]);
        }
        public string[] DefectCode
        {
            get
            {
                return m_arrDefectCode;
            }
            set
            {
                m_arrDefectCode = value;
                for (int i = 0; i < m_arrDefectCode.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "DefectCode" + (i + 1).ToString("D2"), m_arrDefectCode[i]);
            }
        }
        #endregion

        #region FKeyPadName
        private string[] m_arrFKeyPadName = new string[KEYPADCOUNT];

        public void SetFKeyPadName(int anIndex, string anValue)
        {
            m_arrFKeyPadName = FKeyPadName;
            m_arrFKeyPadName[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "FKeyPadName" + (anIndex + 1).ToString("D2"), m_arrFKeyPadName[anIndex]);
        }
        public string[] FKeyPadName
        {
            get
            {
                return m_arrFKeyPadName;
            }
            set
            {
                m_arrFKeyPadName = value;
                for (int i = 0; i < m_arrFKeyPadName.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "FKeyPadName" + (i + 1).ToString("D2"), m_arrFKeyPadName[i]);
            }
        }
        #endregion

        #region FDefectCode
        private string[] m_arrFDefectCode = new string[KEYPADCOUNT + 1];           // +1은 AutoNG 땜에 가상으로 하나 추가함

        public void SetFDefectCode(int anIndex, string anValue)
        {
            m_arrFDefectCode = FDefectCode;
            m_arrFDefectCode[anIndex] = anValue;
            m_XmlSetting.SetSettingString("SubSystem/RVS", "FDefectCode" + (anIndex + 1).ToString("D2"), m_arrFDefectCode[anIndex]);
        }
        public string[] FDefectCode
        {
            get
            {
                return m_arrFDefectCode;
            }
            set
            {
                m_arrFDefectCode = value;
                for (int i = 0; i < m_arrFDefectCode.Length; i++)
                    m_XmlSetting.SetSettingString("SubSystem/RVS", "FDefectCode" + (i + 1).ToString("D2"), m_arrFDefectCode[i]);
            }
        }
        #endregion
        #endregion

        #region Load & Save
        /// <summary>   Loads this object. </summary>
        /// /// <remarks>   suoow, 2014-05-25. </remarks>
        public void Load()
        {
            UsePOP = Convert.ToBoolean(m_XmlSetting.GetSettingInt("SubSystem/POP", "Use", "1"));
            POPIP = m_XmlSetting.GetSettingString("SubSystem/POP", "IP", "55.60.101.135");
            POP_BKIP = m_XmlSetting.GetSettingString("SubSystem/POP", "BK_IP", "55.60.101.141");
            POPSite = m_XmlSetting.GetSettingString("SubSystem/POP", "Site", "LF");
            POPPort = m_XmlSetting.GetSettingInt("SubSystem/POP", "Port", "3306");

            UseDB = m_XmlSetting.GetSettingString("SubSystem/Database", "Use", "1");
            DBIP = m_XmlSetting.GetSettingString("SubSystem/Database", "IP", "localhost");
            DBPort = m_XmlSetting.GetSettingString("SubSystem/Database", "Port", "3306");
            UseRVS = m_XmlSetting.GetSettingString("SubSystem/RVS", "UseRVS", "0");
            RVSIP = m_XmlSetting.GetSettingString("SubSystem/RVS", "IP", "localhost");
            RVSPort = m_XmlSetting.GetSettingString("SubSystem/RVS", "Port", "500006");
            ISType = m_XmlSetting.GetSettingInt("SubSystem/IS", "Type", "0");
            ISType2 = m_XmlSetting.GetSettingInt("SubSystem/IS", "Type2", "0");
            CameraWidth = m_XmlSetting.GetSettingInt("SubSystem/IS", "CameraWidth", "8192");
            CameraWidth2 = m_XmlSetting.GetSettingInt("SubSystem/IS", "CameraWidth2", "8192");
            CameraHeight = m_XmlSetting.GetSettingInt("SubSystem/IS", "CameraHeight", "20000");
            DeviceName = m_XmlSetting.GetSettingString("SubSystem/IS", "DeviceName", "System");
            DeviceName2 = m_XmlSetting.GetSettingString("SubSystem/IS", "DeviceName2", "System");
            CamFile = m_XmlSetting.GetSettingString("SubSystem/IS", "CamFile", "C:\\");
            CamFile2 = m_XmlSetting.GetSettingString("SubSystem/IS", "CamFile2", "C:\\");
            TestID = m_XmlSetting.GetSettingInt("SubSystem/IS", "TestID", "0");
            FileServerIP = m_XmlSetting.GetSettingString("SubSystem/FileServer", "IP", "localhost");
            FileServerPort = m_XmlSetting.GetSettingString("SubSystem/FileServer", "Port", "500005");
            ConnectionCount = m_XmlSetting.GetSettingInt("SubSystem/RVS", "ConnectionCount", "10");
            InnerResolution = m_XmlSetting.GetSettingInt("SubSystem/RVS", "InnerResolution", "100");
            CenterResolution = m_XmlSetting.GetSettingInt("SubSystem/RVS", "CenterResolution", "200");
            OuterResolution = m_XmlSetting.GetSettingInt("SubSystem/RVS", "OuterResolution", "300");
            LoadNumber = m_XmlSetting.GetSettingString("SubSystem/RVS", "LoadNumber", "0,0,0,0,0,0,0,0,0,0");

            for (int i = 0; i < m_arrEquipName.Length; i++)
                m_arrEquipName[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "EquipName" + (i + 1).ToString("D2"), "Equip" + (i + 1).ToString("D2"));
            for (int i = 0; i < m_arrEquipIP.Length; i++)
                m_arrEquipIP[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "EquipIP" + (i + 1).ToString("D2"), "127.0.0.1");
            for (int i = 0; i < m_arrKeyPadName.Length; i++)
                m_arrKeyPadName[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "KeyPadName" + (i + 1).ToString("D2"), "No." + (i + 1).ToString("D2"));
            for (int i = 0; i < m_arrDefectCode.Length; i++)
                m_arrDefectCode[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "DefectCode" + (i + 1).ToString("D2"), "No." + (i + 1).ToString("D2"));
            for (int i = 0; i < m_arrFKeyPadName.Length; i++)
                m_arrFKeyPadName[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "FKeyPadName" + (i + 1).ToString("D2"), "No." + (i + 1).ToString("D2"));
            for (int i = 0; i < m_arrFDefectCode.Length; i++)
                m_arrFDefectCode[i] = m_XmlSetting.GetSettingString("SubSystem/RVS", "FDefectCode" + (i + 1).ToString("D2"), "No." + (i + 1).ToString("D2"));
        }

        /// <summary>   Saves this object. </summary>
        /// <remarks>   suoow, 2014-05-25. </remarks>
        public void Save()
        {
            m_XmlSetting.SetSettingInt("SubSystem/POP", "Port", POPPort);
            m_XmlSetting.SetSettingString("SubSystem/POP", "Site", POPSite);
            m_XmlSetting.SetSettingString("SubSystem/POP", "IP", POPIP);
            m_XmlSetting.SetSettingString("SubSystem/POP", "BK_IP", POP_BKIP);
            m_XmlSetting.SetSettingString("SubSystem/POP", "Use", (UsePOP) ? "1" : "0");

            m_XmlSetting.SetSettingString("SubSystem/Database", "Use", UseDB);
            m_XmlSetting.SetSettingString("SubSystem/Database", "IP", DBIP);
            m_XmlSetting.SetSettingString("SubSystem/Database", "Port", DBPort);
            m_XmlSetting.SetSettingString("SubSystem/RVS", "UseRVS", UseRVS);
            m_XmlSetting.SetSettingString("SubSystem/RVS", "IP", RVSIP);
            m_XmlSetting.SetSettingString("SubSystem/RVS", "Port", RVSPort);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "Type", ISType);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "Type2", ISType2);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "CameraWidth", CameraWidth);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "CameraWidth2", CameraWidth2);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "CameraHeight", CameraHeight);
            m_XmlSetting.SetSettingString("SubSystem/IS", "DeviceName", DeviceName);
            m_XmlSetting.SetSettingString("SubSystem/IS", "DeviceName2", DeviceName2);
            m_XmlSetting.SetSettingString("SubSystem/IS", "CamFile", CamFile);
            m_XmlSetting.SetSettingString("SubSystem/IS", "CamFile2", CamFile2);
            m_XmlSetting.SetSettingInt("SubSystem/IS", "TestID", TestID);
            m_XmlSetting.SetSettingString("SubSystem/FileServer", "IP", FileServerIP);
            m_XmlSetting.SetSettingString("SubSystem/FileServer", "Port", FileServerPort);
            m_XmlSetting.SetSettingInt("SubSystem/RVS", "ConnectionCount", ConnectionCount);
            m_XmlSetting.SetSettingInt("SubSystem/RVS", "InnerResolution", InnerResolution);
            m_XmlSetting.SetSettingInt("SubSystem/RVS", "CenterResolution", CenterResolution);
            m_XmlSetting.SetSettingInt("SubSystem/RVS", "OuterResolution", OuterResolution);
            m_XmlSetting.SetSettingString("SubSystem/RVS", "LoadNumber", LoadNumber);
        }
        #endregion
    }
}
