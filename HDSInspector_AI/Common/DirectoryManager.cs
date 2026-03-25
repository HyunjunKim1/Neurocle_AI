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
 * @file  DirectoryManager.cs
 * @brief
 *  Directory Manager class.
 * 
 * @author : suoow2 <suoow.yeo@haesung.net>
 * @date : 2011.09.14
 * @version : 1.1
 * 
 * <b> Revision Histroy </b>
 * - 2011.09.14 First creation.
 * - 2011.09.15 Some methods added.
 */

using System;
using System.IO;
using System.Diagnostics;

namespace Common
{
    /// <summary>   Manager for directories.  </summary>
    /// <remarks>   suoow2, 2014-09-14. </remarks>
    public static class DirectoryManager
    {
        // DIR 생성
        public static void CreateDirectory(string aszPath)
        {
            if (!Directory.Exists(aszPath))
            {
                Directory.CreateDirectory(aszPath);
            }
        }

        public static void CreateHiddenDirectory(string aszPath)
        {
            if (!Directory.Exists(aszPath))
            {
                DirectoryInfo dir = Directory.CreateDirectory(aszPath); 
                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;  
            }
        }

        // DIR 및 내부 파일 삭제.
        public static bool DeleteDirectory(string aszDeletePath)
        {
            try
            {
                if (Directory.Exists(aszDeletePath))
                {
                    string[] DIRECTORIES = Directory.GetDirectories(aszDeletePath);
                    string[] FILES = null;

                    foreach (string DIRECTORY in DIRECTORIES)
                    {
                        FILES = Directory.GetFiles(DIRECTORY);
                        foreach (string FILE in FILES)
                        {
                            File.Delete(FILE);
                        }
                        Directory.Delete(DIRECTORY);
                    }

                    FILES = Directory.GetFiles(aszDeletePath);
                    foreach (string FILE in FILES)
                    {
                        File.Delete(FILE);
                    }
                    Directory.Delete(aszDeletePath);
                }
                return true;
            }
            catch
            {
                Debug.WriteLine("Exception occured in DeleteDirectory(DirectoryManager.cs)");
                return false;
            }
        }

        // DIR 이동
        public static void MoveDirectory(string aszPath1, string aszPath2)
        {
            try
            {
                Directory.Move(aszPath1, aszPath2);
            }
            catch
            {
                Debug.WriteLine("Exception occured in MoveDirectory(DirectoryManager.cs)");
            }
        }

        // 상위 폴더의 경로 획득
        public static string GetParentPath(string strPath)
        {
            return new DirectoryInfo(strPath).Parent.FullName;
        }

        public static string GetCombinedPathName(string aszParentPath, string aszChildPath)
        {
            string strPath = aszParentPath + aszChildPath;

            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }

            return strPath;
        }

        /// <summary>   Validate directory name. </summary>
        /// <remarks>   suoow2, 2014-09-14. </remarks>
        /// <param name="strDirectoryName"> Pathname of the string directory. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public static bool ValidateDirectoryName(string strDirectoryName)
        {
            strDirectoryName = strDirectoryName.ToUpper();

            if (strDirectoryName.IndexOf(@"\") != -1)
            {
                return false; // invalid DIR-name.
            }
            if (strDirectoryName.IndexOfAny(new char[] { '/', ':', '*', '?', '"', '<', '>', '|' }) != -1)
            {
                return false; // invalid DIR-name.
            }
            else if (strDirectoryName.Length == 3)
            {
                switch (strDirectoryName)
                {
                    case "CON":
                    case "PRN":
                    case "AUX":
                    case "NUL": return false; // invalid DIR-name.
                }
            }
            else if (strDirectoryName.Length == 4)
            {
                switch (strDirectoryName)
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

        public static string[] GetFileEntries(string aszPath)
        {
            try
            {
                return Directory.GetFileSystemEntries(aszPath);
            }
            catch
            {
                return null;
            }
        }

        // Filter가 적용된 파일 조회
        public static string[] GetFileEntries(string aszPath, string aszFilter)
        {
            // usage : GetFileEntries("blahblah", "*.exe");
            try
            {
                return Directory.GetFileSystemEntries(aszPath, aszFilter);
            }
            catch
            {
                return null;
            }
        }

        public static string[] GetDrives()
        {
            try
            {
                return Directory.GetLogicalDrives(); // c:\ d:\ e:\ ...
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // 아래는 시스템에 종속적인 Path에 대한 처리.
        //////////////////////////////////////////////////////////////////////////
        
        // 모델 그룹 경로
        public static string GetGroupImagePath(string aszModelPath, string aszGroupName)
        {
            return Path.Combine(aszModelPath, aszGroupName);
        }

        // Offline 모델 그룹 경로
        public static string GetOfflineGroupImagePath(string aszModelPath, string aszMachineName, string aszGroupName)
        {
            string szMachinePath = Path.Combine(aszModelPath, aszMachineName);
            return Path.Combine(szMachinePath, aszGroupName);
        }

        // 모델 백업 경로
        public static string GetModelBackupPath(string aszModelPath, string aszGroupName, string aszModelName, string aszBackupInfo = null)
        {
            string szModelPath = GetModelImagePath(aszModelPath, aszGroupName, aszModelName);
            string szBackupTime = (!string.IsNullOrEmpty(aszBackupInfo)) ? aszBackupInfo : DateTime.Now.ToString().Replace(':', '-');
            
            szModelPath = Path.Combine(szModelPath, szBackupTime);
            if (!Directory.Exists(szModelPath))
            {
                Directory.CreateDirectory(szModelPath);
            }
            return szModelPath;
        }

        // 모델 이미지 데이터 저장 경로
        public static string GetModelImagePath(string aszModelPath, string aszGroupName, string aszModelName)
        {
            string szModelImagePath = Path.Combine(GetGroupImagePath(aszModelPath, aszGroupName), aszModelName);
            if (!Directory.Exists(szModelImagePath))
            {
                Directory.CreateDirectory(szModelImagePath);
            }
            return szModelImagePath;
        }

        // Offline 모델 이미지 데이터 저장 경로
        public static string GetOfflineModelImagePath(string aszModelPath, string aszMachineName, string aszGroupName, string aszModelName)
        {
            string szOfflineModelImagePath = Path.Combine(GetOfflineGroupImagePath(aszModelPath, aszMachineName, aszGroupName), aszModelName);
            if (!Directory.Exists(szOfflineModelImagePath))
            {
                Directory.CreateDirectory(szOfflineModelImagePath);
            }
            return szOfflineModelImagePath;
        }

        // 기준영상 저장 경로
        public static string GetBasedImagePath(string aszModelPath, string aszGroupName, string aszModelName, Surface aSurface)
        {
            switch (aSurface)
            {
                case Surface.상부검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelName), ((short)Surface.상부검사) + "-Based.bmp");
                case Surface.하부검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelName), ((short)Surface.하부검사) + "-Based.bmp");
                case Surface.투과검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelName), ((short)Surface.투과검사) + "-Based.bmp");
                default:
                    return string.Empty;
            }
        }

        public static string GetCenterLineDataPath(string aszModelPath, string aszGroupName, string aszModelname, Surface aSurface)
        {
            switch (aSurface)
            {
                case Surface.상부검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelname), @"CenterLine\View" + ((short)Surface.상부검사) + ".txt");
                case Surface.하부검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelname), @"CenterLine\View" + ((short)Surface.하부검사)  + ".txt");
                case Surface.투과검사:
                    return Path.Combine(GetModelImagePath(aszModelPath, aszGroupName, aszModelname), @"CenterLine\View" + ((short)Surface.투과검사) + ".txt");
                default:
                    return string.Empty;
            }
        }

        // Offline 기준영상 저장 경로
        public static string GetOfflineBasedImagePath(string aszModelPath, string aszMachineName, string aszGroupName, string aszModelName, Surface aSurface)
        {
            switch (aSurface)
            {
                case Surface.상부검사:
                    return Path.Combine(GetOfflineModelImagePath(aszModelPath, aszMachineName, aszGroupName, aszModelName), ((short)Surface.상부검사) + "-Based.bmp");
                case Surface.하부검사:
                    return Path.Combine(GetOfflineModelImagePath(aszModelPath, aszMachineName, aszGroupName, aszModelName), ((short)Surface.하부검사) + "-Based.bmp");
                case Surface.투과검사:
                    return Path.Combine(GetOfflineModelImagePath(aszModelPath, aszMachineName, aszGroupName, aszModelName), ((short)Surface.투과검사) + "-Based.bmp");
                default:
                    return string.Empty;
            }
        }

        // 섹션 이미지 저장 경로
        public static string GetSectionImagePath(string aszModelPath, string aszGroupName, string aszModelName, string aszSectionName, Surface aSurface)
        {
            string szImagePath = GetModelImagePath(aszModelPath, aszGroupName, aszModelName);

            switch (aSurface)
            {
                case Surface.상부검사:
                    szImagePath += "/11-";
                    break;
                case Surface.하부검사:
                    szImagePath += "/21-";
                    break;
                case Surface.투과검사:
                    szImagePath += "/31-";
                    break;
                default:
                    szImagePath += "/00-";
                    break;
            }

            return String.Format("{0}{1}.bmp", szImagePath, aszSectionName);
        }

        // Offline 섹션 이미지 저장 경로
        public static string GetOfflineSectionImagePath(string aszModelPath, string aszMachineName, string aszGroupName, string aszModelName, string aszSectionName, Surface aSurface)
        {
            string szImagePath = GetOfflineModelImagePath(aszModelPath, aszMachineName, aszGroupName, aszModelName);

            switch (aSurface)
            {
                case Surface.상부검사:
                    szImagePath += "/11-";
                    break;
                case Surface.하부검사:
                    szImagePath += "/21-";
                    break;
                case Surface.투과검사:
                    szImagePath += "/31-";
                    break;
                default:
                    szImagePath += "/00-";
                    break;
            }

            return String.Format("{0}{1}.bmp", szImagePath, aszSectionName);
        }
    }
}
