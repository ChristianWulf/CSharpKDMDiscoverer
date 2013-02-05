using System;
namespace System
{
	[Serializable]
	public sealed class ConsoleCancelEventArgs : EventArgs
	{
		private ConsoleSpecialKey _type;
		private bool _cancel;
		public bool Cancel
		{
			get
			{
				return this._cancel;
			}
			set
			{
				if (this.SpecialKey == ConsoleSpecialKey.ControlBreak && value)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CantCancelCtrlBreak"));
				}
				this._cancel = value;
			}
		}
		public ConsoleSpecialKey SpecialKey
		{
			get
			{
				return this._type;
			}
		}
		internal ConsoleCancelEventArgs(ConsoleSpecialKey type)
		{
			this._type = type;
			this._cancel = false;
		}
	}
}
