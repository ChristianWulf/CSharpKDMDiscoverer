using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class OperationCanceledException : SystemException
	{
		[NonSerialized]
		private CancellationToken _cancellationToken;
		public CancellationToken CancellationToken
		{
			get
			{
				return this._cancellationToken;
			}
			private set
			{
				this._cancellationToken = value;
			}
		}
		public OperationCanceledException() : base(Environment.GetResourceString("OperationCanceled"))
		{
			base.SetErrorCode(-2146233029);
		}
		public OperationCanceledException(string message) : base(message)
		{
			base.SetErrorCode(-2146233029);
		}
		public OperationCanceledException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2146233029);
		}
		public OperationCanceledException(CancellationToken token) : this()
		{
			this.CancellationToken = token;
		}
		public OperationCanceledException(string message, CancellationToken token) : this(message)
		{
			this.CancellationToken = token;
		}
		public OperationCanceledException(string message, Exception innerException, CancellationToken token) : this(message, innerException)
		{
			this.CancellationToken = token;
		}
		protected OperationCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
