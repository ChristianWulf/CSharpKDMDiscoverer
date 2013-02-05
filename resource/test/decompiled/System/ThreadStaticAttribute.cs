using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false), ComVisible(true)]
	[Serializable]
	public class ThreadStaticAttribute : Attribute
	{
	}
}
