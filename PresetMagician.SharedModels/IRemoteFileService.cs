using System;
using System.Collections.Generic;
using System.ServiceModel;
using PresetMagician.Models;

namespace SharedModels
{
    [ServiceContract(Namespace = "https://presetmagician.com")]
    public interface IRemoteFileService
    {
        [OperationContract]
        bool Exists (string file);
        
        [OperationContract]
        long GetSize (string file);
        
        [OperationContract]
        string GetHash (string file);
        
        [OperationContract]
        byte[] GetContents (string file);

       
    }
}