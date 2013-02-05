using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false), ComVisible(true)]
	public sealed class ParamArrayAttribute : Attribute
	{
	}
}
