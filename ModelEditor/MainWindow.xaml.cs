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
        private Engine _engine;

        private float _positionChangeSpeed = 1f;
        private float _rotationChangeSpeed = (float)(Math.PI / 8);
        private float _scaleChangeSpeed = 1.2f;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            _engine = new Engine(BitmapContainer);
            objectList.ItemsSource = _engine.SceneMnager.Scene.MainpObjects;
            _engine.Run();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _engine?.Input.OnMouseLeftButtonDown(e.GetPosition(BitmapContainer));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _engine?.Input.OnMouseLeftButtonUp();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _engine?.Input.OnKeyDown(e.Key);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _engine?.Input.OnKeyUp(e.Key);
        }

        private void Torus_Click(object sender, RoutedEventArgs e)
        {
            _engine.SceneMnager.AddTorus();
        }

        private void SelectedObjectChange(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count - e.RemovedItems.Count >= 0)
                objectMenu.Visibility = Visibility.Visible;
            else
                objectMenu.Visibility = Visibility.Hidden;

            var item = GetSelectedObj();
            TorusMenu.Visibility = item.Name == nameof(Torus) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var objs = _engine.SceneMnager.Scene.Objects;
            objs.RemoveAt(objs.Count - 1);
        }

        private ManipObj GetSelectedObj()
        {
            return objectList.SelectedIndex >= 0 ? _engine.SceneMnager.Scene.MainpObjects[objectList.SelectedIndex] : null;
        }

        private void PositionXUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(_positionChangeSpeed, 0, 0); }
        private void PositionXDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(-_positionChangeSpeed, 0, 0); }
        private void PositionYUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, _positionChangeSpeed, 0); }
        private void PositionYDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, -_positionChangeSpeed, 0); }
        private void PositionZUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, 0, _positionChangeSpeed); }
        private void PositionZDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, 0, -_positionChangeSpeed); }

        private void RotationXUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(_rotationChangeSpeed, 0, 0); }
        private void RotationXDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(-_rotationChangeSpeed, 0, 0); }
        private void RotationYUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, _rotationChangeSpeed, 0); }
        private void RotationYDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, -_rotationChangeSpeed, 0); }
        private void RotationZUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, 0, _rotationChangeSpeed); }
        private void RotationZDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, 0, -_rotationChangeSpeed); }

        private void ScaleUp(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(_scaleChangeSpeed); }
        private void ScaleDown(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1 / _scaleChangeSpeed); }

    }
}
