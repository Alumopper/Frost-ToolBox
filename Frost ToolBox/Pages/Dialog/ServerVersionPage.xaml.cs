using CommunityToolkit.WinUI.UI;
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web.AtomPub;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages.Dialog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerVersionPage : Page, IFrostPage
    {
        List<Version> releaseVersions;
        List<Version> snapshotVersions;
        ContentDialog dialog;
        public Version downloadInfo;

        public ServerVersionPage(ContentDialog dialog)
        {
            this.InitializeComponent();
            this.dialog = dialog;
            getVersions();
        }

        public void Flush()
        {
        }

        public void getVersions()
        {
            BackgroundWorker worker = new();
            worker.DoWork += new(delegate (object sender, DoWorkEventArgs args)
            {
                //获取服务端版本
                using (var httpClient = new HttpClient())
                {
                    var json = httpClient.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json").Result;
                    var versionManifest = JsonConvert.DeserializeObject<VersionManifest>(json);
                    args.Result = versionManifest;
                }
            });
            worker.RunWorkerCompleted += new(delegate(object sender, RunWorkerCompletedEventArgs args)
            {
                //按照类型对版本分类
                VersionManifest versionManifest = (VersionManifest)args.Result;
                snapshotVersions = versionManifest.Versions.Where(v => v.Type == "snapshot" && v.ReleaseTime >= DateTime.Parse("2019-04-23T14:52:44+00:00")).ToList();
                releaseVersions = versionManifest.Versions.Where(v => v.Type == "release" && v.ReleaseTime >= DateTime.Parse("2019-04-23T14:52:44+00:00")).ToList();
                //ui
                LoadingInfo.Visibility = Visibility.Collapsed;
                VersionList.Visibility = Visibility.Visible;
                VersionList.Height = canvas.ActualHeight - 330;
                //把版本写入列表(默认显示正式版)
                VersionList.ItemsSource = releaseVersions;
            });
            worker.RunWorkerAsync();
        }

        public class VersionManifest
        {
            public Version[] Versions { get; set; }
        }

        public class Version
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public DateTime Time { get; set; }
            public DateTime ReleaseTime { get; set; }
        }

        private void VersionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListView)sender;
            if(list.SelectedItem != null)
            {
                dialog.IsPrimaryButtonEnabled = true;
                downloadInfo = list.SelectedItem as Version;
            }
            //提示版本
            while (FrostLeaf.Instance.serverVersionLists.Contains(downloadInfo.Id))
            {
                downloadInfo.Id += '_';
            }
            versionNameTextBox.Text = downloadInfo.Id;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            versionButton.Content = "正式版";
            VersionList.ItemsSource = releaseVersions;
        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            versionButton.Content = "快照版";
            VersionList.ItemsSource = snapshotVersions;
        }

        private void versionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(downloadInfo == null)
            {
                return;
            }
            //发生更改
            if (FrostLeaf.Instance.serverVersionLists.Contains(versionNameTextBox.Text))
            {
                //重复
                versionNameTextBox.Header = "版本名重复，请换一个";
                dialog.IsPrimaryButtonEnabled = false;
                return;
            }
            downloadInfo.Id = versionNameTextBox.Text;
            versionNameTextBox.Header = null;
            dialog.IsPrimaryButtonEnabled = true;
        }
    }
}
