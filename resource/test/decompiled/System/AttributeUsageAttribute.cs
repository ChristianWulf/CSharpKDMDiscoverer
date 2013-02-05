using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true), AttributeUsage(AttributeTargets.Class, Inherited = true)]
	[Serializable]
	public sealed class AttributeUsageAttribute : Attribute
	{
		internal AttributeTargets m_attributeTarget = AttributeTargets.All;
		internal bool m_allowMultiple;
		internal bool m_inherited = true;
		internal static AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);
		public AttributeTargets ValidOn
		{
			get
			{
				return this.m_attributeTarget;
			}
		}
		public bool AllowMultiple
		{
			get
			{
				return this.m_allowMultiple;
			}
			set
			{
				this.m_allowMultiple = value;
			}
		}
		public bool Inherited
		{
			get
			{
				return this.m_inherited;
			}
			set
			{
				this.m_inherited = value;
			}
		}
		public AttributeUsageAttribute(AttributeTargets validOn)
		{
			this.m_attributeTarget = validOn;
		}
		internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
		{
			this.m_attributeTarget = validOn;
			this.m_allowMultiple = allowMultiple;
			this.m_inherited = inherited;
		}
	}
}
