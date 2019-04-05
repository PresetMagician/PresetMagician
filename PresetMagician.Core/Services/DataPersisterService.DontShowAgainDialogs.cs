using System.Collections.Generic;
using System.IO;
using Catel.Collections;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        private static string GetDontShowAgainDialogsStorageFile()
        {
            Directory.CreateDirectory(DefaultDataStoragePath);
            return Path.Combine(DefaultDataStoragePath, DontShowAgainDialogsStorageFile);
        }
        
        public void LoadDontShowAgainDialogs()
        {
            if (File.Exists(GetDontShowAgainDialogsStorageFile()))
            {
                var dontShowAgainDialogs =
                    GetLoadSerializer().Deserialize<HashSet<string>>(
                        File.ReadAllBytes(GetDontShowAgainDialogsStorageFile()));

                _globalService.DontShowAgainDialogs.Clear();
                _globalService.DontShowAgainDialogs.AddRange(dontShowAgainDialogs);
            }

           
        }

        public void SaveDontShowAgainDialogs()
        {
            var data = GetSaveSerializer().Serialize(_globalService.DontShowAgainDialogs);
            File.WriteAllBytes(GetDontShowAgainDialogsStorageFile(), data);
        }
    }
}