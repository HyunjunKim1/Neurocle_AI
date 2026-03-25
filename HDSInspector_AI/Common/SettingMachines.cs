using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Common
{
    public class MachineDBInfo
    {
        public string Name {get; set;}
        public string IP;
        public string SettingPath;
        public string ModelPath;
        public string Code;
    }

    public class IndexedName : NotifyPropertyChanged
    {
        string m_Name;
        int m_Index;
        [XmlElement(ElementName = "Name")]
        public string Name {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                Notify("Name"); } 
        }
        [XmlElement(ElementName = "Index")]
        public int Index
        {
            get
            {
                return m_Index;
            }
            set
            {
                m_Index = value;
                Notify("Index");
            }
        }
        public IndexedName(int id, string name)
        {
            Name = name;
            Index = id;
        }
    }

    public class SettingMachines
    {
        private string m_strPath;
        public int MachineCount { get; set; }
        public ObservableCollection<MachineDBInfo> MachineInfo { get; set; }

        public SettingMachines(string path)
        {
            MachineInfo = new ObservableCollection<MachineDBInfo>();
            if (System.IO.File.Exists(path))
            {
                m_strPath = path;
                Load();
            }
        }

        public void Load()
        {
            MachineInfo.Clear();
            IniFile ini = new IniFile(m_strPath);
            MachineCount = ini.ReadInteger("MachineDBInfo", "MachineCount", 0);
            for (int i = 0; i < MachineCount; i++)
            {
                MachineDBInfo mi = new MachineDBInfo();
                mi.Name = ini.ReadString("MachineDBInfo", "Machine_Name_" + (i + 1).ToString("00"), "");
                mi.Code = ini.ReadString("MachineDBInfo", "Machine_Code_" + (i + 1).ToString("00"), "");
                mi.ModelPath = ini.ReadString("MachineDBInfo", "Machine_ModelPath_" + (i + 1).ToString("00"), "");
                mi.SettingPath = ini.ReadString("MachineDBInfo", "Machine_SettingPath_" + (i + 1).ToString("00"), "");
                mi.IP = ini.ReadString("MachineDBInfo", "Machine_IP_" + (i + 1).ToString("00"), "");
                
                MachineInfo.Add(mi);
            }
        }

        public void Save()
        {
            IniFile ini = new IniFile(m_strPath);
            ini.WriteInteger("MachineDBInfo", "MachineCount", MachineInfo.Count);
            for (int i = 0; i < MachineCount; i++)
            {
                ini.WriteString("MachineDBInfo", "Machine_Name_" + (i + 1).ToString("00"), MachineInfo[i].Name);
                ini.WriteString("MachineDBInfo", "Machine_SettingPath_" + (i + 1).ToString("00"), MachineInfo[i].SettingPath);
            }
        }
    }
}
