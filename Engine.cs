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
        private readonly float _maxFPS = 60;

        public InputManager Input { get; private set; }

        private Scene _scene;
        private Renderer _renderer;
        private Stopwatch _frameStopWatch = new Stopwatch();
        private double _deltaTime;

        public Engine(Panel bitmapConatiner)
        {
            _bitmapConatiner = bitmapConatiner;
            InitBitmap();
            InitScene();

            _renderer = new Renderer(_writableBitmap, _scene);
            Input = new InputManager(_bitmapConatiner,_writableBitmap, _scene);
        }

        private void InitBitmap()
        {
            _writableBitmap = new WriteableBitmap((int)_bitmapConatiner.ActualWidth, (int)_bitmapConatiner.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
            var img = new System.Windows.Controls.Image();
            img.Source = _writableBitmap;
            _bitmapConatiner.Children.Add(img);
        }

        private void InitScene()
        {
            _scene = new Scene();
            //_scene.Camera.Rotate(0, Math.PI / 4, 0);
            var obj = new TestObj();
            _scene.Objects.Add(obj);
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
            _renderer.RenderFrame();
        }
    }
}
