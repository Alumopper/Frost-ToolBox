using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Microsoft.UI;
using System.Drawing;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using FrostLeaf_ToolBox.Pages;
using FrostLeaf_ToolBox;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicScalePage : Page, IFrostPage
    {
        static List<string> readedFiles = new();

        public PicScalePage()
        {
            this.InitializeComponent();
            if (readedFiles.Count != 0)
            {
                _ = AddPictures(readedFiles);
            }
            else
            {
                tipText.Visibility = Visibility.Visible;
            }
        }

        private async void qwq_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                readedFiles = await AddPictures(await GetPictures(items));
            }
        }

        private async Task<IEnumerable<string>> GetPictures(IReadOnlyList<IStorageItem> items)
        {
            var files = new List<string>();

            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    var extension = Path.GetExtension(file.Path);
                    if (extension == ".png" || extension == ".jpg")
                    {
                        files.Add(file.Path);
                    }
                }
                else if (item is StorageFolder folder)
                {
                    var folderFiles = await folder.GetFilesAsync();
                    files = files.Concat(await GetPictures(folderFiles)).ToList();
                }
            }
            return files;
        }

        private async Task<List<string>> AddPictures(IEnumerable<string> files)
        {
            tipText.Visibility = Visibility.Collapsed;
            picPreview.IsItemClickEnabled = true;
            picPreview.SelectionMode = ListViewSelectionMode.Single;
            List<string> result = new ();
            // 'files' now contains a list of paths to all PNG and JPG files that were dropped onto the control.
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                    Bitmap bitmap = new(System.Drawing.Image.FromFile(file));
                    if (decoder.PixelWidth == 16 && decoder.PixelHeight == 16
                        || decoder.PixelWidth == 32 && decoder.PixelHeight == 32
                        || decoder.PixelWidth == 64 && decoder.PixelHeight == 64
                        )
                    {
                        var image = new Microsoft.UI.Xaml.Controls.Image
                        {
                            Source = new BitmapImage(new Uri(file)),
                            Width = 32,
                            Height = 32,

                        };
                        picPreview.Items.Add(image);
                        result.Add(new string(file));
                    }
                }
            }
            if (picPreview.Items.Count != 1)
            {
                picPreview.HorizontalAlignment = HorizontalAlignment.Left;
                picPreview.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                tipText.Visibility = Visibility.Visible;
                picPreview.IsItemClickEnabled = false;
                picPreview.SelectionMode = ListViewSelectionMode.None;
            }
            return result;
        }

        private void picPreview_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        //转换按钮
        private async void Button_Click(object sender, RoutedEventArgs e)
        { 
            if(readedFiles.Count <= 0) {
                log.Text = "未选中任何文件";
                return;
            }
            ReadingPicTip.IsOpen = true;
            // Create a folder picker
            FolderPicker openPicker = new();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                log.Text = "正在转换";
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                //放大图片
                foreach (var item in readedFiles)
                {
                    FileInfo fileInfo = new (item);
                    Bitmap bitmap = new(item);   //原图
                    Bitmap re = new (128, 128);
                    for (int x = 0; x < re.Width; x++)
                    {
                        for (int y = 0; y < re.Height; y++)
                        {
                            re.SetPixel(x, y, bitmap.GetPixel(x / (128/bitmap.Width), y / (128 / bitmap.Width)));
                        }
                    }
                    //储存
                    Directory.CreateDirectory(folder.Path + "\\output\\");
                    re.Save(folder.Path + "\\output\\" + fileInfo.Name);
                }
                log.Text = $"已将{readedFiles.Count}个图片放大到文件夹{folder.Path}";
            }
            ReadingPicTip.IsOpen = false;
        }

        //从项目中选择
        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ReadingPicTip.IsOpen = true;
            (sender as MenuFlyoutItem).IsEnabled = false;
            foreach (var item in FrostLeaf.Instance.settings.ResourceSettings.TextureFolders)
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item);
                if (folder != null)
                {
                    var items = await folder.GetItemsAsync();

                    readedFiles = await AddPictures(await GetPictures(items));
                }
            }
            ReadingPicTip.IsOpen = false;
            (sender as MenuFlyoutItem).IsEnabled = true;
        }

        //从文件夹中选择
        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            (sender as MenuFlyoutItem).IsEnabled = false;
            // Create a folder picker
            FolderPicker openPicker = new();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                var items = await folder.GetItemsAsync();

                readedFiles = await AddPictures(await GetPictures(items));

            }
            (sender as MenuFlyoutItem).IsEnabled = true;
        }

        public void Flush()
        {
            if(FrostLeaf.Instance.settings.ResourceSettings.resourcepack != "")
            {
                readFromProject.IsEnabled = true;
            }
        }
    }
}
