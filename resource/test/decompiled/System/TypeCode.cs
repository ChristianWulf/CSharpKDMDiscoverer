using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public enum TypeCode
	{
		Empty,
		Object,
		DBNull,
		Boolean,
		Char,
		SByte,
		Byte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Decimal,
		DateTime,
		String = 18
	}
}
