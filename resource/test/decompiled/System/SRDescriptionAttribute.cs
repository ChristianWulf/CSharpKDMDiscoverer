using System;
using System.ComponentModel;
namespace System
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class SRDescriptionAttribute : DescriptionAttribute
	{
		public override string Description
		{
			get
			{
			}
		}
		public SRDescriptionAttribute(string description)
		{
		}
	}
}
