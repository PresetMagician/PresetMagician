using System;
using System.ServiceModel;

namespace PresetMagician.RemoteVstHost
{
    public static class WcfUtils
    {
        public static NetNamedPipeBinding GetNetNamedPipeBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = 256000000,
                MaxBufferSize = 256000000,
                SendTimeout = new TimeSpan(0, 0, 0, 60)
            };
        }
    }
}