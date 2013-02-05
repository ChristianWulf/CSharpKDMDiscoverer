using System;
namespace System
{
	internal abstract class IFormattableContract : IFormattable
	{
		string IFormattable.ToString(string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException();
		}
	}
}
