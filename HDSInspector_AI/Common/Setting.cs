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
            General.MachineCode = settings.General.MachineCode;
            General.MachineName = settings.General.MachineName;

            if (settings.General.ColorAVI) 
                General.MachineType = 1;
            else General.MachineType = 0;

            General.MachineIP = settings.General.MachineIP;
            General.ModelPath = settings.General.ModelPath;
            General.ResultPath = settings.General.ImagePath;
            General.RejectNumber = settings.General.RejectValue;
            General.RejectRate = settings.General.RejectRate;
            General.UseDB = settings.SubSystem.UseDB == "0" ? false : true;
            General.DBIP = settings.SubSystem.DBIP;
            General.DBPort = settings.SubSystem.DBPort;

            General.LogSave = settings.Log.LocalSave == 0 ? false : true;
            General.LogLevel = (int)settings.Log.LocalSaveLevel;
            General.LogDPLevel = (int)settings.Log.UIDisplayLevel;
            General.LogKeepDate = (int)settings.Log.KeepDate;

            General.MaxLimitDefect = 256;
            General.UseAutoTemplate = settings.General.UseAutoTemplate;
            General.TemplatePath = settings.General.TemplatePath;
            General.TemplateNetDriveName = settings.General.TemplateNetDriveName;
            General.TemplateNetDriveIP   = settings.General.TemplateNetDriveIP;
            General.TemplateNetDriveID   = settings.General.TemplateNetDriveID;
            General.TemplateNetDrivePW   = settings.General.TemplateNetDrivePW;            
            General.BottomPlate = settings.General.BottomPlate;
            General.OffLine = false;

            #endregion

            #region SubSystems

            #region IS
            for (int i = 0; i < ISPara.cam_num; i++)
            {
                SubSystem.IS.ReScale[i] = 2.0;
                SubSystem.IS.CameraHeight[i] = settings.SubSystem.CameraHeight;
                SubSystem.IS.VisionFlipX[i] = false;
                SubSystem.IS.CameraWidth[i] = settings.SubSystem.CameraWidth;
                SubSystem.IS.CamResolutionX[i] = settings.General.ResolutionX[i];
                SubSystem.IS.CamResolutionY[i] = settings.General.ResolutionY[i];
                SubSystem.IS.CamPageDelay[i] = settings.General.PageDealy[i];
                SubSystem.IS.R_Gain[i] = 1;
                SubSystem.IS.G_Gain[i] = 1;
                SubSystem.IS.B_Gain[i] = 1;
                SubSystem.IS.A_Gain[i] = 1;
                SubSystem.IS.S_Gain[i] = 1;
                SubSystem.IS.SurfType[i] = 1;
                SubSystem.IS.UseFocus[i] = false;
                SubSystem.IS.FGType[i] = settings.SubSystem.ISType;
                SubSystem.IS.DeviceName[i] = settings.SubSystem.DeviceName;
                SubSystem.IS.CamFile[i] = settings.SubSystem.CamFile;
                SubSystem.IS.IP[i] = settings.General.VisionIP[i];
                SubSystem.IS.Port[i] = settings.General.VisionPort[i];
            }
            SubSystem.IS.CAM_NUMS = 3;
            SubSystem.IS.FGType[2] = settings.SubSystem.ISType2;
            SubSystem.IS.CameraWidth[2] = settings.SubSystem.CameraWidth2;
            SubSystem.IS.DeviceName[2] = settings.SubSystem.DeviceName2;
            SubSystem.IS.CamFile[2] = settings.SubSystem.CamFile2;
            SubSystem.IS.TestID = settings.SubSystem.TestID;
            #endregion

            #region VIEWER
            SubSystem.Viewer.TotalViewerNumber = 6;
            SubSystem.Viewer.Use = new bool[ViewerPara.viewcnt];
            SubSystem.Viewer.SurfType = new int[ViewerPara.viewcnt];
            SubSystem.Viewer.Name = new string[ViewerPara.viewcnt];
            SubSystem.Viewer.LCnum = new int[ViewerPara.viewcnt];
            SubSystem.Viewer.Chroma = new int[ViewerPara.viewcnt];
            SubSystem.Viewer.ISNum = new int[ViewerPara.viewcnt];

            for (int i = 0; i < ViewerPara.viewcnt; i++)
            {
                SubSystem.Viewer.Use[i] = true;
                SubSystem.Viewer.SurfType[i] = 10;
                SubSystem.Viewer.Name[i] = "반사";
                SubSystem.Viewer.LCnum[i] = 0;
                SubSystem.Viewer.Chroma[i] = 0;
                SubSystem.Viewer.ISNum[i] = 0;
            }
            #endregion

            #region Light

            for (int i = 0; i < SubSystem.Light.Length; i++)
            {
                SubSystem.Light[i].Surface = i.ToString();
                SubSystem.Light[i].ChannelNumber = i.ToString();
                SubSystem.Light[i].Name = "조명";
                SubSystem.Light[i].Port = "COM3";
            }
            SubSystem.Controller.CanSingleSide = "1";
            SubSystem.Controller.Name = "상부 컨트롤러";
            SubSystem.Controller.Port = "COM3";
            SubSystem.Controller.Maker = "플러스텍";
            SubSystem.Controller.BaudRate = "115200";
            SubSystem.Controller.ChannelType = "16";
            SubSystem.Controller.Type = "0";
            #endregion

            #endregion

            settings = null;
            Save();
        }
    }

    public class Generals
    {
        #region Members
        public string MachineCode;           //DB에서 사용되는 MC Code 나중에 삭제 하도록
        public string MachineName;          //설비 코드 
        public int MachineType;          //Boat1/Boat2의 순서가 CA/BA인지 BA/CA인지
        public string MachineIP;            //설비의 IP      
        public string ModelPath;            //모델의 정보를 저장하는 경로
        public string ResultPath;           //검사 결과를 저장하는 경로
        public bool SaveFailLoss;           //폐기정보를 POP 정보에 사용 할지
        public double RejectRate;            //X-OUT 초과 율 Default 값 설정
        public double RejectNumber;         //Auto NG 개수
        public bool UseDB;                  //DB 사용
        public string DBName;               //DB Name
        public string DBIP;                 //DB IP
        public string DBPort;               //DB Port
        public bool LogSave;
        public int LogLevel;
        public int LogDPLevel;
        public int LogKeepDate;
        public int MaxLimitDefect;
        public bool UseAutoTemplate;
        public string TemplatePath;
        public string TemplateNetDriveIP;
        public string TemplateNetDriveName;
        public string TemplateNetDriveID;
        public string TemplateNetDrivePW;
        public bool BottomPlate;
        public bool OffLine;
        public bool UseServeralScan;
        public int language;
        public bool bEng;
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
            MachineCode         = ini.Read("MACHINE", "Code", "0001");
            MachineName         = ini.Read("MACHINE", "Name", "BAV01");
            MachineType         = ini.Read("MACHINE", "Type", 0);
            MachineIP           = ini.Read("MACHINE", "IP", "127.0.0.1");
            OffLine             = ini.Read("MACHINE", "OffLine", false);
            UseServeralScan     = ini.Read("MACHINE", "SeveralScan", false);
            language            = ini.Read("MACHINE", "language", 0);

            ModelPath           = ini.Read("PATH", "Model", "d:\\Model");
            ResultPath          = ini.Read("PATH", "Result", "d:\\Result");

            UseDB               = ini.Read("DATABASE", "Use", true);
            DBIP                = ini.Read("DATABASE", "IP", "127.0.0.1");
            DBPort              = ini.Read("DATABASE", "Port", "5000");
            DBName              = ini.Read("DATABASE", "Name", "Inlinedb");

            LogSave             = ini.Read("Log", "UseSave", true);
            LogLevel            = ini.Read("Log", "SaveLevel", 1);
            LogDPLevel          = ini.Read("Log", "DPLevel", 1);
            LogKeepDate         = ini.Read("Log", "KeepDate", 60);

            MaxLimitDefect      = ini.Read("INSPECT", "MaxLimitDefect", 512);

            bEng= (language > 0) ? true : false;

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
            ini.Write("MACHINE", "Code", MachineCode);
            ini.Write("MACHINE", "Name", MachineName);
            ini.Write("MACHINE", "Type", MachineType);
            ini.Write("MACHINE", "IP", MachineIP);
            ini.Write("MACHINE", "OffLine", OffLine);
            ini.Write("MACHINE", "SeveralScan", UseServeralScan);
            ini.Write("PATH", "Model", ModelPath);
            ini.Write("PATH", "Result", ResultPath);

            ini.Write("DATABASE", "Use", UseDB);
            ini.Write("DATABASE", "Name", DBName);
            ini.Write("DATABASE", "IP", DBIP);
            ini.Write("DATABASE", "Port", DBPort);

            ini.Write("Log", "UseSave", LogSave);
            ini.Write("Log", "SaveLevel", LogLevel);
            ini.Write("Log", "DPLevel", LogDPLevel);
            ini.Write("Log", "KeepDate", LogKeepDate);
            ini.Write("INSPECT", "MaxLimitDefect", MaxLimitDefect);

            ini.Write("MACHINE", "language", language);
        }
    }

    public class SubSystems
    {
        public const int LightNumber = 5;
        public const int ControllerNumber = 3;
        public ISPara IS;
        public ViewerPara Viewer;
        public LightPara[] Light;
        public ControllerPara Controller;
        private string m_Path;

        public SubSystems(string astrPath)
        {
            IS = new ISPara();
            Viewer = new ViewerPara();
            Light = new LightPara[LightNumber];
            for(int i = 0; i < LightNumber; i++)
                Light[i] = new LightPara();
            Controller = new ControllerPara();
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

            #region IS
            for (int i = 0; i < ISPara.cam_num; i++)
            {
                IS.UseSlave[i]      = ini.Read("IS", "UseSlave" + (i + 1).ToString(), false);
                IS.UseFocus[i]      = ini.Read("IS", "Use_Focus" + (i + 1).ToString(), false);
                IS.ReScale[i]       = ini.Read("IS", "ReScale" + (i + 1).ToString(), 2.0);

                IS.CameraWidth[i]   = ini.Read("IS", "CameraWidth" + (i + 1).ToString(), 0);
                IS.CameraHeight[i]  = ini.Read("IS", "CameraHeight" + (i + 1).ToString(), 0);
                IS.VisionFlipX[i]   = ini.Read("IS", "VisionFlipX" + (i + 1).ToString(), false);
                IS.CamResolutionX[i] = ini.Read("IS", "CameRsolutionX" + (i + 1).ToString(), 0.0);
                IS.CamResolutionY[i] = ini.Read("IS", "CameRsolutionY" + (i + 1).ToString(), 0.0);
                IS.CamPageDelay[i]  = ini.Read("IS", "CamPageDelay" + (i + 1).ToString(), 0);
                IS.R_Gain[i]        = (float)ini.Read("IS", "R_Gain" + (i + 1).ToString(), 1.0);
                IS.G_Gain[i]        = (float)ini.Read("IS", "G_Gain" + (i + 1).ToString(), 1.0);
                IS.B_Gain[i]        = (float)ini.Read("IS", "B_Gain" + (i + 1).ToString(), 1.0);
                IS.SurfType[i]      = ini.Read("IS", "Strenth" + (i + 1).ToString(), 0);

                IS.FGType[i]        = ini.Read("IS", "FG_Type" + (i + 1).ToString(), 0);
                IS.DeviceName[i]    = ini.Read("IS", "DeviceName" + (i + 1).ToString(), "");
                IS.CamFile[i]       = ini.Read("IS", "CamFile" + (i + 1).ToString(), "");
                IS.IP[i]            = ini.Read("IS", "IP" + (i + 1).ToString(), "127.0.0.1");
                IS.Port[i]          = ini.Read("IS", "Port" + (i + 1).ToString(), 0);
            }
            IS.TestID               = ini.Read("IS", "Test_ID", 0);
            IS.CAM_NUMS             = ini.Read("IS", "CAM_NUMS", 3);
            IS.bUseResize           = ini.Read("IS", "UseResize", false);
            IS.LineVelocity         = ini.Read("IS", "LineVelocity", 3);
            IS.OverlapLength        = ini.Read("IS", "OverlapLength", 20);

            #endregion

            #region Viewer
            Viewer.TotalViewerNumber    = ini.Read("VIEWER", "TOTAL", 4);
            Viewer.Use                  = new bool[ViewerPara.viewcnt];
            Viewer.SurfType             = new int[ViewerPara.viewcnt];
            Viewer.Name                 = new string[ViewerPara.viewcnt];
            Viewer.LCnum                = new int[ViewerPara.viewcnt];
            Viewer.Chroma               = new int[ViewerPara.viewcnt];
            Viewer.ISNum                = new int[ViewerPara.viewcnt];
            for (int i = 0; i < ViewerPara.viewcnt; i++)
            {
                Viewer.Use[i]       = ini.Read("VIEWER" + (i + 1).ToString(), "USE", false);           
                Viewer.Name[i]      = ini.Read("VIEWER" + (i + 1).ToString(), "NAME", "상부반사1");
                Viewer.SurfType[i]  = ini.Read("VIEWER" + (i + 1).ToString(), "SURF", 10);
                Viewer.LCnum[i]     = ini.Read("VIEWER" + (i + 1).ToString(), "LCNum", 0);
                Viewer.Chroma[i]    = ini.Read("VIEWER" + (i + 1).ToString(), "Chroma", 0);
                Viewer.ISNum[i]     = ini.Read("VIEWER" + (i + 1).ToString(), "ISNum", 0);
            }
            #endregion

            #region Light
            for(int i = 0; i < SubSystems.LightNumber; i++)
            {
                Light[i].Surface        = ini.Read("Light" + (i + 1).ToString(), "Surface", "");
                Light[i].Name           = ini.Read("Light" + (i + 1).ToString(), "Name", "");
                Light[i].Port           = ini.Read("Light" + (i + 1).ToString(), "Port", "");
                Light[i].ChannelNumber  = ini.Read("Light" + (i + 1).ToString(), "ChannelNumber", "");
            }

            Controller.Name          = ini.Read("CONTROLLER", "Name", "");
            Controller.Port          = ini.Read("CONTROLLER", "Port", "");
            Controller.Type          = ini.Read("CONTROLLER", "Type", "");
            Controller.ChannelType   = ini.Read("CONTROLLER", "ChannelType", "");
            Controller.Maker         = ini.Read("CONTROLLER", "Maker", "");
            Controller.BaudRate      = ini.Read("CONTROLLER", "BaudRate", "");
            Controller.CanSingleSide = ini.Read("CONTROLLER", "CanSingleSide", ""); 

            #endregion


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
            #region IS
            for (int i = 0; i < ISPara.cam_num; i++)
            {
                ini.Write("IS", "UseSlave" + (i + 1).ToString(), IS.UseSlave[i]);
                ini.Write("IS", "UseFocus" + (i + 1).ToString(), IS.UseFocus[i]);
                ini.Write("IS", "ReScale" + (i + 1).ToString(), IS.ReScale[i]);

                ini.Write("IS", "CameraWidth" + (i + 1).ToString(), IS.CameraWidth[i]);
                ini.Write("IS", "CameraHeight" + (i + 1).ToString(), IS.CameraHeight[i]);
                ini.Write("IS", "VisionFlipX" + (i + 1).ToString(), IS.VisionFlipX[i]);
                ini.Write("IS", "CameRsolutionX" + (i + 1).ToString(), IS.CamResolutionX[i]);
                ini.Write("IS", "CameRsolutionY" + (i + 1).ToString(), IS.CamResolutionY[i]);
                ini.Write("IS", "CamPageDelay" + (i + 1).ToString(), IS.CamPageDelay[i]);
                ini.Write("IS", "R_Gain" + (i + 1).ToString(), IS.R_Gain[i]);
                ini.Write("IS", "G_Gain" + (i + 1).ToString(), IS.G_Gain[i]);
                ini.Write("IS", "B_Gain" + (i + 1).ToString(), IS.B_Gain[i]);
                ini.Write("IS", "Strenth" + (i + 1).ToString(), IS.SurfType[i]);
            
                ini.Write("IS", "FG_Type" + (i + 1).ToString(), IS.FGType[i]);
                ini.Write("IS", "DeviceName" + (i + 1).ToString(), IS.DeviceName[i]);
                ini.Write("IS", "CamFile" + (i + 1).ToString(), IS.CamFile[i]);
                ini.Write("IS", "IP" + (i + 1).ToString(), IS.IP[i]);
                ini.Write("IS", "Port" + (i + 1).ToString(), IS.Port[i]);
            }
            ini.Write("IS", "Test_ID", IS.TestID);
            ini.Write("IS", "CAM_NUMS", IS.CAM_NUMS);
            ini.Write("IS", "UseResize", IS.bUseResize);
            ini.Write("IS", "LineVelocity", IS.LineVelocity);
            ini.Write("IS", "OverlapLength", IS.OverlapLength);
            #endregion

            #region VIEWER
            ini.Write("VIEWER", "TOTAL", Viewer.TotalViewerNumber);
            for(int i = 0; i < ViewerPara.viewcnt; i++)
            {
                ini.Write("VIEWER" + (i + 1).ToString(), "USE", Viewer.Use[i]);
                ini.Write("VIEWER" + (i + 1).ToString(), "SURF", Viewer.SurfType[i]);
                ini.Write("VIEWER" + (i + 1).ToString(), "NAME", Viewer.Name[i]);
                ini.Write("VIEWER" + (i + 1).ToString(), "LCNum", Viewer.LCnum[i]);
                ini.Write("VIEWER" + (i + 1).ToString(), "Chroma", Viewer.Chroma[i]);
                ini.Write("VIEWER" + (i + 1).ToString(), "ISNum", Viewer.ISNum[i]);
            }
            #endregion

            #region Light
            for(int i = 0; i < SubSystems.LightNumber; i++)
            {
                ini.Write("Light" + (i + 1).ToString(), "Surface", Light[i].Surface);
                ini.Write("Light" + (i + 1).ToString(), "Name", Light[i].Name);
                ini.Write("Light" + (i + 1).ToString(), "Port", Light[i].Port);
                ini.Write("Light" + (i + 1).ToString(), "ChannelNumber", Light[i].ChannelNumber);
            }

            ini.Write("Controller", "Name", Controller.Name);
            ini.Write("Controller", "Port", Controller.Port);
            ini.Write("Controller", "Type", Controller.Type);
            ini.Write("Controller", "ChannelType", Controller.ChannelType);
            ini.Write("Controller", "Maker", Controller.Maker);
            ini.Write("Controller", "BaudRate", Controller.BaudRate);
            ini.Write("Controller", "CanSingleSide", Controller.CanSingleSide);

            #endregion

        }
    }

    public class ISPara
    {
        public const int cam_num = 3;
        public int CAM_NUMS;
        public int[] FGType = new int[cam_num];                       //Camera Frame GrabberType
        public int[] CameraWidth = new int[cam_num];            //Camera Sensor Size
        public int[] CameraHeight = new int[cam_num];           //Camera Height
        public string[] DeviceName = new string[cam_num];       //Frame Grabber Name
        public string[] CamFile = new string[cam_num];          //Camera Config File Name
        public int TestID;                                      //Test vision ID
        public bool[] UseSlave = new bool[cam_num];             //Camera Slave Use
        public bool[] UseFocus = new bool[cam_num];
        public bool[] VisionFlipX = new bool[cam_num];          //Camera  Flip X
        public string[] IP = new string[cam_num];               //IS IP
        public int[] Port = new int[cam_num];                   // IS Port
        public double[] CamResolutionX = new double[cam_num];   //Camara Resolution X
        public double[] CamResolutionY = new double[cam_num];   //Camara Resolution Y
        public int[] CamPageDelay = new int[cam_num];           //Camera Grabing Delay
        public float[] R_Gain = new float[cam_num];             //Red Gain
        public float[] G_Gain = new float[cam_num];             //Greem Gain
        public float[] B_Gain = new float[cam_num];             //Blue Gain
        public float[] A_Gain = new float[cam_num];             //Analog Gain
        public float[] S_Gain = new float[cam_num];             //System Gain
        public int[] SurfType = new int[cam_num];            //Camera Strenth
        public double[] ReScale = new double[cam_num];          //Rescale witout Dalsa
        public bool bUseResize;

        // For Inline Simulation
        public int LineVelocity;                                // Inline Velocity (m/min)
        public int OverlapLength;                               // Inline Overlap Length (mm)
    }
    public class ViewerPara
    {
        public const int viewcnt = 3; 
        public int TotalViewerNumber;
        public bool[] Use;
        public int[] SurfType;
        public string[] Name;
        public int[] LCnum; // 조명 컨트롤러 번호
        public int[] Chroma;
        public int[] ISNum;

        public Surface ConvertIntToSurface(int surf)
        {
            Surface rtsurf;
            switch(surf)
            {
                case 11:
                    rtsurf = Surface.상부검사;
                    return rtsurf;
                case 21:
                    rtsurf = Surface.하부검사;
                    return rtsurf;
                case 31:
                    rtsurf = Surface.투과검사;
                    return rtsurf;
                default:
                    rtsurf = Surface.상부검사;
                    return rtsurf;
            }
        }
    }

    public class MotionPara
    {
        public bool UseMotion;               
        public string IP;                 
        public int Port;                  
    }
    

    public class LightPara
    {
        public string Surface { get; set; } //상부 하부 투과
        public string Name { get; set; } //동축 or 측광 or 측하
        public string Port { get; set; } // COM3, COM4 ...
        public string ChannelNumber { get; set; } // 1,2,3,4,,,,
    }
    public class ControllerPara
    {
        public string Name { get; set; } //상부 조명용, 하부 조명용 등
        public string Port { get; set; } // COM3, COM4 ...
        public string Type { get; set; } // 프로토콜 타입, 0:플러스텍, 1:알트, 2:알트 매크로
        public string ChannelType { get; set; } // 2,4,8,16,32, 총 채널 수
        public string Maker { get; set; } // 1,2,3,4,,,,
        public string BaudRate { get; set; } //9600, 116500...
        public string CanSingleSide { get; set; } //단방향 가능한 조명인지 여부 0:불가, 1:가능
    }
}
