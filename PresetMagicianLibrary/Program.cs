using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Jacobi.Vst.Interop.Host;
using Foobr;

namespace PresetMagician
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            String dllPath = @"C:\Program Files\VSTPlugins\S-Gear2_x64.dll";
            HostCommandStub hostCommandStub = new HostCommandStub();
            hostCommandStub.Directory = Path.GetDirectoryName(dllPath);
            VstPluginContext.Create(dllPath, hostCommandStub);
         
            Debug.WriteLine("Y");
            Thread.Sleep(1000000);
        }

        /*private static void SimpleVSTTestMain()
        {
            Debug.WriteLine("start");
            var program = new Program();
            VstPluginContext ctx = program.OpenPlugin(@"C:\Program Files (x86)\Steinberg\VstPlugins\Novation\V-Station\V-Station x64.dll");
            Debug.WriteLine("loaded");
            ctx.Dispose();
            Debug.WriteLine("disposed, goodbye");
        }*/

        /*public VstPluginContext OpenPlugin(string pluginPath)
        {
            try
            {
                HostCommandStub hostCmdStub = new HostCommandStub();
                hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

                VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

                // add custom data to the context
                ctx.Set("PluginPath", pluginPath);
                ctx.Set("HostCmdStub", hostCmdStub);

                // actually open the plugin itself
                ctx.PluginCommandStub.Open();

                return ctx;
            }
            catch (Exception e)
            {
                //ssageBox.Show(this, e.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

            // can be null when called from inside the plugin main entry point.
            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called:" + e.Message);
            }
        }*/

        private static void Main2(string[] args)
        {
            /*  RiffChunk riffChunk;
              RiffHeaderChunk fileHeader = null;
              ListInfoChunk listInfo;
              SummaryInformation summaryInformation;
              ControllerAssignments controllerAssignments;
              PluginId pluginId;

              int seekSize;

              Debug.Listeners.Add(new ConsoleTraceListener());

              VstPathScanner vstPathScanner = new VstPathScanner();
              return;

              try
              {
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
          }
              }
              catch (Exception e)
              {
                  Console.Write(e.ToString());
              }

              VstHost vstHost = new VstHost();
          }*/
        }
    }
}