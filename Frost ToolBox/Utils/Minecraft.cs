using FrostLeaf_ToolBox.Pages.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FrostLeaf_ToolBox.Utils
{
    /// <summary>
    /// 一个运行中的Minecraft实例
    /// </summary>
    public class Minecraft
    {
        public string Name;

        Process process;

        BackgroundWorker worker;

        public MinecraftPage rootPage;

        public Minecraft(StorageFile bat, MinecraftPage rootPage)
        {
            this.rootPage = rootPage;
            worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += StartGameProcess;
            worker.ProgressChanged += rootPage.GameLogGet;
            worker.RunWorkerCompleted += rootPage.GameStop;
            process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = bat.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
        }
        
        public void Start()
        {
            worker.RunWorkerAsync();
        }

        
        public void Stop()
        {
            process.Kill();
        }
        
        private void StartGameProcess(object sender, DoWorkEventArgs e)
        {
            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += new(delegate (object o, DataReceivedEventArgs outline)
            {
                if (!string.IsNullOrEmpty(outline.Data))
                {
                    //返回获取的输出
                    worker.ReportProgress(0, outline.Data);
                }
            });
            process.WaitForExit();
        }

    }
}
