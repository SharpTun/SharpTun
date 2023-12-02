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
    /// Represents a virtual Wintun network adapter.
    /// </summary>
    public class ManagedWintunAdapter : IDisposable, IManagedVirtualTunAdapter
    {
        private readonly IntPtr adapterHandle;
        private readonly string name;
        private bool disposed;

        /// <summary>
        /// Creates the high level object encapsulating the Wintun adapter handle.
        /// </summary>
        /// <param name="adapterHandle">Handle provided by 
        /// <see cref="SharpWinTun.WintunCreateAdapter(string, string, in Guid)"/>,
        /// <see cref="SharpWinTun.WintunCreateAdapter(string, string, IntPtr)"/>,
        /// or <see cref="SharpWinTun.WintunOpenAdapter(string)"/>.
        /// </param>
        /// <param name="name">The name of the adapter.  Used for reporting when generating exceptions for an already-disposed adapter.</param>
        /// <exception cref="ArgumentNullException">Adapter handle may not be null.</exception>
        private ManagedWintunAdapter(IntPtr adapterHandle, string name)
        {
            if (IntPtr.Zero == adapterHandle)
            {
                throw new ArgumentNullException(nameof(adapterHandle));
            }
            this.adapterHandle = adapterHandle;
            this.name = name;
            disposed = false;
        }

        /// <summary>
        /// Creates a virtual Wintun adapter with the specified adapter name, tunnel type, and the requested GUID.
        /// </summary>
        /// <param name="AdapterName">The friendly name that end users will see for this adapter.</param>
        /// <param name="TunnelType">e.g. "WinTun"</param>
        /// <param name="RequestedGUID"></param>
        /// <returns>A managed object encapsulating the handle for the newly created virtual network adapter</returns>
        public static ManagedWintunAdapter Create(string AdapterName, string TunnelType, Guid? RequestedGUID)
        {
            var handle = RequestedGUID.HasValue ?
                    SharpWinTun.WintunCreateAdapter(AdapterName, TunnelType, RequestedGUID.Value) :
                    SharpWinTun.WintunCreateAdapter(AdapterName, TunnelType, IntPtr.Zero);

            /*
             * There are actually many ways this function can fail.  For 
             * example, if the caller lacks the necessary privileges to 
             * install the Wintun driver, or if the caller lacks the necessary 
             * privileges to create an adapter
             */

            var error = (Win32Error)Marshal.GetLastSystemError();

            switch (error)
            {
                case Win32Error.ERROR_SUCCESS:
                    break;
                default:
                    Debug.Assert(false, $"GetLastError() = {error}");
                    break;
            }

            return new ManagedWintunAdapter(handle, AdapterName);
        }

        /// <summary>
        /// Opens an existing adapter by name
        /// </summary>
        /// <param name="AdapterName">The name of the existing adapter to open.</param>
        /// <returns></returns>
        public static ManagedWintunAdapter Open(string AdapterName)
        {
            var handle = SharpWinTun.WintunOpenAdapter(AdapterName);

            var error = (Win32Error)Marshal.GetLastSystemError();

            switch (error)
            {
                case Win32Error.ERROR_SUCCESS:
                    break;
                default:
                    Debug.Assert(false, $"GetLastError() = {error}");
                    break;
            }

            return new ManagedWintunAdapter(handle, AdapterName);
        }

        /// <summary>
        /// Releases resources held by the virtual network adapter.
        /// </summary>
        public void Dispose()
        {
            CheckDisposed();

            SharpWinTun.WintunCloseAdapter(adapterHandle);
            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(name);
            }
        }

        /// <summary>
        /// Gets the LUID of the network adapter (used to identify the adapter instance across the platform)
        /// </summary>
        /// <returns>An LUID representing this virtual network adapter on the platform</returns>
        public Luid GetLuid()
        {
            CheckDisposed();
            Luid luid = new Luid();
            SharpWinTun.WintunGetAdapterLUID(adapterHandle, ref luid);

            return luid;
        }

        /// <summary>
        /// Creates a session for this virtual network adapter with the specified buffer capacity.
        /// </summary>
        /// <param name="Capacity">The buffer size for this network adapter.</param>
        /// <returns>A <see cref="ManagedWintunSession"/> representing the session used to send and receive packets for this adapter.</returns>
        public ITunSession Start(int Capacity)
        {
            CheckDisposed();
            var sessionHandle = SharpWinTun.WintunStartSession(adapterHandle, Capacity);

            var lastError = (Win32Error)Marshal.GetLastSystemError();
            switch (lastError)
            {
                default:
                    Debug.Assert(false, $"GetLastError() = {lastError}");
                    break;
                case Win32Error.ERROR_SUCCESS:
                    break;
            }
            return new ManagedWintunSession(sessionHandle);
        }
    }
}
