using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FrostLeaf_ToolBox.Utils
{
    public class Settings
    {
        public List<string> shortcuts;
        public List<string> Shortcuts { get => shortcuts; set => shortcuts = value; }
        public ResourceSettings resourceSettings;
        public ResourceSettings ResourceSettings { get => resourceSettings; set => resourceSettings = value; }
        public string resourceFolder;
        public string ResourceFolder { get => resourceFolder; set => resourceFolder = value; }
        
        public Settings()
        {
            shortcuts = new List<string>();
            resourceSettings = new ResourceSettings();
            resourceFolder = "";
        }

        public async static Task<int> Read(Settings settings)
        {
            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Settings/settings.json"));
            //读取设置json文件中的设置
            using StreamReader stream = new(await f.OpenStreamForReadAsync());
            string json = await stream.ReadToEndAsync();
            Settings wf = JsonConvert.DeserializeObject<Settings>(json);
            settings.Shortcuts = wf.Shortcuts;
            settings.ResourceSettings = wf.ResourceSettings;
            settings.resourceFolder = wf.resourceFolder;
            return 0;
        }

        public async static void Write(Settings settings)
        {
            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Settings/settings.json"));
            //读取设置json文件中的设置
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await FileIO.WriteTextAsync(f, json);
        }
    }

    public class ResourceSettings
    {
        public string datapack;
        public string Datapack { get => datapack; set => datapack = value; }
        public string resourcepack;
        public string Resourcepack { get => resourcepack; set => resourcepack = value; }
        public List<string> textureFolders;
        public List<string> TextureFolders { get => textureFolders; set => textureFolders = value; }


        public ResourceSettings() 
        {
            datapack = "";
            resourcepack = "";
            textureFolders = new List<string>();
        }
    }
}
