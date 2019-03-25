using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Catel.Collections;
using Catel.IoC;
using Drachenkatze.PresetMagician.VendorPresetParser.Spectrasonics;
using PresetMagician;
using PresetMagician.Core.Services;
using PresetMagician.Legacy.Models;
using SQLite;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagicianScratchPad
{
    public class Program
    {
      
        [STAThread]
        static void Main(string[] args)
        {
            var omnisphere = new Spectrasonics_Omnisphere();

            var libraries = omnisphere.GetLibraryFiles(Spectrasonics_Omnisphere.LIBRARYTYPE_PATCHES);

            foreach (var libraryFile in libraries)
            {
                Debug.WriteLine(libraryFile);
            }

            var lib = @"C:\Spectrasonics\STEAM\Omnisphere\Settings Library\Patches\Factory\Omnisphere Library.db";

            var libs = omnisphere.GetLibraries();
            var zi = 0;
            foreach (var library in libs)
            {
                library.BuildMetadata();
                foreach (var file in library.Files)
                {
                    
                    if (file.Extension == ".prt_omn" || file.Extension == ".mlt_omn")
                    {
                        if (file.Attributes.Count == 0)
                        {
                            zi++;
                            Debug.WriteLine(library.Path + " " + library.ContentOffset + " " + file.Filename + " " +
                                            file.Offset);
                        }

                    }
                }
            }
            
            Debug.WriteLine(zi);

         
        }
    }
}