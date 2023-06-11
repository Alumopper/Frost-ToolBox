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
using FrostLeaf_ToolBox.Utils;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.Isolation;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages.Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MinecraftPage : Page
    {
        public bool autoScroll = true;
        
        public Minecraft minecraft;

        public List<LogContent> LogList;
        public List<LogContent> TransLogList;

        public short fliter = Severity.All;

        public MinecraftPage()
        {
            LogList = new();
            this.InitializeComponent();
        }

        public void GameLogGet(object o, ProgressChangedEventArgs args)
        {
            string log = args.UserState.ToString();
            var logContent = new LogContent(log, this);
            if (autoScroll)
            {
                //滚动至底部
                //scroll.ScrollToVerticalOffset(scroll.ActualHeight + scroll.ViewportHeight);
                scroll.ChangeView(null, scroll.ScrollableHeight, null);
            }
            LogList.Add(logContent);
            logList.Items.Add(logContent.grid);
        }

        public void GameStop(object o, RunWorkerCompletedEventArgs args)
        {

        }

        //日志筛选等级
        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            fliter = 0;
            //获取三个按钮的状态
            if (infoButton.IsChecked.Value) fliter = (short)(fliter | Severity.Info);
            if (warnButton.IsChecked.Value) fliter = (short)(fliter | Severity.Warn);
            if (errorButton.IsChecked.Value) fliter = (short)(fliter | Severity.Error);
            if (fatalButton.IsChecked.Value) fliter = (short)(fliter | Severity.Fatal);

            //原始日志
            foreach(LogContent log in LogList)
            {
                log.UpdateVisibility();
            }
        }

        //是否显示翻译后的日志
        private void logToggle_Toggled(object sender, RoutedEventArgs e)
        {

        }

        private void scrollToBottom_Toggled(object sender, RoutedEventArgs e)
        {
            autoScroll = scrollToBottom.IsOn;
        }

        private void scroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            autoScroll = false;
            scrollToBottom.IsOn = false;
        }
    }
}
