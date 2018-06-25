using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuickSprite.Utility;
using Point = System.Windows.Point;

namespace QuickSprite
{
    public partial class MainWindow : Window
    {
        private static readonly LoadImage LoadImage = new LoadImage();
        private static readonly SpriteCutter SpriteCutter = new SpriteCutter();
       
        private static string _fileName;
        private static Point _startPoint;
        private static Point _originPoint;
        private static BitmapImage _bmpImage;
        private static StringBuilder _output = new StringBuilder();
        private static Dictionary<BitmapImage, string> _spriteDictionary = new Dictionary<BitmapImage, string>();

        private Rectangle[] _rectangles;

        // TODO: Cleanup Main

        public MainWindow()
        {
            InitializeComponent();

            var tg = new TransformGroup();
            var st = new ScaleTransform();
            var tt = new TranslateTransform();

            tg.Children.Add(st);
            tg.Children.Add(tt);

            ImageSelector.RenderTransform = tg;

            ImageSelector.MouseMove += Selector_MouseMove;
            ImageSelector.MouseWheel += Selector_MouseWheel;
            ImageSelector.MouseRightButtonUp += Selector_MouseRightUp;
            ImageSelector.MouseRightButtonDown += Selector_MouseRightDown;

            SaveButton.Click += SaveSprites;
            CopyButton.Click += ToClipboard;

            ImagePreview.MouseDown += ResetSprites;
            ImageBackground.MouseDown += ResetSprites;

            SliderPrecision.ValueChanged += UpdateSprites;

            Uindex.PreviewTextInput += IndexTextInput;
        }

        private static void IndexTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextFilter.NumericInput(e.Text);
        }

        private void UpdateSprites(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TreeSprites.Items.Clear();
            _spriteDictionary.Clear();
            ImageSelector.Source = SpriteCutter.GetSprites(_bmpImage, (int)e.NewValue);
            PopulateTree();
        }

        private void ResetSprites(object sender, MouseButtonEventArgs e)
        {
            TreeSprites.Items.Clear();
            _spriteDictionary.Clear();

            SliderPrecision.IsEnabled = false;
            Uindex.IsEnabled = false;

            ImageSelector.Source = null;
            ImagePreview.Source = null;
        }

        private void PopulateTree()
        {
            _rectangles = SpriteCutter.Blobs;
            _spriteDictionary = SpriteCutter.PopulateSprites(_bmpImage, _rectangles);

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

        private void PopulateTree(string file)
        {
            _rectangles = SpriteCutter.Blobs;
            _bmpImage = LoadImage.FromFile(file);

            _spriteDictionary = SpriteCutter.PopulateSprites(_bmpImage, _rectangles);

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

        private void PopulateOutput()
        {
            _output.Clear();

            var i = int.Parse(Uindex.Text);

            if (i < 0)
            {
                foreach (var sprite in _spriteDictionary)
                {
                    _output.Append($"SPRITE  {i} {_fileName}{sprite.Value} QUICKSPRITE\n");
                    i--;
                }
            }
            else
            {
                foreach (var sprite in _spriteDictionary)
                {
                    _output.Append($"SPRITE  {i} {_fileName}{sprite.Value} QUICKSPRITE\n");
                    i++;
                }
            }
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem) sender;
            var rects = _rectangles.ToList();

            foreach (var sprite in _spriteDictionary)
            {
                if (!Equals(sprite.Key, item.Header)) continue;

                rects.RemoveAt(_spriteDictionary.Keys.ToList().IndexOf((BitmapImage) item.Header));
            }

            _rectangles = rects.ToArray();
            ImageSelector.Source = SpriteCutter.UpdateRects(_bmpImage ,_rectangles);
            _spriteDictionary.Remove((BitmapImage) item.Header);

            TreeSprites.Items.Remove(item);
        }

        private void LoadImage_OnDrop(object sender, DragEventArgs e)
        {
            ResetSprites(null, null);
            SliderPrecision.IsEnabled = true;
            Uindex.IsEnabled = true;

            var fileData = e.Data as DataObject;
            if (fileData == null || !fileData.ContainsFileDropList()) return;

            var files = fileData.GetFileDropList();

            ImageSelector.Source = SpriteCutter.GetSprites(LoadImage.FromFile(files[0]), (int)SliderPrecision.Value);
            ImagePreview.Source = LoadImage.FromFile(files[0]);
            _fileName = files[0].Substring(files[0].LastIndexOf('\\') + 1);

            PopulateTree(files[0]);
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

        private async void SaveSprites(object sender, RoutedEventArgs e)
        {
            try
            {
                PopulateOutput();

                SaveButton.Content = "Saving...";

                var saveDialog = new SaveFileDialog
                {
                    Filter = "txt files (*.txt)|*.txt",
                    FileName = "QuickSprite",
                    DefaultExt = ".txt",
                    RestoreDirectory = true
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, _output.ToString());
                    SaveButton.Content = "Saved...";
                }
                else
                    SaveButton.Content = "Aborted...";
            }
            catch (Exception)
            {
                SaveButton.Content = "Index Error...";
                MessageBox.Show("Please check that the index is input correctly.", "Index Error!", MessageBoxButton.OK);
            }

            await Task.Delay(1500);
            SaveButton.Content = "Save Sprites";
        }

        private async void ToClipboard(object sender, RoutedEventArgs e)
        {
            try
            {
                PopulateOutput();

                Clipboard.SetText(_output.ToString());
                CopyButton.Content = "Copied...";
            }
            catch (Exception)
            {
                CopyButton.Content = "Index Error...";
                MessageBox.Show("Please check that the index is input correctly.", "Index Error!", MessageBoxButton.OK);
            }

            await Task.Delay(1500);
            CopyButton.Content = "Copy Sprites";
        }
    }
}
