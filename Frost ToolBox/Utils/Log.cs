using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostLeaf_ToolBox.Utils
{
    public class Log
    {
        InfoBar infoBar;

        public Log(InfoBar infoBar)
        {
            this.infoBar = infoBar;
        }

        public void Success(string title = "", string message = "")
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = InfoBarSeverity.Success;
            infoBar.IsOpen = true;
        }

        public void Error(string title = "", string message = "")
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = InfoBarSeverity.Error;
            infoBar.IsOpen = true;
        }

        public void Warning(string title = "", string message = "")
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = InfoBarSeverity.Warning;
            infoBar.IsOpen = true;
        }

        public void Info(string title = "", string message = "")
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = InfoBarSeverity.Informational;
            infoBar.IsOpen = true;
        }

        public void Exception(Exception e)
        {
            infoBar.Title = e.GetType().Name;
            infoBar.Message = e.Message;
            infoBar.Severity = InfoBarSeverity.Error;
            infoBar.IsOpen = true;
        }
    }
}
