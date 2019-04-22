using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSF;
using PresetMagician.DevTools.Vendors.Roland;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandPatchDump
    {
        public static List<string> ComparePatchDumps(byte[] master, byte[] compareData, RolandTestConfig testConfig, RolandScript rolandScript)
        {
            var errors = new List<string>();
            var masterMemoryMap = GetMemoryMap(master);
            var compareMemoryMap = GetMemoryMap(compareData);

            var masterAddresses = masterMemoryMap.Keys.ToList();
            masterAddresses.Sort();
            foreach (var addr in masterAddresses)
            {
                if (!compareMemoryMap.ContainsKey(addr))
                {
                    var result = rolandScript.FindClosestToAddress(addr);
                    var valuePath = result.closestSection.ValuePath;
                    if (!testConfig.ShouldIgnoreValuePath(valuePath))
                    {
                        errors.Add(
                            $"Compare subject does not contain address 0x{addr:x8} ({result.closestSection.ValuePath})");
                    }

                    continue;
                }

                if (compareMemoryMap[addr] != masterMemoryMap[addr])
                {
                   
                    var result = rolandScript.FindClosestToAddress(addr);
                    var valuePath = result.closestSection.ValuePath;
                    if (!testConfig.ShouldIgnoreValuePath(valuePath))
                    {
                        
                        if (testConfig.ComparisonAllowValueVariance.ContainsKey(valuePath))
                        {
                            var allowedVariance = testConfig.ComparisonAllowValueVariance[valuePath];
                            var minMaster = masterMemoryMap[addr] - allowedVariance;
                            var maxMaster = masterMemoryMap[addr] + allowedVariance;

                            if (compareMemoryMap[addr] <= minMaster && compareMemoryMap[addr] >= maxMaster)
                            {
                                errors.Add(
                                    $"Compare subject value at address 0x{addr:x8} ({result.closestSection.ValuePath}) with value 0x{compareMemoryMap[addr]:x8} does not equal master value 0x{masterMemoryMap[addr]:x8}. Allowed variance {allowedVariance}"); 
                            }

                        }
                        else
                        {
                            errors.Add(
                                $"Compare subject value at address 0x{addr:x8} ({result.closestSection.ValuePath}) with value 0x{compareMemoryMap[addr]:x8} does not equal master value 0x{masterMemoryMap[addr]:x8}");
                        }
                    }

                    continue;
                }
            }
            
            var compareAddresses = compareMemoryMap.Keys.Except(masterAddresses).ToList();
            compareAddresses.Sort();

            foreach (var addr in compareAddresses)
            {
                var result = rolandScript.FindClosestToAddress(addr);
                var valuePath = result.closestSection.ValuePath;
                if (!testConfig.ShouldIgnoreValuePath(valuePath))
                {
                    errors.Add(
                        $"Compare subject contains address 0x{addr:x8} ({result.closestSection.ValuePath}) but it shouldn't");
                }
            }
                

            return errors;
        }

        public static Dictionary<int, int> GetMemoryMap(byte[] data)
        {
            var memMap = new Dictionary<int, int>();
            using (var ms = new MemoryStream(data))
            {
                while (ms.Position != ms.Length)
                {
                    var buffer = new byte[4];

                    ms.Read(buffer, 0, 4);


                    var address = BigEndian.ToInt32(buffer, 0);
                    ms.Read(buffer, 0, 4);


                    var value = BigEndian.ToInt32(buffer, 0);
                    memMap.Add(address, value);
                }
            }

            return memMap;
        }
        
        public static byte[] SortDump(byte[] data)
        {
            var memMap = new Dictionary<int, int>();
            using (var ms = new MemoryStream(data))
            {
                while (ms.Position != ms.Length)
                {
                    var buffer = new byte[4];

                    ms.Read(buffer, 0, 4);


                    var address = BigEndian.ToInt32(buffer, 0);
                    ms.Read(buffer, 0, 4);


                    var value = BigEndian.ToInt32(buffer, 0);
                    memMap.Add(address, value);
                }
            }

            using (var ms = new MemoryStream())
            {
                var keys = memMap.Keys.ToList();
                keys.Sort();
                foreach (var dings in keys)
                {
                    ms.Write(BigEndian.GetBytes(dings), 0, 4);
                    ms.Write(BigEndian.GetBytes(memMap[dings]), 0, 4);
                }

                return ms.ToArray();
            }
        }
    }
}