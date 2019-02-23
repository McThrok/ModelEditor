using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap _writableBitmap;
        private Scene _scene = new Scene();
        private Renderer _renderer;
        private Stopwatch _frameStopWatch = new Stopwatch();
        private double _deltaTime;
        private float maxFPS = 60;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += Run;
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
            Init();

            while (true)
            {
                _deltaTime = _frameStopWatch.Elapsed.TotalMilliseconds;
                _frameStopWatch.Restart();

                var wait = Task.Delay(Convert.ToInt32(1000 / maxFPS));
                RenderFrame();

                await wait;
            }
        }

        private void Init()
        {
            _writableBitmap = new WriteableBitmap((int)BitmapContainer.ActualWidth, (int)BitmapContainer.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
            BitmapImage.Source = _writableBitmap;
            _renderer = new Renderer(_writableBitmap, _scene);

            var obj = new TestObj();
            _scene.Objects.Add(obj);
        }


        private void RenderFrame()
        {
            Console.WriteLine(_deltaTime);
        }
    }
}
