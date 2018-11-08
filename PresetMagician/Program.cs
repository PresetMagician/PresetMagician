using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Media;
using NKSF;
using System.Diagnostics;

namespace TestRIFF
{
    class Program
    {
        static void Main(string[] args)
        {
            RiffChunk riffChunk;
            RiffHeaderChunk fileHeader = null;
            ListInfoChunk listInfo;
            SummaryInformation summaryInformation;
            ControllerAssignments controllerAssignments;
            PluginId pluginId;

            int seekSize;


            Debug.Listeners.Add(new ConsoleTraceListener());

            try { 
            using (var fileStream = new FileStream(@"C:\Users\Felicia Hummel\Documents\Native Instruments\User Content\VStation\VStation Factory\100 Bass1.nksf", FileMode.Open))
            {

                while (fileStream.Position < fileStream.Length)
                {
                    riffChunk = RiffChunk.ReadNext(fileStream);

                    Console.Write("At position ");
                    Console.WriteLine(fileStream.Position);

                    switch (riffChunk.TypeID)
                    {
                        case RiffHeaderChunk.RiffTypeID:
                            fileHeader = new RiffHeaderChunk(riffChunk, fileStream, "NIKS");
                                
                            break;
                        case ListInfoChunk.RiffTypeID:
                            Console.Write("listInfoChunk");
                            listInfo = new ListInfoChunk(riffChunk, fileStream);
                            break;
                        case SummaryInformation.RiffTypeID:
                            summaryInformation = new SummaryInformation(riffChunk, fileStream);
                            break;
                        case ControllerAssignments.RiffTypeID:
                            controllerAssignments = new ControllerAssignments(riffChunk, fileStream);
                            break;
                            case PluginId.RiffTypeID:
                                pluginId = new PluginId(riffChunk, fileStream);
                                break;
                        default:
                            Console.Write("Unknown fourCC ");
                            Console.WriteLine(riffChunk.TypeID);
                            Console.Write("Seeking bytes: ");
                            Console.WriteLine(riffChunk.ChunkSize);
                            // Skip unidentified section

                            seekSize = riffChunk.ChunkSize;

                            if (seekSize % 2 != 0)
                            {
                                seekSize++;
                            }

                            fileStream.Seek(seekSize, SeekOrigin.Current);
                            break;
                    }
                }
            }

                    /*
                    riffHeaderChunk = RiffChunk.ReadNext(fileStream);

                Console.Write(riffHeaderChunk.TypeID);
                if (riffHeaderChunk.TypeID != "RIFF")
                {
                    throw new Exception("Invalid file; RIFF expected.");
                }

                Console.WriteLine(fileStream.Position);

                riffHeaderChunk = RiffChunk.ReadNext(fileStream);

                Console.Write(riffHeaderChunk.TypeID);
                using (var writer = new System.IO.StringWriter())
                {
                    ObjectDumper.Dumper.Dump(riffHeaderChunk, "Object Dumper", writer);
                    Console.Write(writer.ToString());
                    Console.Write(riffHeaderChunk.GetHashCode());
                }
            }*/


                } catch (Exception e)
            {
                Console.Write(e.ToString());
            }

            VstHost vstHost = new VstHost();

            }
    }
}
