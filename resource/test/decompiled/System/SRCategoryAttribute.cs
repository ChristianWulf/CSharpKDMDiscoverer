using System;
using System.ComponentModel;
namespace System
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class SRCategoryAttribute : CategoryAttribute
	{
		public SRCategoryAttribute(string category)
		{
		}
		protected override string GetLocalizedString(string value)
		{
		}
	}
}
