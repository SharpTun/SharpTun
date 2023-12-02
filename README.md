SharpTun is a set of C#/CLI bindings for TUN drivers.

SharpTun relies on WireGuard LLC's Wintun library to do the heavy lifting and 
interface with your computer's operating system.  SharpTun neither requires 
nor depends on WireGuard, and only depends on Wintun, which you can find at 

  https://www.wintun.net/

where you may download the released binaries.  I've developed and tested 
SharpTun against the released version of Wintun 0.14.1 (which is licensed
under WireGuard LLC's "Prebuilt Binaries License"), using the documentation 
of the API in wintun.h (licensed under GPL-2.0 or MIT).

To use SharpTun, place the appropriate wintun.dll in the same directory as 
the SharpTun.dll library.  Wintun, unlike SharpTun, is not architecture 
agnostic; as a native library, you need to select the correct one to load for 
your processor.  For most users, this should be wintun/bin/amd64/wintun.dll 
from the Wintun release ZIP archive.

To create a virtual new network adapter, call

```
SharpTun.Implementation.Wintun.ManagedWintunAdapter.Create(
	string AdapterName, 
	string TunnelType, 
	Guid? RequestedGUID)
```

For example:

```
using(var adapter = SharpTun.Implementation.Wintun.ManagedWintunAdapter.Create(
	"My Virtual Network Adapter 1", 
	"WinTun", 
	Guid.Parse("12345678-1234-5678-abcd-0123456789ab"))) 
{
	// Use the adapter
}
```

You will need local administrator privileges, as this call will install the 
Wintun virtual network adapter driver.  It's possible to avoid needing 
local administrator credentials by installing the driver out-of-band
(e.g. via the WireGuard installer), but you will still need the appropriate 
privileges for adding and configuring a new adapter.  For help on how to do 
this, you will need to consult the documentation for running WireGuard as
a standard user on Windows.  WireGuard supports configuring the Wintun 
driver to allow usage under the built-in Network Configuration Operators 
group.

After creating the network adapter, you can create a session that enables
the network adapter for reading and writing packets by calling Start:

```
SharpTun.Interface.ITunSession.Start(int Capacity);
```

For instance:

```
using(var session = adapter.Start(0x400000)) 
{
	session.SendPacket(GetDataToSend());
	ProcessPacket(session.ReceivePacket());
}
```

As the Wintun library is a C-style library that consumes native resources
(e.g. buffers in the kernel), you should always use the Disposable pattern on
the SharpTun objects to ensure proper cleanup.

Should your application terminate, the Wintun library will perform automatic
clean-up for the resources held by your application.