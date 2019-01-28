using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.ServiceModel;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.VstHost.VST;
using SharedModels;

namespace PresetMagician.ProcessIsolation.Services
{
    
    public partial class RemoteVstService : IRemoteFileService
     {
         public bool Exists(string file)
         {
             return File.Exists(file);
         }

         public long GetSize(string file)
         {
             var fileInfo = new FileInfo(file);
             return fileInfo.Length;
         }

         public string GetHash(string file)
         {
             return HashUtils.getIxxHash(File.ReadAllBytes(file));
         }

         public byte[] GetContents(string file)
         {
             return File.ReadAllBytes(file);
         }
     }
 }