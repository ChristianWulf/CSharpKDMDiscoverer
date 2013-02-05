using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false), ComVisible(true)]
	[Serializable]
	public sealed class CLSCompliantAttribute : Attribute
	{
		private bool m_compliant;
		public bool IsCompliant
		{
			get
			{
				return this.m_compliant;
			}
		}
		public CLSCompliantAttribute(bool isCompliant)
		{
			this.m_compliant = isCompliant;
		}
	}
}
