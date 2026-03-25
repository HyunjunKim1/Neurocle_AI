using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization.Charting.Compatible;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;

namespace Common
{
    public static class language
    {


        public static void Language_setting(Object win, string path, string projectName, string className, int type)
        {
            string filePath = string.Format(path + "\\{0}\\{1}.language", projectName, className);
            
            try
            {
                // StreamReader를 사용하여 파일을 읽음
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    string objName;
                    object obj = null;
                    bool bTitle = false; // window title 플래그
                    
                    // 파일 끝까지 한 줄씩 읽음
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "" || line == " ") continue;                        
                        if (line.Contains("</")) continue;
                        if (line.Contains("<"))
                        {
                            //if (line.Contains("Title"))
                            //{
                            //    bTitle = true;
                            //    //continue;
                            //}
                            objName = line.Substring(1, line.Length - 2);
                            if (objName.ToUpper() == "TITLE") bTitle = true;
                            //obj = FindName(objName);
                            obj = FindObjectByName(win, objName);
                            continue;
                        }

                        string[] str, values;
                        str = line.Split('=');

                        string property = str[0];                               
                        values = str[1].Split(',');

                        string value = (type > 0) ? values[1] : values[0];

                        value = value.Replace("\\n", "\n");

                        #region Title
                        if (bTitle)
                        {
                            SetThisTitle(win, value);
                            bTitle = false;
                        }
                        #endregion

                        #region Button( Btn,Radio Btn, Check box, ToggleBtn )
                        if (obj is Button)
                        {
                            Button btn = (Button)obj;

                            if (property.ToUpper() == "TOOLTIP")
                            {
                                btn.ToolTip = value;
                            }

                            else if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT")
                            {
                                // Content가 StackPanel인 경우 텍스트만 변경
                                if (btn.Content is StackPanel stackPanel)
                                {
                                    // StackPanel 내에서 TextBlock과 Image를 분리하여 TextBlock만 변경
                                    foreach (var child in stackPanel.Children)
                                    {
                                        if (child is TextBlock textBlock)
                                        {
                                            // TextBlock의 텍스트만 변경
                                            textBlock.Text = value;
                                        }
                                    }
                                }
                                else
                                {
                                    // Content가 StackPanel이 아니면 그냥 Content를 변경
                                    btn.Content = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children = {
                                        new Image { Source = btn.Content as ImageSource },  // 기존 이미지 유지
                                        new TextBlock { Text = value }                       // 새로운 텍스트 설정
                                        }
                                    };
                                }
                            }

                            else if (property.ToUpper() == "FONTSIZE")
                            {
                                btn.FontSize = Double.Parse(value);
                            }

                        }

                        else if (obj is RadioButton radioBtn)
                        {
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT") radioBtn.Content = value;
                            else if (property.ToUpper() == "FONTSIZE") radioBtn.FontSize = Double.Parse(value);
                        }

                        else if (obj is CheckBox)
                        {
                            CheckBox checkBox = obj as CheckBox;
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT") checkBox.Content = value;
                            else if (property.ToUpper() == "FONTSIZE") checkBox.FontSize = Double.Parse(value);

                        }

                        else if (obj is ToggleButton btn)
                        {
                            if (property.ToUpper() == "TOOLTIP")
                            {
                                btn.ToolTip = value;
                            }

                            else if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT")
                            {
                                // Content가 StackPanel인 경우 텍스트만 변경
                                if (btn.Content is StackPanel stackPanel)
                                {
                                    // StackPanel 내에서 TextBlock과 Image를 분리하여 TextBlock만 변경
                                    foreach (var child in stackPanel.Children)
                                    {
                                        if (child is TextBlock textBlock)
                                        {
                                            // TextBlock의 텍스트만 변경
                                            textBlock.Text = value;
                                        }
                                    }
                                }
                                else
                                {
                                    // Content가 StackPanel이 아니면 그냥 Content를 변경
                                    btn.Content = new StackPanel
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children = {
                                        new Image { Source = btn.Content as ImageSource },  // 기존 이미지 유지
                                        new TextBlock { Text = value }                       // 새로운 텍스트 설정
                                        }
                                    };
                                }
                            }

                            else if(property.ToUpper()=="FONTSIZE")
                            {
                                btn.FontSize=Double.Parse(value);
                            }
                        }

                        #endregion

                        #region Image
                        else if (obj is Image img)
                        {
                            string uriSource = "pack://application:,,,/HDSInspector;component/Images/" + value;
                            img.Source = new ImageSourceConverter().ConvertFromString(uriSource) as ImageSource;
                        }
                        #endregion

                        #region DataGrid
                        else if (obj is System.Windows.Controls.DataGridComboBoxColumn CbCol)
                        {
                            if (property.ToUpper() == "HEADER") CbCol.Header = value;
                        }

                        else if (obj is System.Windows.Controls.DataGridTextColumn TbCol)
                        {
                            if (property.ToUpper() == "HEADER") TbCol.Header = value;
                        }

                        else if (obj is System.Windows.Controls.DataGridCheckBoxColumn chkCol)
                        {
                            if (property.ToUpper() == "HEADER") chkCol.Header = value;
                        }
                        #endregion

                        #region Text(Box,Block),Label
                        else if (obj is TextBox tbx)
                        {
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT") tbx.Text = value;
                            else if (property.ToUpper() == "FONTSIZE") tbx.FontSize = Double.Parse(value);
                        }

                        else if (obj is TextBlock)
                        {
                            TextBlock tb = obj as TextBlock;

                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT") tb.Text = value;
                            else if (property.ToUpper() == "FONTSIZE") tb.FontSize = Double.Parse(value);
                        }

                        else if (obj is GridViewColumn)
                        {
                            GridViewColumn Gb = obj as GridViewColumn;
                            if (property.ToUpper() == "HEADER") Gb.Header = value;         
                        }

                        else if (obj is DataGridTextColumn)
                        {
                            DataGridTextColumn DG = obj as DataGridTextColumn;
                            if (property.ToUpper() == "HEADER") DG.Header = value;
                        }

                        else if(obj is DataGridTemplateColumn)
                        {
                            DataGridTemplateColumn DGT = obj as DataGridTemplateColumn;
                            if (property.ToUpper() == "HEADER") DGT.Header = value;
                        }


                        else if (obj is CheckBox)
                        {
                            CheckBox checkBox = obj as CheckBox;
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT") checkBox.Content = value;
                            else if (property.ToUpper() == "FONTSIZE") checkBox.FontSize = Double.Parse(value);
                            
                        }
                        else if(obj is Label)
                        {
                            Label label = obj as Label;
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT")
                            {
                                label.Content = value;
                            }
                            else if (property == "FontSize")
                            {
                                label.FontSize = Double.Parse(value);
                            }

                        }

                        #endregion

                        #region GridView
                        else if (obj is GridViewColumn)
                        {
                            GridViewColumn Gb = obj as GridViewColumn;
                            Gb.Header = value;
                        }
                        #endregion

                        #region BusyIndicator
                        else if (obj is Microsoft.Windows.Controls.BusyIndicator bi)
                        {
                            if (property.ToUpper() == "TEXT" || property.ToUpper() == "CONTENT")
                            {
                                bi.BusyContent = value;
                            }
                        }
                        #endregion

                        #region Charting
                        else if (obj is System.Windows.Controls.DataVisualization.Charting.BarSeries)
                        {
                            if (property.ToUpper() == "TITLE") SetThisTitle(obj, value);
                        }

                        else if (obj is System.Windows.Controls.DataVisualization.Charting.Chart)
                        {
                            if (property.ToUpper() == "TITLE") SetThisTitle(obj, value);
                        }
                        #endregion
                    }
                }
            }
            catch
            {

            }

        }

        public static bool SetThisTitle(object obj, string val)
        {   
            if (obj == null || string.IsNullOrEmpty(val)) return false;            

            if(obj is Window win)
            {
                win.Title = val;
            }

            else if(obj is System.Windows.Controls.DataVisualization.Charting.BarSeries bs)
            {
                bs.Title = val;
            }           

            else if(obj is System.Windows.Controls.DataVisualization.Charting.Chart chart)
            {
                chart.Title = val;
            }
            
            return true;
        }

        public static object FindObjectByName(object container, string objName)
        {
            if (container == null) return null;

            // Window나 UserControl에서 객체 찾기
            if (container is Window win)
            {   
                return FindElementByName(win, objName);
            }
            else if (container is UserControl userControl)
            {
                return FindElementByName(userControl, objName);
            }
            else if(container is Page page)
            {
                return FindElementByName(page, objName);
            }
            // 필요한 경우 다른 타입의 컨테이너를 추가로 처리 가능
            
            return null;
        }


        private static object FindElementByName(FrameworkElement container, string objName)
        {
            return container.FindName(objName);
        }


    }

    
}
