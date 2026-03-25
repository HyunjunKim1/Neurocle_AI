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

namespace Common
{
    /// <summary>   Xml node imp.  </summary>
    public class XmlNodeImp : Object
    {
        XmlNode m_Node;
        XmlDocument m_Doc;
        XmlParser m_XmlParser;
        List<XmlNodeImp> m_NodeList = new List<XmlNodeImp>();

        /// <summary>   Initializes a new instance of the XmlNodeImp class. </summary>
        /// <param name="node">         The node. </param>
        /// <param name="doc">          The document. </param>
        /// <param name="XmlParser">    The xml parser. </param>
        public XmlNodeImp(XmlNode node, XmlDocument doc, XmlParser XmlParser)
        {
            m_Node = node;
            m_Doc = doc;
            m_XmlParser = XmlParser;
        }

        /// <summary>   Gets the node. </summary>
        public XmlNode GetNode()
        {
            return m_Node;
        }

        /// <summary>   Adds a node.  </summary>
        public XmlNodeImp AddNode(String nodeName)
        {
            m_Node.AppendChild(m_Doc.CreateTextNode("\n\r"));
            AddBlank(m_Node.ParentNode);

            XmlNodeImp NewNode = new XmlNodeImp(m_Node.AppendChild(m_Doc.CreateElement(nodeName)), m_Doc, m_XmlParser);
            m_NodeList.Add(NewNode);

            return NewNode;
        }

        /// <summary>   Removes the node described by nodeName. </summary>
        public bool RemoveNode(String nodeName)
        {
            XmlNodeImp pNode = GetChildNode(nodeName);
            if (pNode != null)
            {
                XmlNode oldPtr = m_Node.RemoveChild(pNode.GetNode());
                if (oldPtr != null)
                {
                    oldPtr = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>   Removes the node described by nodeName. </summary>
        public bool RemoveNode(XmlNodeImp Node)
        {
            if (Node != null)
            {
                XmlNode oldPtr = m_Node.RemoveChild(Node.GetNode());
                if (oldPtr != null)
                {
                    oldPtr = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>   Adds a blank.  </summary>
        public void AddBlank(XmlNode parentNode)
        {
            while (parentNode != null)
            {
                m_Node.AppendChild(m_Doc.CreateTextNode("\n  "));
                parentNode = parentNode.ParentNode;
            }
        }

        /// <summary>   Ends a node. </summary>
        public void EndNode()
        {
            m_Node.AppendChild(m_Doc.CreateTextNode("\n\r"));
            AddBlank(m_Node.ParentNode.ParentNode);
        }

        /// <summary>   Gets a text. </summary>
        public String GetText(String defaultValue)
        {
            String str = m_Node.ToString();
            if (str == "")
            {
                str = defaultValue;
            }

            return str;
        }

        /// <summary>   Gets a value. </summary>
        public String GetValue(String name, String defaultValue = null)
        {
            XmlNode attribNode = m_Node.Attributes.GetNamedItem(name);

            if (attribNode != null)
            {
                String str = attribNode.ToString();
                return str;
            }
            else return defaultValue;
        }

        /// <summary>   Gets a child node. </summary>
        public XmlNodeImp GetChildNode(String name, XmlNodeImp findAfter = null)
        {
            String nodeName;
            XmlNode childNode;

            if (findAfter == null)
            {
                childNode = m_Node.FirstChild;
            }
            else
            {
                childNode = findAfter.GetNode().NextSibling;
            }

            while (childNode != null)
            {
                nodeName = childNode.Name;
                if (nodeName == name)
                {
                    XmlNodeImp pNode = null;
                    for (int index = 0; index < m_NodeList.Count; index++)
                    {
                        pNode = m_NodeList[index];
                        if (childNode == pNode.GetNode())
                        {
                            return pNode;
                        }
                    }

                    pNode = new XmlNodeImp(childNode, m_Doc, m_XmlParser);
                    m_NodeList.Add(pNode);

                    return pNode;
                }
                childNode = childNode.NextSibling;
            }
            return null;
        }

        /// <summary>   Gets a child node. </summary>
        public XmlNodeImp GetChildNode(String name, String attr, String value)
        {
            XmlNodeImp pNode = GetChildNode(name);
            while (pNode != null)
            {
                if (pNode.GetValue(attr) == value)
                {
                    return pNode;
                }
                pNode = GetChildNode(name, pNode);
            }
            return null;
        }

        /// <summary>   Sets a long. </summary>
        public void SetLong(long value)
        {
            m_Node.InnerText = value.ToString();
        }

        /// <summary>   Sets a double. </summary>
        public void SetDouble(double value)
        {
            m_Node.InnerText = value.ToString();
        }

        /// <summary>   Sets a long. </summary>
        public void SetLong(String nodeName, long value)
        {
            SetString(nodeName, value.ToString());
        }

        /// <summary>   Sets a double. </summary>
        public void SetDouble(String nodeName, double value)
        {
            SetString(nodeName, value.ToString());
        }

        /// <summary>   Sets a string. </summary>
        public void SetString(String value)
        {
            m_Node.InnerText = value;
        }

        /// <summary>   Sets a string. </summary>
        public void SetString(String nodeName, String value)
        {
            XmlAttribute attributePtr = m_Doc.CreateAttribute(nodeName);
            attributePtr.Value = value;

            m_Node.Attributes.SetNamedItem(attributePtr);
        }

        /// <summary>   Gets the first child node. </summary>
        /// <returns>   The first child node. </returns>
        public XmlNodeImp GetFirstChildNode()
        {
            if (m_Node.HasChildNodes == true)
            {
                XmlNodeImp pNewNode = new XmlNodeImp(m_Node.FirstChild, m_Doc, m_XmlParser);
                m_NodeList.Add(pNewNode);
                return pNewNode;
            }
            else
            {
                return null;
            }
        }

        /// <summary>   Gets the next node. </summary>
        /// <returns>   The next node. </returns>
        public XmlNodeImp GetNextNode()
        {
            if (m_Node.NextSibling != null)
            {
                XmlNodeImp pNewNode = new XmlNodeImp(m_Node.NextSibling, m_Doc, m_XmlParser);
                m_NodeList.Add(pNewNode);
                return pNewNode;
            }
            else
            {
                return null;
            }
        }
    }
}
