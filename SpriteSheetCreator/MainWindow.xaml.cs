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
            Columns = (int)Math.Round(ColumnsSlider.Value);
            Rows = (int)Math.Ceiling((Files.Length * 1d) / (Columns * 1d));

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
    }
}
