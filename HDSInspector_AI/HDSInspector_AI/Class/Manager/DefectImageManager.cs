using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Manager
{
    // 검사 이미지 저장 경로 탐색 및 파일 세트 관리
    public class DefectImageManager
    {
        private const string TopCode    = "9011";
        private const string BottomCode = "9021";
        private const string TransCode  = "9031";

        /*
         * [000001]_9011.png
         * 
         * 1번 : 000001 (번호)
         * 2번 : 9011 (카메라)
         * 3번 : png (확장자)
         */

        private static readonly Regex FileNameRegex = new Regex(@"^\[(\d+)\]_(9011|9021|9031)\.(png|txt)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly object _syncLock = new object();
        private readonly string _rootDirectory;

        public InspectionInfo CurrentInfo { get; private set; }
        public string CurrentSystemDirectory { get; private set; }
        public string LastError {  get; private set; }

        public bool HasCurrentInfo
        {
            get
            {
                lock (_syncLock)
                {
                    return CurrentInfo != null && CurrentInfo.IsValid;
                }
            }
        }

        public DefectImageManager(string rootDirectory = @"E:\ImagePath")
        {
            _rootDirectory = rootDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // Main에서 받은 정보 설정
        public bool SetInfo(InspectionInfo info)
        {
            lock (_syncLock)
            {
                LastError = null;

                if (info == null)
                { LastError = "작업 정보가 Null입니다."; return false; }

                if (!info.IsValid)
                { LastError = "받은 정보가 올바르지 않습니다."; return false; }

                InspectionInfo copiedInfo = new InspectionInfo
                {
                    EquipmentID = info.EquipmentID.Trim(),
                    ProductName = info.ProductName.Trim(),
                    OrderNumber = info.OrderNumber.Trim()
                };

                string systemDir = Path.Combine(_rootDirectory, copiedInfo.EquipmentID, copiedInfo.ProductName, copiedInfo.OrderNumber, "system");
                CurrentInfo = copiedInfo;
                CurrentSystemDirectory = systemDir;

                return true;
            }
        }

        public void ClearJob()
        {
            lock (_syncLock)
            {
                CurrentInfo = null;
                CurrentSystemDirectory = null;
                LastError = null;
            }
        }

        // 경로 확인
        public bool IsCurrentSystemDirectoryReady()
        {
            lock (_syncLock)
            {
                LastError = null;

                if (!ValidateCurrentInfo()) return false;

                if (!Directory.Exists(CurrentSystemDirectory)) { LastError = "system 경로가 존재하지 않습니다."; return false; }

                return true;
            }
        }

        // 현재 System 폴더에서 최신 검사번호 파일셋을 가져옴.
        public bool TryGetLastestFileSet(out DefectImageFileSet fileSet)
        {
            fileSet = null;
            LastError = null;

            if (!ValidateCurrentInfo()) return false;

            return TryGetLatestFileSetInternal(CurrentSystemDirectory, out fileSet);
        }

        // 지정한 검사 번호 파일 세트를 가져옴
        public bool TryGetFileSet(int seqNum, out DefectImageFileSet fileSet)
        {
            lock (_syncLock)
            {
                fileSet = null;
                LastError = null;

                if (!ValidateCurrentInfo()) return false;

                if (seqNum < 0) { LastError = "검사 번호가 잘못되었습니다."; return false; }
                if (!Directory.Exists(CurrentSystemDirectory)) { LastError = "system 경로가 존재하지 않습니다."; return false; }

                List<ParsedImageFile> parsedFiles = ParseFiles(CurrentSystemDirectory);
                List<ParsedImageFile> sequenceFiles = parsedFiles.Where(file => file.SequenceNumber == seqNum).ToList();

                if (sequenceFiles.Count == 0) { LastError = "검사번호의 불량 이미지는 없습니다."; return false; }

                fileSet = CreateFileSet(CurrentSystemDirectory,seqNum, sequenceFiles);

                return fileSet.HasAnyImage;
            }
        }
        public bool TryGetAllFileSets(out List<DefectImageFileSet> fileSets)
        {
            lock (_syncLock)
            {
                fileSets = new List<DefectImageFileSet>();

                LastError = null;

                if (!ValidateCurrentInfo()) { return false; }

                if (!Directory.Exists(CurrentSystemDirectory))
                {
                                        LastError = $"system 경로가 존재하지 않습니다: " + $"{CurrentSystemDirectory}";

                    return false;
                }

                List<ParsedImageFile> parsedFiles = ParseFiles(CurrentSystemDirectory);

                if (parsedFiles.Count == 0)
                {
                    LastError =$"불량 이미지가 없습니다: " + $"{CurrentSystemDirectory}";

                    return false;
                }

                IEnumerable<IGrouping<int, ParsedImageFile>>

                groupedFiles = parsedFiles.GroupBy(file => file.SequenceNumber).OrderBy(group => group.Key);

                foreach (IGrouping<int, ParsedImageFile> group in groupedFiles)
                {

                    DefectImageFileSet fileSet = CreateFileSet(CurrentSystemDirectory, group.Key, group.ToList());

                    if (fileSet.HasAnyImage)
                        fileSets.Add(fileSet);
                }

                return fileSets.Count > 0;
            }
        }

        private bool TryGetLatestFileSetInternal(string systemDirectory, out DefectImageFileSet fileSet)
        {
            fileSet = null;

            if (string.IsNullOrWhiteSpace(systemDirectory)) { LastError = "system 경로가 설정되지 않았습니다."; return false; }

            if (!Directory.Exists(systemDirectory)) { LastError = $"system 경로가 존재하지 않습니다: " + $"{systemDirectory}"; return false; }

            try
            {
                List<ParsedImageFile> parsedFiles = ParseFiles(systemDirectory);

                if (parsedFiles.Count == 0) { LastError = $"불량 이미지가 없습니다: " + $"{systemDirectory}"; return false; }

                int lastestSequence = parsedFiles.Max(file => file.SequenceNumber);

                List<ParsedImageFile> lastestFiles = parsedFiles.Where(file => file.SequenceNumber == lastestSequence).ToList();
                fileSet = CreateFileSet(systemDirectory, lastestSequence, lastestFiles);

                if (!fileSet.HasAnyImage) { LastError = "최신 검사번호에 PNG 이미지가 없습니다."; return false; }

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                LastError = ex.Message; return false;
            }
            catch (Exception ex)
            {
                LastError = ex.Message; return false;
            }
        }

        private bool ValidateCurrentInfo()
        {
            if (CurrentInfo == null) { LastError = "현재 작업 정보가 설정되지 않았습니다."; return false; }
            if (!CurrentInfo.IsValid) { LastError = "현재 작업 정보가 올바르지 않습니다."; return false; }
            if (string.IsNullOrWhiteSpace(CurrentSystemDirectory)) { LastError = "현재 system 경로가 설정되지 않았습니다."; return false; }

            return true;
        }

        private static List<ParsedImageFile> ParseFiles(string systemDirectory)
        {
            List<ParsedImageFile> result = new List<ParsedImageFile>();

            IEnumerable<string> files = Directory.EnumerateFiles(systemDirectory, "*.*", SearchOption.TopDirectoryOnly);

            foreach (string filePath in files)
            {

                string fileName = Path.GetFileName(filePath);
                Match match =FileNameRegex.Match(fileName);

                if (!match.Success) 
                    continue;

                int sequenceNumber;

                if (!int.TryParse(match.Groups[1].Value, out sequenceNumber)) 
                    continue;

                

                FileInfo fileInfo = new FileInfo(filePath);

                result.Add(new ParsedImageFile 
                    {
                        SequenceNumber = sequenceNumber,
                        CameraCode = match.Groups[2].Value,
                        Extension = match.Groups[3].Value.ToLowerInvariant(),
                        FilePath = filePath,
                        LastWriteTime = fileInfo.LastWriteTime
                    }
                );
            }

            return result;
        }

        private static DefectImageFileSet CreateFileSet(string systemDirectory, int sequenceNumber, IList<ParsedImageFile> files)
        {
            DefectImageFileSet result = new DefectImageFileSet
                {
                SequenceNumber = sequenceNumber,
                SystemDirectory = systemDirectory

                };

            foreach (ParsedImageFile file in files)
            {
                bool isPng = string.Equals(file.Extension, "png", StringComparison.OrdinalIgnoreCase);
                bool isTxt = string.Equals(file.Extension, "txt", StringComparison.OrdinalIgnoreCase);

                switch(file.CameraCode)
                {
                    case TopCode:
                        if (isPng) { result.TopImagePath = file.FilePath; }
                        else if (isTxt) { result.TopTextPath = file.FilePath; }
                        break;

                    case BottomCode:
                        if (isPng) { result.BottomImagePath = file.FilePath; }
                        else if (isTxt) { result.BottomTextPath = file.FilePath; }
                        break;

                    case TransCode:
                        if (isPng) { result.TransImagePath = file.FilePath; }
                        else if (isTxt) { result.TransTextPath = file.FilePath; }
                        break;

                }

                if (file.LastWriteTime > result.LastWriteTime)
                    result.LastWriteTime = file.LastWriteTime;

            }

            return result;
        }

        private sealed class ParsedImageFile
        {

            public int SequenceNumber{ get; set; }

            public string CameraCode{ get; set; }

            public string Extension{ get; set; }

            public string FilePath{ get; set; }

            public DateTime LastWriteTime { get; set; }
        }
    }
}
