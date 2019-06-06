using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PresetMagician.Core.Models.Audio
{
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class AudioOutputDevice
    {
        [JsonProperty] [DataMember] public string Name { get; set; }
        [JsonProperty] [DataMember] public string Id { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public AudioDeviceType DeviceType { get; set; }

        [JsonProperty] [DataMember] public bool DefaultDevice { get; set; }

        public AudioOutputDevice(string name, string id, AudioDeviceType type)
        {
            Name = name;
            Id = id;
            DeviceType = type;
        }

        public AudioOutputDevice(string name, string id, AudioDeviceType deviceType, bool defaultDevice)
        {
            Name = name;
            Id = id;
            DeviceType = deviceType;
            DefaultDevice = defaultDevice;
        }

        public AudioOutputDevice()
        {
        }

        protected bool Equals(AudioOutputDevice other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Id, other.Id) && DeviceType == other.DeviceType &&
                   DefaultDevice == other.DefaultDevice;
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

            return Equals((AudioOutputDevice) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) DeviceType;
                hashCode = (hashCode * 397) ^ DefaultDevice.GetHashCode();
                return hashCode;
            }
        }
    }
}