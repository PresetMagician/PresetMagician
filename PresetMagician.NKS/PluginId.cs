using MessagePack;

namespace PresetMagician.NKS
{
    [MessagePackObject]
    public class PluginId
    {
        [Key("VST.magic")] public int VSTMagic;
    }
}