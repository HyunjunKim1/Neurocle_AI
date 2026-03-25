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
using System.Xml;
using System.Resources;
using System.Diagnostics;
using System.Windows;

namespace Common
{
    /// <summary>   Xml setting.  </summary>
    public class XmlSetting
    {
        private const char DELIMITER = '/';

        private const int SSS_SUCCESS = 0;
        private const int SSS_NODE_NOT_FOUND = -1;
        private const int SSS_PUT_TEXT_FAILED = -2;
        private const int SSS_SAVE_FAILED = -3;

        private String m_fileName;
        private XmlParser m_XmlParser = null;
        private bool m_fSyncSave = true;

        /// <summary>   Gets a setting long. </summary>
        public long GetSettingLong(String astrKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToInt32(GetSettingString(astrKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToInt32(astrValueName);
            }
        }

        /// <summary>   Gets a setting long. </summary>
        public long GetSettingLong(String astrKey, String astrSubKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToInt32(GetSettingString(astrKey, astrSubKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToInt32(astrValueName);
            }
        }

        /// <summary>   Gets a setting int. </summary>
        public int GetSettingInt(String astrKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToInt32(GetSettingString(astrKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToInt32(astrValueName);
            }
        }

        /// <summary>   Gets a setting int. </summary>
        public int GetSettingInt(String astrKey, String astrSubKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToInt32(GetSettingString(astrKey, astrSubKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToInt32(astrValueName);
            }
        }

        /// <summary>   Gets a setting double. </summary>
        public double GetSettingDouble(String astrKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToDouble(GetSettingString(astrKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToDouble(astrValueName);
            }
        }

        /// <summary>   Gets a setting double. </summary>
        public double GetSettingDouble(String astrKey, String astrSubKey, String astrValueName)
        {
            try
            {
                return System.Convert.ToDouble(GetSettingString(astrKey, astrSubKey, astrValueName));
            }
            catch
            {
                return System.Convert.ToDouble(astrValueName);
            }
            
        }

        /// <summary>   Gets a setting bool. </summary>
        public bool GetSettingBool(String astrKey, String astrValueName)
        {
            try
            {
                int i = System.Convert.ToInt32(GetSettingString(astrKey, astrValueName));
                return (i == 0) ? false : true;
            }
            catch
            {
                int i= System.Convert.ToInt32(astrValueName);
                return (i == 0) ? false : true;
            }
        }

        /// <summary>   Gets a setting bool. </summary>
        public bool GetSettingBool(String astrKey, String astrSubKey, String astrValueName)
        {
            try
            {
                int i = System.Convert.ToInt32(GetSettingString(astrKey, astrSubKey, astrValueName));
                return (i == 0) ? false : true;
            }
            catch
            {
                int i = System.Convert.ToInt32(astrValueName);
                return (i == 0) ? false : true;
            }
        }

        /// <summary>   Gets a setting string. </summary>
        public String GetSettingString(String astrKey, String astrValueName)
        {
            String value = "";

            // Add the value to the base key separated by a '/'
            astrKey += "/" + astrValueName;

            // returns the last node in the chain
            XmlNodeImp pNode = GetNode(astrKey, false);
            if (pNode != null)
            {
                value = pNode.GetNode().InnerText;
            }

            return value;
        }

        /// <summary>   Gets a setting string. </summary>
        public String GetSettingString(String astrKey, String astrSubKey, String astrValueName)
        {
            String value = "";

            // Add the value to the base key separated by a '/'
            astrKey += "/" + astrSubKey + "/" + astrValueName;
            // returns the last node in the chain
            XmlNodeImp pNode = GetNode(astrKey, false);

            if (pNode != null)
            {
                XmlNode node = pNode.GetNode();
                if (node.Name != astrSubKey)
                    value = astrValueName;
                else
                value = pNode.GetNode().InnerText;
            }

            return value;
        }

        /// <summary>   Sets a setting long. </summary>
        public long SetSettingLong(String astrKey, String astrValueName, long anValue)
        {
            String valueStr;
            valueStr = anValue.ToString();

            return SetSettingString(astrKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting long. </summary>
        public long SetSettingLong(String astrKey, String astrSubKey, String astrValueName, long anValue)
        {
            String valueStr;
            valueStr = anValue.ToString();

            return SetSettingString(astrKey, astrSubKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting int. </summary>
        public long SetSettingInt(String astrKey, String astrValueName, int anValue)
        {
            String valueStr;
            valueStr = anValue.ToString();

            return SetSettingString(astrKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting int. </summary>
        public long SetSettingInt(String astrKey, String astrSubKey, String astrValueName, int anValue)
        {
            String valueStr;
            valueStr = anValue.ToString();

            return SetSettingString(astrKey, astrSubKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting bool. </summary>
        public long SetSettingBool(String astrKey, String astrValueName, bool anValue)
        {
            String valueStr;
            if (anValue) valueStr = "1";
            else valueStr = "0";

            return SetSettingString(astrKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting bool. </summary>
        public long SetSettingBool(String astrKey, String astrSubKey, String astrValueName, bool anValue)
        {
            String valueStr;
            if (anValue) valueStr = "1";
            else valueStr = "0";

            return SetSettingString(astrKey, astrSubKey, astrValueName, valueStr);
        }

        /// <summary>   Sets a setting double. </summary>
        public long SetSettingDouble(String astrKey, String astrValueName, double afValue)
        {
            String astrValue;
            astrValue = afValue.ToString();

            return SetSettingString(astrKey, astrValueName, astrValue);
        }

        /// <summary>   Sets a setting double. </summary>
        public long SetSettingDouble(String astrKey, String astrSubKey, String astrValueName, double afValue)
        {
            String astrValue;
            astrValue = afValue.ToString();

            return SetSettingString(astrKey, astrSubKey, astrValueName, astrValue);
        }

        /// <summary>   Sets a setting string. </summary>
        public long SetSettingString(String astrKey, String astrValueName, String value)
        {
            long nRetVal = SSS_SUCCESS;

            // Add the value to the base key separated by a '/'
            astrKey += "/" + astrValueName;

            // returns the last node in the chain
            XmlNodeImp pNode = GetNode(astrKey, true);
            if (pNode != null)
            {
                // set the text of the node (will be the value we sent)
                pNode.SetString(value);

                if (m_fSyncSave)
                {
                    m_XmlParser.Save(m_fileName);
                }
            }
            else
            {
                nRetVal = SSS_NODE_NOT_FOUND;
            }

            return nRetVal;
        }

        /// <summary>   Sets a setting string. </summary>
        public long SetSettingString(String astrKey, String astrSubKey, String astrValueName, String value)
        {
            long nRetVal = SSS_SUCCESS;

            // Add the value to the base key separated by a '/'
            astrKey += "/" + astrSubKey + "/" + astrValueName;

            // Returns the last node in the chain
            XmlNodeImp pNode = GetNode(astrKey, true);
            if (pNode != null)
            {
                // Set the text of the node (will be the value we sent)
                pNode.SetString(value);

                if (m_fSyncSave)
                {
                    m_XmlParser.Save(m_fileName);
                }
            }
            else
            {
                nRetVal = SSS_NODE_NOT_FOUND;
            }

            return nRetVal;
        }

        /// <summary>   Delete a key or chain of keys. </summary>
        public bool DeleteSetting(String astrKey, String astrValueName)
        {
            bool fRetVal = false;

            // Add the value to the base key separated by a '/'
            astrKey += "/" + astrValueName;

            // returns the last node in the chain
            XmlNodeImp pNode = GetNode(astrKey, true);
            if (pNode != null)
            {
                XmlNode pParentNode = pNode.GetNode().ParentNode;
                pParentNode.RemoveChild(pNode.GetNode());

                if (m_fSyncSave) m_XmlParser.Save(m_fileName);
                fRetVal = true;
            }

            return fRetVal;
        }

        /// <summary>   Initializes this object. </summary>
        public bool Initialize(String fileName)
        {
            m_fileName = fileName;
            m_XmlParser = new XmlParser();
            
            // See if the file exists
            try
            {
                using (FileStream fs = File.Open(m_fileName, FileMode.Open, FileAccess.Read))
                {
                    fs.Close();
                    bool fResult = m_XmlParser.Open(m_fileName);
                    if (!fResult)
                    {
                        String bakFile = m_fileName + ".bak";
                        fResult = m_XmlParser.Open(bakFile);
                        if (fResult)
                        {
                            Flush();
                        }
                    }
                    return fResult;
                }
            }
            catch
            {
                String defaultXml = ResourceStringHelper.GetInformationMessage("IDS_DEFAULT_XML");
                return m_XmlParser.Create(defaultXml);
            }
        }

        /// <summary>   Sets a synchronise mode. </summary>
        public void SetSyncMode(bool fSyncSave)
        {
            m_fSyncSave = fSyncSave;
        }

        /// <summary>   Save the XML file. </summary>
        public bool Flush()
        {
            FileSupport fs = new FileSupport();
            if (!fs.IsReadonly(m_fileName))
            {
                String saveFilePath = m_fileName + ".bak";
                if (File.Exists(saveFilePath))
                {
                    try
                    {
                        File.Delete(saveFilePath);
                    }
                    catch {}
                }

                try
                {
                    File.Copy(m_fileName, saveFilePath, false);
                }
                catch {}

                return m_XmlParser.Save(m_fileName);
            }
            else return false;
        }

        /// <summary>   Gets a node. </summary>
        public XmlNodeImp GetNode(String astrKey, bool fAddNodes /*FALSE*/)
        {
            // find a node given a chain of key names
            astrKey.Replace(' ', '_');
            astrKey.Replace('\\', DELIMITER);
            astrKey.TrimEnd(DELIMITER); // remove slashes on the end

            String delimiter = DELIMITER.ToString();
            String token = String.Empty;
            Token tokenizer = new Token(astrKey);

            XmlNodeImp pNode;
            XmlNodeImp pParentNode = m_XmlParser.GetRootNode(null);
            if (pParentNode == null)
            {
                return null;
            }

            while (tokenizer.IsTokenExist())
            {
                token = tokenizer.GetNextToken(delimiter);
                
                // find the node named X directly under the parent
                pNode = pParentNode.GetChildNode(token);
                if (pNode == null)
                {
                    if (fAddNodes)
                    { 
                        // create the node and append to parent (Set only)
                        pNode = pParentNode.AddNode(token);
                    }
                    else
                    {
                        return pParentNode;
                    }
                }
                pParentNode = pNode;
            }

            return pParentNode;
        }
    }
}
