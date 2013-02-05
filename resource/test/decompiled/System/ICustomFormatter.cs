using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	public interface ICustomFormatter
	{
		string Format(string format, object arg, IFormatProvider formatProvider);
	}
}
