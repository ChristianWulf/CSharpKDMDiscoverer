using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class MissingFieldException : MissingMemberException, ISerializable
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
				return Environment.GetResourceString("MissingField_Name", new object[]
				{
					((this.Signature != null) ? (MissingMemberException.FormatSignature(this.Signature) + " ") : "") + this.ClassName + "." + this.MemberName
				});
			}
		}
		public MissingFieldException() : base(Environment.GetResourceString("Arg_MissingFieldException"))
		{
			base.SetErrorCode(-2146233071);
		}
		public MissingFieldException(string message) : base(message)
		{
			base.SetErrorCode(-2146233071);
		}
		public MissingFieldException(string message, Exception inner) : base(message, inner)
		{
			base.SetErrorCode(-2146233071);
		}
		[SecuritySafeCritical]
		protected MissingFieldException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		private MissingFieldException(string className, string fieldName, byte[] signature)
		{
			this.ClassName = className;
			this.MemberName = fieldName;
			this.Signature = signature;
		}
		public MissingFieldException(string className, string fieldName)
		{
			this.ClassName = className;
			this.MemberName = fieldName;
		}
	}
}
