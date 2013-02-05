using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class MissingMethodException : MissingMemberException, ISerializable
	{
		public override string Message
		{
			[SecuritySafeCritical]
			get
			{
				if (this.ClassName == null)
				{
					return base.Message;
				}
				return Environment.GetResourceString("MissingMethod_Name", new object[]
				{
					this.ClassName + "." + this.MemberName + ((this.Signature != null) ? (" " + MissingMemberException.FormatSignature(this.Signature)) : "")
				});
			}
		}
		public MissingMethodException() : base(Environment.GetResourceString("Arg_MissingMethodException"))
		{
			base.SetErrorCode(-2146233069);
		}
		public MissingMethodException(string message) : base(message)
		{
			base.SetErrorCode(-2146233069);
		}
		public MissingMethodException(string message, Exception inner) : base(message, inner)
		{
			base.SetErrorCode(-2146233069);
		}
		[SecuritySafeCritical]
		protected MissingMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		private MissingMethodException(string className, string methodName, byte[] signature)
		{
			this.ClassName = className;
			this.MemberName = methodName;
			this.Signature = signature;
		}
		public MissingMethodException(string className, string methodName)
		{
			this.ClassName = className;
			this.MemberName = methodName;
		}
	}
}
