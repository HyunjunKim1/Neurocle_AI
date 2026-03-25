using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Common
{
    public class LightChannel
    {
        public int Index { get; set; }
        public string Name { get; set; }        // 채널 이름
        public int Number { get; set; }         // 채널 번호
        public int DeviceNumber { get; set; }   // 조명 장비 ID
        public int Stage { get; set; }          // 상-0 / 하-1 / 투-2
        public int DefaultValue { get; set; }   // 채널 기본 값.

        public int CurrentValue { get; set; }   // 현재 조명 값.

        public LightChannel()
        {
            Index = 0;
            Name = string.Empty;
            Number = -1;
            DeviceNumber = -1;
            Stage = -1;
            CurrentValue = 0;
        }

        public override string ToString()
        {
            string szStage = (Stage == 0) ? "상부반사" : ((Stage == 1) ? "하부반사" : "상부투과");
            return string.Format("ID:{0}, DevNo{1}, Stage:{2}, Ch:{3}, Name:{4}", Index, DeviceNumber, szStage, Number, Name);
        }
    }

    public class SettingLight
    {
        public bool Use { get; set; }
        public string Port { get; set; }

        public SettingLight()
        {
            Use = false;
            Port = "COM1";
        }

        public override string ToString()
        {
            return string.Format("Use:{0}, Port{1}", Use ? "T" : "F", Port);
        }
    }

    public class SettingDevice
    {
        #region Instance of SettingDevice 
        private readonly XmlSetting m_XmlSetting;
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }

        public SettingDevice(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
            {
                aXmlSetting = new XmlSetting();
                Load();
            }

            m_XmlSetting = aXmlSetting;
        }
        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////
        // Light Setting.
        //////////////////////////////////////////////////////////////////////////////////////////
        public bool[] LightUse { get; set; }
        public int LightType = 0;
        public string[] LightPort { get; set; }
        public LightChannel[] LightChannels { get; set; }
        public int[] LightVal1 { get; set; }
        public int[] LightVal2 { get; set; }
        public int[] LightVal3 { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////
        // Counter Setting.
        //////////////////////////////////////////////////////////////////////////////////////////
        public long CounterUsed { get; set; }
        public string CounterType { get; set; }

        // 0 - NI Traditional / 1 - NIDAQmx library
        public long NIDAQmxUsed { get; set; }
        public long CounterSample { get; set; }

        #region Ctr 0.
        public long UsedCtr0 { get; set; }
        public long DelayCtr0 { get; set; }
        public long LowCtr0 { get; set; }
        public long HighCtr0 { get; set; }
        public long FilterUseCtr0 { get; set; }
        public double FilterPulseCtr0 { get; set; }
        #endregion

        #region Ctr 1.
        public long UsedCtr1 { get; set; }
        public long DelayCtr1 { get; set; }
        public long LowCtr1 { get; set; }
        public long HighCtr1 { get; set; }
        public long FilterUseCtr1 { get; set; }
        public double FilterPulseCtr1 { get; set; }
        #endregion

        #region Ctr 2.
        public long UsedCtr2 { get; set; }
        public long DelayCtr2 { get; set; }
        public long LowCtr2 { get; set; }
        public long HighCtr2 { get; set; }
        public long FilterUseCtr2 { get; set; }
        public double FilterPulseCtr2 { get; set; }
        #endregion

        #region Ctr3.
        public long UsedCtr3 { get; set; }
        public long DelayCtr3 { get; set; }
        public long LowCtr3 { get; set; }
        public long HighCtr3 { get; set; }
        public long FilterUseCtr3 { get; set; }
        public double FilterPulseCtr3 { get; set; }
        #endregion

        #region SWT Trigger Setting
        public bool SWTUse { get; set; }
        public string TriggerPort1 { get; set; }
        public int TriggerDir1 { get; set; }
        public int TriggerCount1 { get; set; }
        public int TriggerDelay11 { get; set; }
        public int TriggerDelay12 { get; set; }
        public int TriggerDelay13 { get; set; }
        public int TriggerWidth1 { get; set; }
        #endregion

        #region Mark Setting
        public bool MarkerUsed { get; set; }
        public String MarkerIP { get; set; }
        public String MarkerPort { get; set; }
        #endregion

        #region PLC Setting
        public long PlcUsed { get; set; }
        public int PlcType { get; set; }
        public String PlcIP { get; set; }
        public int PlcPort { get; set; }
        public int GoodSlotNumber { get; set; }
        public int NGSlotNumber { get; set; }
        public int AutoNGSlotNumber { get; set; }
        #endregion

        public void Load()
        {
            #region Light Setting.
            LightPort = new string[3];
            LightUse = new bool[3];
            LightType = m_XmlSetting.GetSettingInt("Device", "LightType", "0");
            for (int i = 0; i < 3; i++)
            {
                LightPort[i] = "";
                string nodeName = string.Format("Device/Light{0}", i + 1);
                LightUse[i] = Convert.ToBoolean(m_XmlSetting.GetSettingLong(nodeName, "Use", "0"));
                LightPort[i] = m_XmlSetting.GetSettingString(nodeName, "Port", "COM1");
            }
            #endregion

            #region Counter Setting.
            CounterUsed = m_XmlSetting.GetSettingLong("Device/Counter", "Use", "0");
            CounterType = m_XmlSetting.GetSettingString("Device/Counter", "Type", "0031");
            NIDAQmxUsed = m_XmlSetting.GetSettingLong("Device/Counter", "NIDAQmxUse", "0");
            CounterSample = m_XmlSetting.GetSettingLong("Device/Counter", "SampltTime", "1000");

            UsedCtr0 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr0", "Use", "0");
            DelayCtr0 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr0", "InitDelay", "0");
            LowCtr0 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr0", "Low", "0");
            HighCtr0 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr0", "High", "0");
            FilterUseCtr0 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr0", "FilterUse", "1");
            FilterPulseCtr0 = m_XmlSetting.GetSettingDouble("Device/Counter/Ctr0", "FilterPulse", "0.100");

            UsedCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr1", "Use", "0");
            DelayCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr1", "InitDelay", "0");
            LowCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr1", "Low", "0");
            HighCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr1", "High", "0");
            FilterUseCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr1", "FilterUse", "1");
            FilterPulseCtr1 = m_XmlSetting.GetSettingDouble("Device/Counter/Ctr1", "FilterPulse", "0.100");

            //UsedCtr2 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr2", "Use", "0");
            //DelayCtr2 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr2", "InitDelay", "0");
            //LowCtr2 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr2", "Low", "0");
            //HighCtr2 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr2", "High", "0");
            //FilterUseCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr2", "FilterUse", "1");
            //FilterPulseCtr1 = m_XmlSetting.GetSettingDouble("Device/Counter/Ctr2", "FilterPulse", "0.100");

            //UsedCtr3 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr3", "Use", "0");
            //DelayCtr3 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr3", "InitDelay", "0");
            //LowCtr3 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr3", "Low", "0");
            //HighCtr3 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr3", "High", "0");
            //FilterUseCtr1 = m_XmlSetting.GetSettingLong("Device/Counter/Ctr3", "FilterUse", "1");
            //FilterPulseCtr1 = m_XmlSetting.GetSettingDouble("Device/Counter/Ctr3", "FilterPulse", "0.100");
            #endregion

            #region SWT Trigger Setting
            SWTUse = Convert.ToBoolean(m_XmlSetting.GetSettingLong("Device/SWTtrigger1", "Use", "0"));
            TriggerPort1 = m_XmlSetting.GetSettingString("Device/SWTtrigger1", "Port", "COM1");
            TriggerDir1 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Dir", "1");
            TriggerDelay11 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Delay1", "1000");
            TriggerDelay12 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Delay2", "2000");
            TriggerDelay13 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Delay3", "3000");
            TriggerCount1 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Count", "8");
            TriggerWidth1 = m_XmlSetting.GetSettingInt("Device/SWTtrigger1", "Width", "10");
            #endregion

            #region Marker Setting.
            MarkerUsed = m_XmlSetting.GetSettingBool("Device/Marker", "Use", "0");
            MarkerIP = m_XmlSetting.GetSettingString("Device/Marker", "IP", "localhost");
            MarkerPort = m_XmlSetting.GetSettingString("Device/Marker", "Port", "50006");
            #endregion

            #region PLC Setting.
            PlcUsed = m_XmlSetting.GetSettingLong("Device/PLC", "Use", "0");
            PlcType = m_XmlSetting.GetSettingInt("Device/PLC", "Type", "0");
            PlcIP = m_XmlSetting.GetSettingString("Device/PLC", "IP", "localhost");
            PlcPort = m_XmlSetting.GetSettingInt("Device/PLC", "Port", "50008");
            GoodSlotNumber = m_XmlSetting.GetSettingInt("Device/PLC", "GoodSlotNumber", "99");
            NGSlotNumber = m_XmlSetting.GetSettingInt("Device/PLC", "NGSlotNumber", "1");
            AutoNGSlotNumber = m_XmlSetting.GetSettingInt("Device/PLC", "AUTONGSlotNumber", "2");
            #endregion
        }

        public void Save()
        {
            SaveLight();
            SaveCounter();
            //SaveMarker();
            SavePLC();
            SaveSWT();
        }
        public void SaveSWT()
        {
            m_XmlSetting.SetSettingLong("Device/SWTtrigger1", "Use", Convert.ToInt32(SWTUse));
            m_XmlSetting.SetSettingString("Device/SWTtrigger1", "Port", TriggerPort1);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Dir", TriggerDir1);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Delay1", TriggerDelay11);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Delay2", TriggerDelay12);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Delay3", TriggerDelay13);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Count", TriggerCount1);
            m_XmlSetting.SetSettingInt("Device/SWTtrigger1", "Width", TriggerWidth1);
        }
        public void SaveLight()
        {
            try
            {
                for (int i = 0; i < LightPort.Length; i++)
                {
                    string nodeName = string.Format("Device/Light{0}", i + 1);
                    m_XmlSetting.SetSettingString(nodeName, "Port", LightPort[i]);
                    m_XmlSetting.SetSettingLong(nodeName, "Use", Convert.ToInt32(LightUse[i]));
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in SaveLight(SettingDevice.cs)");
            }
        }

        public void SaveCounter()
        {
            m_XmlSetting.SetSettingLong("Device/Counter", "Use", CounterUsed);
            m_XmlSetting.SetSettingString("Device/Counter", "Type", CounterType);
            m_XmlSetting.SetSettingLong("Device/Counter", "NIDAQmxUse", NIDAQmxUsed);

            m_XmlSetting.SetSettingLong("Device/Counter", "SampltTime", CounterSample);

            m_XmlSetting.SetSettingLong("Device/Counter/Ctr0", "Use", UsedCtr0);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr0", "InitDelay", DelayCtr0);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr0", "Low", LowCtr0);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr0", "High", HighCtr0);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr0", "FilterUse", FilterUseCtr0);
            m_XmlSetting.SetSettingDouble("Device/Counter/Ctr0", "FilterPulse", FilterPulseCtr0);

            m_XmlSetting.SetSettingLong("Device/Counter/Ctr1", "Use", UsedCtr1);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr1", "InitDelay", DelayCtr1);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr1", "Low", LowCtr1);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr1", "High", HighCtr1);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr1", "FilterUse", FilterUseCtr1);
            m_XmlSetting.SetSettingDouble("Device/Counter/Ctr1", "FilterPulse", FilterPulseCtr1);

            m_XmlSetting.SetSettingLong("Device/Counter/Ctr2", "Use", UsedCtr2);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr2", "InitDelay", DelayCtr2);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr2", "Low", LowCtr2);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr2", "High", HighCtr2);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr2", "FilterUse", FilterUseCtr2);
            m_XmlSetting.SetSettingDouble("Device/Counter/Ctr2", "FilterPulse", FilterPulseCtr2);

            m_XmlSetting.SetSettingLong("Device/Counter/Ctr3", "Use", UsedCtr3);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr3", "InitDelay", DelayCtr3);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr3", "Low", LowCtr3);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr3", "High", HighCtr3);
            m_XmlSetting.SetSettingLong("Device/Counter/Ctr3", "FilterUse", FilterUseCtr3);
            m_XmlSetting.SetSettingDouble("Device/Counter/Ctr3", "FilterPulse", FilterPulseCtr3);
        }

        public void SaveMarker()
        {
            m_XmlSetting.SetSettingBool("Device/Marker", "Use", MarkerUsed);
            m_XmlSetting.SetSettingString("Device/Marker", "IP", MarkerIP);
            m_XmlSetting.SetSettingString("Device/Marker", "Port", MarkerPort);
        }

        public void SavePLC()
        {
            m_XmlSetting.SetSettingLong("Device/PLC", "Use", PlcUsed);
            m_XmlSetting.SetSettingInt("Device/PLC", "Type", PlcType);
            m_XmlSetting.SetSettingString("Device/PLC", "IP", PlcIP);
            m_XmlSetting.SetSettingInt("Device/PLC", "Port", PlcPort);
            m_XmlSetting.SetSettingInt("Device/PLC", "GoodSlotNumber", GoodSlotNumber);
            m_XmlSetting.SetSettingInt("Device/PLC", "NGSlotNumber", NGSlotNumber);
            m_XmlSetting.SetSettingInt("Device/PLC", "AUTONGSlotNumber", AutoNGSlotNumber);
        }
    }
}
