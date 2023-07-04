using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages.Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResourcePackUnionPage : Page, IFrostPage
    {
        public ResourcePackUnionPage()
        {
            this.InitializeComponent();
        }

        //从文件夹中选择资源包
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {

        }

        void IFrostPage.Flush()
        {
        }
    }
}
