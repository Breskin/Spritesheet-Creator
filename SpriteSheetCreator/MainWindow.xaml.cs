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
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace SpriteSheetCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string[] Files = { };
        public int Columns = 1, Rows = 1;
        public double Scale = 1;
        public bool FlipHorizontally = false, FlipVertically = false;

        MyCanvas Canvas;

        public MainWindow()
        {
            InitializeComponent();

            Canvas = new MyCanvas(this);
            Grid.Children.Add(Canvas);

            ScaleSlider.Value = 1;
            ColumnsSlider.Value = 1;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Files = Directory.GetFiles(fbd.SelectedPath).Where(s => s.EndsWith(".png")).OrderBy(s => int.Parse(s.Substring(s.LastIndexOf("\\") + 1, s.LastIndexOf(".") - s.LastIndexOf("\\") - 1))).ToArray();

                    ColumnsSlider.Maximum = ColumnsSlider.Value = Files.Length;
                    ColumnsSlider.Minimum = 1;
                    Rows = 1;

                    Canvas.LoadImages();
                    Canvas.UpdateSize();

                    ScaleText.Content = Math.Floor(Scale * 100) + "%" + ((double.IsNaN(Canvas.Width)) ? "" : (" (" + (int)Canvas.Width + "x" + (int)Canvas.Height + ")"));

                    Canvas.InvalidateVisual();
                }
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Generate();
        }

        private void Columns_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Canvas == null)
                return;

            Columns = (int)Math.Round(ColumnsSlider.Value);
            Rows = (int)Math.Ceiling((Canvas.Images.Length * 1d) / (Columns * 1d));

            if (Canvas != null)
            {
                ColumnsText.Content = Columns + "x" + Rows;
                Canvas.UpdateSize();

                ScaleText.Content = Math.Floor(Scale * 100) + "%" + ((double.IsNaN(Canvas.Width)) ? "" : (" (" + (int)Canvas.Width + "x" + (int)Canvas.Height + ")"));
            }
        }

        private void HorizontalFlip_Click(object sender, RoutedEventArgs e)
        {
            FlipHorizontally = (bool)HorizontalFlip.IsChecked;

            Canvas.InvalidateVisual();
        }

        private void OpenGif_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Filter = "|*.gif";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Canvas.LoadFromGif(dialog.FileName);

                    ColumnsSlider.Maximum = ColumnsSlider.Value = Canvas.Images.Length;
                    ColumnsSlider.Minimum = 1;
                    Rows = 1;
                }
            }

            Canvas.UpdateSize();

            ScaleText.Content = Math.Floor(Scale * 100) + "%" + ((double.IsNaN(Canvas.Width)) ? "" : (" (" + (int)Canvas.Width + "x" + (int)Canvas.Height + ")"));

            Canvas.InvalidateVisual();
        }

        private void VerticalFlip_Click(object sender, RoutedEventArgs e)
        {
            FlipVertically = (bool)VerticalFlip.IsChecked;

            Canvas.InvalidateVisual();
        }

        private void Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = ScaleSlider.Value;
            Canvas.UpdateSize();

            ScaleText.Content = Math.Floor(Scale * 100) + "%" + ((double.IsNaN(Canvas.Width)) ? "" : (" (" + (int)Canvas.Width + "x" + (int)Canvas.Height + ")"));

            Canvas.InvalidateVisual();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.RemoveColor(GetPixelColor(this.PointToScreen(e.GetPosition(this))));

            Canvas.InvalidateVisual();
        }

        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);

        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);

        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        private static Color GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);

            ReleaseDC(0, lDC);

            byte a = (byte)((intColor >> 0x18) & 0xffL);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);

            return Color.FromRgb(r, g, b);
        }
    }
}
