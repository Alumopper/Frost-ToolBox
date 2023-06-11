//#define TEST

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
using FrostLeaf_ToolBox.Pages.Game;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameStartPage : Page, IFrostPage
    {
        public TabView GameTabs { get => gameTabs; }

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
            //�ò���Ӧuwp�����ļ��ķ�ʽ������ 
            //ˢ�°汾�б���ļ�
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Versions/versions.json"));
            //��ȡ���е�json�е�values�б�
            var json = await FileIO.ReadTextAsync(file);
            var values = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
            //��values�е�ֵ��ӵ�versions��
            versions = values;
            //versions�е��ļ��Ƿ����
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
                //��־������ж�ʧ���ļ�
                FrostLeaf.Instance.log.Warning("���Ƴ���ʧ���ļ�", missingFiles.ToString());
                //����д��json
                string qwq = Newtonsoft.Json.JsonConvert.SerializeObject(versions);
                //д���ļ�
                await FileIO.WriteTextAsync(file, qwq);
            }
            //ˢ������tab�е��б�
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
                //����Ϸ����
                sender.TabItems.Remove(args.Tab);
            }
        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            if((sender as TabView).TabItems.Count == 0)
            {
                (sender as TabView).TabItems.Add(CreateNewTab());
                (sender as TabView).SelectedIndex = 0;
            }
#if TEST
            TabViewItem tab = new()
            {
                Header = "Minecraft",
                IconSource = new SymbolIconSource() { Symbol = Symbol.Send }
            };
            Frame f = new();
            f.Navigate(typeof(Game.MinecraftPage));
            tab.Content = f;

            (sender as TabView).TabItems.Add(tab);
#endif
        }

        private static TabViewItem CreateNewTab()
        {
            TabViewItem tab = new()
            {
                Header = "������Ϸ",
                IconSource = new SymbolIconSource() { Symbol = Symbol.Send }
            };
            Frame f = new();
            f.Navigate(typeof(Game.GameStartHomePage));
            tab.Content = f;
            (f.Content as GameStartHomePage).Flush();
            return tab;
        }
    }
}
