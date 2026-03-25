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
 * @file  NumberTextBox.cs
 * @brief 
 *  number text box custom control.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.05.19
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.05.02 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Common
{
    /// <summary>   Number text box.  </summary>
    /// <remarks>   suoow2, 2014-05-19. </remarks>
    public class NumberTextBox : TextBox
    {
        #region Override

        /// <summary>   Raises the initialized event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <exception cref="Exception">    Thrown when exception. </exception>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnInitialized(EventArgs e)
        {
            decimal decimalValue;

            // if TextBox.Text is not null or empty.
            if (!String.IsNullOrEmpty(Text))
            {
                if (decimal.TryParse(Text, out decimalValue))
                {
                    if (!(MaxValue >= decimalValue && MinValue <= decimalValue))
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }

            if (Value != null)
            {
                if (!(Value <= MaxValue && Value >= MinValue))
                {
                    throw new Exception();
                }
            }

            //create tooltip.
            //object ToolTipString = "입력허용 범위 : " + MinValue + " ~ " + MaxValue;
            //if (Round > 0)
            //{
            //    ToolTipString += "\n입력가능 소수자리 : " + Round;
            //}
            //ToolTip = ToolTipString;

            base.OnInitialized(e);
        }

        /// <summary>   Raises the lost focus event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            // final validation.
            if (MaxValue <= Value)
            {
                Value = MaxValue;
            }
            else if (Value <= MinValue)
            {
                Value = MinValue;
            }

            Text = Value.ToString();

            base.OnLostFocus(e);
        }

        /// <summary>   Raises the got focus event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            SelectionStart = 0;
            SelectionLength = Text.Length;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (SelectionLength == 0)
            {
                SelectionStart = 0;
                SelectionLength = Text.Length;
            }            
        }

        /// <summary>   Raises the preview key down event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // cancel space and comma inputs.
            if (e.Key == Key.Space || e.Key == Key.OemComma)
            {
                e.Handled = true;
                return;
            }

            // if round == 0, cancel dot input.
            if (Round == 0 && e.Key == Key.OemPeriod)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>   Raises the text input event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            decimal decimalValue;
            string stringValue;

            if (SelectionLength > 0)
            {
                stringValue = Text.Remove(SelectionStart, SelectionLength);
            }
            else
            {
                stringValue = Text;
            }

            // if TextBox.Text.Length > 0
            if (stringValue.Length > 0)
            {
                if (SelectionStart == Text.Length)
                {
                    stringValue = stringValue + e.Text;
                }
                else if (SelectionStart == 0)
                {
                    stringValue = e.Text + stringValue;
                }
                else if (SelectionStart > 0 && SelectionStart < stringValue.Length)
                {
                    stringValue = stringValue.Substring(0, SelectionStart) + e.Text + stringValue.Substring(SelectionStart, stringValue.Length - SelectionStart);
                }
            }
            else
            {
                stringValue = stringValue + e.Text;
            }

            // stored current cusor position.
            int tempSelectionStart = SelectionStart;
            if (e.Text == "-" && Text.IndexOf('-') == -1)
            {
                if (decimal.TryParse(stringValue, out decimalValue))
                {
                    if (MaxValue >= decimalValue && MinValue <= decimalValue)
                    {
                        Text = "-" + Text;
                        e.Handled = true;
                        // update current cursor position. (-)
                        SelectionStart = tempSelectionStart + 1;
                        return;
                    }
                }
            }

            if (stringValue == "-" || stringValue == "." || stringValue == "-.")
            {
                base.OnTextInput(e);
            }

            if (decimal.TryParse(stringValue, out decimalValue))
            {
                if (MaxValue >= decimalValue && MinValue <= decimalValue)
                {
                    if (Round > 0 && stringValue.IndexOf('.') > -1)
                    {
                        // split by '.'
                        string[] tmpSplit = stringValue.Split('.');

                        // check round length.
                        if (tmpSplit[1].Length <= Round)
                        {
                            base.OnTextInput(e);
                        }
                        else
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    base.OnTextInput(e);
                }
            }

            // cancelation.
            e.Handled = true;
            return;
        }

        /// <summary>   Raises the text changed event. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            decimal decimalValue;

            if (decimal.TryParse(Text, out decimalValue))
            {
                Value = decimalValue;
            }
            else
            {
                Value = null;
            }
        }
        #endregion

        #region Property
        /// <summary> The Decimal maximum value </summary>
        private const Decimal DefaultMax = Decimal.MaxValue;

        /// <summary> The Decimal minimum value </summary>
        private const Decimal DefaultMin = Decimal.MinValue;

        /// <summary> The maximum value property </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(Decimal?), typeof(NumberTextBox), new FrameworkPropertyMetadata(DefaultMax));

        /// <summary> The minimum value property </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(Decimal?), typeof(NumberTextBox), new FrameworkPropertyMetadata(DefaultMin));

        /// <summary>   Gets or sets the MaxValueProperty. </summary>
        /// <value> The MaxValueProperty. </value>
        public Decimal? MaxValue
        {
            get { return (Decimal?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>   Gets or sets the MinValueProperty. </summary>
        /// <value> The MinValueProperty. </value>
        public Decimal? MinValue
        {
            get { return (Decimal?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary> The round property </summary>
        public static readonly DependencyProperty RoundProperty = DependencyProperty.Register("Round", typeof(Int32?), typeof(NumberTextBox), new FrameworkPropertyMetadata(null));

        /// <summary>   Gets or sets the RoundProperty. </summary>
        /// <value> The RoundProperty. </value>
        public Int32? Round
        {
            get { return (Int32?)GetValue(RoundProperty); }
            set { SetValue(RoundProperty, value); }
        }

        /// <summary> The value property </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Decimal?), typeof(NumberTextBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        /// <summary>   Gets or sets the ValueProperty. </summary>
        /// <value> The ValueProperty. </value>
        public Decimal? Value
        {
            get { return (Decimal?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>   Executes the value changed action. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="obj">  The object. </param>
        /// <param name="args"> Event information to send to registered event handlers. </param>
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumberTextBox control = (NumberTextBox)obj;
            RoutedPropertyChangedEventArgs<Decimal?> e = new RoutedPropertyChangedEventArgs<Decimal?>((Decimal?)args.OldValue, (Decimal?)args.NewValue, ValueChangedEvent);
            control.OnValueChanged(e);
        }

        /// <summary> The value changed event </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Decimal?>), typeof(NumberTextBox));

        /// <summary>   Gets the value changed. </summary>
        /// <value> The value changed. </value>
        public event RoutedPropertyChangedEventHandler<Decimal?> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>   Executes the value changed action. </summary>
        /// <remarks>   suoow2, 2014-06-18. </remarks>
        /// <param name="args"> The arguments. </param>
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<Decimal?> args)
        {
            RaiseEvent(args);
        }
        #endregion
    }
}