using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTun.Interface;

namespace SharpTun.Implementation.UniversalTapTun
{
    internal class DevTunPacketFormatHelper : IPacketFormatHelper
    {
        public static readonly DevTunPacketFormatHelper Instance = new();

        /// <inheritdoc/>
        public int GetL3DatagramPayloadBeginIndex(byte[] packet)
        {
            return 4;
        }

        /// <inheritdoc/>
        public int GetProtocolVersion(byte[] packet)
        {
            return packet[3] << 8 | packet[2];
        }
    }
}
