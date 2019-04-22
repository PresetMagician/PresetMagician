namespace PresetMagician.VendorPresetParser.Roland.Internal
{
    public class KoaPreset
    {
        public string PresetName { get; set; }
        public string BankFile { get; set; }
        public int Index { get; set; }
        public byte[] PresetData { get; set; }
    }
}