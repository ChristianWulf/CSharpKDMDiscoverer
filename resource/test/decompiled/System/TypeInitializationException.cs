using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public sealed class TypeInitializationException : SystemException
	{
		private string _typeName;
		public string TypeName
		{
			get
			{
				if (this._typeName == null)
				{
					return string.Empty;
				}
				return this._typeName;
			}
		}
		private TypeInitializationException() : base(Environment.GetResourceString("TypeInitialization_Default"))
		{
			base.SetErrorCode(-2146233036);
		}
		private TypeInitializationException(string message) : base(message)
		{
			base.SetErrorCode(-2146233036);
		}
		public TypeInitializationException(string fullTypeName, Exception innerException) : base(Environment.GetResourceString("TypeInitialization_Type", new object[]
		{
			fullTypeName
		}), innerException)
		{
			this._typeName = fullTypeName;
			base.SetErrorCode(-2146233036);
		}
		internal TypeInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._typeName = info.GetString("TypeName");
		}
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("TypeName", this.TypeName, typeof(string));
		}
	}
}
