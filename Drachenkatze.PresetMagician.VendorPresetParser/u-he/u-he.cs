using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    
    abstract public class u_he: AbstractVendorPresetParser
    {
        public string getDataDirectory (string DataDirectoryName)
        {
            var vstPluginsPath = Path.GetDirectoryName(VstPlugin.PluginDLLPath);

            return Path.Combine(vstPluginsPath, DataDirectoryName);
        }

        [STAThread]
        public static bool IsShortcut(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string directory = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(directory);
            Shell32.FolderItem folderItem = folder.ParseName(file);

            if (folderItem != null)
            {
                return folderItem.IsLink;
            }

            return false;
        }

        [STAThread]
        public static string ResolveShortcut(string path)
        {
            if (IsShortcut(path))
            {
                string directory = Path.GetDirectoryName(path);
                string file = Path.GetFileName(path);

                Shell32.Shell shell = new Shell32.Shell();
                Shell32.Folder folder = shell.NameSpace(directory);
                Shell32.FolderItem folderItem = folder.ParseName(file);

                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }
            return string.Empty;
        }
    }
}
