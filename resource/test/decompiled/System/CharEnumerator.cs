using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public sealed class CharEnumerator : ICloneable, IEnumerator<char>, IEnumerator, IDisposable
	{
		private string str;
		private int index;
		private char currentElement;
		object IEnumerator.Current
		{
			get
			{
				if (this.index == -1)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
				}
				if (this.index >= this.str.Length)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
				}
				return this.currentElement;
			}
		}
		public char Current
		{
			get
			{
				if (this.index == -1)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
				}
				if (this.index >= this.str.Length)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
				}
				return this.currentElement;
			}
		}
		internal CharEnumerator(string str)
		{
			this.str = str;
			this.index = -1;
		}
		[SecuritySafeCritical]
		public object Clone()
		{
			return base.MemberwiseClone();
		}
		public bool MoveNext()
		{
			if (this.index < this.str.Length - 1)
			{
				this.index++;
				this.currentElement = this.str[this.index];
				return true;
			}
			this.index = this.str.Length;
			return false;
		}
		public void Dispose()
		{
			if (this.str != null)
			{
				this.index = this.str.Length;
			}
			this.str = null;
		}
		public void Reset()
		{
			this.currentElement = '\0';
			this.index = -1;
		}
	}
}
