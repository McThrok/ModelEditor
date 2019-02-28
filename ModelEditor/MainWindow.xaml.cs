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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            _engine = new Engine(BitmapContainer);
            _engine.SceneMnager.AddTorus();
            objectList.ItemsSource = _engine.SceneMnager.Scene.Objects;

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
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _engine?.Input.OnMouseWheel(e.Delta);
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
                ObjectMenu.Visibility = Visibility.Visible;
            else
                ObjectMenu.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var objs = _engine.SceneMnager.Scene.Objects;
            objs.RemoveAt(objs.Count - 1);
        }
    }
}
