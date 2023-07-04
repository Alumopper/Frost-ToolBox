using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FrostLeaf_ToolBox.Pages
{
    public sealed partial class CommandTestPage : Page, IFrostPage
    {

        public CommandTestPage()
        {
            this.InitializeComponent();
        }

        public void Flush()
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openServerVersionDownloadWindow();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private async void openServerVersionDownloadWindow()
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Title = "服务端下载",
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                PrimaryButtonText = "下载",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close,
                IsPrimaryButtonEnabled = false
            };
            dialog.Content = new Dialog.ServerVersionPage(dialog);
            
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //下载
                var info = (dialog.Content as Dialog.ServerVersionPage).downloadInfo;
                //下载的路径
                //资源路径是否有效
                if (!Directory.Exists(FrostLeaf.Instance.settings.ResourceFolder))
                {
                    FrostLeaf.Instance.log.Error("工具箱资源路径无效，请检查设置");
                }
                else
                {
                    var targetPath = FrostLeaf.Instance.settings.ResourceFolder + "\\.minecraftserver\\" + info.Id;
                    //创建文件夹
                    while (Directory.Exists(targetPath))
                    {
                        targetPath += "_";
                    }
                    Directory.CreateDirectory(targetPath);
                    targetPath += "\\server.jar";
                    
                    CancellationTokenSource cancellationTokenSource = new();
                    //打开下载提示对话框
                    ContentDialog dialog2 = new()
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "服务端下载",
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        PrimaryButtonText = "取消",
                        IsPrimaryButtonEnabled = true
                    };
                    var context = new Dialog.ProgressDisplayDialog(dialog2);
                    dialog2.Content = context;

                    dialog2.PrimaryButtonClick += (s, args) =>
                    {
                        cancellationTokenSource.Cancel();
                    };
                    
                    dialog2.Opened += async (sender, e) =>
                    {
                        try
                        {
                            context.ProgressBar.IsIndeterminate = true;
                            context.TextBlock.Text = "获取下载地址中";
                            string url;

                            using (var httpClient = new HttpClient())
                            {
                                //获取下载地址
                                var json = httpClient.GetStringAsync(info.Url).Result;
                                var versionManifest = JsonNode.Parse(json);
                                url = versionManifest["downloads"]["server"]["url"].ToString();
                            }

                            //UI
                            context.ProgressBar.IsIndeterminate = false;
                            context.ProgressBar.Value = 0;
                            context.TextBlock.Text = "准备开始下载";



                            //开始下载，获取响应
                            HttpClient client = new()
                            {
                                Timeout = TimeSpan.FromSeconds(10)
                            };
                            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token);
                            using var stream = await response.Content.ReadAsStreamAsync();
                            using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 8128, true);
                            var totalSize = response.Content.Headers.ContentLength ?? -1L;

                            if (totalSize == -1)
                            {
                                //无效响应
                                context.ProgressBar.ShowError = true;
                                //UI
                                context.TextBlock.Text = "下载错误";
                                return;
                            }

                            var totalRead = 0L;
                            var timer = Stopwatch.StartNew();   //下载速度时间记录
                            long totalMiliSeconds = 0;
                            long totalMiliSeconds2 = 0;
                            var buffer = new byte[5 * 1024];
                            var speed = 0.0;    //下载的速度
                            var isMoreToRead = true;
                            do
                            {
                                if (cancellationTokenSource.IsCancellationRequested)
                                {
                                    return;
                                }
                                //开始读取
                                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                                if (read == 0)
                                {
                                    //如果读取完了
                                    isMoreToRead = false;
                                    await fileStream.FlushAsync();
                                    break;
                                }

                                //写入
                                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                                if (timer.ElapsedMilliseconds - totalMiliSeconds2 > 300) // Only update UI every 64KB of data downloaded
                                {
                                    totalMiliSeconds2 = timer.ElapsedMilliseconds;
                                    speed = read / ((timer.ElapsedMilliseconds - totalMiliSeconds) / 1000.0);
                                    context.TextBlock.Text = $"{GetSpeedStr(speed)}\t\t\t{totalRead / 1024.0 / 1024.0:0.00}/{totalSize / 1024.0 / 1024.0:0.00}MB";
                                    var progress = ((double)totalRead) / totalSize * 100;
                                    context.ProgressBar.Value = progress;
                                }
                                totalRead += read;
                                totalMiliSeconds = timer.ElapsedMilliseconds;

                            } while (isMoreToRead);

                            //完成下载任务
                            dialog2.PrimaryButtonText = null;
                            dialog2.CloseButtonText = "完成";
                            context.TextBlock.Text = "下载完成";
                        }
                        catch(Exception ex)
                        {
                            context.ProgressBar.ShowError = true;
                            context.TextBlock.Text = "下载时遇到了错误！\n" + ex.Message;
                            dialog2.PrimaryButtonText = null;
                            dialog2.CloseButtonText = "确认";
                        }
                        
                    };
                    var re = await dialog2.ShowAsync();

                    if(re == ContentDialogResult.Primary)
                    {
                        //取消下载
                        // 删除未下载完成的文件
                        var qwq = new FileInfo(targetPath);
                        if (qwq.Exists)
                        {
                            qwq.Directory.Delete(true);
                        }
                    }
                }
            }
        }

        private string GetSpeedStr(double byteSpeed)
        {
            if(byteSpeed > 1024*1024)
            {
                return $"{byteSpeed / 1024 / 1024: 0.00}MB/s";
            }
            else if (byteSpeed > 1024)
            {
                return $"{byteSpeed / 1024: 0.00}KB/s";
            }
            else
            {
                return $"{byteSpeed: 0.00}B/s";
            }
        }
    }
}
