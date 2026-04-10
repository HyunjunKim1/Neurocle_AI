using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Media3D;
using System.Security.Cryptography;
using System.Runtime;

namespace Common
{
    public class Setting
    {
        private string m_Path = "";
        private string m_GeneralPath;
        private string m_DevicePath;
        private string m_JobPath;

        public string m_language_Path;
        
        public Generals General;
        public SubSystems SubSystem;

        public Setting(string astrPath)
        {
            m_Path = astrPath;
            m_GeneralPath = m_Path + "\\Setting.ini";
            m_DevicePath = m_Path + "\\SubSystem.ini";

            General = new Generals(m_GeneralPath);
            SubSystem = new SubSystems(m_DevicePath);
        }

        public bool Exists()
        {
            if (File.Exists(m_GeneralPath)) return true;
            return false;
        }

        public int Load()
        {
            int nRet = 0;
            bool bCreate = false;
            DateTime dt = DateTime.Now;
            string dir = string.Format($"{m_Path}\\Setting_Backup\\{dt.Year.ToString()}-{dt.Month.ToString()}");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                bCreate = true;
            }

            if (!General.Load())
            {
                nRet += 1;
            }
            else
            {
                if (bCreate)
                {
                    File.Copy(m_DevicePath, dir + "\\SubSystem.ini");
                }
            }
            if (!SubSystem.Load())
            {
                nRet += 2;
            }
            else
            {
                if (bCreate)
                {
                    File.Copy(m_GeneralPath, dir + "\\Setting.ini");
                }
            }
            return nRet;
        }

        public void Save()
        {
            General.Save();
            SubSystem.Save();
        }

        public void SettingConversion()
        {
            Settings settings = Settings.GetSettings();
            settings.Load();

            #region General

            if (settings.General.ColorAVI) 
                General.MachineType = 1;
            else General.MachineType = 0;

            General.CamType = settings.General.CamType;
            General.MachineIP = settings.General.MachineIP;
            General.ModelPath = settings.General.ModelPath;
            General.ResultPath = settings.General.ImagePath;

            General.Simulation = settings.General.Simulation;

            General.LogSave = settings.Log.LocalSave == 0 ? false : true;
            General.LogKeepDate = (int)settings.Log.KeepDate;

            General.MaxLimitDefect = 256;

            #endregion

            #region SubSystems

            SubSystem.Grabber.Name = "상부 컨트롤러";
            SubSystem.Grabber.Port = "COM3";
            SubSystem.Grabber.Maker = "플러스텍";
            SubSystem.Grabber.BaudRate = "115200";
            SubSystem.Grabber.ChannelType = "16";
            SubSystem.Grabber.Type = "0";

            #endregion

            settings = null;
            Save();
        }
    }

    public class Generals
    {
        #region Members
        public int MachineType;
        public string CamType;              // Camera Type
        public string MachineIP;            //설비의 IP

        public bool Simulation;

        public bool UseServeralScan;

        public string ModelPath;            //모델의 정보를 저장하는 경로
        public string ResultPath;           //검사 결과를 저장하는 경로

        public bool LogSave;
        public int LogKeepDate;

        public int MaxLimitDefect;
        #endregion

        private string m_Path;
        public Generals(string astrPath)
        {
            m_Path = astrPath;
        }
        public bool Load()
        {
            if (!File.Exists(m_Path))
            {
                FileStream fs = File.Create(m_Path);
                fs.Close();
                Save();
                return false;
            }

            IniFile ini = new IniFile(m_Path);
            MachineType         = ini.Read("MACHINE", "Type", 0);
            MachineIP           = ini.Read("MACHINE", "IP", "127.0.0.1");
            Simulation          = ini.Read("MACHINE", "Simulation", false);

            UseServeralScan     = ini.Read("MACHINE", "SeveralScan", false);

            ModelPath           = ini.Read("PATH", "Model", "d:\\Model");
            ResultPath          = ini.Read("PATH", "Result", "d:\\Result");

            LogSave             = ini.Read("LOG", "UseSave", true);
            LogKeepDate         = ini.Read("LOG", "KeepDate", 60);

            MaxLimitDefect      = ini.Read("INSPECT", "MaxLimitDefect", 512);

            return true;
        }

        public void Save()
        {
            if (!File.Exists(m_Path))
            {
                FileStream fs = File.Create(m_Path);
                fs.Close();
            }
            IniFile ini = new IniFile(m_Path);
            ini.Write("MACHINE", "Type", MachineType);
            ini.Write("MACHINE", "IP", MachineIP);
            ini.Write("MACHINE", "Simulation", Simulation);

            ini.Write("MACHINE", "SeveralScan", UseServeralScan);

            ini.Write("PATH", "Model", ModelPath);
            ini.Write("PATH", "Result", ResultPath);

            ini.Write("Log", "UseSave", LogSave);
            ini.Write("Log", "KeepDate", LogKeepDate);

            ini.Write("INSPECT", "MaxLimitDefect", MaxLimitDefect);
        }
    }

    public class SubSystems
    {
        public GrabberPara Grabber;
        private string m_Path;

        public SubSystems(string astrPath)
        {
            Grabber = new GrabberPara();
            m_Path = astrPath;
        }

        public bool Load()
        {
            if (!File.Exists(m_Path))
            {
                FileStream fs = File.Create(m_Path);
                fs.Close();
                Save();
                return false;
            }
            IniFile ini = new IniFile(m_Path);

            Grabber.Name          = ini.Read("GRABBER", "Name", "");
            Grabber.Port          = ini.Read("GRABBER", "Port", "");
            Grabber.Type          = ini.Read("GRABBER", "Type", "");
            Grabber.ChannelType   = ini.Read("GRABBER", "ChannelType", "");
            Grabber.Maker         = ini.Read("GRABBER", "Maker", "");
            Grabber.BaudRate      = ini.Read("GRABBER", "BaudRate", "");


            return true;
        }

        public void Save()
        {
            if (!File.Exists(m_Path))
            {
                FileStream fs = File.Create(m_Path);
                fs.Close();
            }
            IniFile ini = new IniFile(m_Path);

            ini.Write("GRABBER", "Name", Grabber.Name);
            ini.Write("GRABBER", "Port", Grabber.Port);
            ini.Write("GRABBER", "Type", Grabber.Type);
            ini.Write("GRABBER", "ChannelType", Grabber.ChannelType);
            ini.Write("GRABBER", "Maker", Grabber.Maker);
            ini.Write("GRABBER", "BaudRate", Grabber.BaudRate);

        }
    }

    public class GrabberPara
    {
        public string Name { get; set; } //상부 조명용, 하부 조명용 등
        public string Port { get; set; } // COM3, COM4 ...
        public string Type { get; set; } // 프로토콜 타입, 0:플러스텍, 1:알트, 2:알트 매크로
        public string ChannelType { get; set; } // 2,4,8,16,32, 총 채널 수
        public string Maker { get; set; } // 1,2,3,4,,,,
        public string BaudRate { get; set; } //9600, 116500...
    }
}
