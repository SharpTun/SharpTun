/* SPDX-License-Identifier: Apache-2.0
 * Copyright 2023 jdstroy.  All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpTun.Implementation.Wintun.NativeLink;
using SharpTun.Interface;

namespace SharpTun.Implementation.Wintun
{
    /// <summary>
    /// Managed API for a Wintun session (e.g. allows reading from and writing to a 
    /// Wintun adapter).
    /// </summary>
    public class ManagedWintunSession : IDisposable, ITunSession
    {
        private readonly IntPtr sessionHandle;
        private readonly NTEvent evt;
        private bool disposed;

        /// <summary>
        /// Represents a Wintun session opened from a Wintun adapter.
        /// </summary>
        /// <param name="sessionHandle">The handle provided by the adapter when calling WintunStartSession</param>
        /// <exception cref="InvalidOperationException">Thrown if the session handle is null.</exception>
        internal ManagedWintunSession(IntPtr sessionHandle)
        {
            if (IntPtr.Zero == sessionHandle)
            {
                throw new InvalidOperationException(nameof(sessionHandle));
            }
            this.sessionHandle = sessionHandle;
            disposed = false;
            evt = new NTEvent(SharpWinTun.WintunGetReadWaitEvent(sessionHandle));
        }

        /// <summary>
        /// Releases the resources associated with the Wintun session.
        /// </summary>
        public void Dispose()
        {
            CheckDisposed();

            SharpWinTun.WintunEndSession(sessionHandle);
            disposed = true;
        }

        /// <summary>
        /// Sends a packet through this session on the adapter.
        /// </summary>
        /// <param name="packet">Raw OSI level 3 packet to send on the adapter to the host.</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="InternalBufferOverflowException"></exception>
        public void SendPacket(byte[] packet)
        {
            CheckDisposed();

            Debug.Assert(packet.Length <= SharpWinTun.WINTUN_MAX_IP_PACKET_SIZE);

            IntPtr spaceAllocated = SharpWinTun.WintunAllocateSendPacket(sessionHandle, packet.Length);

            Debug.Assert(spaceAllocated != IntPtr.Zero);

            var lastError = (Win32Error)Marshal.GetLastSystemError();
            switch (lastError)
            {
                default:
                    Debug.Assert(false, $"GetLastError() = {lastError}");
                    break;
                case Win32Error.ERROR_SUCCESS:
                    break;
                case Win32Error.ERROR_HANDLE_EOF:
                    throw new InvalidOperationException("Wintun adapter is terminating");
                case Win32Error.ERROR_BUFFER_OVERFLOW:
                    throw new InternalBufferOverflowException("Wintun buffer is full");
            }

            Marshal.Copy(packet, 0, spaceAllocated, packet.Length);

            SharpWinTun.WintunSendPacket(sessionHandle, spaceAllocated);
        }


        /// <summary>
        /// Polls for a raw, OSI Level 3 packet from the host.  If no packets are available, this method blocks.
        /// </summary>
        /// <returns>Raw level 3 packet data received on the adapter from the host.</returns>
        /// <exception cref="InvalidOperationException">If the TUN adapter is terminating / has terminated.</exception>
        /// <exception cref="InvalidDataException">If the TUN buffer has been corrupted</exception>
        public byte[] ReceivePacket()
        {
            CheckDisposed();
            do
            {
                int size;
                IntPtr packet = SharpWinTun.WintunReceivePacket(sessionHandle, out size);
                var lastError = (Win32Error)Marshal.GetLastSystemError();
                switch (lastError)
                {
                    case Win32Error.ERROR_NO_MORE_ITEMS:
                        evt.WaitFor(1);
                        continue;

                    case Win32Error.ERROR_HANDLE_EOF:
                        throw new InvalidOperationException("Wintun adapter is terminating");
                    case Win32Error.ERROR_INVALID_DATA:
                        throw new InvalidDataException("Wintun buffer is corrupt");
                    default:
                        Debug.Assert(false, $"GetLastError() = {lastError}");
                        break;
                    case Win32Error.ERROR_SUCCESS:
                        break;
                }

                Debug.Assert(IntPtr.Zero != packet);

                try
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(packet, buffer, 0, size);
                    return buffer;
                }
                finally
                {
                    SharpWinTun.WintunReleaseReceivePacket(sessionHandle, packet);
                }
            } while (true);
        }

        /// <summary>
        /// Checks if the session has been <see cref="Dispose"/>d.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object has already been disposed.</exception>
        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        public IPacketFormatHelper GetHelper()
        {
            return WintunPacketFormatHelper.Instance;
        }
    }
}