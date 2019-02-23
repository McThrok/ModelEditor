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

namespace ModelEditor
{
    public class Engine
    {
        private readonly WriteableBitmap _writableBitmap;
        private readonly float _maxFPS = 60;

        private Scene _scene;
        private Renderer _renderer;
        private Stopwatch _frameStopWatch = new Stopwatch();
        private double _deltaTime;

        public Engine(WriteableBitmap writableBitmap)
        {
            _writableBitmap = writableBitmap;

            InitScene();
            InitRenderer();
        }

        private void InitRenderer()
        {
            _renderer = new Renderer(_writableBitmap, _scene);
        }
        private void InitScene()
        {
            _scene = new Scene();
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
            Console.WriteLine(_deltaTime);
            _renderer.RenderFrame();
        }
    }
}
