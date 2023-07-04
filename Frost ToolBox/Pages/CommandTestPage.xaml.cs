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
                Title = "���������",
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                PrimaryButtonText = "����",
                CloseButtonText = "ȡ��",
                DefaultButton = ContentDialogButton.Close,
                IsPrimaryButtonEnabled = false
            };
            dialog.Content = new Dialog.ServerVersionPage(dialog);
            
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //����
                var info = (dialog.Content as Dialog.ServerVersionPage).downloadInfo;
                //���ص�·��
                //��Դ·���Ƿ���Ч
                if (!Directory.Exists(FrostLeaf.Instance.settings.ResourceFolder))
                {
                    FrostLeaf.Instance.log.Error("��������Դ·����Ч����������");
                }
                else
                {
                    var targetPath = FrostLeaf.Instance.settings.ResourceFolder + "\\.minecraftserver\\" + info.Id;
                    //�����ļ���
                    while (Directory.Exists(targetPath))
                    {
                        targetPath += "_";
                    }
                    Directory.CreateDirectory(targetPath);
                    targetPath += "\\server.jar";
                    
                    CancellationTokenSource cancellationTokenSource = new();
                    //��������ʾ�Ի���
                    ContentDialog dialog2 = new()
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "���������",
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        PrimaryButtonText = "ȡ��",
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
                            context.TextBlock.Text = "��ȡ���ص�ַ��";
                            string url;

                            using (var httpClient = new HttpClient())
                            {
                                //��ȡ���ص�ַ
                                var json = httpClient.GetStringAsync(info.Url).Result;
                                var versionManifest = JsonNode.Parse(json);
                                url = versionManifest["downloads"]["server"]["url"].ToString();
                            }

                            //UI
                            context.ProgressBar.IsIndeterminate = false;
                            context.ProgressBar.Value = 0;
                            context.TextBlock.Text = "׼����ʼ����";



                            //��ʼ���أ���ȡ��Ӧ
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
                                //��Ч��Ӧ
                                context.ProgressBar.ShowError = true;
                                //UI
                                context.TextBlock.Text = "���ش���";
                                return;
                            }

                            var totalRead = 0L;
                            var timer = Stopwatch.StartNew();   //�����ٶ�ʱ���¼
                            long totalMiliSeconds = 0;
                            long totalMiliSeconds2 = 0;
                            var buffer = new byte[5 * 1024];
                            var speed = 0.0;    //���ص��ٶ�
                            var isMoreToRead = true;
                            do
                            {
                                if (cancellationTokenSource.IsCancellationRequested)
                                {
                                    return;
                                }
                                //��ʼ��ȡ
                                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                                if (read == 0)
                                {
                                    //�����ȡ����
                                    isMoreToRead = false;
                                    await fileStream.FlushAsync();
                                    break;
                                }

                                //д��
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

                            //�����������
                            dialog2.PrimaryButtonText = null;
                            dialog2.CloseButtonText = "���";
                            context.TextBlock.Text = "�������";
                        }
                        catch(Exception ex)
                        {
                            context.ProgressBar.ShowError = true;
                            context.TextBlock.Text = "����ʱ�����˴���\n" + ex.Message;
                            dialog2.PrimaryButtonText = null;
                            dialog2.CloseButtonText = "ȷ��";
                        }
                        
                    };
                    var re = await dialog2.ShowAsync();

                    if(re == ContentDialogResult.Primary)
                    {
                        //ȡ������
                        // ɾ��δ������ɵ��ļ�
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
