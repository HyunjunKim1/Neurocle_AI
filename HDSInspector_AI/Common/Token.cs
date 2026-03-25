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

namespace Common
{
    /// <summary>   Token.  </summary>
    public class Token
    {
        private String m_tokenStr = string.Empty;
        private String m_original = string.Empty;
        private bool m_bMoreTokenExist = false;

        public Token(String str)
        {
            m_original = str;

            char[] trimChar = { '\n', '\r', '\t', ' ' };
            m_original.Trim(trimChar);
            if (!string.IsNullOrEmpty(m_original))
            {
                m_bMoreTokenExist = true;
            }
            else
            {
                m_bMoreTokenExist = false;
            }
        }

        /// <summary>   Query if this object is token exist. </summary>
        public bool IsTokenExist()
        {
            return m_bMoreTokenExist;
        }

        /// <summary>   Gets the next token. </summary>
        /// <returns>   The next token. </returns>
        public String GetNextToken(String delimiter, int offset = 0)
        {
            int pos = -1;

            // 더 이상 토큰이 존재하지 않는 경우.
            if (!m_bMoreTokenExist)
            {
                return "";
            }

            char[] trimChar = { '\n', '\r', '\t', ' ' };
            m_original.Trim(trimChar);
            do
            {
                pos = m_original.IndexOf(delimiter);
                if (pos == -1)
                {
                    m_tokenStr = m_original.Trim(trimChar);
                    m_bMoreTokenExist = false;

                    return m_tokenStr;
                }
                else
                {
                    m_tokenStr = m_original.Substring(0, pos).Trim(trimChar);

                    // Update original string.
                    m_original = m_original.Substring(pos + 1, m_original.Length - (pos + 1)).Trim(trimChar);
                }
                offset--;
            } while (offset > 0);

            return m_tokenStr;
        }
    }
}
