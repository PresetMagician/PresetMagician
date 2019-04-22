using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace PresetMagician.VendorPresetParser.Roland.Internal
{
    public class RolandConverter
    {
        private List<RolandValueStruct> ValueStructs;
        private RolandFileMemory FileMemory = new RolandFileMemory();
        private RolandExportConfig ExportConfig;

        public RolandConverter(RolandExportConfig exportConfig)
        {
            ExportConfig = exportConfig;
        }

        public void LoadDefinitionFromCsv(TextReader reader)
        {
            using (var csv = new CsvReader(reader))
            {
                ValueStructs = csv.GetRecords<RolandValueStruct>().ToList();
            }
        }

        public void LoadDefinitionFromCsvString(string data)
        {
            using (var csv = new StringReader(data))
            {
                LoadDefinitionFromCsv(csv);
            }
        }

        public void LoadDefinitionFromCsvFile(string file)
        {
            using (var csv = new StreamReader(file))
            {
                LoadDefinitionFromCsv(csv);
            }
        }

        public void SetFileMemory(byte[] data)
        {
            FileMemory.SetFileData(data);
        }

        private void PostProcess()
        {
            foreach (var postProcessor in ExportConfig.PostProcessors)
            {
                switch (postProcessor)
                {
                    case "SYSTEM-1.PATCH_LEVEL_SW":

                        var patchLevelSw = GetByName("fm.PATCH.EXTEND.PATCH LEVEL SW");

                        if (patchLevelSw.GetMemoryValue(FileMemory) == 0)
                        {
                            var patchLevel = GetByName("fm.PATCH.EXTEND.PATCH LEVEL");
                            patchLevel.SetMemoryValue(128);
                        }

                        break;
                }
            }
        }

        public RolandValueStruct GetByName(string valuePath)
        {
            foreach (var rvs in ValueStructs)
            {
                if (rvs.ValuePath == valuePath)
                {
                    return rvs;
                }
            }

            return null;
        }
        
        public int? GetMemoryValue(string valuePath)
        {
            var rvs = GetByName(valuePath);

            return rvs?.GetMemoryValue(FileMemory);
        }

        public byte[] Export()
        {
            PostProcess();
            using (var ms = new MemoryStream())
            {
                foreach (var rvs in ValueStructs)
                {
                    if (ExportConfig.ShouldExport(rvs.ValuePath))
                    {
                        var result = rvs.ConvertMemoryValue(FileMemory);

                        if (result.address != null && result.data != null)
                        {
                            ms.Write(result.address, 0, 4);
                            ms.Write(result.data, 0, 4);
                        }
                    }
                }

                ms.Write(ExportConfig.Suffix, 0, ExportConfig.Suffix.Length);

                return ms.ToArray();
            }
        }
    }
}