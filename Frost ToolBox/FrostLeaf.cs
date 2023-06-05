using FrostLeaf_ToolBox.Pages;
using FrostLeaf_ToolBox.Pages;
using FrostLeaf_ToolBox.Utils;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private FrostLeaf()
        {
            //读取设置json文件中的设置
            Settings.Read(settings);
            pages["HomePage"] = new Lazy<IFrostPage>(() => new HomePage());
            pages["PicScalePage"] = new Lazy<IFrostPage>(() => new PicScalePage());
            pages["SettingPage"] = new Lazy<IFrostPage>(() => new SettingPage());
            pages["TagManagerPage"] = new Lazy<IFrostPage>(() => new TagManagerPage());
        }
    }
}
