using System.IO;
using Squirrel.Shell;

namespace PresetMagician.Utils
{
    public class ShortcutUtils
    {
        public static bool IsShortcut(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

           
                var shellLink = new ShellLink(path);

                if (shellLink.Target.Length > 0 && Directory.Exists(shellLink.Target))
                {
                    shellLink.Dispose();
                    return true;
                }

                shellLink.Dispose();
            

            return false;
        }
        
        public static string ResolveShortcut(string path)
        {
            if (!ShortcutUtils.IsShortcut(path))
            {
                return string.Empty;
            }
            
            var shellLink = new ShellLink(path);
            string targetPath = null;

            if (shellLink.Target.Length > 0 && Directory.Exists(shellLink.Target))
            {
                targetPath= shellLink.Target;
            }

            shellLink.Dispose();

            return targetPath;
        }
    }
}