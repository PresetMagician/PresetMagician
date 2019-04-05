using System.Collections.Generic;
using System.IO;
using Catel.Collections;
using Catel.Services;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        private static string GetRememberMyChoiceResultsStorageFile()
        {
            Directory.CreateDirectory(DefaultDataStoragePath);
            return Path.Combine(DefaultDataStoragePath, RememberMyChoiceResults);
        }
        
        public void LoadRememberMyChoiceResults()
        {
            if (File.Exists(GetRememberMyChoiceResultsStorageFile()))
            {
                var dontAskAgain =
                    GetLoadSerializer().Deserialize<Dictionary<string, MessageResult>>(
                        File.ReadAllBytes(GetRememberMyChoiceResultsStorageFile()));

                _globalService.RememberMyChoiceResults.Clear();
                _globalService.RememberMyChoiceResults.AddRange(dontAskAgain);
            }

           
        }

        public void SaveRememberMyChoiceResults()
        {
            var data = GetSaveSerializer().Serialize(_globalService.RememberMyChoiceResults);
            File.WriteAllBytes(GetRememberMyChoiceResultsStorageFile(), data);
        }
    }
}