using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpriteSheetCreator
{
    class MyCanvas : Canvas
    {
        MainWindow Window;

        BitmapSource[] Images = { };

        public MyCanvas(MainWindow window)
        {
            Window = window;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Images.Length > 0)
            {
                for (int i = 0; i < Images.Length; i++)
                {
                    dc.DrawImage(Transform(Images[i]), new System.Windows.Rect(Images[i].PixelWidth * (i % Window.Columns) * Window.Scale, Images[i].PixelHeight * (i / Window.Columns) * Window.Scale, Images[i].PixelWidth * Window.Scale, Images[i].PixelHeight * Window.Scale));
                }
            }
        }

        public void LoadImages()
        {
            Images = new BitmapImage[Window.Files.Length];

            for (int i = 0; i < Window.Files.Length; i++)
            {
                Images[i] = new BitmapImage(new Uri(Window.Files[i]));
            }
        }

        public void UpdateSize()
        {
            if (Images.Length > 0)
            {
                Width = Window.Columns * Images[0].PixelWidth * Window.Scale;
                Height = Window.Rows * Images[0].PixelHeight * Window.Scale;
            }
        }

        public void Generate()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "|*.png";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)Width, (int)Height);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

                g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0)), new System.Drawing.RectangleF(0, 0, bitmap.Width, bitmap.Height));

                for (int i = 0; i < Images.Length; i++)
                {
                    g.DrawImage(BitmapImage2Bitmap(Transform(Images[i])), (int)(Images[i].PixelWidth * (i % Window.Columns) * Window.Scale), (int)(Images[i].PixelHeight * (i / Window.Columns) * Window.Scale), (int)(Images[i].PixelWidth * Window.Scale), (int)(Images[i].PixelHeight * Window.Scale));
                }

                bitmap.MakeTransparent();

                bitmap.Save(dialog.FileName);
            }
        }

        TransformedBitmap Transform(BitmapSource b)
        {
            Transform tr = new ScaleTransform((Window.FlipHorizontally) ? -1 : 1, (Window.FlipVertically) ? -1 : 1);

            TransformedBitmap transformedBmp = new TransformedBitmap();
            transformedBmp.BeginInit();
            transformedBmp.Source = b;
            transformedBmp.Transform = tr;
            transformedBmp.EndInit();

            return transformedBmp;
        }

        private System.Drawing.Bitmap BitmapImage2Bitmap(BitmapSource bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }
    }
}
