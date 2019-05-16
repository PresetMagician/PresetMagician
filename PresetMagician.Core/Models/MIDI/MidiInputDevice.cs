using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PresetMagician.Core.Models.MIDI
{
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class MidiInputDevice
    {
        [JsonProperty] [DataMember] public string Name { get; set; }

        protected bool Equals(MidiInputDevice other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((MidiInputDevice) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}