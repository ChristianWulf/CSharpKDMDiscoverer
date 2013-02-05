using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public enum StringComparison
	{
		CurrentCulture,
		CurrentCultureIgnoreCase,
		InvariantCulture,
		InvariantCultureIgnoreCase,
		Ordinal,
		OrdinalIgnoreCase
	}
}
