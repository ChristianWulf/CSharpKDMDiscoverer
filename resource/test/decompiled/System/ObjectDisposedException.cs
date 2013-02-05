using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class ObjectDisposedException : InvalidOperationException
	{
		private string objectName;
		public override string Message
		{
			[SecuritySafeCritical]
			get
			{
				string text = this.ObjectName;
				if (text == null || text.Length == 0)
				{
					return base.Message;
				}
				string resourceString = Environment.GetResourceString("ObjectDisposed_ObjectName_Name", new object[]
				{
					text
				});
				return base.Message + Environment.NewLine + resourceString;
			}
		}
		public string ObjectName
		{
			get
			{
				if (this.objectName == null)
				{
					return string.Empty;
				}
				return this.objectName;
			}
		}
		private ObjectDisposedException() : this(null, Environment.GetResourceString("ObjectDisposed_Generic"))
		{
		}
		public ObjectDisposedException(string objectName) : this(objectName, Environment.GetResourceString("ObjectDisposed_Generic"))
		{
		}
		public ObjectDisposedException(string objectName, string message) : base(message)
		{
			base.SetErrorCode(-2146232798);
			this.objectName = objectName;
		}
		public ObjectDisposedException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2146232798);
		}
		[SecuritySafeCritical]
		protected ObjectDisposedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.objectName = info.GetString("ObjectName");
		}
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ObjectName", this.ObjectName, typeof(string));
		}
	}
}
