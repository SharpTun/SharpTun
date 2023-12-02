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
using System.Runtime.InteropServices;

namespace SharpTun.Implementation.Wintun.NativeLink
{

    /// <summary>
    /// Describes a local identifier for an adapter.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        /// <summary>
        /// Specifies a DWORD that contains the unsigned lower numbers of the id.
        /// </summary>
        public uint LowPart;
        /// <summary>
        /// Specifies a LONG that contains the signed high numbers of the id.
        /// </summary>
        public int HighPart;
    }

    internal class Kernel32
    {
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern uint WaitForSingleObject(IntPtr handle, int milliseconds);
    }

    internal enum Win32Error : int
    {
        ERROR_SUCCESS = 0x0,
        ERROR_NO_MORE_ITEMS = 0x103,
        ERROR_HANDLE_EOF = 0x26,
        ERROR_INVALID_DATA = 0xd,
        ERROR_FILE_NOT_FOUND = 0x2,
        ERROR_BUFFER_OVERFLOW = 0x6f,
    }

    internal enum WaitForSingleObjectReturn : uint
    {
        WAIT_ABANDONED = 0x00000080,
        WAIT_OBJECT_0 = 0x00000000,
        WAIT_TIMEOUT = 0x00000102,
        WAIT_FAILED = 0xFFFFFFFF
    }

    internal class NTEvent
    {
        private readonly IntPtr hEvent;

        internal NTEvent(IntPtr hEvent)
        {
            this.hEvent = hEvent;
        }

        internal WaitForSingleObjectReturn WaitFor(int milliseconds)
        {
            return (WaitForSingleObjectReturn)Kernel32.WaitForSingleObject(hEvent, milliseconds);
        }
    }
}
