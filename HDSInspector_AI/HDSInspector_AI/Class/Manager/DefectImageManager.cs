using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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

        // Main S/W에서 GrabDone이던 Inspection Done이던 뭐던 이벤트 받아오면 바로 발생시키자.
        public event Action<DefectImageFileSet> InspectionImageReady;

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

        public void ClearInfo()
        {
            lock (_syncLock)
            {
                CurrentInfo = null;
                CurrentSystemDirectory = null;
                LastError = null;
            }
        }

        /// <summary>
        /// Main S/W의 검사 완료 신호를 처리
        /// Main S/W에서 검사 완료 신호를 받으면 이 함수를 호출하여 최신 검사번호의 불량 이미지 세트를 가져오고 InspectionImageReady 이벤트를 발생시킴.
        /// 일단 이게 호출되면 PNG / TXT 저장이 모두 완료되었다고 가정함.
        /// 그래서 이거 메인에서 플래그 받을땐 저장 끝난 시점에 받아야함
        /// </summary>
        /// <param name="sequenceNumber"></param>
        /// <returns></returns>
        public bool ProcessInspectionComplete(int sequenceNumber)
        {
            DefectImageFileSet fileSet;

            lock(_syncLock)
            {
                LastError = null;

                if (!ValidateCurrentInfo()) return false;
                if(sequenceNumber < 0)
                {
                    LastError = $"잘못된 검사 번호입니다. : {sequenceNumber}";

                    return false;
                }

                if(!Directory.Exists(CurrentSystemDirectory))
                {
                    LastError = $"System 경로가 존재하지 않습니다. : {CurrentSystemDirectory}";

                    return false;
                }

                fileSet = CreateSequenceFileSet(sequenceNumber);
            }

            // Event는 Lock 밖에서 호출해야함
            // 혹시나 UI 작업 수행가능성이 있기에 lock 안에서 호출하면 씹히거나 버벅거리거나 할지도..

            try
            {
                InspectionImageReady?.Invoke(fileSet);
            }
            catch (Exception ex)
            {
                LastError = $"InspectionImageReady Event 처리 실패함 : {ex.Message}";

                return false;
            }

            return true;
        }

        private DefectImageFileSet CreateSequenceFileSet(int sequenceNumber)
        {
            List<ParsedImageFile> parsedFiles = ParseFiles(CurrentSystemDirectory);
            List<ParsedImageFile> sequenceFiles = parsedFiles.Where(x => x.SequenceNumber == sequenceNumber).ToList();

            // png,txt가 다 없어도 FileSet은 정상생성.
            return CreateFileSet(CurrentSystemDirectory, sequenceNumber, sequenceFiles);
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

                fileSet = CreateFileSet(CurrentSystemDirectory, seqNum, sequenceFiles);

                return fileSet.HasAnyImage;
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
                Match match = FileNameRegex.Match(fileName);

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

                switch (file.CameraCode)
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
