using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Windows.Media;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Threading;

namespace ModelEditor
{
    public class Engine
    {
        private Panel _bitmapContainer;
        private Panel _intersectionBitmapContainer;
        private BitmapBuffer _bitmapBuffer;
        private BitmapBuffer _intersectionBitmapBuffer0;
        private BitmapBuffer _intersectionBitmapBuffer1;
        private WriteableBitmap _writableBitmap;
        private WriteableBitmap _intersectionWritableBitmap0;
        private WriteableBitmap _intersectionWritableBitmap1;
        private readonly float _maxFPS = 30;

        public InputManager Input { get; private set; }
        public Scene Scene { get; private set; }
        public Renderer Renderer { get; private set; }

        private Stopwatch _frameStopWatch = new Stopwatch();
        private double _deltaTime;

        public Engine(Panel bitmapContainer, Panel intersectionBitmapContainer)
        {
            _bitmapContainer = bitmapContainer;
            _intersectionBitmapContainer = intersectionBitmapContainer;
            InitBitmap();
            InitBitmapIntersectionBitmap();

            Scene = new Scene();
            Renderer = new Renderer(_bitmapBuffer, _intersectionBitmapBuffer0, _intersectionBitmapBuffer1, Scene);
            Scene.Init(new RayCaster(Renderer.GetRenderAccessor()));
            Input = new InputManager(_bitmapContainer, _bitmapBuffer, Scene, new RayCaster(Renderer.GetRenderAccessor()));

            var trans = Matrix4x4.CreateTranslation(1, 2, 3);
            var ly = Matrix4x4.CreateRotationY((float)-Math.PI / 4);
            var lz = Matrix4x4.CreateRotationZ((float)Math.PI / 4);
            var c = Matrix4x4.CreateRotationX(2 * (float)Math.PI / 3);
            var rz = Matrix4x4.CreateRotationZ((float)-Math.PI / 4);
            var ry = Matrix4x4.CreateRotationY((float)Math.PI / 4);

            var q1 = lz * ly;
            var q2 = c * q1;
            var q3 = rz * q2;
            var q4 = ry * q3;
        }

        private void InitBitmapIntersectionBitmap()
        {
            _intersectionBitmapBuffer0 = new BitmapBuffer(200, 200);
            _intersectionWritableBitmap0 = new WriteableBitmap(200,200, 96, 96, PixelFormats.Bgr32, null);
            var img0 = new System.Windows.Controls.Image();
            img0.Source = _intersectionWritableBitmap0;
            _intersectionBitmapContainer.Children.Insert(0,(img0));


            _intersectionBitmapBuffer1 = new BitmapBuffer(200, 200);
            _intersectionWritableBitmap1 = new WriteableBitmap(200, 200, 96, 96, PixelFormats.Bgr32, null);
            var img1 = new System.Windows.Controls.Image();
            img1.Source = _intersectionWritableBitmap1;
            _intersectionBitmapContainer.Children.Insert(1, (img1));
        }

        private void InitBitmap()
        {
            _bitmapBuffer = new BitmapBuffer((int)_bitmapContainer.ActualWidth, (int)_bitmapContainer.ActualHeight);

            _writableBitmap = new WriteableBitmap((int)_bitmapContainer.ActualWidth, (int)_bitmapContainer.ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            var img = new System.Windows.Controls.Image();
            img.Source = _writableBitmap;
            _bitmapContainer.Children.Add(img);
        }

        public async void Run()
        {
            while (true)
            {
                _deltaTime = _frameStopWatch.Elapsed.TotalMilliseconds;
                _frameStopWatch.Restart();

                var wait = Task.Delay(Convert.ToInt32(1000 / _maxFPS));


                Input.Update(_deltaTime);
                var rendering = Task.Run(() =>
                 {
                     try
                     {
                         Renderer.RenderFrame();
                     }
                     catch (Exception e)
                     {
                     }
                 });
                _writableBitmap.FromByteArray(_bitmapBuffer.Source);
                _intersectionWritableBitmap0.FromByteArray(_intersectionBitmapBuffer0.Source);
                _intersectionWritableBitmap1.FromByteArray(_intersectionBitmapBuffer1.Source);

                await rendering;
                await wait;

            }
        }


    }
}
