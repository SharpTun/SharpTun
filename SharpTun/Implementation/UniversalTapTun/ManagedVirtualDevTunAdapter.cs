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
using Microsoft.Win32.SafeHandles;
using SharpTun.Implementation.UniversalTapTun.NativeLink;
using SharpTun.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharpTun.Implementation.UniversalTapTun
{
    internal class ManagedVirtualDevTunAdapter : IManagedVirtualTunAdapter
    {
        private readonly string driverNode;
        private readonly SafeFileHandle tunDeviceHandle;
        private readonly string interfaceName;
        private bool disposed = false;

        private ManagedVirtualDevTunAdapter(string driverNode, SafeFileHandle tunDeviceHandle, string interfaceName)
        {
            this.driverNode = driverNode;
            this.tunDeviceHandle = tunDeviceHandle;
            this.interfaceName = interfaceName;
        }

        public void Dispose()
        {
            CheckDisposed();
            disposed = true;
            tunDeviceHandle.Dispose();
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(interfaceName);
            }
        }

        public ITunSession Start(int capacity)
        {
            IFREQ interfaceRequest = new IFREQ(interfaceName, NET_IF_CONSTANTS.IFF_TUN | NET_IF_CONSTANTS.IFF_MULTI_QUEUE);
            SafeFileHandle handle = SharedTunInterfaceOpen(driverNode, ref interfaceRequest);
            return new DevTunSession(handle, capacity);
        }

        public static ManagedVirtualDevTunAdapter Create(string driverNode, string Name)
        {
            if (Name.Length > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(Name));
            }

            if (!Name.Contains("%d"))
            {
                throw new ArgumentException($"Parameter {nameof(Name)} needs to be a template.");
            }

            IFREQ interfaceRequest = new IFREQ(Name, NET_IF_CONSTANTS.IFF_TUN | NET_IF_CONSTANTS.IFF_MULTI_QUEUE);
            SafeFileHandle tunDeviceHandle = SharedTunInterfaceOpen(driverNode, ref interfaceRequest);

            return new ManagedVirtualDevTunAdapter(driverNode, tunDeviceHandle, interfaceRequest.ifrn_name);
        }

        internal static SafeFileHandle SharedTunInterfaceOpen(string driverNode, ref IFREQ interfaceRequest)
        {
            SafeFileHandle tunDeviceHandle = File.OpenHandle(driverNode, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None);
            if (LibC.ioctl(tunDeviceHandle.DangerousGetHandle(), NET_IF_CONSTANTS.TUNSETIFF, ref interfaceRequest) < 0)
            {
                tunDeviceHandle.Close();
                throw new InvalidOperationException($"Error: {Marshal.GetLastSystemError()}");
            }

            return tunDeviceHandle;
        }
    }

    internal class NET_IF_CONSTANTS
    {
        internal const int IF_NAMESIZE = 16;
        internal const short IFF_TUN = 0x0001;
        internal const int TUNSETIFF = 0x400454ca;
        internal const int IFF_MULTI_QUEUE = 0x0100;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct IFREQ
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NET_IF_CONSTANTS.IF_NAMESIZE)]
        internal string ifrn_name;
        internal short ifr_flags;

        public IFREQ(string ifrn_name, short ifr_flags)
        {
            this.ifrn_name = ifrn_name;
            this.ifr_flags = ifr_flags;
        }
    }
}
