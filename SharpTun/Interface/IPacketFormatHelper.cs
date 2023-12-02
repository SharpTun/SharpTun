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

namespace SharpTun.Interface
{
    public interface IPacketFormatHelper
    {

        /// <summary>
        /// Returns the protocol version from the provided packet.
        /// </summary>
        /// <param name="packet">The data of the packet to inspect</param>
        /// <returns>The protocol version contained in the payload of the packet</returns>
        public int GetProtocolVersion(byte[] packet);

        /// <summary>
        /// Returns the index of the byte which corresponds to the beginning of the L3 packet payload
        /// </summary>
        /// <param name="packet">The data of the packet to inspect</param>
        /// <returns>The index of <paramref name="packet"/> which starts the L3 packet payload.</returns>
        public int GetL3DatagramPayloadBeginIndex(byte[] packet);
    }
}