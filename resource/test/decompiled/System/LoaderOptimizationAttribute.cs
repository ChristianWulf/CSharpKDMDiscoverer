using System;
using System.Runtime.InteropServices;
namespace System
{
	[AttributeUsage(AttributeTargets.Method), ComVisible(true)]
	public sealed class LoaderOptimizationAttribute : Attribute
	{
		internal byte _val;
		public LoaderOptimization Value
		{
			get
			{
				return (LoaderOptimization)this._val;
			}
		}
		public LoaderOptimizationAttribute(byte value)
		{
			this._val = value;
		}
		public LoaderOptimizationAttribute(LoaderOptimization value)
		{
			this._val = (byte)value;
		}
	}
}
