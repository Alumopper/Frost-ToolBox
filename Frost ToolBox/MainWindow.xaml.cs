using Frost_ToolBox.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Frost_ToolBox
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static Window mainWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Frost ToolBox";
            MainWindow.mainWindow = this;
        }

        private void toolNv_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Tag)
                {
                    case "PicScalePage":
                        {
                            contentFrame.Navigate(typeof(PicScalePage));
                            break;
                        }
                    case "TagManagerPage":
                        {
                            contentFrame.Navigate(typeof(TagManagerPage));
                            break;
                        }
                    case "HomePage":
                        {
                            contentFrame.Navigate(typeof(HomePage));
                            break;
                        }
                }
            }
        }
    }
}
