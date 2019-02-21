using System;
using System.Collections.Generic;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
            Loaded += Init;
        }

        private async void Init(object sender, RoutedEventArgs e)
        {
            WriteableBitmap wb = new WriteableBitmap((int)BitmapContainer.ActualWidth, (int)BitmapContainer.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
            wb.Clear(Colors.Black);
            BitmapImage.Source = wb;

            //for (int i = 0; i < 400; i++)
            //{

            //    await Task.Delay(10);

            //    wb.SetPixel(10, 13, Colors.Black);

            //    wb.DrawLine(i, i, i, i+100, Colors.Green);

            //}   

        }
    }
}
