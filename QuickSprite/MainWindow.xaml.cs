using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuickSprite.Utility;

namespace QuickSprite
{
    public partial class MainWindow : Window
    {
        private static readonly LoadImage LoadImage = new LoadImage();
        private static readonly SpriteCutter SpriteCutter = new SpriteCutter();

        private static string _fileName;
        private static Point _startPoint;
        private static Point _originPoint;
        private static StringBuilder _output = new StringBuilder();
        private static Dictionary<BitmapImage, string> _spriteDictionary = new Dictionary<BitmapImage, string>();


        public MainWindow()
        {
            InitializeComponent();

            var tg = new TransformGroup();
            var st = new ScaleTransform();
            var tt = new TranslateTransform();

            tg.Children.Add(st);
            tg.Children.Add(tt);

            ImageSelector.RenderTransform = tg;

            ImageSelector.MouseWheel += Selector_MouseWheel;
            ImageSelector.MouseRightButtonDown += Selector_MouseRightDown;
            ImageSelector.MouseRightButtonUp += Selector_MouseRightUp;
            ImageSelector.MouseMove += Selector_MouseMove;

            SaveButton.Click += SaveSprites;

            ImagePreview.MouseDown += ResetSprites;
            ImageBackground.MouseDown += ResetSprites;
        }

        private void ResetSprites(object sender, MouseButtonEventArgs e)
        {
            TreeSprites.Items.Clear();
            _spriteDictionary.Clear();
            _output.Clear();
            ImageSelector.Source = null;
            ImagePreview.Source = null;
        }

        private void PopulateTree(string file)
        {
            var rectangles = SpriteCutter.Blobs;
            var bmpImage = LoadImage.FromFile(file);

            _spriteDictionary = SpriteCutter.PopulateSprites(bmpImage, rectangles);

            foreach (var sprite in _spriteDictionary)
            {
                var item = new TreeViewItem
                {
                    Header = sprite.Key
                };

                item.Selected += RemoveItem;
                TreeSprites.Items.Add(item);
            }
        }

        private static void PopulateOutput()
        {
            var i = 400;

            foreach (var sprite in _spriteDictionary)
            {
                _output.Append($"SPRITE  {i} {_fileName}{sprite.Value} QUICKSPRITE\n");
                i++;
            }

        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem) sender;

            _output.Clear();
            _spriteDictionary.Remove((BitmapImage) item.Header);
            PopulateOutput();
            TreeSprites.Items.Remove(item);
        }

        private void LoadImage_OnDrop(object sender, DragEventArgs e)
        {
            ResetSprites(null, null);

            var fileData = e.Data as DataObject;
            if (fileData == null || !fileData.ContainsFileDropList()) return;

            var files = fileData.GetFileDropList();

            ImageSelector.Source = SpriteCutter.GetSprites(LoadImage.FromFile(files[0]));
            ImagePreview.Source = LoadImage.FromFile(files[0]);
            _fileName = files[0].Substring(files[0].LastIndexOf('\\') + 1);

            PopulateTree(files[0]);
            PopulateOutput();
        }

        private void Selector_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            ImageSelector.CaptureMouse();
            var translateTrans = (TranslateTransform) ((TransformGroup) ImageSelector.RenderTransform).Children
                .First(tt => tt is TranslateTransform);

            _startPoint = e.GetPosition(SelectorBorder);
            _originPoint = new Point(translateTrans.X, translateTrans.Y);
        }

        private void Selector_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            ImageSelector.ReleaseMouseCapture();
        }

        private void Selector_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ImageSelector.IsMouseCaptured) return;

            var translateTrans =
                (TranslateTransform) ((TransformGroup) ImageSelector.RenderTransform).Children
                .First(tt => tt is TranslateTransform);

            var vec = _startPoint - e.GetPosition(SelectorBorder);
            translateTrans.X = _originPoint.X - vec.X;
            translateTrans.Y = _originPoint.Y - vec.Y;
        }

        private void Selector_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var transGroup = (TransformGroup) ImageSelector.RenderTransform;
            var scaleTrans = (ScaleTransform) transGroup.Children[0];

            var zoom = e.Delta > 0 ? .2 : -.2;

            if (!(scaleTrans.ScaleX + zoom > 0.2) || !(scaleTrans.ScaleY + zoom > 0.2)) return;


            scaleTrans.ScaleX += zoom;
            scaleTrans.ScaleY += zoom;
        }

        private static void SaveSprites(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt",
                FileName = "QuickSprite",
                DefaultExt = ".txt",
                RestoreDirectory = true
            };

            if (saveDialog.ShowDialog() == true)
                File.WriteAllText(saveDialog.FileName, _output.ToString());
        }
    }
}
