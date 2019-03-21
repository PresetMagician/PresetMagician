using System.Collections.Generic;
using Anotar.Catel;
using Ceras;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public class BankFile : ModelBase
    {
        private static HashSet<string> _editableProperties = new HashSet<string>();

        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

        [Include] public string Path { get; set; }

        [Include] public string BankName { get; set; }

        [Include] public string ProgramRange { get; set; }

        public List<(int start, int length)> GetProgramRanges()
        {
            var ranges = new List<(int start, int length)>();

            if (ProgramRange == null)
            {
                return ranges;
            }

            var trimmedProgramRange = ProgramRange.Trim();

            if (trimmedProgramRange == "")
            {
                return ranges;
            }

            var rangesToParse = trimmedProgramRange.Split(',');

            foreach (var rangeToParse in rangesToParse)
            {
                var trimmedRangeToParse = rangeToParse.Trim();

                if (trimmedRangeToParse == "")
                {
                    continue;
                }

                if (!trimmedRangeToParse.Contains("-"))
                {
                    if (int.TryParse(trimmedRangeToParse, out var num))
                    {
                        ranges.Add((num, 1));
                    }
                    else
                    {
                        LogTo.Warning($"Cannot parse range specification {trimmedRangeToParse}, ignoring.");
                        continue;
                    }
                }
                else
                {
                    var rangeSpecs = trimmedRangeToParse.Split('-');
                    if (rangeSpecs.Length != 2)
                    {
                        LogTo.Warning($"Cannot parse range specification {trimmedRangeToParse}, ignoring.");
                        continue;
                    }

                    if (int.TryParse(rangeSpecs[0].Trim(), out var num1) &&
                        int.TryParse(rangeSpecs[1].Trim(), out var num2))
                    {
                        ranges.Add((num1, num2 - num1 + 1));
                    }
                    else
                    {
                        LogTo.Warning($"Cannot parse range specification {trimmedRangeToParse}, ignoring.");
                        continue;
                    }
                }
            }

            return ranges;
        }
    }
}