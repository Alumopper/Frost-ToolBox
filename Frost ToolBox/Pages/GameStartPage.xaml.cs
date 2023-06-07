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
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameStartPage : Page, IFrostPage
    {
        public List<string> versions = new();

        public GameStartPage()
        {
            this.InitializeComponent();
            VersionFlush();
        }

        public void Flush()
        {
            
        }

        public async void VersionFlush()
        {
            //好不适应uwp访问文件的方式。。。 
            //刷新版本列表的文件
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Versions/versions.json"));
            //读取其中的json中的values列表
            var json = await FileIO.ReadTextAsync(file);
            var values = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
            //将values中的值添加到versions中
            versions = values;
            //versions中的文件是否存在
            string missingFiles = "";
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("Versions") == null)
            {
                await ApplicationData.Current.LocalFolder.CreateFolderAsync("Versions");
            }
            foreach (var version in versions)
            {
                var file2 = (await ApplicationData.Current.LocalFolder.GetFolderAsync("Versions")).
                    TryGetItemAsync(version);
                if (file2 == null)
                {
                    versions.Remove(version);
                    missingFiles += version + "\n";  
                }
            }
            if(missingFiles != "")
            {
                //日志输出所有丢失的文件
                FrostLeaf.Instance.log.Warning("已移除丢失的文件", missingFiles.ToString());
                //重新写入json
                string qwq = Newtonsoft.Json.JsonConvert.SerializeObject(versions);
                //写入文件
                await FileIO.WriteTextAsync(file, qwq);
            }
            //刷新所有tab中的列表
            foreach (TabViewItem tab in gameTabs.TabItems)
            {
                if ((tab.Content as Frame).Content is Game.GameStartHomePage page)
                {
                    page.Flush();
                }
            }
        }

        private void TabView_AddTabButtonClick(TabView sender, object args)
        {
            TabViewItem tab = CreateNewTab();
            sender.TabItems.Add(tab);
            sender.SelectedItem = tab;
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if((args.Tab.Content as Frame).Content is Game.GameStartHomePage) 
            {
                sender.TabItems.Remove(args.Tab);
            }
            else
            {
                //
            }
            if(sender.TabItems.Count == 0)
            {
                TabView_Loaded(sender, null);
            }
        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            if((sender as TabView).TabItems.Count == 0)
            {
                (sender as TabView).TabItems.Add(CreateNewTab());
                (sender as TabView).SelectedIndex = 0;
            }
        }

        private static TabViewItem CreateNewTab()
        {
            TabViewItem tab = new()
            {
                Header = "启动游戏",
                IconSource = new SymbolIconSource() { Symbol = Symbol.Send }
            };
            Frame f = new();
            f.Navigate(typeof(Game.GameStartHomePage));
            tab.Content = f;
            return tab;
        }
    }
}
