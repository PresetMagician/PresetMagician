using Drachenkatze.PresetMagician.NKSF.NKSF;
using GSF.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.NKSF
{
    public class Class1
    {
        public void test()
        {
            var fileStream2 = new FileStream(@"C:\Users\Drachenkatze\Desktop\PunchBox2.nksf", FileMode.Create);
            using (var fileStream = new FileStream(@"C:\Users\Drachenkatze\Desktop\foo.nksf", FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);
                n.Write(fileStream2);
            }
        }
    }
}