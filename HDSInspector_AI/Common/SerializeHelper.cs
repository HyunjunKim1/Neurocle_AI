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
/**
 * @file  SerializeHelper.cs
 * @brief
 *  do Serialize and De-Serialize
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.06.01
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.06.01 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Common
{
    /// <summary>   Serialize helper.  </summary>
    /// <remarks>   suoow2, 2014-06-01. </remarks>
    public class SerializeHelper
    {
        /// <summary>   Serialize object. </summary>
        /// <remarks>   suoow2, 2014-06-01. </remarks>
        /// <param name="type">         The type. </param>
        /// <param name="obj">          The object. </param>
        /// <param name="saveFilePath"> Full pathname of the save file. </param>
        public static void SerializeObject(Type type, object obj, string saveFilePath)
        {
            XmlSerializer xml = new XmlSerializer(type);

            using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xml.Serialize(fileStream, obj);
            }
        }

        /// <summary>   De serialize object. </summary>
        /// <remarks>   suoow2, 2014-06-01. </remarks>
        /// <param name="type">         The type. </param>
        /// <param name="openFilePath"> Full pathname of the open file. </param>
        /// <returns>   . </returns>
        public static object DeSerializeObject(Type type, string openFilePath)
        {
            XmlSerializer xml = new XmlSerializer(type);
            object obj = null;

            using (FileStream fileStream = new FileStream(openFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                obj = xml.Deserialize(fileStream);
            }

            return obj;
        }
    }
}
