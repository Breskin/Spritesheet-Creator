using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpriteSheetCreator
{
    class MyCanvas : Canvas
    {
        MainWindow Window;

        public BitmapSource[] Images { get; private set; } = { };

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

        public void LoadFromGif(string path)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            System.Drawing.Size imageSize = new System.Drawing.Size(image.Size.Width, image.Size.Height);
            System.Drawing.Imaging.FrameDimension frameSize = new System.Drawing.Imaging.FrameDimension(image.FrameDimensionsList[0]);

            int frames = image.GetFrameCount(frameSize);

            Images = new BitmapImage[frames];

            for (int i = 0; i < frames; i++)
            {
                System.Drawing.Bitmap b = new System.Drawing.Bitmap(imageSize.Width, imageSize.Height);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);

                image.SelectActiveFrame(frameSize, i);
                g.DrawImage(image, 0, 0);

                Images[i] = Bitmap2BitmapImage(b);

                b.Dispose();
            }
        }

        public void RemoveColor(Color c)
        {
            for (int i = 0; i < Images.Length; i++)
            {
                System.Drawing.Bitmap bitmap = BitmapImage2Bitmap(Images[i]);
                bitmap.MakeTransparent(System.Drawing.Color.FromArgb(c.R, c.G, c.B));
                Images[i] = Bitmap2BitmapImage(bitmap);
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

                g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 255)), new System.Drawing.RectangleF(0, 0, bitmap.Width, bitmap.Height));
                
                for (int i = 0; i < Images.Length; i++)
                {
                    g.DrawImage(BitmapImage2Bitmap(Transform(Images[i])), (int)(Images[i].PixelWidth * (i % Window.Columns) * Window.Scale), (int)(Images[i].PixelHeight * (i / Window.Columns) * Window.Scale), (int)(Images[i].PixelWidth * Window.Scale), (int)(Images[i].PixelHeight * Window.Scale));
                }

                bitmap.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255));

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
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        private BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private static BitmapSource CreateTransparency(BitmapSource source, Color c)
        {
            if (source.Format != PixelFormats.Bgra32)
            {
                return source;
            }

            var bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            var stride = bytesPerPixel * source.PixelWidth;
            var buffer = new byte[stride * source.PixelHeight];

            source.CopyPixels(buffer, stride, 0);

            for (int y = 0; y < source.PixelHeight; y++)
            {
                for (int x = 0; x < source.PixelWidth; x++)
                {
                    var i = stride * y + bytesPerPixel * x;
                    var b = buffer[i];
                    var g = buffer[i + 1];
                    var r = buffer[i + 2];
                    var a = buffer[i + 3];

                    if (c.R == r && c.G == g && c.B == b)
                    {
                        buffer[i + 3] = 0;
                    }
                }
            }

            return BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format, null, buffer, stride);
        }
    }
}
