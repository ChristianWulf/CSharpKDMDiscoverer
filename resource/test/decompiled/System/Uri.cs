using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;
namespace System
{
	[TypeConverter(typeof(UriTypeConverter))]
	[Serializable]
	public class Uri : ISerializable
	{
		public static readonly string UriSchemeFile;
		public static readonly string UriSchemeFtp;
		public static readonly string UriSchemeGopher;
		public static readonly string UriSchemeHttp;
		public static readonly string UriSchemeHttps;
		public static readonly string UriSchemeMailto;
		public static readonly string UriSchemeNews;
		public static readonly string UriSchemeNntp;
		public static readonly string UriSchemeNetTcp;
		public static readonly string UriSchemeNetPipe;
		public static readonly string SchemeDelimiter;
		public string AbsolutePath
		{
			get
			{
			}
		}
		public string AbsoluteUri
		{
			get
			{
			}
		}
		public string Authority
		{
			get
			{
			}
		}
		public string Host
		{
			get
			{
			}
		}
		public UriHostNameType HostNameType
		{
			get
			{
			}
		}
		public bool IsDefaultPort
		{
			get
			{
			}
		}
		public bool IsFile
		{
			get
			{
			}
		}
		public bool IsLoopback
		{
			get
			{
			}
		}
		public bool IsUnc
		{
			get
			{
			}
		}
		public string LocalPath
		{
			get
			{
			}
		}
		public string PathAndQuery
		{
			get
			{
			}
		}
		public int Port
		{
			get
			{
			}
		}
		public string Query
		{
			get
			{
			}
		}
		public string Fragment
		{
			get
			{
			}
		}
		public string Scheme
		{
			get
			{
			}
		}
		public string OriginalString
		{
			get
			{
			}
		}
		public string DnsSafeHost
		{
			get
			{
			}
		}
		public bool IsAbsoluteUri
		{
			get
			{
			}
		}
		public string[] Segments
		{
			get
			{
			}
		}
		public bool UserEscaped
		{
			get
			{
			}
		}
		public string UserInfo
		{
			get
			{
			}
		}
		public Uri(string uriString)
		{
		}
		[Obsolete("The constructor has been deprecated. Please use new Uri(string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
		public Uri(string uriString, bool dontEscape)
		{
		}
		public Uri(string uriString, UriKind uriKind)
		{
		}
		public Uri(Uri baseUri, string relativeUri)
		{
		}
		[Obsolete("The constructor has been deprecated. Please new Uri(Uri, string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
		public Uri(Uri baseUri, string relativeUri, bool dontEscape)
		{
		}
		public Uri(Uri baseUri, Uri relativeUri)
		{
		}
		protected Uri(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}
		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
		protected void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}
		public static UriHostNameType CheckHostName(string name)
		{
		}
		public static bool CheckSchemeName(string schemeName)
		{
		}
		public static int FromHex(char digit)
		{
		}
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override int GetHashCode()
		{
		}
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override string ToString()
		{
		}
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool operator ==(Uri uri1, Uri uri2)
		{
		}
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool operator !=(Uri uri1, Uri uri2)
		{
		}
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override bool Equals(object comparand)
		{
		}
		public string GetLeftPart(UriPartial part)
		{
		}
		public static string HexEscape(char character)
		{
		}
		public static char HexUnescape(string pattern, ref int index)
		{
		}
		public static bool IsHexDigit(char character)
		{
		}
		public static bool IsHexEncoding(string pattern, int index)
		{
		}
		[Obsolete("The method has been deprecated. Please use MakeRelativeUri(Uri uri). http://go.microsoft.com/fwlink/?linkid=14202")]
		public string MakeRelative(Uri toUri)
		{
		}
		public Uri MakeRelativeUri(Uri uri)
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Parse()
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Canonicalize()
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Escape()
		{
		}
		[Obsolete("The method has been deprecated. Please use GetComponents() or static UnescapeDataString() to unescape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual string Unescape(string path)
		{
		}
		[Obsolete("The method has been deprecated. Please use GetComponents() or static EscapeUriString() to escape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected static string EscapeString(string str)
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void CheckSecurity()
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual bool IsReservedCharacter(char character)
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected static bool IsExcludedCharacter(char character)
		{
		}
		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual bool IsBadFileSystemCharacter(char character)
		{
		}
		public static bool TryCreate(string uriString, UriKind uriKind, out Uri result)
		{
		}
		public static bool TryCreate(Uri baseUri, string relativeUri, out Uri result)
		{
		}
		public static bool TryCreate(Uri baseUri, Uri relativeUri, out Uri result)
		{
		}
		public bool IsBaseOf(Uri uri)
		{
		}
		public string GetComponents(UriComponents components, UriFormat format)
		{
		}
		public bool IsWellFormedOriginalString()
		{
		}
		public static bool IsWellFormedUriString(string uriString, UriKind uriKind)
		{
		}
		public static int Compare(Uri uri1, Uri uri2, UriComponents partsToCompare, UriFormat compareFormat, StringComparison comparisonType)
		{
		}
		public static string UnescapeDataString(string stringToUnescape)
		{
		}
		public static string EscapeUriString(string stringToEscape)
		{
		}
		public static string EscapeDataString(string stringToEscape)
		{
		}
	}
}
