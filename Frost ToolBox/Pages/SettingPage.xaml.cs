using FrostLeaf_ToolBox.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.Labs.WinUI;
using FrostLeaf_ToolBox;
using FrostLeaf_ToolBox.Pages;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page, IFrostPage
    {
        public SettingPage()
        {
            this.InitializeComponent();
            Flush();
        }

        public void Flush()
        {
            resourcepackPath.Description = FrostLeaf.Instance.settings.ResourceSettings.Resourcepack == ""?"未选择": FrostLeaf.Instance.settings.ResourceSettings.Resourcepack;
            datapackPath.Description = FrostLeaf.Instance.settings.ResourceSettings.Datapack == ""?"未选择": FrostLeaf.Instance.settings.ResourceSettings.Datapack;
            version.Text = FrostLeaf.Instance.Version;
            //检查工具箱资源文件夹地址信息是否有效
            if (FrostLeaf.Instance.settings.resourceFolder == "")
            {
                resourcePath.Description = "未选择";
            }
            else
            {
                if (Directory.Exists(FrostLeaf.Instance.settings.resourceFolder))
                {
                    resourcePath.Description = FrostLeaf.Instance.settings.resourceFolder;
                }
                else
                {
                    FrostLeaf.Instance.log.Error($"资源文件夹地址无效: {FrostLeaf.Instance.settings.resourceFolder}");
                    FrostLeaf.Instance.settings.resourceFolder = "";
                    resourcePath.Description = "未选择";
                }
            }
        }

        //数据包
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            FolderPicker openPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                datapackPath.Description = folder.Path;
                FrostLeaf.Instance.settings.ResourceSettings.Datapack = folder.Path;
                Settings.Write(FrostLeaf.Instance.settings);
            }
            (sender as Button).IsEnabled = true;
        }

        //资源包
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            resourcepackPath.Description = "正在检索材质文件";
            FolderPicker openPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var fs = await ResourcepackHelper.GetTextureFolders(folder);
                if(fs.Count > 0)
                {
                    resourcepackPath.Description = folder.Path;
                    FrostLeaf.Instance.settings.ResourceSettings.Resourcepack = folder.Path;
                    Settings.Write(FrostLeaf.Instance.settings);
                    FrostLeaf.Instance.settings.ResourceSettings.textureFolders = fs;
                }
                else
                {
                    InvalidResourcePack.IsOpen = true;
                    resourcepackPath.Description = "未选择";
                }
            }
            (sender as Button).IsEnabled = true;
        }

        //初始化
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "重置设置";
            dialog.PrimaryButtonText = "重置";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = new Dialog.ResetSettings();
            
            var result = await dialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                FrostLeaf.Instance.settings = new Settings();
                Settings.Write(FrostLeaf.Instance.settings);
                Flush();
            }

        }
        
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //选择新的文件夹
            (sender as Button).IsEnabled = false;
            resourcepackPath.Description = "正在选择服务端数据文件夹";
            FolderPicker openPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                resourcePath.Description = folder.Path;
                FrostLeaf.Instance.settings.resourceFolder = folder.Path;
                Settings.Write(FrostLeaf.Instance.settings);
            }
            (sender as Button).IsEnabled = true;
        }
    }
}
