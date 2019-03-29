namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class WzooBlock
    {
        public readonly byte[] DeviceType = new byte[12];
        public readonly byte[] ConfigType = new byte[4];
        public byte[] BlockData;
        public int BlockLength;
        public int BlockVersion;
    }
}