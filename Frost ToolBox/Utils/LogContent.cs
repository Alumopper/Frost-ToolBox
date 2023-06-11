using FrostLeaf_ToolBox.Pages.Game;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrostLeaf_ToolBox.Utils
{
    public class LogContent
    {
        public static readonly SolidColorBrush infoBrush = new(Colors.White) {Opacity = 0.1};
        public static readonly SolidColorBrush infoForeBrush = new(Colors.Black);
        public static readonly SolidColorBrush warnBrush = new(Colors.Orange) {Opacity = 0.1 };
        public static readonly SolidColorBrush warnForeBrush = new(Colors.DarkOrange);
        public static readonly SolidColorBrush errorBrush = new (Colors.Red) { Opacity = 0.1 };
        public static readonly SolidColorBrush errorForeBrush = new(Colors.DarkRed);

        public static LogContent lastSeverity;

        public SolidColorBrush severityBrush;

        public SolidColorBrush severityForeBrush;

        public readonly string message;

        public readonly string source;

        public Grid grid;

        public Visibility visibility = Visibility.Visible;
        public Visibility Visibility 
        { 
            get => visibility; 
            set{
                visibility = value;
                grid.Visibility = value;
            } 
        }

        private readonly MinecraftPage page;

        public readonly short severity;

        public List<LogContent> description;
        
        public LogContent(string source, MinecraftPage page)
        {
            //eg: [23:15:31] [main/INFO]: Loading tweak class name optifine.OptiFineTweaker
            //severity: INFO
            //message: Loading tweak class name optifine.OptiFineTweaker
            this.source = source;
            this.page = page;
            //是否满足基本日志格式
            if (!Regex.Match(source, "^\\[[0-9]{2}:[0-9]{2}:[0-9]{2}\\] \\[.*?/(INFO|WARN|ERROR|FATAL)\\]: .*$").Success)
            {
                //不是基本格式，跟随上一个日志的内容
                if (lastSeverity == null)
                {
                    this.severity = Severity.Info;
                }
                else
                {
                    this.severity = lastSeverity.severity;
                    lastSeverity.description.Add(this);
                }
                this.message = source;
            }
            else
            {
                string delimiter = "]";
                string[] substrings = source.Split(delimiter, 3);
                this.severity = Severity.GetValueFromString(substrings[1].Split('/').Last());
                this.message = substrings[2][..2];
                description = new();
                lastSeverity = this;
            }
            SetColors(severity);
            grid = new()
            {
                Visibility = visibility,
                Background = severityBrush,
                Padding = new Thickness(1)
            };
            grid.Children.Add(new TextBlock()
            {
                Text = source,
                Foreground = severityForeBrush,
                TextWrapping = TextWrapping.Wrap
            });
            UpdateVisibility();
        }

        public override string ToString()
        {
            return source;
        }

        private void SetColors(short severity)
        {
            switch (severity)
            {
                case Severity.Info:
                    severityBrush = infoBrush;
                    severityForeBrush = infoForeBrush;
                    break;
                case Severity.Warn:
                    severityBrush = warnBrush;
                    severityForeBrush = warnForeBrush;
                    break;
                case Severity.Error:
                case Severity.Fatal:
                    severityBrush = errorBrush;
                    severityForeBrush = errorForeBrush;
                    break;
            }
        }

        public void UpdateVisibility()
        {
            if ((severity & page.fliter) == 0)
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                Visibility = Visibility.Visible;
            }
        }

    }
    
    public readonly struct Severity
    {
        public const short Info = 1;
        public const short Warn = 1 << 1;
        public const short Error = 1 << 2;
        public const short Fatal = 1 << 3;

        public const short All = Info | Warn | Error | Fatal;

        public static short GetValueFromString(string value)
        {
            return value.ToLower() switch
            {
                "info" => Info,
                "warn" => Warn,
                "error" => Error,
                "fatal" => Fatal,
                _ => Error
            };
        }
    }
}
