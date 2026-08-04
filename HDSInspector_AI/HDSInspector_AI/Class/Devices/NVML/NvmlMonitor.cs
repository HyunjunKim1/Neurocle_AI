using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HDSInspector_AI.Class.Devices.NVML
{

    /// <summary>   NVIDIA GPU Status monitor.   </summary>
    /// <remarks>   hjkim, 2026-08-04.           </remarks>
    public sealed class NvmlMonitor : IDisposable
    {
        private readonly object _syncLock = new object();
        private IntPtr _deviceHandle = IntPtr.Zero;
        
        private bool _isInitialized;
        private bool _isDisposed;
        private uint _gpuIndex;

        public bool IsInitialized
        {
            get
            {
                lock (_syncLock) return _isInitialized;
            }
        }

        public uint GpuIndex
        {
            get
            {
                return _gpuIndex;
            }
        }

        public uint DeviceCount
        {
            get; private set;
        }

        public string LastError
        {
            get; private set;
        }

        /// <summary>
        /// NVML 및 지정 GPU 이니셜하는거, 기본값은 파이토치 GPU 확인이랑 마찬가지로 GPU 0번임. 
        /// </summary>
        /// <param name="gpuIndex"></param>
        /// <returns></returns>
        public bool Initialize(uint gpuIndex = 0)
        {
            lock (_syncLock)
            {
                ThrowIfDisposed();

                if (_isInitialized) return true;

                LastError = null;

                string loadError;

                if(!NvmlNative.TryLoadLibrary(out loadError))
                {
                    LastError = loadError;

                    return false;
                }

                NvmlNative.NVML_RETURNS result = NvmlNative.Init();

                if (result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlInit_v2", result);

                    NvmlNative.UnloadLibrary();

                    return false;
                }

                result = NvmlNative.DeviceGetCount(out uint deviceCount);

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlDeviceGetCount_v2", result);

                    ShutdownInternal();

                    return false;
                }

                DeviceCount = deviceCount;

                if (DeviceCount == 0)
                {
                    LastError = "접근 가능한 NVIDIA GPU가 없습니다.";

                    ShutdownInternal();

                    return false;
                }

                if(gpuIndex >= DeviceCount)
                {
                    LastError = "GPU Index가 올바르지 않습니다.";

                    ShutdownInternal();

                    return false;
                }

                result = NvmlNative.DeviceGetHandleByIndex(gpuIndex, out _deviceHandle);

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlDeviceGetHandleByIndex_v2", result);

                    ShutdownInternal();

                    return false;
                }

                _gpuIndex = gpuIndex;
                _isInitialized = true;

                return true;
            }
        }

        /// <summary>
        /// 현재 GPU 연산 사용률 조회, 0~100 사이로 반환함
        /// </summary>
        /// <param name="gpuUsagePercent"></param>
        /// <returns></returns>
        public bool TryGetGpuUtilization(out uint gpuUsagePercent)
        {
            lock(_syncLock)
            {
                gpuUsagePercent = 0;
                if(!ValidateInitialized())
                    return false;

                NvmlNative.NVML_RETURNS result = NvmlNative.DeviceGetUtilizationRates(_deviceHandle, out NvmlNative.NvmlUtilization utilization);

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlDeviceGetUtilizationRates", result);

                    return false;
                }

                gpuUsagePercent = Math.Min(utilization.Gpu, 100U);

                LastError = null;

                return true;
            }
        }

        /// <summary>
        /// GPU 연산 사용률이랑 메모리 컨트롤러 사용률 조회
        /// </summary>
        /// <param name="gpuUsagePercent"></param>
        /// <param name="memoryControllerUsagePercent"></param>
        /// <returns></returns>
        public bool TryGetUtilization(out uint gpuUsagePercent, out uint memoryControllerUsagePercent)
        {
            lock (_syncLock)
            {
                gpuUsagePercent = 0;
                memoryControllerUsagePercent = 0;

                if (!ValidateInitialized())
                    return false;

                NvmlNative.NVML_RETURNS result = NvmlNative.DeviceGetUtilizationRates(_deviceHandle, out NvmlNative.NvmlUtilization utilization);

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlDeviceGetUtilizationRates", result);

                    return false;
                }

                gpuUsagePercent = Math.Min(utilization.Gpu, 100U);
                memoryControllerUsagePercent = Math.Min(utilization.Memory, 100U);

                LastError = null;

                return false;
            }

        }

        /// <summary>
        /// GPU VRAM 사용량 조회
        /// </summary>
        /// <param name="totalBytes"></param>
        /// <param name="usedBytes"></param>
        /// <param name="freeBytes"></param>
        /// <returns></returns>
        public bool TryGetMemoryInfo(out ulong totalBytes, out ulong usedBytes, out ulong freeBytes)
        {
            lock (_syncLock)
            {
                totalBytes = 0;
                usedBytes = 0;
                freeBytes = 0;

                if (!ValidateInitialized())
                    return false;

                NvmlNative.NVML_RETURNS result = NvmlNative.DeviceGetMemoryInfo(_deviceHandle, out NvmlNative.NvmlMemory memory);

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                {
                    LastError = CreateErrorMessage("nvmlDeviceGetMemoryInfo", result);

                    return false;
                }

                totalBytes = memory.Total;
                usedBytes = memory.Used;
                freeBytes = memory.Free;

                LastError = null;

                return true;
            }
        }

        public void Shutdown()
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                    return;

                ShutdownInternal();
            }
        }

        private bool ValidateInitialized()
        {
            if(_isDisposed)
            {
                LastError = "NvmlMonitor가 이미 해제되어있습니다.";

                return false;
            }

            if(!_isInitialized || _deviceHandle == IntPtr.Zero)
            {
                LastError = "NVML이 초기화 되지 않았습니다.";

                return false;
            }

            return true;
        }

        private void ShutdownInternal()
        {
            if(_isInitialized)
            {
                NvmlNative.NVML_RETURNS result = NvmlNative.Shutdown();

                if(result != NvmlNative.NVML_RETURNS.SUCCESS)
                    LastError = CreateErrorMessage("nvmlShutdown", result);
                else
                {
                    try
                    {
                        // Init은 성공했는데 이후에 실패한 경우에도 shutdown 필요할지도모름
                        NvmlNative.Shutdown();
                    }

                    catch(Exception e) 
                    {
                        MessageBox.Show($"{e.Message}");
                    }
                }

                _deviceHandle = IntPtr.Zero;
                _isInitialized = false;
                DeviceCount = 0;

                NvmlNative.UnloadLibrary();
            }    
        }

        private static string CreateErrorMessage(string functionName, NvmlNative.NVML_RETURNS result)
        {
            return $@"{functionName} 실패 : " + $@"{NvmlNative.GetErrorMessage(result)}";
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(NvmlMonitor));
            }
        }

        public void Dispose()
        {
            lock(_syncLock)
            {
                if (_isDisposed) return;

                ShutdownInternal();
                _isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
