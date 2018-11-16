using System.IO;
using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace Drachenkatze.PresetMagician.NKSF
{
    public class Class1
    {
        public void test()
        {
            var fileStream2 = new FileStream(@"C:\Users\Drachenkatze\Desktop\PunchBox2.nksf", FileMode.Create);
            using (var fileStream = new FileStream(@"C:\Users\Drachenkatze\Desktop\foo2.nksf", FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);
                n.Write(fileStream2);
            }
        }
    }
}