/*********************************************************************************
 * Copyright(c) 2011,2012,2013 by Samsung Techwin.
 * 
 * This software is copyrighted by, and is the sole property of Samsung Techwin.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Samsung Techwin. This software may only be used in accordance with
 * the corresponding license agreement. Any unauthorized use, duplication, 
 * transmission, distribution, or disclosure of this software is expressly 
 * forbidden.
 *
 * This Copyright notice may not be removed or modified without prior written
 * consent of Samsung Techwin.Samsung Techwin reserves the right to modify this 
 * software without notice.
 *
 * Samsung Techwin.
 * KOREA 
 * http://www.samsungtechwin.co.kr
 *********************************************************************************/
/**
 * @file  SettingOption.cs
 * @brief
 *  Setting about Option node in Settings.xml file
 * 
 * @author : Cheol Min <shin.chul-min@samsung.com>
 * @date : 2011.05.25
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.25 First creation.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /** 
     * @brief This class is Handling Option node in Settings.xml file
     * @author Cheolmin Shin
     * @date 2011.05.25
     */
    public class SettingOption
    {
        /**
         * @var m_XmlSetting
         * @brief Xmlsetting value comes from Settings class
        */
        private XmlSetting m_XmlSetting = null;

        /**
         * @var m_PCSPort
         * @brief PCS Port number
        */
        private long m_PCSPort = 0;

        public long PCSPort
        {
            get { return m_PCSPort; }
            set { m_PCSPort = value; }
        }

        private String m_DBIP = null;
        public String DBIP
        {
            get { return m_DBIP = m_XmlSetting.GetSettingString("Option/Database", "DBIP", "localhost");  }
            set { m_DBIP = value; }
        }


        private String m_DBPort = null;
        public String DBPort
        {
            get { return m_DBPort = m_XmlSetting.GetSettingString("Option/Database", "Port", "3306"); }
            set { m_DBPort = value; }
        }


        private String m_UsePLC = null;
        public String UsePLC
        {
            get { return m_UsePLC = m_XmlSetting.GetSettingString("Option/PLC", "UsePLC", "0"); }
            set { m_UsePLC = value; }
        }


        /**
         * @fn SettingOption(XmlSetting aXmlSetting)
         * @brief default constructor.
         */
        public SettingOption(XmlSetting aXmlSetting)
        {
            if (aXmlSetting == null)
                aXmlSetting = new XmlSetting();

            m_XmlSetting = aXmlSetting;
        }

        /**
         * @fn GetXmlSetting()
         * @brief Get m_XmlSetting value.
         */
        public XmlSetting GetXmlSetting()
        {
            return m_XmlSetting;
        }
    }
}
