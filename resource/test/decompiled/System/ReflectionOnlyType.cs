using System;
namespace System
{
	[Serializable]
	internal class ReflectionOnlyType : RuntimeType
	{
		public override RuntimeTypeHandle TypeHandle
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
			}
		}
		private ReflectionOnlyType()
		{
		}
	}
}
