using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.Method), ComVisible(true)]
	public sealed class MTAThreadAttribute : Attribute
	{
	}
}
