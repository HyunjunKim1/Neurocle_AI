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
 * @file  NotifyCollectionChangedHelper.cs
 * @brief
 *  NotifyCollectionChanged Helper class. (it boosts update of ObservableCollection)
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.06.18
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.06.18 - First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;

namespace Common
{
    /// <summary>   Notify collection changed helper.  </summary>
    /// <remarks>   suoow2, 2014-06-18. </remarks>
    public static class NotifyCollectionChangedHelper
    {
        /// <summary>   Gets or sets a dictionary of collections. </summary>
        /// <value> A Dictionary of collections. </value>
        private static Dictionary<INotifyCollectionChanged, Delegate> CollectionDictionary
        {
            get;
            set;
        }

        /// <summary>   Initializes static members of the NotifyCollectionChangedHelper class. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        static NotifyCollectionChangedHelper()
        {
            CollectionDictionary = new Dictionary<INotifyCollectionChanged, Delegate>();
        }

        /// <summary>
        /// An INotifyCollectionChanged extension method that pasue notify collection changed.
        /// </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="Target">   The Target to act on. </param>
        public static void PasueNotifyCollectionChanged(this INotifyCollectionChanged Target)
        {
            FieldInfo CollectionChangedField = Target.GetCollectionChangedFiled();
            Delegate Result = CollectionChangedField.GetValue(Target) as Delegate;
            CollectionDictionary[Target] = Result;
            CollectionChangedField.SetValue(Target, null);
        }

        /// <summary>
        /// An INotifyCollectionChanged extension method that resume notify collection chaged.
        /// </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="Target">   The Target to act on. </param>
        public static void ResumeNotifyCollectionChaged(this INotifyCollectionChanged Target)
        {
            if (CollectionDictionary.ContainsKey(Target) == false)
            {
                return;
            }

            Target.GetCollectionChangedFiled().SetValue(Target, CollectionDictionary[Target]);
            CollectionDictionary.Remove(Target);
        }

        /// <summary>   An INotifyCollectionChanged extension method that refreshes. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="Target">   The Target to act on. </param>
        public static void Refresh(this INotifyCollectionChanged Target)
        {
            MethodInfo CollectionResetMethod = Target.GetCollectionResetMethod();

            CollectionResetMethod.Invoke(Target, null);
        }

        /// <summary>
        /// An INotifyCollectionChanged extension method that gets a collection changed filed.
        /// </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="Target">   The Target to act on. </param>
        /// <returns>   The collection changed filed. </returns>
        public static FieldInfo GetCollectionChangedFiled(this INotifyCollectionChanged Target)
        {
            FieldInfo Field = Target.GetType().GetField("CollectionChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            return Field;
        }

        /// <summary>
        /// An INotifyCollectionChanged extension method that gets a collection reset method.
        /// </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="Target">   The Target to act on. </param>
        /// <returns>   The collection reset method. </returns>
        public static MethodInfo GetCollectionResetMethod(this INotifyCollectionChanged Target)
        {
            MethodInfo Method = Target.GetType().GetMethod("OnCollectionReset", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return Method;
        }
    }
}
