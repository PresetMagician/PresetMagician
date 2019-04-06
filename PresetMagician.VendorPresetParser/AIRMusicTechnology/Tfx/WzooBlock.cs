namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class WzooBlock
    {
        public readonly byte[] ConfigType = new byte[4];
        public readonly byte[] DeviceType = new byte[12];
        public byte[] BlockData;
        public int BlockLength;
        public int BlockVersion;
    }
}