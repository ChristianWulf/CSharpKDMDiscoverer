using System;
using System.Runtime.CompilerServices;
using System.Security;
namespace System
{
	internal static class ConfigServer
	{
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void RunParser(IConfigHandler factory, string fileName);
	}
}
