using System;
using System.IO;
using System.Windows.Media.Imaging;
using QuickSprite.Properties;

namespace QuickSprite.Utility
{
    public class LoadImage
    {
        private static BitmapImage _image;
        private static Stream _media;

        public BitmapImage FromFile(string file)
        {
            DisposeOldMedia();

            try
            {
                _image = new BitmapImage {CacheOption = BitmapCacheOption.None};
                _media = File.OpenRead(file);

                _image.BeginInit();
                _image.StreamSource = _media;
                _image.EndInit();
                _image.Freeze();
                return _image;
            }
            catch (Exception)
            {
                return SpriteCutter.ToBitmapImage(Resources.pattern_unknown);
            }
        }

        private static void DisposeOldMedia()
        {
            if (_media == null) return;

            _media.Close();
            _media.Dispose();
            _media = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }
    }
}
