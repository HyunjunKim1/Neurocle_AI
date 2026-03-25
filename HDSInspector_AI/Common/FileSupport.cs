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
 * @file  FileSupport.cs
 * @brief
 *  This class is necessary to load the bitmap image file. 
 * 
 * @author : All
 * @date : 2011.06.08
 * @version : 1.0
 * 
 * <b> Revision Histroy </b>
 * - 2011.06.08 First creation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Common
{
    /// <summary>   File support.  </summary>
    /// <remarks>   All, 2014-06-08. </remarks>
    public class FileSupport
    {
        /// <summary>   File copy. </summary>
        /// <remarks>   suoow2, 2014-06-08. </remarks>
        /// <param name="strSourcePath">        Full pathname of the string source file. </param>
        /// <param name="strDestinationPath">   Full pathname of the string destination file. </param>
        public static void FileCopy(string strSourcePath, string strDestinationPath)
        {
            if (string.IsNullOrEmpty(strSourcePath) || string.IsNullOrEmpty(strDestinationPath))
            {
                return;
            }

            try
            {
                if (File.Exists(strDestinationPath))
                {
                    File.Delete(strDestinationPath);
                }

                File.Copy(strSourcePath, strDestinationPath);
            }
            catch
            {
                Debug.WriteLine("Exception occured in FileCopy(FileSupport.cs)");
            }
        }

        /// <summary>   Saves an image file. </summary>
        /// <remarks>   suoow2, 2014-09-17. </remarks>
        /// <param name="strFilePath">  Full pathname of the string file. </param>
        /// <param name="source">       Source for the. </param>
        public static bool SaveImageFile(string strFilePath, BitmapSource source)
        {
            try
            {
                using (FileStream stream = new FileStream(strFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.Flush();
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(source));
                    encoder.Save(stream);
                }

                return true;
            }
            catch
            {
                Debug.WriteLine("Exception occured in SaveImageFile(FileSupport.cs)");
                return false;
            }
        }

        // 특정 파일을 삭제 시도한다. 2012-02-22, suoow2 Added.
        public static bool TryDeleteFile(string aszFilePath)
        {
            if (File.Exists(aszFilePath))
            {
                try
                {
                    File.Delete(aszFilePath);
                }
                catch
                {
                    Debug.WriteLine("Exception occured in TryDeleteFile(FileSupport.cs)");
                    return false;
                }
            }
            return true;
        }

        /// <summary>   Deletes the files described by deletePath. </summary>
        /// <remarks>   suoow2, 2014-10-24. </remarks>
        /// <param name="deletePath">   Full pathname of the delete file. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public static bool DeleteFiles(string deletePath)
        {
            try
            {
                if (Directory.Exists(deletePath))
                {
                    string[] FILES = Directory.GetFiles(deletePath);
                    foreach (string FILE in FILES)
                    {
                        try
                        {
                            File.Delete(FILE);
                        }
                        catch
                        {
                            Debug.WriteLine("");
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>   Query if 'strName' is can create directory. </summary>
        /// <remarks>   suoow2, 2014-06-08. </remarks>
        /// <param name="strName">  Name of the string. </param>
        /// <returns>   true if can create directory, false if not. </returns>
        public static bool IsCanCreateDirectory(string strName)
        {
            strName = strName.ToUpper();

            if (strName.IndexOf(@"\") != -1)
            {
                return false; // invalid DIR-name.
            }
            if (strName.IndexOfAny(new char[] { '/', ':', '*', '?', '"', '<', '>', '|' }) != -1)
            {
                return false; // invalid DIR-name.
            }
            else if (strName.Length == 3)
            {
                switch (strName)
                {
                    case "CON":
                    case "PRN":
                    case "AUX":
                    case "NUL": return false; // invalid DIR-name.
                }
            }
            else if (strName.Length == 4)
            {
                switch (strName)
                {
                    case "COM1":
                    case "COM2":
                    case "COM3":
                    case "COM4":
                    case "COM5":
                    case "COM6":
                    case "COM7":
                    case "COM8":
                    case "COM9":
                    case "LPT1":
                    case "LPT2":
                    case "LPT3":
                    case "LPT4":
                    case "LPT5":
                    case "LPT6":
                    case "LPT7":
                    case "LPT8":
                    case "LPT9": return false; // invalid DIR-name.
                }
            }

            return true; // valid DIR-name.
        }

        /// <summary>   Gets a file. </summary>
        /// <remarks>   suoow, 2014-06-08. </remarks>
        /// <param name="strPath">  Full pathname of the string file. </param>
        /// <param name="anIndex">  Zero-based index of an. </param>
        /// <returns>   The file. </returns>
        public static string GetFile(string strPath, int anIndex)
        {
            try
            {
                string[] FILES = Directory.GetFiles(strPath);

                return FILES[anIndex];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>   Query if 'path' is file. </summary>
        /// <remarks>   Sungbok, Hong, 2014-06-08. </remarks>
        /// <param name="path">         Full pathname of the file. </param>
        /// <param name="fOkAtBlank">   (optional) the ok at blank. </param>
        /// <returns>   true if file, false if not. </returns>
        public bool IsFile(String path, bool fOkAtBlank)
        {
            return FileCheck(path, fOkAtBlank);
        }

        /// <summary>   File check. </summary>
        /// <remarks>   Sungbok, Hong, 2014-06-08. </remarks>
        /// <param name="path">         Full pathname of the file. </param>
        /// <param name="fOkAtBlank">   (optional) the ok at blank. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool FileCheck(String path, bool fOkAtBlank)
        {
            FileAttributes result = File.GetAttributes(path);

            if ((result & FileAttributes.Directory) > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>   Force directories. </summary>
        /// <remarks>   Shin.Cheol-min, 2012-02-21. </remarks>
        /// <param name="aszFilePath">  Full pathname of the asz file. </param>
        public static void ForceDirectories(string aszFilePath)
        {
            aszFilePath.Replace('/', '\\');
            string szFileName;
            int nLastSeperator = aszFilePath.LastIndexOf('\\');
            if (nLastSeperator == -1)
                return;

            szFileName = aszFilePath.Substring(nLastSeperator + 1);
            if (szFileName.IndexOf('.') == -1)
                Directory.CreateDirectory(aszFilePath);
            else
                Directory.CreateDirectory(aszFilePath.Substring(0, nLastSeperator));
        }

        /// <summary>   Query if 'path' is readonly. </summary>
        /// <remarks>   Sungbok, Hong, 2014-06-08. </remarks>
        /// <param name="path"> Full pathname of the file. </param>
        /// <returns>   true if readonly, false if not. </returns>
        public bool IsReadonly(String path)
        {
            FileAttributes result = File.GetAttributes(path);
            if ((result & FileAttributes.ReadOnly) == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>   Query if 'path' is directory. </summary>
        /// <remarks>   Sungbok, Hong, 2014-06-08. </remarks>
        /// <param name="path">         Full pathname of the file. </param>
        /// <param name="fOkAtBlank">   (optional) the ok at blank. </param>
        /// <returns>   true if directory, false if not. </returns>
        public bool IsDirectory(String path, bool fOkAtBlank = false)
        {
            return DirectoryCheck(path, fOkAtBlank);
        }

        /// <summary>   Directory check. </summary>
        /// <remarks>   Sungbok, Hong, 2014-06-08. </remarks>
        /// <param name="path">         Full pathname of the file. </param>
        /// <param name="fOkAtBlank">   (optional) the ok at blank. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool DirectoryCheck(String path, bool fOkAtBlank = false)
        {
            FileAttributes result = File.GetAttributes(path);

            if ((result & FileAttributes.Directory) == 0)
            {
                return false;
            }

            return true;
        }
    }
}
