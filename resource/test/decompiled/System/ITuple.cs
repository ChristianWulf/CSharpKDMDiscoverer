using System;
using System.Collections;
using System.Text;
namespace System
{
	internal interface ITuple
	{
		int Size
		{
			get;
		}
		string ToString(StringBuilder sb);
		int GetHashCode(IEqualityComparer comparer);
	}
}
