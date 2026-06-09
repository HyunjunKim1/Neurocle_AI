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
using static OpenCvSharp.LineIterator;
using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11Texture2D = SharpDX.Direct3D11.Texture2D;

namespace HDSInspector_AI.Class.Devices
{
    public class devImageRendering
    {
        #region P/Invoke
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        #endregion

        public D3DImage ImageSource { get; private set; }

        private D3D11Device     _d3d11Device;

        private Direct3DEx      _d3d9;
        private DeviceEx        _d3d9Device;

        private D3D11Texture2D  _sharedTexture11;
        private Texture         _shareTexture9;



        public devImageRendering()
        {
            ImageSource = new D3DImage();

            InitializeD3D11();
            InitializeD3D9();

            //TestFillRed();
        }

        private void TestFillRed()
        {
            int width = 300;
            int height = 300;

            byte[] pixel = new byte[width * height * 4];

            for(int i = 0; i < pixel.Length; i+=4)
            {
                pixel[i + 0] = 0;
                pixel[i + 1] = 0;
                pixel[i + 2] = 255;
                pixel[i + 3] = 255;
            }
            _d3d11Device.ImmediateContext.UpdateSubresource(pixel, _sharedTexture11);
            _d3d11Device.ImmediateContext.Flush();
            ImageSource.Lock();
            ImageSource.AddDirtyRect(new Int32Rect(0, 0, width, height));
            ImageSource.Unlock();
        }

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
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared
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
