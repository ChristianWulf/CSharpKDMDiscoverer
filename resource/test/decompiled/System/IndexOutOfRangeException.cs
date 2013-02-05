using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public sealed class IndexOutOfRangeException : SystemException
	{
		public IndexOutOfRangeException() : base(Environment.GetResourceString("Arg_IndexOutOfRangeException"))
		{
			base.SetErrorCode(-2146233080);
		}
		public IndexOutOfRangeException(string message) : base(message)
		{
			base.SetErrorCode(-2146233080);
		}
		public IndexOutOfRangeException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2146233080);
		}
		internal IndexOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
