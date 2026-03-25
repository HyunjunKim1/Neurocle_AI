/*********************************************************************************
 * Copyright(c) 2015 by Haesung DS.
 * 
 * This software is copyrighted by, and is the sole property of Haesung DS.
 * All rigths, title, ownership, or other interests in the software remain the
 * property of Haesung DS. This software may only be used in accordance with
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
 * @file  NotifyPropertyChanged.cs
 * @brief
 *  It Implements INotifyPropertyChanged Interface.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.10.25
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.10.25 First creation.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Common
{
    /// <summary>   Notify property changed.  </summary>
    /// <remarks>   suoow2, 2014-10-25. </remarks>
    [Serializable]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary> Event queue for all listeners interested in PropertyChanged events. </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>   Notifies. </summary>
        /// <remarks>   suoow2, 2014-08-09. </remarks>
        /// <param name="strPropertyName">  Name of the property. </param>
        public void Notify(string strPropertyName)
        {
            PropertyChangedEventHandler p = PropertyChanged;
            if (p != null)
            {
                p(this, new PropertyChangedEventArgs(strPropertyName));
            }
        }
    }
}
