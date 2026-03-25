using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>   It supports INI file.  </summary>
    public class IniFile
    {
        /// <summary> The in INI file path </summary>
        private string INIfilepath;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        /// <summary>   Initializes a new instance of the IniFile class. </summary>
        /// <param name="path"> Full pathname of the file. </param>
        public IniFile(string path)
        {
            this.INIfilepath = path;
        }

        #region Old
        /// <summary>   Reads a string. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sDefault"> true to default. </param>
        /// <returns>   The string. </returns>
        public string ReadString(string sID, string sKey, string sDefault)
        {
            StringBuilder sb = new StringBuilder(1024);

            GetPrivateProfileString(sID, sKey, "", sb, 1024, this.INIfilepath);

            string val = sb.ToString();
            if (val == "")
            {
                return sDefault;
            }
            else
            {
                return val;
            }
        }

        /// <summary>   Writes a string. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sValue">   true to value. </param>
        public void WriteString(string sID, string sKey, string sValue)
        {
            WritePrivateProfileString(sID, sKey, sValue, this.INIfilepath);
        }

        /// <summary>   Reads a date time. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sDefault"> true to default. </param>
        /// <returns>   The date time. </returns>
        public DateTime ReadDateTime(string sID, string sKey, DateTime sDefault)
        {
            StringBuilder sb = new StringBuilder(1024);

            GetPrivateProfileString(sID, sKey, "", sb, 1024, this.INIfilepath);

            string s = sb.ToString();
            if (s == "")
            {
                return sDefault;
            }

            DateTime val = Convert.ToDateTime(s);
            return val;
        }

        /// <summary>   Writes a date time. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sValue">   true to value. </param>
        public void WriteDateTime(string sID, string sKey, DateTime sValue)
        {
            string val = sValue.ToString();
            WritePrivateProfileString(sID, sKey, val, this.INIfilepath);
        }

        /// <summary>   Reads an integer. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sDefault"> true to default. </param>
        /// <returns>   The integer. </returns>
        public int ReadInteger(string sID, string sKey, int sDefault)
        {
            StringBuilder sb = new StringBuilder(1024);

            GetPrivateProfileString(sID, sKey, "", sb, 1024, this.INIfilepath);

            string s = sb.ToString();
            if (s == "")
            {
                return sDefault;
            }

            int val = Convert.ToInt32(s);
            return val;
        }

        /// <summary>   Writes an integer. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sValue">   true to value. </param>
        public void WriteInteger(string sID, string sKey, int sValue)
        {
            string val = sValue.ToString();
            WritePrivateProfileString(sID, sKey, val, this.INIfilepath);
        }

        /// <summary>   Reads a double. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sDefault"> true to default. </param>
        /// <returns>   The double. </returns>
        public double ReadDouble(string sID, string sKey, double sDefault)
        {
            StringBuilder sb = new StringBuilder(1024);

            GetPrivateProfileString(sID, sKey, "", sb, 1024, this.INIfilepath);

            string s = sb.ToString();
            if (s == "")
            {
                return sDefault;
            }

            double val = Convert.ToDouble(s);
            return val;
        }

        /// <summary>   Writes a double. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sValue">   true to value. </param>
        public void WriteDouble(string sID, string sKey, double sValue)
        {
            string val = sValue.ToString();
            WritePrivateProfileString(sID, sKey, val, this.INIfilepath);
        }

        /// <summary>   Reads a bool. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sDefault"> true to default. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool ReadBool(string sID, string sKey, bool sDefault)
        {
            StringBuilder sb = new StringBuilder(1024);

            GetPrivateProfileString(sID, sKey, "", sb, 1024, this.INIfilepath);

            string s = sb.ToString();
            if (s == "")
            {
                return sDefault;
            }
            else if (s == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>   Writes a bool. </summary>
        /// <param name="sID">      The identifier. </param>
        /// <param name="sKey">     The key. </param>
        /// <param name="sValue">   true to value. </param>
        public void WriteBool(string sID, string sKey, bool sValue)
        {
            string val;
            if (sValue)
            {
                val = "1";
            }
            else
            {
                val = "0";
            }

            WritePrivateProfileString(sID, sKey, val, this.INIfilepath);
        }
        #endregion

        #region New

        public string Read(string section, string key, string value)
        {
            // capa = 255
            StringBuilder readData = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", readData, 255, INIfilepath);

            if (readData.ToString() == string.Empty)
                return value;
            else
                return readData.ToString();
        }

        /// <summary>
        /// Overload Read function
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Read(string section, string key, int value)
        {
            return int.Parse(Read(section, key, value.ToString()));
        }
        public float Read(string section, string key, float value)
        {
            return float.Parse(Read(section, key, value.ToString()));
        }
        public double Read(string section, string key, double value)
        {
            return double.Parse(Read(section, key, value.ToString()));
        }
        public decimal Read(string section, string key, decimal value)
        {
            return decimal.Parse(Read(section, key, value.ToString()));
        }
        public bool Read(string section, string key, bool value)
        {
            return bool.Parse(Read(section, key, value.ToString()));
        }

        public void Write<T>(string section, string key, T value)
        {
            WritePrivateProfileString(section, key, value.ToString(), INIfilepath);
        }
        #endregion

    }
}
