using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.Enum, Inherited = false), ComVisible(true)]
	[Serializable]
	public class FlagsAttribute : Attribute
	{
	}
}
