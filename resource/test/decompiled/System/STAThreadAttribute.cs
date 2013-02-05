using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true), AttributeUsage(AttributeTargets.Method)]
	public sealed class STAThreadAttribute : Attribute
	{
	}
}
