using System;
using System.Globalization;
using System.IO;
using System.Linq;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxHybrid3 : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x00, 0x00, 0x1f, 0xd6};


        public override void PostProcess()
        {
            // replace filename in metadata

            var ms2 = new MemoryStream(WzooBlock.BlockData);

            if (WzooBlock.BlockData.Length == 6584)
            {
                ms2.Seek(5556, SeekOrigin.Begin);

                var pos = ms2.Position;

                for (var i = pos; i < pos + 1024; i++)
                {
                    WzooBlock.BlockData[i] = 0;
                }

                for (var i = 0; i < PatchName.Length; i++)
                {
                    WzooBlock.BlockData[pos + i] = PatchName[i];
                }
            }
            else if (WzooBlock.BlockData.Length == 5080)
            {
                Parameters[15] = MigratePitchModSource(Parameters[15]);
                Parameters[122] = MigratePitchModSource(Parameters[122]);
                Parameters[123] = MigratePitchModSource(Parameters[123]);
                Parameters[124] = MigratePitchModSource(Parameters[124]);
                Parameters[137] = MigratePitchModSource(Parameters[137]);
                Parameters[138] = MigratePitchModSource(Parameters[138]);
                Parameters[139] = MigratePitchModSource(Parameters[139]);
                Parameters[151] = MigratePitchModSource(Parameters[151]);
                Parameters[152] = MigratePitchModSource(Parameters[152]);
                Parameters[153] = MigratePitchModSource(Parameters[153]);
                Parameters[168] = MigratePitchModSource(Parameters[168]);
                Parameters[169] = MigratePitchModSource(Parameters[169]);
                Parameters[170] = MigratePitchModSource(Parameters[170]);
                Parameters[179] = MigratePitchModSource(Parameters[179]);
                Parameters[180] = MigratePitchModSource(Parameters[180]);
                Parameters[181] = MigratePitchModSource(Parameters[181]);
                Parameters[239] = MigratePitchModSource(Parameters[239]);
                Parameters[245] = MigratePitchModSource(Parameters[245]);
                Parameters[252] = MigratePitchModSource(Parameters[252]);

                Parameters.Insert(291, 0);
                Parameters.Insert(292, 0);
                Parameters.Insert(293, 0.7);
                Parameters.Insert(294, 0.5);
                Parameters.Insert(295, 0.5);
                Parameters.Insert(296, 0.5);
                Parameters.Insert(297, 0);
                Parameters.Insert(298, 0);
                Parameters.Insert(299, 0);
                Parameters.Insert(300, 1);
                Parameters.Insert(301, 0);
                Parameters.Insert(302, 0);
                Parameters.Insert(303, 1);
                Parameters.Insert(304, 0);
                Parameters.Insert(305, 0);
                Parameters.Insert(306, 0);
                Parameters.Insert(307, 0.2);
                Parameters.Insert(308, 0.4);
                Parameters.Insert(309, 0);
                Parameters.Insert(310, 0);
                Parameters.Insert(311, 0);
                Parameters.Insert(312, 0.5);
                Parameters.Insert(313, 0.5);
                Parameters.Insert(314, 0.5);
                Parameters.Insert(315, 0.5);
                Parameters.Insert(316, 0.5);
                Parameters.Insert(317, 0.5);
                Parameters.Insert(318, 0.3);
                Parameters.Insert(319, 0.05);
                Parameters.Insert(320, 0.1);
                Parameters[321] = 0.6;
                Parameters[322] = 0.1;
                Parameters[323] = 0.5;

                Parameters[334] = MigratePitchModSource(Parameters[334]);
                Parameters[441] = MigratePitchModSource(Parameters[441]);
                Parameters[442] = MigratePitchModSource(Parameters[442]);
                Parameters[443] = MigratePitchModSource(Parameters[443]);
                Parameters[456] = MigratePitchModSource(Parameters[456]);
                Parameters[457] = MigratePitchModSource(Parameters[457]);
                Parameters[458] = MigratePitchModSource(Parameters[458]);
                Parameters[470] = MigratePitchModSource(Parameters[470]);
                Parameters[471] = MigratePitchModSource(Parameters[471]);
                Parameters[472] = MigratePitchModSource(Parameters[472]);
                Parameters[487] = MigratePitchModSource(Parameters[487]);
                Parameters[488] = MigratePitchModSource(Parameters[488]);
                Parameters[489] = MigratePitchModSource(Parameters[489]);
                Parameters[498] = MigratePitchModSource(Parameters[498]);
                Parameters[499] = MigratePitchModSource(Parameters[499]);
                Parameters[500] = MigratePitchModSource(Parameters[500]);
                Parameters[558] = MigratePitchModSource(Parameters[558]);
                Parameters[564] = MigratePitchModSource(Parameters[564]);
                Parameters[571] = MigratePitchModSource(Parameters[571]);

                Parameters.Insert(610, 0);
                Parameters.Insert(611, 0);
                Parameters.Insert(612, 0.7);
                Parameters.Insert(613, 0.5);
                Parameters.Insert(614, 0.5);
                Parameters.Insert(615, 0.5);
                Parameters.Insert(616, 0);
                Parameters.Insert(617, 0);
                Parameters.Insert(618, 0);
                Parameters.Insert(619, 1);
                Parameters.Insert(620, 0);
                Parameters.Insert(621, 0);
                Parameters.Insert(622, 1);
                Parameters.Insert(623, 0);
                Parameters.Insert(624, 0);
                Parameters.Insert(625, 0);
                Parameters.Insert(626, 0.2);
                Parameters.Insert(627, 0.4);
                Parameters.Insert(628, 0);
                Parameters.Insert(629, 0);
                Parameters.Insert(630, 0);
                Parameters.Insert(631, 0.5);
                Parameters.Insert(632, 0.5);
                Parameters.Insert(633, 0.5);
                Parameters.Insert(634, 0.5);
                Parameters.Insert(635, 0.5);
                Parameters.Insert(636, 0.5);
                Parameters.Insert(637, 0.3);
                Parameters.Insert(638, 0.05);
                Parameters.Insert(639, 0.1);
                Parameters[640] = 0.6;
                Parameters[641] = 0.1;
                Parameters[642] = 0.5;

                var buf1 = new byte[2532 + 12];
                var buf2 = new byte[2532];
                var buf3 = new byte[1028];
                ms2.Seek(0, SeekOrigin.Begin);

                ms2.Read(buf1, 0, 2532 + 12);
                ms2.Read(buf2, 0, 2532);

                using (var msbuf1 = new MemoryStream())
                {
                    msbuf1.Write(buf1, 0, buf1.Length);
                    msbuf1.Seek(0, SeekOrigin.End);

                    for (var i = 0; i < 240; i++)
                    {
                        msbuf1.WriteByte(0x00);
                    }

                    msbuf1.Seek(6, SeekOrigin.Begin);
                    msbuf1.WriteByte(0x19);
                    msbuf1.WriteByte(0xAC);
                    msbuf1.WriteByte(0x01);
                    msbuf1.WriteByte(0x02);

                    buf1 = msbuf1.ToArray();
                }

                using (var msbuf2 = new MemoryStream())
                {
                    msbuf2.Write(buf2, 0, buf2.Length);

                    for (var i = 0; i < 240; i++)
                    {
                        msbuf2.WriteByte(0xFF);
                    }

                    buf2 = msbuf2.ToArray();
                }

                using (var msbuf3 = new MemoryStream())
                {
                    for (var i = 0; i < PatchName.Length; i++)
                    {
                        msbuf3.WriteByte(PatchName[i]);
                    }

                    for (var i = 0; i < 1024 - PatchName.Length; i++)
                    {
                        msbuf3.WriteByte(0x00);
                    }

                    msbuf3.WriteByte(0xDE);
                    msbuf3.WriteByte(0xAD);
                    msbuf3.WriteByte(0xBE);
                    msbuf3.WriteByte(0xEF);

                    buf3 = msbuf3.ToArray();
                }


                WzooBlock.BlockData = buf1.Concat(buf2).Concat(buf3).ToArray();
            }
            else
            {
                throw new Exception("Unknown block length");
            }

            EndChunk = VendorResources.Hybrid3EndChunk;
        }

        private double MigratePitchModSource(double value)
        {
            var roundedValue = value.ToString("F2",
                CultureInfo.InvariantCulture);

            switch (roundedValue)
            {
                case "0.00":
                    return 0;
                case "0.05":
                    return 0.05882353d;
                case "0.10":
                    return 0.1176471d;
                case "0.15":
                    return 0.1764706d;
                case "0.20":
                    return 0.2941176d;
                case "0.25":
                    return 0.35294113d;
                case "0.30":
                    return 0.4117647d;
                case "0.35":
                    return 0.470588d;
                case "0.40":
                    return 0.5294117d;
                case "0.45":
                    return 0.58823523d;
                case "0.50":
                    return 0.64705d;
                case "0.55":
                    return 0.7058824d;
                case "0.60":
                    return 0.7647059d;
                case "0.65":
                    return 0.8235294d;
                case "0.70":
                    return 0.8823529d;
                case "0.75":
                    return 0.9411765d;
                case "0.80":
                    return 1d;
                default:
                    throw new Exception(
                        "Unknown mapping for PitchModSource");
            }
        }
    }
}