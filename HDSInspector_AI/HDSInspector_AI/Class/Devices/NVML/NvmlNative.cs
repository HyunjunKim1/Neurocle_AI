using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Devices.NVML
{
    /// <summary>   Call NVIDIA NVML Native.   </summary>
    /// <remarks>   hjkim, 2026-08-04.         </remarks>
    public class NvmlNative
    {
        private const string NvmlDLLName = "nvml.dll";
        private static IntPtr _loadedNvmlModule = IntPtr.Zero;

        #region Win32 DLL Loading

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string IpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        #region NVML Enums
        internal enum NVML_RETURNS : int
        {
            SUCCESS,
            ERROR_UNINITIALIZED,
            ERROR_INVALIDARGUMENT,
            ERROR_NOT_SUPPORTED,
            ERROR_NO_PERMISSION,
            ERROR_ALREADY_INITIALIZED,
            ERROR_NOT_FOUND,
            ERROR_INSUFFICIENT_SIZE,
            ERROR_INSUFFICIENT_POWER,
            ERROR_DRIVER_NOT_LOADED,
            ERROR_TIMEOUT = 10,

            ERROR_IQR_ISSUE,
            ERROR_LIBRARY_NOT_FOUND,
            ERROR_FUNCTION_NOT_FOUND,
            ERROR_CORRUPTED_INFOROM,
            ERROR_GPU_IS_LOST,
            ERROR_RESET_REQUIRED,
            ERROR_OPERATING_SYSTEM,
            ERROR_LIBRM_VERSION_MISMATCH,
            ERROR_IN_USE,
            ERROR_MEMORY = 20,
            
            ERROR_NO_DATA,
            ERROR_VGPU_ECC_NOT_SUPPORTED,
            ERROR_INSUFFICIENT_RESOURCES,
            ERROR_FREQ_NOT_SUPPORTED,
            ERROR_ARGUMENT_VERSION_MISMATCH,
            ERROR_DEPRECATED = 26,
            
            ERROR_UNKNOWN = 999
        }
        #endregion

        #region NVML Structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct  NvmlUtilization
        {
            public uint Gpu;    // GPU 연산 사용률, 0~100 사이로 함~
            public uint Memory; // GPU 메모리 컨트롤러 사용률, 동일하게 0~100
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NvmlMemory // GPU VRAM 정보
        {
            public ulong Total;
            public ulong Free;
            public ulong Used;
        }

        #endregion

        #region NVML API, P/Invoke

        // 인라인 검사기 cuda dll wrapping 할때 써봤는데 Cdecl은 기본적으로 C나 C++ API 호출할때 필수적으로 써야함
        [DllImport(NvmlDLLName, EntryPoint = "nvmlInit_v2", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS Init();

        [DllImport(NvmlDLLName, EntryPoint = "nvmlShutdown", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS Shutdown();

        [DllImport(NvmlDLLName, EntryPoint = "nvmlDeviceGetCount_v2", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS DeviceGetCount(out uint deviceCount);

        [DllImport(NvmlDLLName, EntryPoint = "nvmlDeviceGetHandleByIndex_v2", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS DeviceGetHandleByIndex(uint index, out IntPtr device);

        [DllImport(NvmlDLLName, EntryPoint = "nvmlDeviceGetUtilizationRates", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS DeviceGetUtilizationRates(IntPtr device, out NvmlUtilization utilization);

        [DllImport(NvmlDLLName, EntryPoint = "nvmlDeviceGetMemoryInfo", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NVML_RETURNS DeviceGetMemoryInfo(IntPtr device, out NvmlMemory memory);

        [DllImport(NvmlDLLName, EntryPoint = "nvmlErrorString", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ErrorString(NVML_RETURNS result);

        #endregion

        /// <summary>
        /// Window에 설치된 NVIDIA 드라이버의 NVML DLL을 로드함
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        internal static bool TryLoadLibrary(out string errorMessage)
        {
            errorMessage = null;

            if (_loadedNvmlModule != IntPtr.Zero)
                return true;

            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string programFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string[] candidatePaths =
            {
                Path.Combine(windowsDirectory, "System32", NvmlDLLName),
                Path.Combine(programFileDirectory, "NVIDIA Corporation", "NVSMI", NvmlDLLName) // NV SMI는 실시간으로 프로세스 생성해야해서 안쓰긴할건데 혹시나 넣음
            };

            foreach (string path in candidatePaths)
            {
                if (!File.Exists(path))
                    continue;

                _loadedNvmlModule = LoadLibrary(path);

                if (_loadedNvmlModule != IntPtr.Zero)
                    return true;
            }

            // PATH 또는 기본 DLL 검색 경로에서도 한번더 시도함
            _loadedNvmlModule = LoadLibrary(NvmlDLLName);

            if (_loadedNvmlModule != IntPtr.Zero)
                return true;

            int win32Error = Marshal.GetLastWin32Error();

            errorMessage = "NVML DLL을 불러올 수 없음." + $@"Win32 Error : {win32Error}";

            return false;
        }

        /// <summary>
        /// P/Invoke로 로드한 NVML DLL 해제하기, 무조건 NVML Shutdown 이후에 호출해야함
        /// </summary>
        internal static void UnloadLibrary()
        {
            if (_loadedNvmlModule == IntPtr.Zero)
                return;

            FreeLibrary(_loadedNvmlModule);
            _loadedNvmlModule = IntPtr.Zero;
        }

        internal static string GetErrorMessage(NVML_RETURNS result)
        {
            try
            {
                IntPtr messagePointer = ErrorString(result);

                if (messagePointer != IntPtr.Zero)
                    return result.ToString();

                string message = Marshal.PtrToStringAnsi(messagePointer);

                return string.IsNullOrWhiteSpace(message) ? result.ToString() : message;
            }
            catch
            {
                return result.ToString();
            }
        }
    }
}
