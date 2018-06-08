using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace QuickSprite.Utility
{
    public class SpriteCutter
    {
        private static readonly Dictionary<BitmapImage, string> SpriteKvp = new Dictionary<BitmapImage, string>();

        public Rectangle[] Blobs { get; private set; }

        public BitmapImage GetSprites(BitmapImage image)
        {
            var bitmap = new Bitmap(ToBitmap(image));
            var nBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            var bc = new BlobCounter
            {
                MinHeight = 1,
                MinWidth = 1,
                FilterBlobs = false
            };

            bitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
            bc.ProcessImage(bitmap);

            var blobRects = bc.GetObjectsRectangles();
            Blobs = blobRects;

            var g = Graphics.FromImage(nBitmap);
            var p = new Pen(Color.WhiteSmoke, 1);

            g.DrawImage(bitmap, 0, 0);

            if (blobRects.Length < 0) return ToBitmapImage(bitmap);

            foreach (var rectangle in blobRects)
                g.DrawRectangle(p, rectangle);

            return ToBitmapImage(nBitmap);
        }

        public static BitmapImage UpdateRects(BitmapImage image, Rectangle[] rectangles)
        {
            var bitmap = new Bitmap(ToBitmap(image));
            var nBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            var g = Graphics.FromImage(nBitmap);
            var p = new Pen(Color.WhiteSmoke, 1);

            bitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
            g.DrawImage(bitmap, 0, 0);

            if (rectangles.Length < 0) return ToBitmapImage(bitmap);

            foreach (var rectangle in rectangles)
                g.DrawRectangle(p, rectangle);

            return ToBitmapImage(nBitmap);
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
