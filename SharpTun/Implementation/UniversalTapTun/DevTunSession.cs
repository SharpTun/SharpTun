using Microsoft.Win32.SafeHandles;
using SharpTun.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpTun.Implementation.UniversalTapTun
{
    internal class DevTunSession : ITunSession
    {
        private readonly SafeFileHandle handle;
        private bool disposed = false;
        private readonly byte[] buffer;
        //private readonly FileStream fstream;

        internal DevTunSession(SafeFileHandle handle, int capacity)
        {
            this.handle = handle;
            buffer = new byte[capacity];
            //fstream = new FileStream(handle, FileAccess.ReadWrite, capacity);
        }

        public void Dispose()
        {
            CheckDisposed();
            handle.Dispose();
            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException($"{typeof(DevTunSession).Name}:{handle}");
            }
        }

        public IPacketFormatHelper GetHelper()
        {
            return DevTunPacketFormatHelper.Instance;
        }

        public byte[] ReceivePacket()
        {
            int size;

            //size = fstream.Read(buffer, 0, buffer.Length);

            size = RandomAccess.Read(handle, buffer, 0);

            return buffer.Take(size).ToArray();
        }

        public void SendPacket(byte[] packet)
        {
            //fstream.Write(packet);
            RandomAccess.Write(handle, packet, 0);
        }
    }
}
