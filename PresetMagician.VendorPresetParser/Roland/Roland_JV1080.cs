using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Properties;
using PresetMagician.VendorPresetParser.Roland.Internal;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_JV1080 : RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449423665};

        protected override string GetProductName()
        {
            return "JV-1080";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_JV_1080_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_JV_1080_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_JV_1080_Script;
        }

        protected override void PostProcessPreset(PresetParserMetadata metadata, RolandConverter converter)
        {
            string typeName = null;
            string subTypeName = null;
            string characteristicName = null;
            switch (converter.GetMemoryValue("fm.pat.com.patCategory"))
            {
                case 1: // AC.PIANO
                    typeName = "Piano / Keys";
                    characteristicName = "Acoustic";
                    break;
                case 2: // EL.PIANO
                    typeName = "Piano / Keys";
                    subTypeName = "Electric Piano";
                    break;
                case 3: // KEYBOARDS
                    typeName = "Piano / Keys";
                    break;
                case 4: // BELL
                    typeName = "Mallet Instruments";
                    subTypeName = "Bell";
                    break;
                case 5: // MALLET
                    typeName = "Mallet Instruments";
                    break;
                case 6: // ORGAN
                    typeName = "Organ";
                    break;
                case 7: // ACCORDION
                    typeName = "Organ";
                    subTypeName = "Accordion";
                    break;
                case 8: // HARMONICA
                    typeName = "Reed Instruments";
                    subTypeName = "Harmonica";
                    break;
                case 9: // AC.GUITAR
                    typeName = "Guitar";
                    subTypeName = "Acoustic";
                    characteristicName = "Acoustic";
                    break;
                case 10: // EL.GUITAR
                    typeName = "Guitar";
                    subTypeName = "Electric";
                    characteristicName = "Electric";
                    break;
                case 11: // DIST.GUITAR
                    typeName = "Guitar";
                    subTypeName = "Electric";
                    characteristicName = "Distorted";
                    break;
                case 12: // BASS
                    typeName = "Bass";
                    break;
                case 13: // SYNTH BASS
                    typeName = "Synth Bass";
                    break;
                case 14: // STRINGS
                    typeName = "Bowed Strings";
                    break;
                case 15: // ORCHESTRA
                    typeName = "Gerne";
                    subTypeName = "Orchestral";
                    break;
                case 16: // HIT&STAB
                    characteristicName = "Stabs & Hits";
                    break;
                case 17: // WIND
                    typeName = "Ethnic World";
                    subTypeName = "Flutes & Wind";
                    break;
                case 18: // FLUTE
                    typeName = "Flute";
                    break;
                case 19: // AC.BRASS
                    typeName = "Brass";
                    break;
                case 20: // SYNTH BRASS
                    typeName = "Brass";
                    subTypeName = "Synth";
                    characteristicName = "Synthetic";
                    break;
                case 21: // SAX
                    typeName = "Reed Instruments";
                    subTypeName = "Saxophone";
                    break;
                case 22: // HARD LEAD
                    typeName = "Synth Lead";
                    characteristicName = "Hard";
                    break;
                case 23: // SOFT LEAD
                    typeName = "Synth Lead";
                    characteristicName = "Soft / Warm";
                    break;
                case 24: // TECHNO SYNTH
                    typeName = "Genre";
                    subTypeName = "Techno";
                    break;
                case 25: // PULSATING
                    typeName = "Arp/Sequence";
                    subTypeName = "Pulsing";
                    break;
                case 26: // SYNTH FX
                    typeName = "Synth Misc";
                    subTypeName = "FX";
                    break;
                case 27: // OTHER SYNTH
                    typeName = "Synth Misc";
                    break;
                case 28: // BRIGHT PAD
                    typeName = "Synth Pad";
                    characteristicName = "Bright";
                    break;
                case 29: // SOFT PAD
                    typeName = "Synth Pad";
                    characteristicName = "Soft / Warm";
                    break;
                case 30: // VOX
                    characteristicName = "Vox";
                    break;
                case 31: // PLUCKED
                    characteristicName = "Pluck";
                    break;
                case 32: // ETHNIC
                    typeName = "Ethnic World";
                    break;
                case 33: // FRETTED
                    break;
                case 34: // PERCUSSION
                    typeName = "Percussion";
                    break;
                case 35: // SOUND FX
                    typeName = "Sound Effects";
                    break;
                case 36: // BEAT&GROOVE
                    break;
                case 37: // DRUMS
                    typeName = "Drums";
                    break;
                case 38: // COMBINATION
                    typeName = "Combination";
                    break;
            }

            if (typeName != null)
            {
                metadata.Types.Add(new Type() {TypeName = typeName, SubTypeName = subTypeName});
            }

            if (characteristicName != null)
            {
                metadata.Characteristics.Add(new Characteristic() {CharacteristicName = characteristicName});
            }
        }
    }
}