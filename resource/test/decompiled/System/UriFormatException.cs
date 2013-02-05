using System;
using System.Runtime;
using System.Runtime.Serialization;
namespace System
{
	[Serializable]
	public class UriFormatException : FormatException, ISerializable
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public UriFormatException()
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public UriFormatException(string textString)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public UriFormatException(string textString, Exception e)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected UriFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}
	}
}
