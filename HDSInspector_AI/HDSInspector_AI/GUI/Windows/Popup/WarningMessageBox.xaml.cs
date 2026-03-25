using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HDSInspector_AI.GUI.Windows.Popup
{
    /// <summary>
    /// WarningMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WarningMessageBox : Window
    {
        #region 생성자
        public WarningMessageBox()
            : this(string.Empty, string.Empty)
        {

        }
        public WarningMessageBox(string strWarningMessage)
            : this(strWarningMessage, string.Empty)
        {

        }

        #endregion

        public WarningMessageBox(string strWarningMessage, string strTitle)
        {
            InitializeComponent();
            InitializeEvent();

            if (!string.IsNullOrEmpty(strWarningMessage))
            {
                this.txtMessage.Text = strWarningMessage;
            }
            if (!string.IsNullOrEmpty(strTitle))
            {
                this.Title = strTitle;
            }
        }

        private void InitializeEvent()
        {
            this.btnOK.Click += new RoutedEventHandler(btnOK_Click);
            this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            this.KeyDown += new KeyEventHandler(WarningMessageBox_KeyDown);
        }


        #region Event handlers.

        private void WarningMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                CloseWindowWithInputMessage();
            }
            else if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            CloseWindowWithInputMessage();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }
        #endregion

        #region Close functions.
        private void CloseWindowWithInputMessage()
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CloseWindow()
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion
    }
}
