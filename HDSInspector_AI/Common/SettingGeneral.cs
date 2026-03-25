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
using System.Runtime;

namespace Common
{
    /// <summary>   Setting general.  </summary>
    /// <remarks>   suoow, 2014-05-25. </remarks>
    public class SettingGeneral
    {
        private readonly XmlSetting m_XmlSetting;
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        public SettingGeneral(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
            {
                aXmlSetting = new XmlSetting();
                Load();
            }

            m_XmlSetting = aXmlSetting;
        }

        /// <summary>   Loads this object. </summary>
        /// <remarks>   suoow, 2014-05-25. </remarks>
        public void Load()
        {
            MachineType = m_XmlSetting.GetSettingString("General", "MachineType", "ELF_AVI");
            MachineIP = m_XmlSetting.GetSettingString("General", "IP", "192.168.30.160");
            MachinePort = m_XmlSetting.GetSettingString("General", "Port", "50002");
            MachineCode = m_XmlSetting.GetSettingString("General", "MachineCode", "0001");
            MachineName = m_XmlSetting.GetSettingString("General", "MachineName", "EAV19");
            SaveDefectData = m_XmlSetting.GetSettingLong("General", "SaveDefectData", "1");
            SaveAI = m_XmlSetting.GetSettingLong("General", "SaveAI", "0");
            TestInspectMode = m_XmlSetting.GetSettingBool("General", "SetupMode", "1");
            ScanVelocity = m_XmlSetting.GetSettingInt("General", "ScanVelocity", "300");
            ImagePath = m_XmlSetting.GetSettingString("General", "ImagePath", @"C:\\ImagePath\\");
            ModelPath = m_XmlSetting.GetSettingString("General", "ModelPath", @"C:\\ModelPath\\");
            VerifyPath = m_XmlSetting.GetSettingString("General", "VerifyPath", "z:");
            SaveServer = m_XmlSetting.GetSettingBool("General", "SaveServer", "0");
            RejectRate = m_XmlSetting.GetSettingDouble("General", "Reject_Rate", "10");
            RejectValue = m_XmlSetting.GetSettingDouble("General", "Reject_Value", "10");
            LaserEnable = m_XmlSetting.GetSettingBool("General", "LaserEnable", "1");
            m_VisionIP[0] = m_XmlSetting.GetSettingString("General", "VisionIP1", "127.0.0.1");
            m_VisionIP[1] = m_XmlSetting.GetSettingString("General", "VisionIP2", "127.0.0.1");
            m_VisionIP[2] = m_XmlSetting.GetSettingString("General", "VisionIP3", "127.0.0.1");
            m_VisionPort[0] = m_XmlSetting.GetSettingInt("General", "VisionPort1", "15000");
            m_VisionPort[1] = m_XmlSetting.GetSettingInt("General", "VisionPort2", "15000");
            m_VisionPort[2] = m_XmlSetting.GetSettingInt("General", "VisionPort3", "15000");
            m_ResolutionX[0] = m_XmlSetting.GetSettingDouble("General", "ResolutionX1", "12");
            m_ResolutionX[1] = m_XmlSetting.GetSettingDouble("General", "ResolutionX2", "12");
            m_ResolutionX[2] = m_XmlSetting.GetSettingDouble("General", "ResolutionX3", "12");
            m_ResolutionY[0] = m_XmlSetting.GetSettingDouble("General", "ResolutionY1", "12");
            m_ResolutionY[1] = m_XmlSetting.GetSettingDouble("General", "ResolutionY2", "12");
            m_ResolutionY[2] = m_XmlSetting.GetSettingDouble("General", "ResolutionY3", "12");
            m_PageDelay[0] = m_XmlSetting.GetSettingInt("General", "PageDelay1", "100");
            m_PageDelay[1] = m_XmlSetting.GetSettingInt("General", "PageDelay2", "100");
            m_PageDelay[2] = m_XmlSetting.GetSettingInt("General", "PageDelay3", "100");
            AlignResolution = m_XmlSetting.GetSettingDouble("General", " AlignResolution", "22");
            LastSelectedGroup = m_XmlSetting.GetSettingInt("General", "LastSelectedGroup", "-1");
            LastSelectedModel = m_XmlSetting.GetSettingString("General", "LastSelectedModel", "");
            IsUseRejectRate = m_XmlSetting.GetSettingInt("General", "IsUseRejectRate", "1");
            LastLot = m_XmlSetting.GetSettingString("General", "LastLot", "Lot");
            LastPaperLot = m_XmlSetting.GetSettingString("General", "LastPaperLot", "Lot");
            LastPaperInfo = m_XmlSetting.GetSettingString("General", "LastPaperInfo", "Lot");
            LastUser = m_XmlSetting.GetSettingString("General", "LastUser", "DS");
            LastInspect = m_XmlSetting.GetSettingString("General", "LastInspect", "1,1,1");
            SAPPath = m_XmlSetting.GetSettingString("General", "SAPPath", @"D:\\loss\\");
            ProcessCode = m_XmlSetting.GetSettingString("General", "ProcessCode", "VI21");
            PLCResultType = m_XmlSetting.GetSettingInt("General", "PLCResultType", "0");
            PLCPassType = m_XmlSetting.GetSettingInt("General", "PLCPassType", "0");
            AlignAlgo[0] = m_XmlSetting.GetSettingInt("General", "AlignAlgo1", "2");
            AlignAlgo[1] = m_XmlSetting.GetSettingInt("General", "AlignAlgo2", "2");
            AlignAlgo[2] = m_XmlSetting.GetSettingInt("General", "AlignAlgo3", "0");
            AlignThres[0] = m_XmlSetting.GetSettingInt("General", "AlignThres1", "30");
            AlignThres[1] = m_XmlSetting.GetSettingInt("General", "AlignThres2", "30");
            AlignThres[2] = m_XmlSetting.GetSettingInt("General", "AlignThres3", "30");
            RevNo = m_XmlSetting.GetSettingString("General", "Rev_No", "전체");
            CurrRev = m_XmlSetting.GetSettingString("General", "Curr_Rev", "R7");
            SelectedRev = m_XmlSetting.GetSettingInt("General", "SelectedRev", "0");
            RevPW = m_XmlSetting.GetSettingBool("General", "UseRevPassword", "0");
            RevPassword = m_XmlSetting.GetSettingString("General", "RevPassword", "");
            UseAutoTemplate = m_XmlSetting.GetSettingBool("General", "UseAutoTemplate", "0");
            BottomPlate = m_XmlSetting.GetSettingBool("General", "BottomPlate", "0");
            TemplatePath = m_XmlSetting.GetSettingString("General", "TemplatePath", @"\\192.168.30.96\Template\");
            TemplateNetDriveName = m_XmlSetting.GetSettingString("General", "TemplateNetDriveName", "Y:");
            TemplateNetDriveIP = m_XmlSetting.GetSettingString("General", "TemplateNetDriveIP", "\\192.168.30.96");
            TemplateNetDriveID = m_XmlSetting.GetSettingString("General", "TemplateNetDriveID", "admin");
            TemplateNetDrivePW = m_XmlSetting.GetSettingString("General", "TemplateNetDrivePW", "qwer1234!");
            AINetDriveName = m_XmlSetting.GetSettingString("General", "AINetDriveName", "Z:");
            AINetDriveIP = m_XmlSetting.GetSettingString("General", "AINetDriveIP", "\\127.0.0.1");
            AINetDriveID = m_XmlSetting.GetSettingString("General", "AINetDriveID", "admin");
            AINetDrivePW = m_XmlSetting.GetSettingString("General", "AINetDrivePW", "qwer1234!");
            ColorAVI = m_XmlSetting.GetSettingBool("General", "ColorAVI", "0");
        }

        /// <summary>   Saves this object. </summary>
        /// <remarks>   suoow, 2014-05-25. </remarks>
        public void Save()
        {
            m_XmlSetting.SetSettingString("General", "MachineType", MachineType);
            m_XmlSetting.SetSettingString("General", "IP", MachineIP);
            m_XmlSetting.SetSettingString("General", "Port", MachinePort);
            m_XmlSetting.SetSettingString("General", "MachineCode", MachineCode);
            m_XmlSetting.SetSettingString("General", "MachineName", MachineName);
            m_XmlSetting.SetSettingLong("General", "SaveDefectData", SaveDefectData);
            m_XmlSetting.SetSettingLong("General", "SaveAI", SaveAI);
            m_XmlSetting.SetSettingBool("General", "SetupMode", TestInspectMode);
            m_XmlSetting.SetSettingInt("General", "ScanVelocity", ScanVelocity);
            m_XmlSetting.SetSettingString("General", "ImagePath", ImagePath);
            m_XmlSetting.SetSettingString("General", "ModelPath", ModelPath);
            m_XmlSetting.SetSettingBool("General", "SaveServer", SaveServer);
            m_XmlSetting.SetSettingString("General", "VerifyPath", VerifyPath);
            m_XmlSetting.SetSettingDouble("General", "Reject_Rate", RejectRate);
            m_XmlSetting.SetSettingDouble("General", "Reject_Value", RejectValue);
            m_XmlSetting.SetSettingBool("General", "LaserEnable", LaserEnable);
            m_XmlSetting.SetSettingString("General", "VisionIP1", m_VisionIP[0]);
            m_XmlSetting.SetSettingString("General", "VisionIP2", m_VisionIP[1]);
            m_XmlSetting.SetSettingString("General", "VisionIP3", m_VisionIP[2]);
            m_XmlSetting.SetSettingInt("General", "VisionPort1", m_VisionPort[0]);
            m_XmlSetting.SetSettingInt("General", "VisionPort2", m_VisionPort[1]);
            m_XmlSetting.SetSettingInt("General", "VisionPort3", m_VisionPort[2]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionX1", m_ResolutionX[0]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionX2", m_ResolutionX[1]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionX3", m_ResolutionX[2]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionY1", m_ResolutionY[0]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionY2", m_ResolutionY[1]);
            m_XmlSetting.SetSettingDouble("General", "ResolutionY3", m_ResolutionY[2]);
            m_XmlSetting.SetSettingInt("General", "PageDelay1", m_PageDelay[0]);
            m_XmlSetting.SetSettingInt("General", "PageDelay2", m_PageDelay[1]);
            m_XmlSetting.SetSettingInt("General", "PageDelay3", m_PageDelay[2]);
            m_XmlSetting.SetSettingDouble("General", " AlignResolution", AlignResolution);
            m_XmlSetting.SetSettingInt("General", "LastSelectedGroup", LastSelectedGroup);
            m_XmlSetting.SetSettingString("General", "LastSelectedModel", LastSelectedModel);
            m_XmlSetting.SetSettingInt("General", "IsUseRejectRate", IsUseRejectRate);
            m_XmlSetting.SetSettingString("General", "LastLot", LastLot);
            m_XmlSetting.SetSettingString("General", "LastPaperLot", LastPaperLot);
            m_XmlSetting.SetSettingString("General", "LastPaperInfo", LastPaperInfo);
            m_XmlSetting.SetSettingString("General", "LastUser", LastUser);
            m_XmlSetting.SetSettingString("General", "LastInspect", LastInspect);
            m_XmlSetting.SetSettingString("General", "SAPPath", SAPPath);
            m_XmlSetting.SetSettingString("General", "ProcessCode", ProcessCode);
            m_XmlSetting.SetSettingInt("General", "PLCResultType", PLCResultType);
            m_XmlSetting.SetSettingInt("General", "PLCPassType", PLCPassType);
            m_XmlSetting.SetSettingInt("General", "AlignAlgo1", AlignAlgo[0]);
            m_XmlSetting.SetSettingInt("General", "AlignAlgo2", AlignAlgo[1]);
            m_XmlSetting.SetSettingInt("General", "AlignAlgo3", AlignAlgo[2]);
            m_XmlSetting.SetSettingInt("General", "AlignThres1", AlignThres[0]);
            m_XmlSetting.SetSettingInt("General", "AlignThres2", AlignThres[1]);
            m_XmlSetting.SetSettingInt("General", "AlignThres3", AlignThres[2]);
            m_XmlSetting.SetSettingString("General", "Rev_No", RevNo);
            m_XmlSetting.SetSettingInt("General", "SelectedRev", SelectedRev);
            m_XmlSetting.SetSettingString("General", "Curr_Rev", CurrRev);
            m_XmlSetting.SetSettingBool("General", "UseRevPassword", RevPW);
            m_XmlSetting.SetSettingString("General", "RevPassword", RevPassword);
            m_XmlSetting.SetSettingBool("General", "UseAutoTemplate", UseAutoTemplate);

            m_XmlSetting.SetSettingBool("General", "BottomPlate", BottomPlate);
            m_XmlSetting.SetSettingString("General", "TemplatePath", TemplatePath);
            m_XmlSetting.SetSettingBool("General", "ColorAVI", ColorAVI);

        }

        #region Properties.
        public String MachineType { get; set; }
        public String MachineIP { get; set; }
        public String MachinePort { get; set; }
        public String MachineCode { get; set; }
        public String MachineName { get; set; }
        public String ModelPath { get; set; }
        public String ImagePath { get; set; }
        public String VerifyPath { get; set; }
        public String SAPPath { get; set; }
        public String ProcessCode { get; set; }

        public bool SaveServer{ get; set;}
        public bool UseAutoTemplate { get; set; }
        public long SaveDefectData { get; set; }
        public long SaveAI { get; set; }
        public bool TestInspectMode { get; set; }
        public int ScanVelocity { get; set; }

        public int IsUseRejectRate { get; set; }
        public double RejectRate { get; set; }
        public double RejectValue { get; set; }
        public string RevNo { get; set; }
        public int SelectedRev { get; set; }
        public string CurrRev { get; set; }
        public string RevPassword { get; set; }
        public bool RevPW { get; set; }

        public string[] VisionIP
        {
            get { return m_VisionIP; }
            set { m_VisionIP = value; }
        }

        public int[] VisionPort
        {
            get { return m_VisionPort; }
            set { m_VisionPort = value; }
        }

        public double[] ResolutionX
        {
            get { return m_ResolutionX; }
            set { m_ResolutionX = value; }
        }

        public double[] ResolutionY
        {
            get { return m_ResolutionY; }
            set { m_ResolutionY = value; }
        }

        public int PLCResultType;

        public int PLCPassType;

        public bool BottomPlate { get; set; }
        public bool ColorAVI { get; set; }

        public string TemplatePath { get; set; }

        public string TemplateNetDriveName { get; set; }

        public string TemplateNetDriveIP { get; set; }

        public string TemplateNetDriveID { get; set; }

        public string TemplateNetDrivePW { get; set; }

        public string AINetDriveName { get; set; }
                      
        public string AINetDriveIP { get; set; }
                      
        public string AINetDriveID { get; set; }
                      
        public string AINetDrivePW { get; set; }

        public int[] AlignAlgo = new int[3];

        public int[] AlignThres = new int[3];

        public int[] PageDealy
        {
            get { return m_PageDelay; }
            set { m_PageDelay = value; }
        }

        public bool LaserEnable { get; set; }
        public double AlignResolution { get; set; }

        public int LastSelectedGroup { get; set; }
        //public int LastSelectedModel { get; set; }
        public string LastSelectedModel { get; set; }
        public string LastLot { get; set; }
        public string LastPaperLot { get; set; }
        public string LastPaperInfo { get; set; }
        public string LastUser { get; set; }
        public string LastInspect { get; set; }
        #endregion

        #region Private member variables.
        private double[] m_ResolutionX = new double[3];
        private double[] m_ResolutionY = new double[3];
        private string[] m_VisionIP = new string[3];
        private int[] m_VisionPort = new int[3];
        private int[] m_PageDelay = new int[3];
        #endregion
    }
}
