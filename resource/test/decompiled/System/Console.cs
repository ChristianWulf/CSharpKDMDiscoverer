using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
namespace System
{
	public static class Console
	{
		[Flags]
		internal enum ControlKeyState
		{
			RightAltPressed = 1,
			LeftAltPressed = 2,
			RightCtrlPressed = 4,
			LeftCtrlPressed = 8,
			ShiftPressed = 16,
			NumLockOn = 32,
			ScrollLockOn = 64,
			CapsLockOn = 128,
			EnhancedKey = 256
		}
		internal sealed class ControlCHooker : CriticalFinalizerObject
		{
			private bool _hooked;
			[SecurityCritical]
			private Win32Native.ConsoleCtrlHandlerRoutine _handler;
			[SecurityCritical]
			internal ControlCHooker()
			{
				this._handler = new Win32Native.ConsoleCtrlHandlerRoutine(Console.BreakEvent);
			}
			~ControlCHooker()
			{
				this.Unhook();
			}
			[SecuritySafeCritical]
			internal void Hook()
			{
				if (!this._hooked)
				{
					if (!Win32Native.SetConsoleCtrlHandler(this._handler, true))
					{
						__Error.WinIOError();
					}
					this._hooked = true;
				}
			}
			[SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal void Unhook()
			{
				if (this._hooked)
				{
					if (!Win32Native.SetConsoleCtrlHandler(this._handler, false))
					{
						__Error.WinIOError();
					}
					this._hooked = false;
				}
			}
		}
		private sealed class ControlCDelegateData
		{
			internal ConsoleSpecialKey ControlKey;
			internal bool Cancel;
			internal bool DelegateStarted;
			internal ManualResetEvent CompletionEvent;
			internal ConsoleCancelEventHandler CancelCallbacks;
			internal ControlCDelegateData(ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks)
			{
				this.ControlKey = controlKey;
				this.CancelCallbacks = cancelCallbacks;
				this.CompletionEvent = new ManualResetEvent(false);
			}
		}
		private const int _DefaultConsoleBufferSize = 256;
		private const short AltVKCode = 18;
		private const int NumberLockVKCode = 144;
		private const int CapsLockVKCode = 20;
		private const int MinBeepFrequency = 37;
		private const int MaxBeepFrequency = 32767;
		private const int MaxConsoleTitleLength = 24500;
		private static TextReader _in;
		private static TextWriter _out;
		private static TextWriter _error;
		private static ConsoleCancelEventHandler _cancelCallbacks;
		private static Console.ControlCHooker _hooker;
		[SecurityCritical]
		private static Win32Native.InputRecord _cachedInputRecord;
		private static bool _haveReadDefaultColors;
		private static byte _defaultColors;
		private static bool _wasOutRedirected;
		private static bool _wasErrorRedirected;
		private static Encoding _inputEncoding;
		private static Encoding _outputEncoding;
		private static object s_InternalSyncObject;
		private static IntPtr _consoleInputHandle;
		private static IntPtr _consoleOutputHandle;
		public static event ConsoleCancelEventHandler CancelKeyPress
		{
			[SecuritySafeCritical]
			add
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (Console.InternalSyncObject)
				{
					Console._cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Combine(Console._cancelCallbacks, value);
					if (Console._hooker == null)
					{
						Console._hooker = new Console.ControlCHooker();
						Console._hooker.Hook();
					}
				}
			}
			[SecuritySafeCritical]
			remove
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (Console.InternalSyncObject)
				{
					Console._cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Remove(Console._cancelCallbacks, value);
					if (Console._hooker != null && Console._cancelCallbacks == null)
					{
						Console._hooker.Unhook();
					}
				}
			}
		}
		private static object InternalSyncObject
		{
			get
			{
				if (Console.s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange<object>(ref Console.s_InternalSyncObject, value, null);
				}
				return Console.s_InternalSyncObject;
			}
		}
		private static IntPtr ConsoleInputHandle
		{
			[SecurityCritical]
			get
			{
				if (Console._consoleInputHandle == IntPtr.Zero)
				{
					Console._consoleInputHandle = Win32Native.GetStdHandle(-10);
				}
				return Console._consoleInputHandle;
			}
		}
		private static IntPtr ConsoleOutputHandle
		{
			[SecurityCritical]
			get
			{
				if (Console._consoleOutputHandle == IntPtr.Zero)
				{
					Console._consoleOutputHandle = Win32Native.GetStdHandle(-11);
				}
				return Console._consoleOutputHandle;
			}
		}
		public static TextWriter Error
		{
			[SecuritySafeCritical]
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (Console._error == null)
				{
					Console.InitializeStdOutError(false);
				}
				return Console._error;
			}
		}
		public static TextReader In
		{
			[SecuritySafeCritical]
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (Console._in == null)
				{
					lock (Console.InternalSyncObject)
					{
						if (Console._in == null)
						{
							Stream stream = Console.OpenStandardInput(256);
							TextReader @in;
							if (stream == Stream.Null)
							{
								@in = StreamReader.Null;
							}
							else
							{
								Encoding encoding = Encoding.GetEncoding((int)Win32Native.GetConsoleCP());
								@in = TextReader.Synchronized(new StreamReader(stream, encoding, false, 256, false));
							}
							Thread.MemoryBarrier();
							Console._in = @in;
						}
					}
				}
				return Console._in;
			}
		}
		public static TextWriter Out
		{
			[SecuritySafeCritical]
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (Console._out == null)
				{
					Console.InitializeStdOutError(true);
				}
				return Console._out;
			}
		}
		public static Encoding InputEncoding
		{
			[SecuritySafeCritical]
			get
			{
				Encoding inputEncoding;
				lock (Console.InternalSyncObject)
				{
					if (Console._inputEncoding != null)
					{
						inputEncoding = Console._inputEncoding;
					}
					else
					{
						uint consoleCP = Win32Native.GetConsoleCP();
						Console._inputEncoding = Encoding.GetEncoding((int)consoleCP);
						inputEncoding = Console._inputEncoding;
					}
				}
				return inputEncoding;
			}
			[SecuritySafeCritical]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				uint codePage = (uint)value.CodePage;
				lock (Console.InternalSyncObject)
				{
					if (!Win32Native.SetConsoleCP(codePage))
					{
						__Error.WinIOError();
					}
					Console._inputEncoding = (Encoding)value.Clone();
					Console._in = null;
				}
			}
		}
		public static Encoding OutputEncoding
		{
			[SecuritySafeCritical]
			get
			{
				Encoding outputEncoding;
				lock (Console.InternalSyncObject)
				{
					if (Console._outputEncoding != null)
					{
						outputEncoding = Console._outputEncoding;
					}
					else
					{
						uint consoleOutputCP = Win32Native.GetConsoleOutputCP();
						Console._outputEncoding = Encoding.GetEncoding((int)consoleOutputCP);
						outputEncoding = Console._outputEncoding;
					}
				}
				return outputEncoding;
			}
			[SecuritySafeCritical]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (Console.InternalSyncObject)
				{
					if (Console._out != null && !Console._wasOutRedirected)
					{
						Console._out.Flush();
						Console._out = null;
					}
					if (Console._error != null && !Console._wasErrorRedirected)
					{
						Console._error.Flush();
						Console._error = null;
					}
					uint codePage = (uint)value.CodePage;
					if (!Win32Native.SetConsoleOutputCP(codePage))
					{
						__Error.WinIOError();
					}
					Console._outputEncoding = (Encoding)value.Clone();
				}
			}
		}
		public static ConsoleColor BackgroundColor
		{
			[SecuritySafeCritical]
			get
			{
				bool flag;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo(false, out flag);
				if (!flag)
				{
					return ConsoleColor.Black;
				}
				Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 240);
				return Console.ColorAttributeToConsoleColor(c);
			}
			[SecuritySafeCritical]
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				Win32Native.Color color = Console.ConsoleColorToColorAttribute(value, true);
				bool flag;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo(false, out flag);
				if (!flag)
				{
					return;
				}
				short num = bufferInfo.wAttributes;
				num &= -241;
				num = (short)((ushort)num | (ushort)color);
				Win32Native.SetConsoleTextAttribute(Console.ConsoleOutputHandle, num);
			}
		}
		public static ConsoleColor ForegroundColor
		{
			[SecuritySafeCritical]
			get
			{
				bool flag;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo(false, out flag);
				if (!flag)
				{
					return ConsoleColor.Gray;
				}
				Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 15);
				return Console.ColorAttributeToConsoleColor(c);
			}
			[SecuritySafeCritical]
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				Win32Native.Color color = Console.ConsoleColorToColorAttribute(value, false);
				bool flag;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo(false, out flag);
				if (!flag)
				{
					return;
				}
				short num = bufferInfo.wAttributes;
				num &= -16;
				num = (short)((ushort)num | (ushort)color);
				Win32Native.SetConsoleTextAttribute(Console.ConsoleOutputHandle, num);
			}
		}
		public static int BufferHeight
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().dwSize.Y;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetBufferSize(Console.BufferWidth, value);
			}
		}
		public static int BufferWidth
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().dwSize.X;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetBufferSize(value, Console.BufferHeight);
			}
		}
		public static int WindowHeight
		{
			[SecuritySafeCritical]
			get
			{
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
				return (int)(bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top + 1);
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetWindowSize(Console.WindowWidth, value);
			}
		}
		public static int WindowWidth
		{
			[SecuritySafeCritical]
			get
			{
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
				return (int)(bufferInfo.srWindow.Right - bufferInfo.srWindow.Left + 1);
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetWindowSize(value, Console.WindowHeight);
			}
		}
		public static int LargestWindowWidth
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Win32Native.GetLargestConsoleWindowSize(Console.ConsoleOutputHandle).X;
			}
		}
		public static int LargestWindowHeight
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Win32Native.GetLargestConsoleWindowSize(Console.ConsoleOutputHandle).Y;
			}
		}
		public static int WindowLeft
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().srWindow.Left;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetWindowPosition(value, Console.WindowTop);
			}
		}
		public static int WindowTop
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().srWindow.Top;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetWindowPosition(Console.WindowLeft, value);
			}
		}
		public static int CursorLeft
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().dwCursorPosition.X;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetCursorPosition(value, Console.CursorTop);
			}
		}
		public static int CursorTop
		{
			[SecuritySafeCritical]
			get
			{
				return (int)Console.GetBufferInfo().dwCursorPosition.Y;
			}
			[SecuritySafeCritical]
			set
			{
				Console.SetCursorPosition(Console.CursorLeft, value);
			}
		}
		public static int CursorSize
		{
			[SecuritySafeCritical]
			get
			{
				IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
				Win32Native.CONSOLE_CURSOR_INFO cONSOLE_CURSOR_INFO;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
				return cONSOLE_CURSOR_INFO.dwSize;
			}
			[SecuritySafeCritical]
			set
			{
				if (value < 1 || value > 100)
				{
					throw new ArgumentOutOfRangeException("value", value, Environment.GetResourceString("ArgumentOutOfRange_CursorSize"));
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
				Win32Native.CONSOLE_CURSOR_INFO cONSOLE_CURSOR_INFO;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
				cONSOLE_CURSOR_INFO.dwSize = value;
				if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
			}
		}
		public static bool CursorVisible
		{
			[SecuritySafeCritical]
			get
			{
				IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
				Win32Native.CONSOLE_CURSOR_INFO cONSOLE_CURSOR_INFO;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
				return cONSOLE_CURSOR_INFO.bVisible;
			}
			[SecuritySafeCritical]
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
				Win32Native.CONSOLE_CURSOR_INFO cONSOLE_CURSOR_INFO;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
				cONSOLE_CURSOR_INFO.bVisible = value;
				if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cONSOLE_CURSOR_INFO))
				{
					__Error.WinIOError();
				}
			}
		}
		public static string Title
		{
			[SecuritySafeCritical]
			get
			{
				StringBuilder stringBuilder = new StringBuilder(24501);
				Win32Native.SetLastError(0);
				int consoleTitle = Win32Native.GetConsoleTitle(stringBuilder, stringBuilder.Capacity);
				if (consoleTitle == 0)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 0)
					{
						stringBuilder.Length = 0;
					}
					else
					{
						__Error.WinIOError(lastWin32Error, string.Empty);
					}
				}
				else
				{
					if (consoleTitle > 24500)
					{
						throw new InvalidOperationException(Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
					}
				}
				return stringBuilder.ToString();
			}
			[SecuritySafeCritical]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Length > 24500)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				if (!Win32Native.SetConsoleTitle(value))
				{
					__Error.WinIOError();
				}
			}
		}
		public static bool KeyAvailable
		{
			[SecuritySafeCritical]
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (Console._cachedInputRecord.eventType == 1)
				{
					return true;
				}
				Win32Native.InputRecord ir = default(Win32Native.InputRecord);
				int num = 0;
				while (true)
				{
					if (!Win32Native.PeekConsoleInput(Console.ConsoleInputHandle, out ir, 1, out num))
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error == 6)
						{
							break;
						}
						__Error.WinIOError(lastWin32Error, "stdin");
					}
					if (num == 0)
					{
						return false;
					}
					if (Console.IsKeyDownEvent(ir) && !Console.IsModKey(ir))
					{
						return true;
					}
					if (!Win32Native.ReadConsoleInput(Console.ConsoleInputHandle, out ir, 1, out num))
					{
						__Error.WinIOError();
					}
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleKeyAvailableOnFile"));
			}
		}
		public static bool NumberLock
		{
			[SecuritySafeCritical]
			get
			{
				short keyState = Win32Native.GetKeyState(144);
				return (keyState & 1) == 1;
			}
		}
		public static bool CapsLock
		{
			[SecuritySafeCritical]
			get
			{
				short keyState = Win32Native.GetKeyState(20);
				return (keyState & 1) == 1;
			}
		}
		public static bool TreatControlCAsInput
		{
			[SecuritySafeCritical]
			get
			{
				IntPtr consoleInputHandle = Console.ConsoleInputHandle;
				if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
				}
				int num = 0;
				if (!Win32Native.GetConsoleMode(consoleInputHandle, out num))
				{
					__Error.WinIOError();
				}
				return (num & 1) == 0;
			}
			[SecuritySafeCritical]
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				IntPtr consoleInputHandle = Console.ConsoleInputHandle;
				if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
				}
				int num = 0;
				bool consoleMode = Win32Native.GetConsoleMode(consoleInputHandle, out num);
				if (value)
				{
					num &= -2;
				}
				else
				{
					num |= 1;
				}
				if (!Win32Native.SetConsoleMode(consoleInputHandle, num))
				{
					__Error.WinIOError();
				}
			}
		}
		[SecuritySafeCritical]
		private static void InitializeStdOutError(bool stdout)
		{
			lock (Console.InternalSyncObject)
			{
				if (!stdout || Console._out == null)
				{
					if (stdout || Console._error == null)
					{
						TextWriter textWriter = null;
						Stream stream;
						if (stdout)
						{
							stream = Console.OpenStandardOutput(256);
						}
						else
						{
							stream = Console.OpenStandardError(256);
						}
						if (stream == Stream.Null)
						{
							textWriter = TextWriter.Synchronized(StreamWriter.Null);
						}
						else
						{
							int consoleOutputCP = (int)Win32Native.GetConsoleOutputCP();
							Encoding encoding = Encoding.GetEncoding(consoleOutputCP);
							textWriter = TextWriter.Synchronized(new StreamWriter(stream, encoding, 256, false)
							{
								HaveWrittenPreamble = true, 
								AutoFlush = true
							});
						}
						if (stdout)
						{
							Console._out = textWriter;
						}
						else
						{
							Console._error = textWriter;
						}
					}
				}
			}
		}
		[SecuritySafeCritical]
		private static Stream GetStandardFile(int stdHandleName, FileAccess access, int bufferSize)
		{
			IntPtr stdHandle = Win32Native.GetStdHandle(stdHandleName);
			SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, false);
			if (safeFileHandle.IsInvalid)
			{
				safeFileHandle.SetHandleAsInvalid();
				return Stream.Null;
			}
			if (stdHandleName != -10 && !Console.ConsoleHandleIsValid(safeFileHandle))
			{
				return Stream.Null;
			}
			return new __ConsoleStream(safeFileHandle, access);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Beep()
		{
			Console.Beep(800, 200);
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Beep(int frequency, int duration)
		{
			if (frequency < 37 || frequency > 32767)
			{
				throw new ArgumentOutOfRangeException("frequency", frequency, Environment.GetResourceString("ArgumentOutOfRange_BeepFrequency", new object[]
				{
					37, 
					32767
				}));
			}
			if (duration <= 0)
			{
				throw new ArgumentOutOfRangeException("duration", duration, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Win32Native.Beep(frequency, duration);
		}
		[SecuritySafeCritical]
		public static void Clear()
		{
			Win32Native.COORD cOORD = default(Win32Native.COORD);
			IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
			if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
			}
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
			int num = (int)(bufferInfo.dwSize.X * bufferInfo.dwSize.Y);
			int num2 = 0;
			if (!Win32Native.FillConsoleOutputCharacter(consoleOutputHandle, ' ', num, cOORD, out num2))
			{
				__Error.WinIOError();
			}
			num2 = 0;
			if (!Win32Native.FillConsoleOutputAttribute(consoleOutputHandle, bufferInfo.wAttributes, num, cOORD, out num2))
			{
				__Error.WinIOError();
			}
			if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cOORD))
			{
				__Error.WinIOError();
			}
		}
		[SecurityCritical]
		private static Win32Native.Color ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
		{
			if ((color & (ConsoleColor)(-16)) != ConsoleColor.Black)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"));
			}
			Win32Native.Color color2 = (Win32Native.Color)color;
			if (isBackground)
			{
				color2 <<= 4;
			}
			return color2;
		}
		[SecurityCritical]
		private static ConsoleColor ColorAttributeToConsoleColor(Win32Native.Color c)
		{
			if ((short)(c & Win32Native.Color.BackgroundMask) != 0)
			{
				c >>= 4;
			}
			return (ConsoleColor)c;
		}
		[SecuritySafeCritical]
		public static void ResetColor()
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			bool flag;
			Console.GetBufferInfo(false, out flag);
			if (!flag)
			{
				return;
			}
			short attributes = (short)Console._defaultColors;
			Win32Native.SetConsoleTextAttribute(Console.ConsoleOutputHandle, attributes);
		}
		[SecuritySafeCritical]
		public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
		{
			Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, Console.BackgroundColor);
		}
		[SecuritySafeCritical]
		public unsafe static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			if (sourceForeColor < ConsoleColor.Black || sourceForeColor > ConsoleColor.White)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceForeColor");
			}
			if (sourceBackColor < ConsoleColor.Black || sourceBackColor > ConsoleColor.White)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceBackColor");
			}
			Win32Native.COORD dwSize = Console.GetBufferInfo().dwSize;
			if (sourceLeft < 0 || sourceLeft > (int)dwSize.X)
			{
				throw new ArgumentOutOfRangeException("sourceLeft", sourceLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceTop < 0 || sourceTop > (int)dwSize.Y)
			{
				throw new ArgumentOutOfRangeException("sourceTop", sourceTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceWidth < 0 || sourceWidth > (int)dwSize.X - sourceLeft)
			{
				throw new ArgumentOutOfRangeException("sourceWidth", sourceWidth, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceHeight < 0 || sourceTop > (int)dwSize.Y - sourceHeight)
			{
				throw new ArgumentOutOfRangeException("sourceHeight", sourceHeight, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (targetLeft < 0 || targetLeft > (int)dwSize.X)
			{
				throw new ArgumentOutOfRangeException("targetLeft", targetLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (targetTop < 0 || targetTop > (int)dwSize.Y)
			{
				throw new ArgumentOutOfRangeException("targetTop", targetTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceWidth == 0 || sourceHeight == 0)
			{
				return;
			}
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CHAR_INFO[] array = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
			dwSize.X = (short)sourceWidth;
			dwSize.Y = (short)sourceHeight;
			Win32Native.COORD bufferCoord = default(Win32Native.COORD);
			Win32Native.SMALL_RECT sMALL_RECT = default(Win32Native.SMALL_RECT);
			sMALL_RECT.Left = (short)sourceLeft;
			sMALL_RECT.Right = (short)(sourceLeft + sourceWidth - 1);
			sMALL_RECT.Top = (short)sourceTop;
			sMALL_RECT.Bottom = (short)(sourceTop + sourceHeight - 1);
			bool flag;
			fixed (Win32Native.CHAR_INFO* ptr = array)
			{
				flag = Win32Native.ReadConsoleOutput(Console.ConsoleOutputHandle, ptr, dwSize, bufferCoord, ref sMALL_RECT);
			}
			if (!flag)
			{
				__Error.WinIOError();
			}
			Win32Native.COORD cOORD = default(Win32Native.COORD);
			cOORD.X = (short)sourceLeft;
			Win32Native.Color color = Console.ConsoleColorToColorAttribute(sourceBackColor, true);
			color |= Console.ConsoleColorToColorAttribute(sourceForeColor, false);
			short wColorAttribute = (short)color;
			for (int i = sourceTop; i < sourceTop + sourceHeight; i++)
			{
				cOORD.Y = (short)i;
				int num;
				if (!Win32Native.FillConsoleOutputCharacter(Console.ConsoleOutputHandle, sourceChar, sourceWidth, cOORD, out num))
				{
					__Error.WinIOError();
				}
				if (!Win32Native.FillConsoleOutputAttribute(Console.ConsoleOutputHandle, wColorAttribute, sourceWidth, cOORD, out num))
				{
					__Error.WinIOError();
				}
			}
			Win32Native.SMALL_RECT sMALL_RECT2 = default(Win32Native.SMALL_RECT);
			sMALL_RECT2.Left = (short)targetLeft;
			sMALL_RECT2.Right = (short)(targetLeft + sourceWidth);
			sMALL_RECT2.Top = (short)targetTop;
			sMALL_RECT2.Bottom = (short)(targetTop + sourceHeight);
			fixed (Win32Native.CHAR_INFO* ptr2 = array)
			{
				flag = Win32Native.WriteConsoleOutput(Console.ConsoleOutputHandle, ptr2, dwSize, bufferCoord, ref sMALL_RECT2);
			}
		}
		[SecurityCritical]
		private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
		{
			bool flag;
			return Console.GetBufferInfo(true, out flag);
		}
		[SecuritySafeCritical]
		private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
		{
			succeeded = false;
			IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
			if (!(consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE))
			{
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO result;
				bool consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(consoleOutputHandle, out result);
				if (!consoleScreenBufferInfo)
				{
					consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-12), out result);
					if (!consoleScreenBufferInfo)
					{
						consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-10), out result);
					}
					if (!consoleScreenBufferInfo)
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error == 6 && !throwOnNoConsole)
						{
							return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
						}
						__Error.WinIOError(lastWin32Error, null);
					}
				}
				if (!Console._haveReadDefaultColors)
				{
					Console._defaultColors = (byte)(result.wAttributes & 255);
					Console._haveReadDefaultColors = true;
				}
				succeeded = true;
				return result;
			}
			if (!throwOnNoConsole)
			{
				return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
			}
			throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
		}
		[SecuritySafeCritical]
		public static void SetBufferSize(int width, int height)
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.SMALL_RECT srWindow = Console.GetBufferInfo().srWindow;
			if (width < (int)(srWindow.Right + 1) || width >= 32767)
			{
				throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
			}
			if (height < (int)(srWindow.Bottom + 1) || height >= 32767)
			{
				throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
			}
			Win32Native.COORD size = default(Win32Native.COORD);
			size.X = (short)width;
			size.Y = (short)height;
			if (!Win32Native.SetConsoleScreenBufferSize(Console.ConsoleOutputHandle, size))
			{
				__Error.WinIOError();
			}
		}
		[SecuritySafeCritical]
		public unsafe static void SetWindowSize(int width, int height)
		{
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
			bool flag = false;
			Win32Native.COORD size = default(Win32Native.COORD);
			size.X = bufferInfo.dwSize.X;
			size.Y = bufferInfo.dwSize.Y;
			if ((int)bufferInfo.dwSize.X < (int)bufferInfo.srWindow.Left + width)
			{
				if ((int)bufferInfo.srWindow.Left >= 32767 - width)
				{
					throw new ArgumentOutOfRangeException("width", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
				}
				size.X = (short)((int)bufferInfo.srWindow.Left + width);
				flag = true;
			}
			if ((int)bufferInfo.dwSize.Y < (int)bufferInfo.srWindow.Top + height)
			{
				if ((int)bufferInfo.srWindow.Top >= 32767 - height)
				{
					throw new ArgumentOutOfRangeException("height", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
				}
				size.Y = (short)((int)bufferInfo.srWindow.Top + height);
				flag = true;
			}
			if (flag && !Win32Native.SetConsoleScreenBufferSize(Console.ConsoleOutputHandle, size))
			{
				__Error.WinIOError();
			}
			Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
			srWindow.Bottom = (short)((int)srWindow.Top + height - 1);
			srWindow.Right = (short)((int)srWindow.Left + width - 1);
			if (!Win32Native.SetConsoleWindowInfo(Console.ConsoleOutputHandle, true, &srWindow))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (flag)
				{
					Win32Native.SetConsoleScreenBufferSize(Console.ConsoleOutputHandle, bufferInfo.dwSize);
				}
				Win32Native.COORD largestConsoleWindowSize = Win32Native.GetLargestConsoleWindowSize(Console.ConsoleOutputHandle);
				if (width > (int)largestConsoleWindowSize.X)
				{
					throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", new object[]
					{
						largestConsoleWindowSize.X
					}));
				}
				if (height > (int)largestConsoleWindowSize.Y)
				{
					throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", new object[]
					{
						largestConsoleWindowSize.Y
					}));
				}
				__Error.WinIOError(lastWin32Error, string.Empty);
			}
		}
		[SecuritySafeCritical]
		public unsafe static void SetWindowPosition(int left, int top)
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
			Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
			int num = left + (int)srWindow.Right - (int)srWindow.Left + 1;
			if (left < 0 || num > (int)bufferInfo.dwSize.X || num < 0)
			{
				throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
			}
			int num2 = top + (int)srWindow.Bottom - (int)srWindow.Top + 1;
			if (top < 0 || num2 > (int)bufferInfo.dwSize.Y || num2 < 0)
			{
				throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
			}
			srWindow.Bottom -= (short)((int)srWindow.Top - top);
			srWindow.Right -= (short)((int)srWindow.Left - left);
			srWindow.Left = (short)left;
			srWindow.Top = (short)top;
			if (!Win32Native.SetConsoleWindowInfo(Console.ConsoleOutputHandle, true, &srWindow))
			{
				__Error.WinIOError();
			}
		}
		[SecuritySafeCritical]
		public static void SetCursorPosition(int left, int top)
		{
			if (left < 0 || left >= 32767)
			{
				throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (top < 0 || top >= 32767)
			{
				throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			IntPtr consoleOutputHandle = Console.ConsoleOutputHandle;
			if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, new Win32Native.COORD
			{
				X = (short)left, 
				Y = (short)top
			}))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = Console.GetBufferInfo();
				if (left < 0 || left >= (int)bufferInfo.dwSize.X)
				{
					throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
				}
				if (top < 0 || top >= (int)bufferInfo.dwSize.Y)
				{
					throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
				}
				__Error.WinIOError(lastWin32Error, string.Empty);
			}
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static ConsoleKeyInfo ReadKey()
		{
			return Console.ReadKey(false);
		}
		[SecurityCritical]
		private static bool IsAltKeyDown(Win32Native.InputRecord ir)
		{
			return (ir.keyEvent.controlKeyState & 3) != 0;
		}
		[SecurityCritical]
		private static bool IsKeyDownEvent(Win32Native.InputRecord ir)
		{
			return ir.eventType == 1 && ir.keyEvent.keyDown;
		}
		[SecurityCritical]
		private static bool IsModKey(Win32Native.InputRecord ir)
		{
			short virtualKeyCode = ir.keyEvent.virtualKeyCode;
			return (virtualKeyCode >= 16 && virtualKeyCode <= 18) || virtualKeyCode == 20 || virtualKeyCode == 144 || virtualKeyCode == 145;
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static ConsoleKeyInfo ReadKey(bool intercept)
		{
			int num = -1;
			Win32Native.InputRecord cachedInputRecord;
			if (Console._cachedInputRecord.eventType == 1)
			{
				cachedInputRecord = Console._cachedInputRecord;
				if (Console._cachedInputRecord.keyEvent.repeatCount == 0)
				{
					Console._cachedInputRecord.eventType = -1;
				}
				else
				{
					Console._cachedInputRecord.keyEvent.repeatCount = Console._cachedInputRecord.keyEvent.repeatCount - 1;
				}
			}
			else
			{
				while (true)
				{
					bool flag = Win32Native.ReadConsoleInput(Console.ConsoleInputHandle, out cachedInputRecord, 1, out num);
					if (!flag || num == 0)
					{
						break;
					}
					short virtualKeyCode = cachedInputRecord.keyEvent.virtualKeyCode;
					if ((Console.IsKeyDownEvent(cachedInputRecord) || virtualKeyCode == 18) && (cachedInputRecord.keyEvent.uChar != '\0' || !Console.IsModKey(cachedInputRecord)))
					{
						ConsoleKey consoleKey = (ConsoleKey)virtualKeyCode;
						if (!Console.IsAltKeyDown(cachedInputRecord) || ((consoleKey < ConsoleKey.NumPad0 || consoleKey > ConsoleKey.NumPad9) && consoleKey != ConsoleKey.Clear && consoleKey != ConsoleKey.Insert && (consoleKey < ConsoleKey.PageUp || consoleKey > ConsoleKey.DownArrow)))
						{
							goto IL_DF;
						}
					}
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleReadKeyOnFile"));
				IL_DF:
				if (cachedInputRecord.keyEvent.repeatCount > 1)
				{
					cachedInputRecord.keyEvent.repeatCount = cachedInputRecord.keyEvent.repeatCount - 1;
					Console._cachedInputRecord = cachedInputRecord;
				}
			}
			Console.ControlKeyState controlKeyState = (Console.ControlKeyState)cachedInputRecord.keyEvent.controlKeyState;
			bool shift = (controlKeyState & Console.ControlKeyState.ShiftPressed) != (Console.ControlKeyState)0;
			bool alt = (controlKeyState & (Console.ControlKeyState.RightAltPressed | Console.ControlKeyState.LeftAltPressed)) != (Console.ControlKeyState)0;
			bool control = (controlKeyState & (Console.ControlKeyState.RightCtrlPressed | Console.ControlKeyState.LeftCtrlPressed)) != (Console.ControlKeyState)0;
			ConsoleKeyInfo result = new ConsoleKeyInfo(cachedInputRecord.keyEvent.uChar, (ConsoleKey)cachedInputRecord.keyEvent.virtualKeyCode, shift, alt, control);
			if (!intercept)
			{
				Console.Write(cachedInputRecord.keyEvent.uChar);
			}
			return result;
		}
		private static bool BreakEvent(int controlType)
		{
			if (controlType != 0 && controlType != 1)
			{
				return false;
			}
			ConsoleCancelEventHandler cancelCallbacks = Console._cancelCallbacks;
			if (cancelCallbacks == null)
			{
				return false;
			}
			ConsoleSpecialKey controlKey = (controlType == 0) ? ConsoleSpecialKey.ControlC : ConsoleSpecialKey.ControlBreak;
			Console.ControlCDelegateData controlCDelegateData = new Console.ControlCDelegateData(controlKey, cancelCallbacks);
			WaitCallback callBack = new WaitCallback(Console.ControlCDelegate);
			if (!ThreadPool.QueueUserWorkItem(callBack, controlCDelegateData))
			{
				return false;
			}
			TimeSpan timeout = new TimeSpan(0, 0, 30);
			controlCDelegateData.CompletionEvent.WaitOne(timeout, false);
			if (!controlCDelegateData.DelegateStarted)
			{
				return false;
			}
			controlCDelegateData.CompletionEvent.WaitOne();
			controlCDelegateData.CompletionEvent.Close();
			return controlCDelegateData.Cancel;
		}
		private static void ControlCDelegate(object data)
		{
			Console.ControlCDelegateData controlCDelegateData = (Console.ControlCDelegateData)data;
			try
			{
				controlCDelegateData.DelegateStarted = true;
				ConsoleCancelEventArgs consoleCancelEventArgs = new ConsoleCancelEventArgs(controlCDelegateData.ControlKey);
				controlCDelegateData.CancelCallbacks(null, consoleCancelEventArgs);
				controlCDelegateData.Cancel = consoleCancelEventArgs.Cancel;
			}
			finally
			{
				controlCDelegateData.CompletionEvent.Set();
			}
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardError()
		{
			return Console.OpenStandardError(256);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardError(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return Console.GetStandardFile(-12, FileAccess.Write, bufferSize);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardInput()
		{
			return Console.OpenStandardInput(256);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardInput(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return Console.GetStandardFile(-10, FileAccess.Read, bufferSize);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardOutput()
		{
			return Console.OpenStandardOutput(256);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardOutput(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return Console.GetStandardFile(-11, FileAccess.Write, bufferSize);
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetIn(TextReader newIn)
		{
			if (newIn == null)
			{
				throw new ArgumentNullException("newIn");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			newIn = TextReader.Synchronized(newIn);
			lock (Console.InternalSyncObject)
			{
				Console._in = newIn;
			}
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetOut(TextWriter newOut)
		{
			if (newOut == null)
			{
				throw new ArgumentNullException("newOut");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			Console._wasOutRedirected = true;
			newOut = TextWriter.Synchronized(newOut);
			lock (Console.InternalSyncObject)
			{
				Console._out = newOut;
			}
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetError(TextWriter newError)
		{
			if (newError == null)
			{
				throw new ArgumentNullException("newError");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			Console._wasErrorRedirected = true;
			newError = TextWriter.Synchronized(newError);
			lock (Console.InternalSyncObject)
			{
				Console._error = newError;
			}
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static int Read()
		{
			return Console.In.Read();
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static string ReadLine()
		{
			return Console.In.ReadLine();
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine()
		{
			Console.Out.WriteLine();
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(bool value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char[] buffer)
		{
			Console.Out.WriteLine(buffer);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char[] buffer, int index, int count)
		{
			Console.Out.WriteLine(buffer, index, count);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(decimal value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(double value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(float value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(int value)
		{
			Console.Out.WriteLine(value);
		}
		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(uint value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(long value)
		{
			Console.Out.WriteLine(value);
		}
		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(ulong value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(object value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string value)
		{
			Console.Out.WriteLine(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0)
		{
			Console.Out.WriteLine(format, arg0);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1)
		{
			Console.Out.WriteLine(format, arg0, arg1);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			Console.Out.WriteLine(format, arg0, arg1, arg2);
		}
		[CLSCompliant(false), SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			Console.Out.WriteLine(format, array);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, params object[] arg)
		{
			if (arg == null)
			{
				Console.Out.WriteLine(format, null, null);
				return;
			}
			Console.Out.WriteLine(format, arg);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0)
		{
			Console.Out.Write(format, arg0);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1)
		{
			Console.Out.Write(format, arg0, arg1);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1, object arg2)
		{
			Console.Out.Write(format, arg0, arg1, arg2);
		}
		[SecuritySafeCritical, CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			Console.Out.Write(format, array);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, params object[] arg)
		{
			if (arg == null)
			{
				Console.Out.Write(format, null, null);
				return;
			}
			Console.Out.Write(format, arg);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(bool value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char[] buffer)
		{
			Console.Out.Write(buffer);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char[] buffer, int index, int count)
		{
			Console.Out.Write(buffer, index, count);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(double value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(decimal value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(float value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(int value)
		{
			Console.Out.Write(value);
		}
		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(uint value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(long value)
		{
			Console.Out.Write(value);
		}
		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(ulong value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(object value)
		{
			Console.Out.Write(value);
		}
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string value)
		{
			Console.Out.Write(value);
		}
		[SecuritySafeCritical]
		private unsafe static bool ConsoleHandleIsValid(SafeFileHandle handle)
		{
			if (handle.IsInvalid)
			{
				return false;
			}
			byte b = 65;
			int num2;
			int num = __ConsoleStream.WriteFile(handle, &b, 0, out num2, IntPtr.Zero);
			return num != 0;
		}
	}
}
