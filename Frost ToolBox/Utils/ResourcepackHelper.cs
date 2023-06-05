using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FrostLeaf_ToolBox.Utils
{
    public class ResourcepackHelper
    {
        public static async Task<List<string>> GetTextureFolders(StorageFolder folder)
        {
            List<string> result = new List<string>();
            foreach (var item in await folder.GetFoldersAsync())
            {
                if (item.Name == "textures")
                {
                    result.Add(item.Path);
                }
                else
                {
                    result = result.Concat(await GetTextureFolders(item)).ToList();
                }
            }
            return result;
        }
    }
}
