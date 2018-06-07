using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using AForge.Imaging;

namespace QuickSprite.Utility
{
    public class SpriteCutter
    {
        private static readonly Dictionary<BitmapImage, string> SpriteKvp = new Dictionary<BitmapImage, string>();

        public Rectangle[] Blobs { get; private set; }

        public BitmapImage GetSprites(BitmapImage image)
        {
            var bitmapped = new Bitmap(ToBitmap(image));
            bitmapped = ApplyGrayscale(bitmapped);

            var bc = new BlobCounter
            {
                MinHeight = 1,
                MinWidth = 1,
                FilterBlobs = false
            };

            bc.ProcessImage(bitmapped);

            var blobRects = bc.GetObjectsRectangles();
            Blobs = blobRects;

            var g = Graphics.FromImage(bitmapped);
            var p = new Pen(Color.WhiteSmoke, 1);

            if (blobRects.Length > 0)
            {
                foreach (var rectangle in blobRects)
                {
                    g.DrawRectangle(p, rectangle);
                }

                return ToBitmapImage(bitmapped);
            }
            else
            {
                return ToBitmapImage(bitmapped);
            }
        }

        public static Dictionary<BitmapImage, string> PopulateSprites(BitmapImage image, Rectangle[] rectangles)
        {

            var oBitmap = new Bitmap(ToBitmap(image));
            var bitmaps = rectangles.Select(rectangle => oBitmap.Clone(rectangle, oBitmap.PixelFormat)).ToList();
            var bitmapImages = bitmaps.Select(ToBitmapImage).ToList();
            var cords = rectangles.Select(rectangle => $"{rectangle.X,5}{rectangle.Y,5}{rectangle.Width,5}{rectangle.Height,5}").ToList();

            for (int i = 0; i < rectangles.Length; i++)
            {
                SpriteKvp.Add(bitmapImages[i], cords[i]);
            }

            return SpriteKvp;
        }

        private static Bitmap ApplyGrayscale(Bitmap image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var oColor = image.GetPixel(x, y);
                    var gScale = (int) ((oColor.R * 0.3) + (oColor.G * 0.59) + (oColor.B * 0.11));
                    var nColor = Color.FromArgb(oColor.A, gScale, gScale, gScale);
                    image.SetPixel(x, y, nColor);
                }
            }

            return image;
        }


        /* Thanks to: https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa */
        private static Bitmap ToBitmap(BitmapImage image)
        {
            using (var ms = new MemoryStream())
            {
                var bmpEncoder = new BmpBitmapEncoder();

                bmpEncoder.Frames.Add(BitmapFrame.Create(image));
                bmpEncoder.Save(ms);

                var bitmap = new Bitmap(ms);
                ms.Dispose();

                return bitmap;
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                ms.Dispose();

                return bitmapImage;
            }
        }
    }
}
