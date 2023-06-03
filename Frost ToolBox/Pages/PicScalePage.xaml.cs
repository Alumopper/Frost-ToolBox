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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Frost_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicScalePage : Page
    {
        static List<string> readedFiles = new();

        public PicScalePage()
        {
            this.InitializeComponent();
            if (readedFiles.Count != 0)
            {
                _ = addPictures(readedFiles);
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
                        foreach (var f in folderFiles)
                        {
                            var extension = Path.GetExtension(f.Path);
                            if (extension == ".png" || extension == ".jpg")
                            {
                                files.Add(f.Path);
                            }
                        }
                    }
                }

                readedFiles = await addPictures(files);
            }
        }

        private async Task<List<string>> addPictures(List<string> files)
        {
            tipText.Visibility = Visibility.Collapsed;
            List<string> result = new List<string>();
            // 'files' now contains a list of paths to all PNG and JPG files that were dropped onto the control.
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                    Bitmap bitmap = new(System.Drawing.Image.FromFile(file));
                    if (decoder.PixelWidth == 16 && decoder.PixelHeight == 16)
                    {
                        var image = new Microsoft.UI.Xaml.Controls.Image
                        {
                            Source = new BitmapImage(new Uri(file)),
                            Width = decoder.PixelWidth * 2,
                            Height = decoder.PixelHeight * 2,

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
            }
            return result;
        }

        private void picPreview_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        { 
            if(readedFiles.Count <= 0) {
                log.Text = "未选中任何文件";
                return;
            }
            // Create a folder picker
            FolderPicker openPicker = new();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow);

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
                    FileInfo fileInfo = new FileInfo(item);
                    Bitmap bitmap = new Bitmap(item);
                    Bitmap re = new Bitmap(128, 128);
                    for (int x = 0; x < re.Width; x++)
                    {
                        for (int y = 0; y < re.Height; y++)
                        {
                            re.SetPixel(x, y, bitmap.GetPixel(x / 8, y / 8));
                        }
                    }
                    //储存
                    Directory.CreateDirectory(folder.Path + "\\output\\");
                    re.Save(folder.Path + "\\output\\" + fileInfo.Name);
                }
                log.Text = $"已将{readedFiles.Count}个图片放大到文件夹{folder.Path}";
            }
        }
    }
}
