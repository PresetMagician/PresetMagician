using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MessagePack;

namespace PresetMagician.NKS
{
    [MessagePackObject]
    public class SummaryInformation
    {
        public enum AsString
        {
            INST = 0,
            FX = 1
        }

        [Key("author")] public String author;

        [Key("comment")] public String comment;

        [Key("deviceType")] public String deviceType;

        [Key("name")] public String name;

        [Key("uuid")] public Guid uuid;

        [Key("vendor")] public String vendor;

        [Key("bankchain")] public ObservableCollection<String> bankChain;

        [Key("types")] public List<List<String>> Types { get; set; }

        [Key("modes")] public List<String> Modes { get; set; }

        public SummaryInformation()
        {
            bankChain = new ObservableCollection<String>();
            Types = new List<List<string>>();
            Modes = new List<string>();
            uuid = Guid.NewGuid();
            author = null;
            comment = null;
        }
    }
}