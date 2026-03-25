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
 * @file  ColorPalette.cs
 * @brief
 *  Color palette class.
 * 
 * @author : suoow <suoow.yeo@haesung.net>
 * @date : 2011.07.30
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.07.30 First creation.
 */

using System;
using System.Windows.Media;

namespace Common
{
    /// <summary>   Color palette.  </summary>
    public static class ColorPalette
    {
        public static Color[] m_colors = new Color[]
        {
            Colors.White,       /* White */
            Colors.Red,         /* Red */
            Colors.YellowGreen, /* Blue */
            Colors.RoyalBlue,   /* YellowGreen */
            Colors.Orange,      /* Yellow */
            
            Colors.Sienna,      /* Brown */
            Colors.DeepPink,    /* Pink */
            Colors.DarkOrchid,  /* Purple */
            Colors.OrangeRed,   /* Orange */
            Colors.Aqua,        /* Aqua */
            
            Colors.DarkGray,    /* DarkGray */
            Colors.DodgerBlue,  /* Mint */
            Colors.Tan,         /* Tan */
            Colors.Teal,        /* Teal */
            Colors.Olive        /* Olive */
        };

        public static Color GetColor(int anColor)
        {
            try
            {
                if (anColor < m_colors.Length)
                    return m_colors[anColor];
                else
                    return Colors.DarkGray;
            }
            catch
            {
                return Color.FromArgb(120, 192, 192, 192);
            }
        }

        public static int GetIndex(Color anColor)
        {
            try
            {
                int nLength = m_colors.Length;
                for (int nIndex = 0; nIndex < nLength; nIndex++)
                {
                    if (anColor == m_colors[nIndex])
                    {
                        return nIndex;
                    }
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
