using FrostLeaf_ToolBox.Pages;
using FrostLeaf_ToolBox;
using FrostLeaf_ToolBox.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static Window mainWindow;

        Dictionary<string,string> toolNames = new ()
        {
            { "标签管理","TagManagerPage" },
            { "图片缩放","PicScalePage" }
        };
        ObservableCollection<string> suggestions = new ObservableCollection<string>();

        public MainWindow()
        {
            this.InitializeComponent(); 
            this.Title = "FrostLeaf ToolBox";
            MainWindow.mainWindow = this;
        }

        private void toolNv_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if(args.IsSettingsSelected)
            {
                FrostLeaf.Instance.pages["SettingPage"].Value.Flush();
                contentFrame.Content = FrostLeaf.Instance.pages["SettingPage"].Value;
                return;
            }
            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem != null)
            {
                changePage(selectedItem.Tag as string);
            }
        }

        public void changePage(string page)
        {
            IFrostPage fpage = FrostLeaf.Instance.pages[page].Value;
            fpage.Flush();
            contentFrame.Content = fpage;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            suggestions.Clear();
            toolNames.Keys.ToList().ForEach(t =>
            {
                if (t.Contains(sender.Text))
                {
                    suggestions.Add(t);
                }
            });
            if(suggestions.Count == 0)
            {
                suggestions.Add("无结果");
            }
            sender.ItemsSource = suggestions;
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(toolNames.ContainsKey(sender.Text))
            {
                changePage(toolNames[sender.Text]);
            }
        }
    }
}
