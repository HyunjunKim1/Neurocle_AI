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
 * @file  EquipInformation.cs
 * @brief
 *  It tells informations aboue equipment.
 * 
 * @author :suoow <suoow.yeo@haesung.net>
 * @date : 2011.10.01
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.10.01 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>   Values that represent EquipStatus.  </summary>
    public enum EquipStatus
    {
        CONNECT = 0,
        CONNECTWAIT = 1,        
        VERIFY = 2,
        VERIFYWAIT = 3,
        VERIFYSKIP = 4,
        INSPECT = 5,
        BADWAIT = 6
    }

    /// <summary>   Information about the equip.  </summary>
    public static class EquipInformation
    {
        public static string GetEquipStatus(int aEquipStatus)
        {
            switch (aEquipStatus)
            {
                case (int)EquipStatus.CONNECT:
                    return "연결됨";
                case (int)EquipStatus.CONNECTWAIT:
                    return "연결대기";
                case (int)EquipStatus.VERIFY:
                    return "VERIFY";
                case (int)EquipStatus.VERIFYWAIT:
                    return "VERIFY 대기";
                case (int)EquipStatus.VERIFYSKIP:
                    return "VERIFY SKIP";
                case (int)EquipStatus.INSPECT:
                    return "검사중";
                case (int)EquipStatus.BADWAIT:
                    return "불량검증";
                default:
                    return "정보없음";
            }
        }
    }
}
