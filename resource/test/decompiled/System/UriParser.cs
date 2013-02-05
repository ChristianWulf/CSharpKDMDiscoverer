using System;
using System.Runtime;
namespace System
{
	public abstract class UriParser
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected UriParser()
		{
		}
		protected virtual UriParser OnNewUri()
		{
		}
		protected virtual void OnRegister(string schemeName, int defaultPort)
		{
		}
		protected virtual void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
		{
		}
		protected virtual string Resolve(Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
		{
		}
		protected virtual bool IsBaseOf(Uri baseUri, Uri relativeUri)
		{
		}
		protected virtual string GetComponents(Uri uri, UriComponents components, UriFormat format)
		{
		}
		protected virtual bool IsWellFormedOriginalString(Uri uri)
		{
		}
		public static void Register(UriParser uriParser, string schemeName, int defaultPort)
		{
		}
		public static bool IsKnownScheme(string schemeName)
		{
		}
		static UriParser()
		{
		}
	}
}
