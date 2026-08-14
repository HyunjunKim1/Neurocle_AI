using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Media3D;
using System.Security.Cryptography;
using System.Runtime;
using System.Xml.Schema;
using System.Runtime.CompilerServices;

namespace Common
{
    public class Setting
    {
        private string m_Path = "";
        private string m_GeneralPath;
        private string m_DevicePath;

        private string _neuroclePath;
        private string _defectSpecPath;

        public string m_language_Path;
        
        public Generals             General;
        public SubSystems           SubSystem;
        public Neurocles            Neurocle;
        public DefectSpecSettings   DefectSpec;
        public InferenceSettings    Inference;


        public Setting(string astrPath)
        {
            m_Path = astrPath;
            m_GeneralPath = m_Path + "\\Setting.ini";
            m_DevicePath = m_Path + "\\SubSystem.ini";

            _neuroclePath = "\\Neurocle.ini";
            _defectSpecPath = "\\DefectSpec.ini";

            General     = new Generals(m_GeneralPath);
            SubSystem   = new SubSystems(m_DevicePath);

            Neurocle    = new Neurocles(_neuroclePath);
            Inference   = new InferenceSettings(_neuroclePath);
            DefectSpec  = new DefectSpecSettings(_defectSpecPath);
        }

        public bool Exists()
        {
            if (File.Exists(m_GeneralPath)) return true;
            return false;
        }

        public bool Load()
        {
            bool ReadSucc = true;

            ReadSucc &= General.Load();
            ReadSucc &= SubSystem.Load();
            ReadSucc |= Neurocle.Load();
            ReadSucc &= Inference.Load();
            ReadSucc &= DefectSpec.Load();

            return ReadSucc;
        }

        public void Save()
        {
            General.Save();
            SubSystem.Save();
            Neurocle.Save();
            Inference.Save(); 
            DefectSpec.Save();
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
        public string MachineIP;            // Main 설비의 IP
        public int MachinePort;             // Main 설비 포트

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
            MachinePort         = ini.Read("MACHINE", "Port", 500);
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
            ini.Write("MACHINE", "Port", MachinePort);
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

    public class Neurocles
    {
        private readonly string _path;

        public int GpuIndex;
        public NeurocleCameraSetting Top;
        public NeurocleCameraSetting Bottom;
        public NeurocleCameraSetting Trans;

        public Neurocles(string path)
        {
            _path = path;

            Top     = new NeurocleCameraSetting();
            Bottom  = new NeurocleCameraSetting();
            Trans   = new NeurocleCameraSetting();
        }

        public bool Load()
        {
            if (!File.Exists(_path))
            {
                using(FileStream fs = File.Create(_path)) { }

                SetDefault();

                Save();

                return false;
            }

            IniFile ini = new IniFile(_path);

            // Device
            GpuIndex = ini.Read("DEVICE", "GpuIndex", 0);

            LoadCamera(ini, "TOP", Top);
            LoadCamera(ini, "BOTTOM", Bottom);
            LoadCamera(ini, "TRANS", Trans);

            return true;
        }

        private static void LoadCamera(IniFile ini, string section, NeurocleCameraSetting setting)
        {
            setting.ClassificationModelPath     = ini.Read(section, "ClassificationModel", "");
            setting.ClassificationPredictorPath = ini.Read(section, "ClassificationPredictor", "");
            setting.ClassificationBatchSize     = ini.Read(section, "ClassificationBatchSize", 64);

            setting.SegmentationModelPath       = ini.Read(section, "SegmentationModel", "");
            setting.SegmentationPredictorPath   = ini.Read(section, "SegmentationPredictor", "");
            setting.SegmentationBatchSize       = ini.Read(section, "SegmentationBatchSize", 32);

            setting.UseFP16 = ini.Read(section, "UseFP16", false);
        }

        public void Save()
        {
            IniFile ini = new IniFile(_path);

            ini.Write("DEVICE", "GpuIndex", GpuIndex);

            SaveCamera(ini, "TOP", Top);
            SaveCamera(ini, "BOTTOM", Bottom);
            SaveCamera(ini, "TRANS", Trans);
        }

        public static void SaveCamera(IniFile ini, string section, NeurocleCameraSetting setting)
        {
            // CLF
            ini.Write(section, "ClassificationModel",     setting.ClassificationModelPath);
            ini.Write(section, "ClassificationPredictor", setting.ClassificationPredictorPath);
            ini.Write(section, "ClassificationBatchSize", setting.ClassificationBatchSize);

            // SEG
            ini.Write(section, "SegmentationModel",     setting.SegmentationModelPath);
            ini.Write(section, "SegmentationPredictor", setting.SegmentationPredictorPath);
            ini.Write(section, "SegmentationBatchSize", setting.SegmentationBatchSize);

            ini.Write(section, "UseFP16", setting.UseFP16);
        }

        private void SetDefault()
        {
            GpuIndex = 0;
            Top     = new NeurocleCameraSetting();
            Bottom  = new NeurocleCameraSetting();
            Trans   = new NeurocleCameraSetting();
        }
    }

    public class NeurocleCameraSetting
    {
        public string ClassificationModelPath { get; set; }
        public string ClassificationPredictorPath { get; set; }
        public int ClassificationBatchSize { get; set; } = 64;

        public string SegmentationModelPath { get; set; }
        public string SegmentationPredictorPath { get; set; }
        public int SegmentationBatchSize { get; set; } = 32;

        public bool UseFP16 { get; set; }
    }

    public class InferenceSettings
    {
        private readonly string _path;

        public double TopResolutionUmPerPixel;
        public double BottomResolutionUmPerPixel;
        public double TransResolutionUmPerPixel;

        public InferenceSettings(string path) { _path = path; }

        public bool Load()
        {
            if (!File.Exists(_path)) return false;

            IniFile ini = new IniFile(_path);

            TopResolutionUmPerPixel = ini.Read("INFERENCE", "TopResolutionUmPerPixel", 1.0);
            BottomResolutionUmPerPixel = ini.Read("INFERENCE", "BottomResolutionUmPerPixel", 1.0);
            TransResolutionUmPerPixel = ini.Read("INFERENCE", "TransResolutionUmPerPixel", 1.0);

            return true;
        }

        public void Save()
        {
            IniFile ini = new IniFile(_path);

            ini.Write("INFERENCE", "TopResolutionUmPerPixel", TopResolutionUmPerPixel);
            ini.Write("INFERENCE", "BottomResolutionUmPerPixel", BottomResolutionUmPerPixel);
            ini.Write("INFERENCE", "TransResolutionUmPerPixel", TransResolutionUmPerPixel);
        }

        public double GetResolution(string camera)
        {
            switch(camera.ToUpperInvariant())
            {
                case "TOP":
                    return TopResolutionUmPerPixel;

                case "BOTTOM":
                    return BottomResolutionUmPerPixel;

                case "TRANS":
                    return TransResolutionUmPerPixel;

                default:
                    return 0.0;
            }
        }
    }


    public class DefectSpecSettings
    {
        private readonly string _path;
        private readonly Dictionary<string, DefectSpecSettingItem> _items;

        private static readonly string[] Sections =
        {
            "TOP_CONTAMINANT",
            "TOP_PARTICLE",
            "TOP_UNDERETCHING",
            "TOP_FLASH",
            "TOP_VOID",

            "BOTTOM_CONTAMINANT",
            "BOTTOM_PARTICLE",
            "BOTTOM_UNDERETCHING",

            "TRANS_PARTICLE",
            "TRANS_UNDERETCHING",
            "TRANS_PUNCH"
        };

        public DefectSpecSettings(string path)
        {
            _path = path;
            _items = new Dictionary<string, DefectSpecSettingItem>();
        }

        public DefectSpecSettingItem Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            DefectSpecSettingItem item;

            if(_items.TryGetValue(key, out item)) return item;

            return null;
        }

        public bool Load()
        {
            if (!File.Exists(_path))
            {
                using (FileStream fs = File.Create(_path)) { }

                CreateDefaults();
                Save();

                return false;
            }

            IniFile ini = new IniFile(_path);

            _items.Clear();

            foreach(string section in Sections)
            {
                _items[section] = new DefectSpecSettingItem
                {
                    Enable = ini.Read(section, "Enable", true),
                    ClassificationThreshold = ini.Read(section, "ClassificationThreshold", 0.90),
                    ClassificationMargin = ini.Read(section, "ClassificationMargin", 0.20),
                    JudgeMethod = ini.Read(section, "JudgeMethod", "Direct"),
                    DirectJudgement = ini.Read(section, "DirectJudgement", "Unknown"),
                    ThresholdUm = ini.Read(section, "ThresholdUm", 0.0)
                };
            }

            return true;
        }

        public void Save()
        {
            IniFile ini = new IniFile(_path);

            foreach(KeyValuePair<string, DefectSpecSettingItem> pair in _items)
            {
                DefectSpecSettingItem item = pair.Value;

                ini.Write(pair.Key, "Enable", item.Enable);
                ini.Write(pair.Key, "ClassificationThreshold", item.ClassificationThreshold);
                ini.Write(pair.Key, "ClassificationMargin", item.ClassificationMargin);
                ini.Write(pair.Key, "JudgeMethod", item.JudgeMethod);
                ini.Write(pair.Key, "DirectJudgement", item.DirectJudgement);
                ini.Write(pair.Key, "ThresholdUm", item.ThresholdUm);
            }
        }

        private void CreateDefaults()
        {
            _items.Clear();

            foreach(string section in Sections)
            {
                _items[section] = new DefectSpecSettingItem
                {
                    Enable = true,
                    ClassificationThreshold = 0.90,
                    ClassificationMargin = 0.20,
                    JudgeMethod = "Direct",
                    DirectJudgement = "Unknown",
                    ThresholdUm = 0.0
                };
            }
        }
    }
}
