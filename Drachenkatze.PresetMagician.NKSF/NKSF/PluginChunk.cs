using System.IO;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class PluginChunk : RIFFBase
    {
        public const string RiffTypeID = "PCHK";

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }
    }
}