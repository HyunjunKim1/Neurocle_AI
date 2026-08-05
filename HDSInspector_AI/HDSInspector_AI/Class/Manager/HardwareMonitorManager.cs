using HDSInspector_AI.Class.Devices.NVML;
using HDSInspector_AI.Class.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HDSInspector_AI.Class.Manager
{
    /// <summary>   CPU, GPU, 저장 드라이브 상태 관리자 </summary>
    /// <remarks>   hjkim, 2026-08-04.                  </remarks>
    public sealed class HardwareMonitorManager : IDisposable
    {
        private readonly object _syncLock = new object();
        private readonly NvmlMonitor _nvmlMonitor;
        private PerformanceCounter _cpuCounter;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isGpuAvailable;
        private readonly string _driveName;
        private readonly uint _gpuIndex;

        public bool IsInitialized
        {
            get
            {
                lock (_syncLock) return _isInitialized;
            }
        }

        public bool IsGpuAvailable
        {
            get
            {
                lock (_syncLock) return _isGpuAvailable;
            }
        }

        public string LastError { get; private set; }

        public HardwareMonitorManager(string driveName = "E:\\", uint gpuIndex = 0)
        {
            _driveName = NormalizeDriveName(driveName);
            _gpuIndex = gpuIndex;
            _nvmlMonitor = new NvmlMonitor();
        }

        // GPU PerformanceCenter와 NVML을 초기화함
        public bool Initialize()
        {
            lock (_syncLock)
            {
                ThrowIfDisposed();

                if (_isInitialized) return true;

                LastError = null;

                try
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);

                    // PerfomanceCounter의 첫번째 값은 대부분 0임. 확인해보자.
                    _cpuCounter.NextValue();
                }
                catch (Exception ex)
                {
                    LastError = $@"CPU Performance 초기화 실패 : {ex.Message}";

                    DisposeCpuCounter();

                    return false;
                }

                // GPU 초기화 상태는 전체 모니터링을 실패로 처리하지않음. CPU와 디스크는 계속 표기하도록 하자
                _isGpuAvailable = _nvmlMonitor.Initialize(_gpuIndex);

                if (!_isGpuAvailable)
                    LastError = "GPU NVML 초기화 실패 : " + _nvmlMonitor.LastError;

                _isInitialized = true;

                return true;
            }
        }

        public HardwareStatus ReadStatus()
        {
            lock (_syncLock)
            {
                ThrowIfDisposed();

                HardwareStatus status = new HardwareStatus();

                if (!_isInitialized)
                {
                    status.ErrorMessage = "Hardware Manager가 초기화 안되었습니다.";

                    return status;
                }

                string errorMessage = null;

                ReadCpuStatus(status, ref errorMessage);
                ReadGpuStatus(status, ref errorMessage);
                ReadDriveStatus(status, ref errorMessage);

                status.ErrorMessage = errorMessage;

                return status;
            }
        }

        // UI 쓰레드가 측정작업으로 지연안되도록 ThreadPool에서 HW Status를 조회하는거로 만듦
        public Task<HardwareStatus> ReadStatusAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return ReadStatus();
            }, cancellationToken);
        }

        private void ReadCpuStatus(HardwareStatus status, ref string errorMessage)
        {
            try
            {
                float cpuUsage = _cpuCounter.NextValue();

                status.CpuUsagePercent = Clamp(cpuUsage, 0.0f, 100.0f);
            }
            catch (Exception ex)
            {
                status.CpuUsagePercent = 0.0f;

                AppendError(ref errorMessage, $"CPU 조회 실패 : {ex.Message}");
            }
        }

        private void ReadGpuStatus(HardwareStatus status, ref string errorMessage)
        {
            status.IsGPUAvailable = _isGpuAvailable;

            if (!_isGpuAvailable)
            {
                AppendError(ref errorMessage, $"GPU 사용 불가 : {_nvmlMonitor.LastError}");

                return;
            }

            bool utilizationSuccess = _nvmlMonitor.TryGetUtilization(out uint gpuUsage, out uint memoryControllerUsage);

            if (utilizationSuccess)
            {
                status.GpuMemoryUsagePercent = gpuUsage;
                status.GpuMemoryControllerUsagePercent = memoryControllerUsage;
            }
            else
                AppendError(ref errorMessage, $"GPU 사용률 조회 실패 : {_nvmlMonitor.LastError}");

            bool memorySuccess = _nvmlMonitor.TryGetMemoryInfo(out ulong totalBytes, out ulong usedBytes, out ulong freeBytes);

            if (memorySuccess)
            {
                status.GpuMemoryTotalBytes = totalBytes;
                status.GpuMemoryUsedBytes = usedBytes;
                status.gpuMemoryFreeBytes = freeBytes;
            }
            else
                AppendError(ref errorMessage, $"GPU 메모리 조회 실패 : {_nvmlMonitor.LastError}");
        }

        private void ReadDriveStatus(HardwareStatus status, ref string errorMessage)
        {
            try
            {
                DriveInfo drive = new DriveInfo(_driveName);

                if (!drive.IsReady)
                {
                    status.isDriveReady = false;

                    AppendError(ref errorMessage, $"{_driveName} 드라이브가 준비되지 않았습니다.");

                    return;
                }

                status.isDriveReady = true;
                status.DriveTotalBytes = (ulong)drive.TotalSize;
                status.DriveFreeBytes = (ulong)drive.AvailableFreeSpace;
                status.DriveUsedBytes = (ulong)drive.TotalSize - (ulong)drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                status.isDriveReady = false;

                AppendError(ref errorMessage, $"{_driveName} 드라이브 조회 실패 : {ex.Message}");
            }
        }

        private static void AppendError(ref string currentMessage, string newMessage)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                return;

            if (string.IsNullOrWhiteSpace(currentMessage))
                return;

            currentMessage += Environment.NewLine + newMessage;
        }

        private static string NormalizeDriveName(string driveName)
        {
            if (string.IsNullOrWhiteSpace(driveName))
                return "E:\\";

            string normalized = driveName.Trim();

            if (normalized.Length == 1)
                normalized += ":\\";
            else if (normalized.Length == 2 && normalized[1] == ':')
                normalized += "\\";

            return normalized;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;

            return value;
        }

        private void DisposeCpuCounter()
        {
            if (_cpuCounter == null) return;

            try
            {
                _cpuCounter.Dispose();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
            _cpuCounter = null;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(HardwareMonitorManager));
            }
        }

        public void Dispose()
        {
            lock(_syncLock)
            {
                if (_isDisposed) return;

                DisposeCpuCounter();

                _nvmlMonitor?.Dispose();

                _isGpuAvailable = false;
                _isInitialized = false;
                _isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}

namespace HDSInspector_AI.Class.Manager
{
    public class HardwareStatus
    {
        // CPU 전체 사용률
        public float CpuUsagePercent { get; set; }

        // GPU 연산 사용률
        public uint? GpuMemoryControllerUsagePercent { get; set; }

        // GPU 전체 VRAM 용량
        public ulong GpuMemoryTotalBytes { get; set; }

        // GPU 사용중인 VRAM
        public ulong GpuMemoryUsedBytes { get; set; }

        // GPU 남은 BRAM
        public ulong gpuMemoryFreeBytes { get; set; }

        // E드라이브로 쓸거같은데, 드라이브 전체 용량
        public ulong DriveTotalBytes { get; set; }

        // 드라이브 사용 용량
        public ulong DriveUsedBytes { get; set; }

        // 드라이브 남은 용량
        public ulong DriveFreeBytes { get; set; }

        // 드라이브 사용률
        public double DriveUsagePercent
        {
            get
            {
                if (DriveTotalBytes <= 0)
                    return 0.0;

                return DriveUsedBytes * 100.0 / DriveTotalBytes;
            }
        }

        // GPU VRAM 사용률
        public double GpuMemoryUsagePercent
        {
            get
            {
                if (GpuMemoryTotalBytes == 0)
                    return 0.0;

                return GpuMemoryUsedBytes * 100.0 / GpuMemoryTotalBytes;
            }
        set { }
        }

        // 드라이브 접근 가능 여부
        public bool isDriveReady { get; set; }

        // NVML GPU 조회 가능 여부
        public bool IsGPUAvailable { get; set; }

        // HW 조회 오류 메세지
        public string ErrorMessage { get; set; }
    }
}

