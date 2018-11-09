using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace PresetMagician.VST
{
    public class VstPathScanner
    {
        private static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();

        public static readonly string[] vstPaths = {
                @"\VSTPlugins",
                @"\Steinberg\VSTPlugins",
                @"\Common Files\VST2",
                @"\Common Files\Steinberg\VST2",
                @"\Native Instruments\VSTPlugins 64 bit",
                @"\Native Instruments\VSTPlugins 32 bit"
        };

        public VstPathScanner()
        {
            getCommonVSTPluginDirectories();

            return;
            string[] drives = System.Environment.GetLogicalDrives();

            foreach (string dr in drives)
            {
                System.IO.DriveInfo di = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read. This
                // is not necessarily the appropriate action in all scenarios.
                if (!di.IsReady)
                {
                    Console.WriteLine("The drive {0} could not be read", di.Name);
                    continue;
                }

                if (di.DriveType == System.IO.DriveType.Fixed)
                {
                    System.IO.DirectoryInfo rootDir = di.RootDirectory;

                    WalkDirectoryTree(rootDir);
                }
            }
        }

        public static ObservableCollection<DirectoryInfo> getCommonVSTPluginDirectories()
        {
            string[] drives = System.Environment.GetLogicalDrives();

            ObservableCollection<DirectoryInfo> vstLocations = new ObservableCollection<DirectoryInfo>();
            String vstLocation;

            foreach (String vstPath in VstPathScanner.vstPaths)
            {
                vstLocation = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + vstPath;
                if (Directory.Exists(vstLocation))
                {
                    vstLocations.Add(new DirectoryInfo(vstLocation));
                }

                vstLocation = Environment.GetEnvironmentVariable("ProgramFiles") + vstPath;
                if (Directory.Exists(vstLocation))
                {
                    vstLocations.Add(new DirectoryInfo(vstLocation));
                }
            }

            return vstLocations;
        }

        private static void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.dll");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                log.Add(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    Console.WriteLine(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }
    }
}