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

namespace Common
{
    /// <summary>   Values that represent Surface.  </summary>
    public enum Surface
    {
        상부검사 = 11,
        하부검사 = 21,
    }

    public enum ChannelType
    {
        RED = 1,                // Red
        GREEN = 2,             // Green 
        BLUE = 3,               // Blue 

        GRAY = 0        // Gray
    }

    /// <summary>   Values that represent NG Image Rectangle fill.  </summary>
    public enum RectFill
    {
        
        //GOOD = 0,               /* 양품 */
        //ALIGN = 1,              /* Align */
        //GROOVE = 2,             /* GROOVE */
        //LEAD = 3,               /* 리드 */
        //HALFECHING = 4,         /* 하프에칭 */
        //DOWNSET = 5,            /* 다운셋 */
        //TAPE = 6,               /* 테이프 */
        //PLATE = 7,              /* 도금 */
        //SURFACE = 8,            /* 표면 */
        //SPACE = 9,              /* 공간 */
        //OTHER = 10,             /* 기타 */
        
        GOOD = 0,
        UNPLATE = 1,
        FLASH = 2,
        BACK_FLASH = 3,

        OTHER = 10,
        NOTHING = 20
    }

    /// <summary>   Information about the NG.  </summary>
    public static class NGInformationHelper
    {
        public static RectFill GetNGEnumName(string astrNGName)
        {
            switch (astrNGName)
            {
                /*
                case "Align":
                    return RectFill.ALIGN;
                case "Groove":
                    return RectFill.GROOVE;
                case "리드":
                    return RectFill.LEAD;
                case "Half 에칭":
                    return RectFill.HALFECHING;
                case "다운셋":
                    return RectFill.DOWNSET;
                case "테이프":
                    return RectFill.TAPE;
                case "도금":
                    return RectFill.PLATE;
                case "표면":
                    return RectFill.SURFACE;
                case "공간":
                    return RectFill.SPACE;
                */
                case "미도금":
                    return RectFill.UNPLATE;
                case "Flash":
                    return RectFill.FLASH;
                case "Back Flash":
                    return RectFill.BACK_FLASH;
                default:
                    return RectFill.GOOD;
            }
        }

        public static string GetNGName(int aNGEnumNumber)
        {
            switch (aNGEnumNumber)
            {
                /*
                case (int)RectFill.ALIGN:
                    return "Align";
                case (int)RectFill.GROOVE:
                    return "Groove";
                case (int)RectFill.LEAD:
                    return "리드";
                case (int)RectFill.HALFECHING:
                    return "Half 에칭";
                case (int)RectFill.DOWNSET:
                    return "다운셋";
                case (int)RectFill.TAPE:
                    return "테이프";
                case (int)RectFill.PLATE:
                    return "도금";
                case (int)RectFill.SURFACE:
                    return "표면";
                case (int)RectFill.SPACE:
                    return "공간";
                */
                case (int)RectFill.UNPLATE:
                    return "미도금";
                case (int)RectFill.FLASH:
                    return "Flash";
                case (int)RectFill.BACK_FLASH:
                    return "Back Flash";

                default:
                    return "-";
            }
        }
    }
}
