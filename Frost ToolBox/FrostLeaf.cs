using FrostLeaf_ToolBox.Pages;
using FrostLeaf_ToolBox.Pages.Game;
using FrostLeaf_ToolBox.Utils;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.WebUI;

namespace FrostLeaf_ToolBox
{
    public class FrostLeaf
    {
        private static readonly FrostLeaf _instance = new ();

        public static FrostLeaf Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 设置
        /// </summary>
        public Settings settings = new ();

        public Dictionary<string, Lazy<IFrostPage>> pages = new();

        public Logger log;

        public List<string> serverVersionLists;

        private FileSystemWatcher serverWatcher;

        public string Version
        {
            get
            {
                return string.Format("版本: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision
                    );
            }
        }

        //public void DataInit()
        //{
        //    //初始化数据列表
        //    //获取服务器
        //}

        private FrostLeaf()
        {
            SettingsInit();
            //页面初始化
            pages["HomePage"] = new Lazy<IFrostPage>(() => new HomePage());
            pages["PicScalePage"] = new Lazy<IFrostPage>(() => new PicScalePage());
            pages["SettingPage"] = new Lazy<IFrostPage>(() => new SettingPage());
            pages["TagManagerPage"] = new Lazy<IFrostPage>(() => new TagManagerPage());
            pages["GameStartPage"] = new Lazy<IFrostPage>(() => new GameStartPage());
            pages["ResourcePackUnionPage"] = new Lazy<IFrostPage>(() => new ResourcePackUnionPage());
            pages["FunctionCallStackPage"] = new Lazy<IFrostPage>(() => new TODOPage());
            pages["MCMODEditorPage"] = new Lazy<IFrostPage>(() => new TODOPage());
            pages["CommandTestPage"] = new Lazy<IFrostPage>(() => new CommandTestPage());
            pages["TestPage"] = new Lazy<IFrostPage>(() => new TODOPage());
        }

        public List<string> GetServerVersions()
        {
            //服务器版本列表
            string path = settings.resourceFolder + "\\.minecraftserver";
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetDirectories().Select(x => x.Name).ToList();
            }
            return new();
        }

        public async void SettingsInit()
        {
            //读取设置json文件中的设置
            await Settings.Read(settings);
            //读取所有服务器版本信息
            serverVersionLists = GetServerVersions();
            //文件夹监视 
            serverWatcher = new FileSystemWatcher(settings.resourceFolder + "\\.minecraftserver")
            {
                NotifyFilter = NotifyFilters.DirectoryName
            };
            serverWatcher.Created += (sender, args) =>
            {
                serverVersionLists = GetServerVersions();
            };
            serverWatcher.Changed += (s, a) =>
            {
                serverVersionLists = GetServerVersions();
            };
            serverWatcher.Deleted += (s, a) =>
            {
                serverVersionLists = GetServerVersions();
            };
            serverWatcher.Renamed += (s, a) =>
            {
                serverVersionLists = GetServerVersions();
            };
        }
    }
}
