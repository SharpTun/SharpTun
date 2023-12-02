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
    /// Determines the level of logging, passed to WINTUN_LOGGER_CALLBACK.
    /// </summary>
    public enum WINTUN_LOGGER_LEVEL
    {
        /// <summary>
        /// &lt; Informational
        /// </summary>
        WINTUN_LOG_INFO,
        /// <summary>
        /// &lt; Warning
        /// </summary>
        WINTUN_LOG_WARN,
        /// <summary>
        /// &lt; Error
        /// </summary>
        WINTUN_LOG_ERR
    }

    /// <summary>
    /// Called by internal logger to report diagnostic messages
    /// </summary>
    /// <param name="Level">Message level.</param>
    /// <param name="Timestamp">Message timestamp in in 100ns intervals since 1601-01-01 UTC.</param>
    /// <param name="Message">Message text.</param>
    public delegate void WintunLoggerCallback(
        WINTUN_LOGGER_LEVEL Level,
        ulong Timestamp,
        [MarshalAs(UnmanagedType.LPWStr)] string Message);

    public static class ShrarpWinTunLogger
    {
        /// <summary>
        /// Sets logger callback function. 
        /// </summary>
        /// <param name="logger">Delegate to callback function to use as a new global logger.
        /// NewLogger may be called from various threads concurrently.  Should the logging 
        /// require serialization, you must handle serialization in NewLogger.</param>
        public static void SetLogger(WintunLoggerCallback logger)
        {
            SharpWinTun.WintunSetLogger(logger);
        }


        /// <summary>
        /// Reset logger callback function. 
        /// </summary>
        /// <param name="logger">Disables logging from Wintun.</param>
        public static void ResetLogger()
        {
            SharpWinTun.WintunSetLogger(IntPtr.Zero);
        }
    }

    /// <summary>
    /// Thin wrapper around Wintun.dll library.
    /// </summary>
    internal static class SharpWinTun
    {
        internal static readonly int WINTUN_MAX_IP_PACKET_SIZE = 0xFFFF;

        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunCreateAdapter([MarshalAs(UnmanagedType.LPWStr)] string AdapterName, [MarshalAs(UnmanagedType.LPWStr)] string TunnelType, in Guid RequestedGUID);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunCreateAdapter([MarshalAs(UnmanagedType.LPWStr)] string AdapterName, [MarshalAs(UnmanagedType.LPWStr)] string TunnelType, IntPtr RequestedGUID);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunOpenAdapter([MarshalAs(UnmanagedType.LPWStr)] string AdapterName);
        [DllImport("wintun.dll")]
        public static extern void WintunCloseAdapter(IntPtr Adapter);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern bool WintunDeleteDriver();
        [DllImport("wintun.dll")]
        public static extern void WintunGetAdapterLUID(IntPtr Adapter, ref Luid luid);
        [DllImport("wintun.dll")]
        public static extern int WintunGetRunningDriverVersion();
        [DllImport("wintun.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void WintunSetLogger(WintunLoggerCallback NewLogger);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern void WintunSetLogger(IntPtr mustBeNull);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunStartSession(IntPtr Adapter, int Capacity);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunAllocateSendPacket(IntPtr session, int PacketSize);
        [DllImport("wintun.dll")]
        public static extern void WintunEndSession(IntPtr session);
        [DllImport("wintun.dll")]
        public static extern IntPtr WintunGetReadWaitEvent(IntPtr session);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern IntPtr WintunReceivePacket(IntPtr session, out int size);
        [DllImport("wintun.dll")]
        public static extern void WintunReleaseReceivePacket(IntPtr session, IntPtr packet);
        [DllImport("wintun.dll", SetLastError = true)]
        public static extern void WintunSendPacket(IntPtr session, IntPtr packet);
    }
}