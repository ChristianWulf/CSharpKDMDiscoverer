using System;
using System.Diagnostics.Contracts;
using System.Reflection;
namespace System
{
	internal abstract class TypeContracts : Type
	{
		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			return Contract.Result<FieldInfo[]>();
		}
		public new static Type GetTypeFromHandle(RuntimeTypeHandle handle)
		{
			return Contract.Result<Type>();
		}
		public override Type[] GetInterfaces()
		{
			return Contract.Result<Type[]>();
		}
	}
}
