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

namespace ModelEditor
{
    public class Engine
    {
        private Panel _bitmapConatiner;
        private WriteableBitmap _writableBitmap;
        private readonly float _maxFPS = 30;

        public InputManager Input { get; private set; }
        public Scene Scene { get; private set; }
        public Renderer Renderer { get; private set; }

        private Stopwatch _frameStopWatch = new Stopwatch();
        private double _deltaTime;

        public Engine(Panel bitmapConatiner)
        {
            _bitmapConatiner = bitmapConatiner;
            InitBitmap();

            Scene = new Scene();
            Renderer = new Renderer(_writableBitmap, Scene);
            Scene.RayCaster = new RayCaster(Renderer.GetRenderAccessor());
            Input = new InputManager(_bitmapConatiner,_writableBitmap, Scene, new RayCaster(Renderer.GetRenderAccessor()));

            var trans = Matrix4x4.CreateTranslation(1, 2, 3);
            var ly = Matrix4x4.CreateRotationY((float)-Math.PI / 4);
            var lz = Matrix4x4.CreateRotationZ((float)Math.PI / 4);
            var c = Matrix4x4.CreateRotationX(2*(float)Math.PI / 3);
            var rz = Matrix4x4.CreateRotationZ((float)-Math.PI / 4);
            var ry = Matrix4x4.CreateRotationY((float)Math.PI / 4);

            var q1 = lz * ly;
            var q2 = c * q1;
            var q3 = rz * q2;
            var q4 = ry * q3;

        }

        private void InitBitmap()
        {
            _writableBitmap = new WriteableBitmap((int)_bitmapConatiner.ActualWidth, (int)_bitmapConatiner.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
            var img = new System.Windows.Controls.Image();
            img.Source = _writableBitmap;
            _bitmapConatiner.Children.Add(img);
        }

        public async void Run()
        {
            while (true)
            {
                _deltaTime = _frameStopWatch.Elapsed.TotalMilliseconds;
                _frameStopWatch.Restart();

                var wait = Task.Delay(Convert.ToInt32(1000 / _maxFPS));
                Update();

                await wait;
            }
        }

        private void Update()
        {
            //Console.WriteLine(_deltaTime);

            Input.Update(_deltaTime);
            Renderer.RenderFrame();
        }
    }
}
