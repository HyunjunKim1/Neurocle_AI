/* ============================================================= Memo =============================================================
 * 
 * 1. WPF에선 기본적으로 DirectX 9 버전을 기반으로 GPU Rendering이 가능함
 * 2. DirectX 9 버전은 GPU Rendering을 하더라도 오래된 버전이기에, 현대적 그래픽 기능과 최적화가 불가능함
 * 3. 그래서 DirectX 11 버전을 기반으로 그래픽 렌더링 후, GPU 내부 메모리 공유를 통해 WPF에서 DirectX 9로 받아올 수 있도록 공유 메모리 설정함
 * 
 * ================================================================================================================================
 */
using Common;
using ControlzEx.Behaviors;
using nrt;
using OpenCvSharp;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static OpenCvSharp.LineIterator;
using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11Texture2D = SharpDX.Direct3D11.Texture2D;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Devices
{
    /// <summary>   Use GPU DirectX Rendering Functions </summary>
    /// <remarks>   hjkim, 2026-06-09.                  </remarks>
    public class devImageRendering
    {
        #region P/Invoke
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        #endregion

        public D3DImage ImageSource { get; private set; }

        private D3D11Device     _d3d11Device;       // DirectX 11

        private Direct3DEx      _d3d9;              // DirectX 9
        private DeviceEx        _d3d9Device;        // DirectX 9 Device

        private D3D11Texture2D  _sharedTexture11;   // DirectX 11 Texture
        private Texture         _shareTexture9;     // DirectX 9 Texture



        public devImageRendering()
        {
            ImageSource = new D3DImage();

            InitializeD3D11();
            InitializeD3D9();
        }
       
        /// <summary>
        /// Initialize DirectX 9
        /// </summary>
        private void InitializeD3D9()
        {
            _d3d9 = new Direct3DEx();

            var pp = new SharpDX.Direct3D9.PresentParameters
            {
                Windowed = true,
                SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = PresentInterval.Default
            };

            _d3d9Device = new DeviceEx(
                _d3d9,
                0,
                DeviceType.Hardware,
                IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing |
                CreateFlags.Multithreaded |
                CreateFlags.FpuPreserve,
                pp);
        }

        /// <summary>
        /// Initialize DirectX 11
        /// </summary>
        private void InitializeD3D11()
        {
            _d3d11Device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
        }

        private int _texWidth;
        private int _texHeight;

        public void Load(Mat mat)
        {
            if(_sharedTexture11 == null || _texWidth != mat.Width || _texHeight != mat.Height)
                CreateSharedTexture(mat.Width, mat.Height);

            Rendering(mat);
        }

        /// <summary>
        /// Mat → DirectX11 → DirectX9 GPU Rendering
        /// </summary>
        /// <param name="mat"></param>
        private void Rendering(Mat mat)
        {
            using (Mat cvtMat = new Mat())
            {
                if (mat.Type() != MatType.CV_8UC4)
                {
                    Cv2.CvtColor(mat, cvtMat, ColorConversionCodes.BGR2BGRA);
                }

                _d3d11Device.ImmediateContext.UpdateSubresource(
                    new DataBox(
                        cvtMat.Data,
                        (int)cvtMat.Step(),
                        0),
                    _sharedTexture11);

                _d3d11Device.ImmediateContext.Flush();
                
                ImageSource.Lock();
                ImageSource.AddDirtyRect(new Int32Rect(0, 0, cvtMat.Width, cvtMat.Height));
                ImageSource.Unlock();
            }

            mat.Release();
            mat.Dispose();
            mat = null;
        }

        /// <summary>
        /// DirectX9 ↔ DirectX11's share GPU memory
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void CreateSharedTexture(int width, int height)
        {
            _texHeight = height;
            _texWidth = width;

            var desc = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = width,
                Height = height,

                MipLevels = 1,
                ArraySize = 1,

                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,

                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared // DirectX11과 DirectX9 간, 메모리 공유를 위한 Flags
            };

            _sharedTexture11 = new D3D11Texture2D(_d3d11Device, desc);
            
            using (var resource = _sharedTexture11.QueryInterface<SharpDX.DXGI.Resource>())
            {
                IntPtr handle = resource.SharedHandle;

                _shareTexture9 = new Texture(
                _d3d9Device,
                width,
                height,
                1,
                SharpDX.Direct3D9.Usage.RenderTarget,
                SharpDX.Direct3D9.Format.A8R8G8B8,
                Pool.Default,
                ref handle);

                SetBackBuffer();
            }
        }

        private void SetBackBuffer()
        {
            using (SharpDX.Direct3D9.Surface surface = _shareTexture9.GetSurfaceLevel(0))
            {
                ImageSource.Lock();

                ImageSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);

                ImageSource.Unlock();
            }
        }
    }
}
