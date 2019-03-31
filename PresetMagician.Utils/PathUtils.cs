using System.IO;

namespace PresetMagician.Utils
{
    public static class PathUtils
    {
        public static string SanitizeDirectory(string directory)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                directory = directory.Replace(c, '_');
            }

            return directory;
        }

        public static string SanitizeFilename(string filename)
        {

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }

            return filename;
        }
    }
}