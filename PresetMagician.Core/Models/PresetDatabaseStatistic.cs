namespace PresetMagician.Core.Models
{
    public class PresetDatabaseStatistic
    {
        public string PluginName { get; set; }
        public int PresetCount { get; set; }
        public long PresetUncompressedSize { get; set; }
        public long PresetCompressedSize { get; set; }

        public double SavedSpace
        {
            get
            {
                if (PresetUncompressedSize != 0)
                {
                    return 1-PresetCompressedSize / (double) PresetUncompressedSize;
                }

                return 0;
            }
        }
    }
}