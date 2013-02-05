using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	public interface IFormatProvider
	{
		object GetFormat(Type formatType);
	}
}
