using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TagManagerPage : Page, IFrostPage
    {

        StorageFolder folder = null;

        HashSet<string> tags = new();

        Dictionary<string, HashSet<string>> scoreboards = new();

        //Dictionary<string, string> spell = new();

        public TagManagerPage()
        {
            this.InitializeComponent();
        }

        public void Flush()
        {
            if (FrostLeaf.Instance.settings.ResourceSettings.datapack != "")
            {
                readFromProject.IsEnabled = true;
            }
            readButton.IsEnabled = folder != null;
            logText.Text = folder == null ? "δѡ���ļ���" : folder.Path;
            outputButton.IsEnabled = folder != null;
        }

        //����Ŀ��ѡ��
        private async void readFromProject_Click(object sender, RoutedEventArgs e)
        {
            //ui
            ReadingPicTip.IsOpen = true;
            (sender as MenuFlyoutItem).IsEnabled = false;
            //��ȡ�ļ���
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(FrostLeaf.Instance.settings.ResourceSettings.Datapack);
            if (folder != null)
            {
                this.folder = folder;
                Flush();
            }
            //ui
            ReadingPicTip.IsOpen = false;
            (sender as MenuFlyoutItem).IsEnabled = true;
        }

        //���ļ�����ѡ��
        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            (sender as MenuFlyoutItem).IsEnabled = false;
            // Create a folder picker
            FolderPicker openPicker = new();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                this.folder = folder;
                Flush();
            }
            (sender as MenuFlyoutItem).IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //��ʼ��ȡ�ļ�
            result.Visibility = Visibility.Collapsed;
            outputButton.IsEnabled = false;
            readButton.IsEnabled = false;
            //tipText.Visibility = Visibility.Visible;
            //tipText.Text = "���ڶ�ȡ�ļ�: ";
            readProgressBar.Visibility = Visibility.Visible;
            tagPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
            //��ʼ�����ļ����е�ȫ���ļ�
            BackgroundWorker worker = new ()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += new ProgressChangedEventHandler((sender, e) =>
            {
                if(e.ProgressPercentage == 114514)
                {
                    tipText.Text = $"���ڶ�ȡ�ļ�: {e.UserState as string}";
                }
                else
                {
                    tipText.Text = $"���ڼ����ļ�: {e.UserState as string}";
                }
            });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( (sender, e)=> {
                //��ui����ʾ���
                tipText.Visibility = Visibility.Collapsed;
                readButton.IsEnabled = true;
                outputButton.IsEnabled = true;
                readProgressBar.Visibility = Visibility.Collapsed;
                result.RootNodes.Clear();
                //��
                result.RootNodes.Add(new TreeViewNode() { Content = "��ǩ" });
                //��ǩ���
                foreach (var i in tags.OrderBy(obj => obj)) 
                {
                    result.RootNodes[0].Children.Add(new TreeViewNode() { Content = i }); 
                }
                //�Ƿְ����
                result.RootNodes.Add(new TreeViewNode() { Content = "�Ƿְ�" });
                foreach(var kv in scoreboards.OrderBy(kv => kv.Key))
                {
                    var scb = new TreeViewNode() { Content = kv.Key };
                    foreach(var i in kv.Value.OrderBy(i => i))
                    {
                        scb.Children.Add(new() { Content = i });
                    }
                    result.RootNodes[1].Children.Add(scb);
                }
                result.Visibility = Visibility.Visible;
                tagPreview.HorizontalAlignment = HorizontalAlignment.Center;
            });
            worker.RunWorkerAsync(folder);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            walkFiles(sender as BackgroundWorker, e.Argument as StorageFolder);
        }

        private async void outputButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder outputString = new("");

            //дtag
            outputString.AppendLine("��ǩ: ");
            foreach (var tag in tags.OrderBy(t => t))
            {
                outputString.AppendLine("\t" + tag);
            }
            //�Ƿְ�
            outputString.AppendLine("�Ƿְ�:");
            foreach (var kv in scoreboards.OrderBy(t => t.Key))
            {
                outputString.AppendLine($"\t{kv.Key}");
                foreach (var sh in kv.Value.OrderBy(sh => sh))
                {
                    outputString.AppendLine($"\t\t{sh}");
                }
            }

            //����д��
            //��ȡ������
            FileSavePicker savePicker = new FileSavePicker();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);

            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("�ı��ļ�", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "output";
            StorageFile file = await savePicker.PickSaveFileAsync();

            if(file != null)
            {
                CachedFileManager.DeferUpdates(file);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var tw = new StreamWriter(stream))
                    {
                        tw.WriteLine(outputString.ToString());
                    }
                }
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == FileUpdateStatus.Complete)
                {
                    FrostLeaf.Instance.log.Success("�ѵ����ļ�" + file.Name);
                }
                else if (status == FileUpdateStatus.CompleteAndRenamed)
                {
                    FrostLeaf.Instance.log.Success("�ѵ����ļ�: " + file.Name);
                }
                else
                {
                    FrostLeaf.Instance.log.Error("δ�ܵ����ļ�: " + file.Name);
                }
            }
        }

        private async void walkFiles(BackgroundWorker sender, StorageFolder folder)
        {
            var items = folder.GetItemsAsync().AsTask().Result;

            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    if(file.FileType == ".mcfunction")
                    {
                        sender.ReportProgress(114514, file.Path);
                        await readFile(file);
                    }
                    else
                    {
                        sender.ReportProgress(1919810, file.Path);
                    }
                }
                else if (item is StorageFolder childFolder)
                {
                    // Folder found, traverse recursively
                    walkFiles(sender, childFolder);
                }
            }
        }

        private async Task readFile(StorageFile file)
        {
            //���ж�ȡ
            using (StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // ������������
                    // tag
                    // tag=xxx
                    var match = Regex.Match(line, @"tag=(?<tagName>.*?)(,|\])");
                    if (match.Success)
                    {
                        string tagValue = match.Groups["tagName"].Value;
                        if (tagValue.StartsWith("!"))
                        {
                            tagValue = tagValue[1..];
                        }
                        tags.Add(tagValue);
                        continue;
                    }

                    //TODO ������Tag:[]����������
                    //���Ƿְ�ͼǷ��ֻ���������
                    //scoreboard players set aaa bbb 1
                    var scMatch = Regex.Match(line, @"scoreboard players (set|add|remove|set) (?<score_holder>.*?) (?<scoreboard>.*?) ([0-9]*)");
                    if(!scMatch.Success)
                    {
                        scMatch = Regex.Match(line, @"scoreboard players (enable|get|reset) (?<score_holder>.*?) (?<scoreboard>.*)");
                    }
                    if (scMatch.Success)
                    {
                        string scValue = scMatch.Groups["scoreboard"].Value;
                        if (scoreboards.ContainsKey(scValue))
                        {
                            var qwq = scMatch.Groups["score_holder"].Value;
                            if (!qwq.StartsWith("@"))
                            {
                                scoreboards[scValue].Add(scMatch.Groups["score_holder"].Value);
                            }
                        }
                        else
                        {
                            scoreboards.Add(scValue, new HashSet<string>());
                        }
                        continue;
                    }

                    //scoreboard players operation target_score_holder target_objective += source_score_holder source_objective
                    var scMatch2 = Regex.Match(line, @"scoreboard players operation (?<target_score_holder>.*?) (?<target_objective>.*?) (?<op>.*?) (?<source_score_holder>.*?) (?<source_objective>.*)");
                    if (scMatch2.Success)
                    {
                        string scValue = scMatch2.Groups["target_objective"].Value;
                        if (!scoreboards.ContainsKey(scValue))
                        {
                            scoreboards.Add(scValue, new HashSet<string>());
                        }
                        var qwq = scMatch2.Groups["target_score_holder"].Value;
                        if (!qwq.StartsWith("@"))
                        {
                            scoreboards[scValue].Add(scMatch2.Groups["target_score_holder"].Value);
                        }

                        string scValue2 = scMatch2.Groups["source_objective"].Value;
                        if (!scoreboards.ContainsKey(scValue2))
                        {
                            scoreboards.Add(scValue2, new HashSet<string>());
                        }
                        var qwq2 = scMatch2.Groups["source_score_holder"].Value;
                        if (!qwq2.StartsWith("@"))
                        {
                            scoreboards[scValue].Add(scMatch2.Groups["source_score_holder"].Value);
                        }
                        continue;
                    }

                }
            }
        }
    }
}
