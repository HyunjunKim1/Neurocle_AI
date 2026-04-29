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
 * @file  ContextMenuHelper.cs
 * @brief
 *  This class is necessary to create contextmenu.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.08.01
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.08.01 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Common.Drawing
{
    /// <summary>   Context menu object.  </summary>
    /// <remarks>   suoow2, 2014-08-01. </remarks>
    public class ContextMenuObject
    {
        public int m_nID;
        public int m_nParentID;
        public string m_strName;
        public string m_strImageUri;
        public bool m_bHasEvent;
    }

    /// <summary>   Context menu helper.  </summary>
    /// <remarks>   suoow2, 2014-08-01. </remarks>
    public class ContextMenuHelper
    {
        /// <summary> The mouse down event handler </summary>
        private RoutedEventHandler m_MouseDownEventHandler;

        /// <summary> The menu hash table </summary>
        private Hashtable m_MenuHashTable = new Hashtable();

        /// <summary>   Initializes a new instance of the ContextMenuHelper class. </summary>
        /// <remarks>   suoow2, 2014-08-01. </remarks>
        /// <param name="MouseDownEventHandler">    The mouse down event handler. </param>
        public ContextMenuHelper(RoutedEventHandler MouseDownEventHandler)
        {
            this.m_MouseDownEventHandler = MouseDownEventHandler;
        }

        /// <summary>   Creates a menu. </summary>
        /// <remarks>   suoow2, 2014-08-01. </remarks>
        /// <param name="listContextMenuObject">    The list context menu object. </param>
        /// <returns>   ContextMenu </returns>
        public ContextMenu CreateMenu(List<ContextMenuObject> listContextMenuObject)
        {
            ContextMenu contextMenu = new ContextMenu();

            m_MenuHashTable[0] = contextMenu;

            foreach (ContextMenuObject menuObject in listContextMenuObject)
            {
                MenuItem menuItem = new MenuItem { Header = menuObject.m_strName, Tag = menuObject.m_nID };
                if (!String.IsNullOrEmpty(menuObject.m_strImageUri))
                {
                    menuItem.Icon = new Image() { Source = new BitmapImage(new Uri(menuObject.m_strImageUri, UriKind.Relative)), Width = 22 };
                }

                if (menuObject.m_bHasEvent)
                {
                    menuItem.Click += m_MouseDownEventHandler;
                }

                m_MenuHashTable[menuObject.m_nID] = menuItem;

                // 0 : root hierarchy.
                // else : sub-context menus.
                if (menuObject.m_nParentID == 0)
                {
                    ContextMenu rootCTXMenu = m_MenuHashTable[menuObject.m_nParentID] as ContextMenu;
                    if (rootCTXMenu != null)
                    {
                        rootCTXMenu.Items.Add(menuItem);
                    }
                }
                else
                {
                    MenuItem subMenuItem = m_MenuHashTable[menuObject.m_nParentID] as MenuItem;
                    if (subMenuItem != null)
                    {
                        subMenuItem.Items.Add(menuItem);
                    }
                }
            }

            return contextMenu;
        }
    }
}
