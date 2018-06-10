using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace QuickSprite.Utility
{
    public class SpriteCutter
    {
        private static readonly Dictionary<BitmapImage, string> SpriteKvp = new Dictionary<BitmapImage, string>();

        public Rectangle[] Blobs { get; private set; }

        public BitmapImage GetSprites(BitmapImage image, int precision)
        {
            var bitmap = new Bitmap(ToBitmap(image));
            var nBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            var bc = new BlobCounter
            {
                MinHeight = precision,
                MinWidth = precision,
                FilterBlobs = true,
                ObjectsOrder = ObjectsOrder.YX
            };

            bitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
            bc.ProcessImage(bitmap);

            var blobRects = bc.GetObjectsRectangles();
            Blobs = RemoveIntersects(blobRects);

            var g = Graphics.FromImage(nBitmap);
            var p = new Pen(Color.WhiteSmoke, 1);

            g.DrawImage(bitmap, 0, 0);

            if (Blobs.Length < 0) return ToBitmapImage(bitmap);

            foreach (var rectangle in Blobs)
                g.DrawRectangle(p, rectangle);

            bitmap.Dispose();
            g.Dispose();

            return ToBitmapImage(nBitmap);
        }

        private static Rectangle[] RemoveIntersects(Rectangle[] rects)
        {
            if (rects == null) return null;

            var nRects = rects.ToList();
            var iRects = rects.SelectMany((x, i) => rects.Skip(i + 1), Tuple.Create)
                .Where(x => x.Item1.IntersectsWith(x.Item2))
                .ToList();

            foreach (var tuple in iRects)
            {
                var x = ((int)tuple.Item1.X < (int)tuple.Item2.X) ? (int)tuple.Item1.X : (int)tuple.Item2.X;
                var y = ((int)tuple.Item1.Y < (int)tuple.Item2.Y) ? (int)tuple.Item1.Y : (int)tuple.Item2.Y;
                var offset = Rectangle.Intersect(tuple.Item1, tuple.Item2).Size;
                var width = (int )tuple.Item1.Width + (int) tuple.Item2.Width - offset.Width;
                var height = (int) tuple.Item1.Height + (int) tuple.Item2.Height - offset.Height;

                var rec1Index = nRects.IndexOf(tuple.Item1);
                var rec2Index = nRects.IndexOf(tuple.Item2);

                if (rec1Index < 1 || rec2Index < 1) continue;

                nRects[rec1Index] = new Rectangle(x, y, width, height);
                nRects.RemoveAt(rec2Index);
            }

            return nRects.ToArray();
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

            bitmap.Dispose();
            g.Dispose();

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

            oBitmap.Dispose();

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
