using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class VC2Writer
    {
        public static MemoryStream WriteVC2(string pluginData)
        {
            var ms = new MemoryStream();
            
                ms.Write(new byte[] { 0x56, 0x43, 0x32, 0x21 }, 0, 4);
                byte[] data = Encoding.UTF8.GetBytes(pluginData);
                ms.Write(LittleEndian.GetBytes(data.Length), 0, 4);
                ms.Write(data, 0, data.Length);
                ms.WriteByte(0);

            return ms;
        
        }
    }
}
