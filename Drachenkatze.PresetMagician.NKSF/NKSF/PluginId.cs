using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class PluginId
    {
        [Key("VST.magic")] public int VSTMagic;
    }
}