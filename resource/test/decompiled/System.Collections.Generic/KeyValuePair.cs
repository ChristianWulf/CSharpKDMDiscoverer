using System;
using System.Text;
namespace System.Collections.Generic
{
	[Serializable]
	public struct KeyValuePair<TKey, TValue>
	{
		private TKey key;
		private TValue value;
		public TKey Key
		{
			get
			{
				return this.key;
			}
		}
		public TValue Value
		{
			get
			{
				return this.value;
			}
		}
		public KeyValuePair(TKey key, TValue value)
		{
			this.key = key;
			this.value = value;
		}
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			if (this.Key != null)
			{
				StringBuilder arg_31_0 = stringBuilder;
				TKey tKey = this.Key;
				arg_31_0.Append(tKey.ToString());
			}
			stringBuilder.Append(", ");
			if (this.Value != null)
			{
				StringBuilder arg_65_0 = stringBuilder;
				TValue tValue = this.Value;
				arg_65_0.Append(tValue.ToString());
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}
	}
}
