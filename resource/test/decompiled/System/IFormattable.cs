using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	public interface IFormattable
	{
		string ToString(string format, IFormatProvider formatProvider);
	}
}
