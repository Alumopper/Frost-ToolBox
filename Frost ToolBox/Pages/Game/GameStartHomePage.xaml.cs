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
using FrostLeaf_ToolBox.Utils;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages.Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameStartHomePage : Page
    {

        public StorageFile selectedBatFile = null;

        public GameStartHomePage()
        {
            this.InitializeComponent();
            //Versions.ItemsSource = (FrostLeaf.Instance.pages["GameStartPage"].Value as GameStartPage).versions;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                //��ȡԴ�ļ�·��
                var items = await e.DataView.GetStorageItemsAsync();
                List<string> file_names = (from item in items.OfType<StorageFile>()
                                           where item.FileType == ".bat"
                                           select item.Name).ToList();
                var versions = (FrostLeaf.Instance.pages["GameStartPage"].Value as GameStartPage).versions;
                versions = versions.Union(file_names).ToList();
                (FrostLeaf.Instance.pages["GameStartPage"].Value as GameStartPage).versions = versions;
                Versions.ItemsSource = versions;
                AddVersionTip.Subtitle = "�����" + file_names.Count + "�����нű�";
                AddVersionTip.IsOpen = true;
                //���ļ����Ƶ�Versions�ļ�����
                if (ApplicationData.Current.LocalFolder.TryGetItemAsync("Versions") == null)
                {
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync("Versions");
                }
                foreach (var item in items.OfType<StorageFile>())
                {
                    await item.CopyAsync((await ApplicationData.Current.LocalFolder.GetFolderAsync("Versions")), item.Name, NameCollisionOption.ReplaceExisting);
                }
                //����д��json
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Versions/versions.json"));
                //��ȡ����json�ļ��е�����
                string json = JsonConvert.SerializeObject(versions, Formatting.Indented);
                await FileIO.WriteTextAsync(f, json);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void Versions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBatFile = ApplicationData.Current.LocalFolder.GetFolderAsync("Versions").AsTask().Result.GetFileAsync(Versions.SelectedItem.ToString()).AsTask().Result;
            GameStartButton.Content = selectedBatFile.Name[..selectedBatFile.Name.LastIndexOf(".")];
        }

        public void Flush()
        {
            Versions.ItemsSource = (FrostLeaf.Instance.pages["GameStartPage"].Value as GameStartPage).versions;
        }
    }
}
