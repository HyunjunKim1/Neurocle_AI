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
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace Common
{
    /// <summary>   Xml parser.  </summary>
    public class XmlParser
    {
        /// <summary> The root node </summary>
        private XmlNodeImp m_RootNode;

        /// <summary> The document </summary>
        private XmlDocument m_Doc;

        /// <summary>   Creates this object. </summary>
        public bool Create(String szDefaultData)
        {
            m_Doc = new XmlDocument();
            if (!string.IsNullOrEmpty(szDefaultData))
            {
                try
                {
                    m_Doc.LoadXml(szDefaultData);
                    m_RootNode = new XmlNodeImp(m_Doc.DocumentElement, m_Doc, this);
                }
                catch (XmlException e)
                {
                    Debug.WriteLine(e.Message);
                    m_Doc = null;

                    return false;
                }
            }
            return true;
        }

        /// <summary>   Opens. </summary>
        public bool Open(String lpszPathName)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(lpszPathName) && File.Exists(lpszPathName))
            {
                m_Doc = new XmlDocument();
                
                try
                {
                    m_Doc.Load(lpszPathName);
                    m_RootNode = new XmlNodeImp(m_Doc.DocumentElement, m_Doc, this);
                    result = true;
                }
                catch (XmlException e)
                {
                    Debug.WriteLine(e.Message);
                    m_Doc = null;
                }
            }
            return result;
        }

        /// <summary>   Saves. </summary>
        public bool Save(String lpszPathName)
        {
            try
            {
                m_Doc.Save(lpszPathName);
            }
            catch (XmlException e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>   Gets a root node. </summary>
        public XmlNodeImp GetRootNode(String nodeName)
        {
            if (m_RootNode == null && nodeName != null)
            {
                m_RootNode = new XmlNodeImp(m_Doc.AppendChild(m_Doc.CreateElement(nodeName)), m_Doc, this);
            }
            return m_RootNode;
        }
    }
}
