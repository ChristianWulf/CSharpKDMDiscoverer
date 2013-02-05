using System;
using System.Globalization;
namespace System
{
	[Serializable]
	internal sealed class CultureAwareComparer : StringComparer
	{
		private CompareInfo _compareInfo;
		private bool _ignoreCase;
		internal CultureAwareComparer(CultureInfo culture, bool ignoreCase)
		{
			this._compareInfo = culture.CompareInfo;
			this._ignoreCase = ignoreCase;
		}
		public override int Compare(string x, string y)
		{
			if (object.ReferenceEquals(x, y))
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return this._compareInfo.Compare(x, y, this._ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}
		public override bool Equals(string x, string y)
		{
			return object.ReferenceEquals(x, y) || (x != null && y != null && this._compareInfo.Compare(x, y, this._ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0);
		}
		public override int GetHashCode(string obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (this._ignoreCase)
			{
				return this._compareInfo.GetHashCodeOfString(obj, CompareOptions.IgnoreCase);
			}
			return this._compareInfo.GetHashCodeOfString(obj, CompareOptions.None);
		}
		public override bool Equals(object obj)
		{
			CultureAwareComparer cultureAwareComparer = obj as CultureAwareComparer;
			return cultureAwareComparer != null && this._ignoreCase == cultureAwareComparer._ignoreCase && this._compareInfo.Equals(cultureAwareComparer._compareInfo);
		}
		public override int GetHashCode()
		{
			int hashCode = this._compareInfo.GetHashCode();
			if (!this._ignoreCase)
			{
				return hashCode;
			}
			return ~hashCode;
		}
	}
}
