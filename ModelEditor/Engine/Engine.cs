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
            Input = new InputManager(_bitmapConatiner,_writableBitmap, Scene);
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
