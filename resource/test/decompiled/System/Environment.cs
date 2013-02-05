using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
namespace System
{
	[ComVisible(true)]
	public static class Environment
	{
		internal sealed class ResourceHelper
		{
			internal class GetResourceStringUserData
			{
				public Environment.ResourceHelper m_resourceHelper;
				public string m_key;
				public CultureInfo m_culture;
				public string m_retVal;
				public bool m_lockWasTaken;
				public GetResourceStringUserData(Environment.ResourceHelper resourceHelper, string key, CultureInfo culture)
				{
					this.m_resourceHelper = resourceHelper;
					this.m_key = key;
					this.m_culture = culture;
				}
			}
			private string m_name;
			private ResourceManager SystemResMgr;
			private Stack currentlyLoading;
			internal bool resourceManagerInited;
			internal ResourceHelper(string name)
			{
				this.m_name = name;
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
			internal string GetResourceString(string key)
			{
				if (key == null || key.Length == 0)
				{
					return "[Resource lookup failed - null or empty resource name]";
				}
				return this.GetResourceString(key, null);
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
			internal string GetResourceString(string key, CultureInfo culture)
			{
				if (key == null || key.Length == 0)
				{
					return "[Resource lookup failed - null or empty resource name]";
				}
				Environment.ResourceHelper.GetResourceStringUserData getResourceStringUserData = new Environment.ResourceHelper.GetResourceStringUserData(this, key, culture);
				RuntimeHelpers.TryCode code = new RuntimeHelpers.TryCode(this.GetResourceStringCode);
				RuntimeHelpers.CleanupCode backoutCode = new RuntimeHelpers.CleanupCode(this.GetResourceStringBackoutCode);
				RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(code, backoutCode, getResourceStringUserData);
				return getResourceStringUserData.m_retVal;
			}
			[SecuritySafeCritical]
			private void GetResourceStringCode(object userDataIn)
			{
				Environment.ResourceHelper.GetResourceStringUserData getResourceStringUserData = (Environment.ResourceHelper.GetResourceStringUserData)userDataIn;
				Environment.ResourceHelper resourceHelper = getResourceStringUserData.m_resourceHelper;
				string key = getResourceStringUserData.m_key;
				CultureInfo arg_1B_0 = getResourceStringUserData.m_culture;
				Monitor.Enter(resourceHelper, ref getResourceStringUserData.m_lockWasTaken);
				if (resourceHelper.currentlyLoading != null && resourceHelper.currentlyLoading.Count > 0 && resourceHelper.currentlyLoading.Contains(key))
				{
					try
					{
						StackTrace stackTrace = new StackTrace(true);
						stackTrace.ToString(System.Diagnostics.StackTrace.TraceFormat.NoResourceLookup);
					}
					catch (StackOverflowException)
					{
					}
					catch (NullReferenceException)
					{
					}
					catch (OutOfMemoryException)
					{
					}
					getResourceStringUserData.m_retVal = "[Resource lookup failed - infinite recursion or critical failure detected.]";
					return;
				}
				if (resourceHelper.currentlyLoading == null)
				{
					resourceHelper.currentlyLoading = new Stack(4);
				}
				if (!resourceHelper.resourceManagerInited)
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						RuntimeHelpers.RunClassConstructor(typeof(ResourceManager).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(ResourceReader).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(RuntimeResourceSet).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(BinaryReader).TypeHandle);
						resourceHelper.resourceManagerInited = true;
					}
				}
				resourceHelper.currentlyLoading.Push(key);
				if (resourceHelper.SystemResMgr == null)
				{
					resourceHelper.SystemResMgr = new ResourceManager(this.m_name, typeof(object).Assembly);
				}
				string @string = resourceHelper.SystemResMgr.GetString(key, null);
				resourceHelper.currentlyLoading.Pop();
				getResourceStringUserData.m_retVal = @string;
			}
			[PrePrepareMethod]
			private void GetResourceStringBackoutCode(object userDataIn, bool exceptionThrown)
			{
				Environment.ResourceHelper.GetResourceStringUserData getResourceStringUserData = (Environment.ResourceHelper.GetResourceStringUserData)userDataIn;
				Environment.ResourceHelper resourceHelper = getResourceStringUserData.m_resourceHelper;
				if (exceptionThrown && getResourceStringUserData.m_lockWasTaken)
				{
					resourceHelper.SystemResMgr = null;
					resourceHelper.currentlyLoading = null;
				}
				if (getResourceStringUserData.m_lockWasTaken)
				{
					Monitor.Exit(resourceHelper);
				}
			}
		}
		[Serializable]
		internal enum OSName
		{
			Invalid,
			Unknown,
			WinNT = 128,
			Nt4,
			Win2k,
			MacOSX = 256,
			Tiger,
			Leopard
		}
		public enum SpecialFolderOption
		{
			None,
			Create = 32768,
			DoNotVerify = 16384
		}
		[ComVisible(true)]
		public enum SpecialFolder
		{
			ApplicationData = 26,
			CommonApplicationData = 35,
			LocalApplicationData = 28,
			Cookies = 33,
			Desktop = 0,
			Favorites = 6,
			History = 34,
			InternetCache = 32,
			Programs = 2,
			MyComputer = 17,
			MyMusic = 13,
			MyPictures = 39,
			Recent = 8,
			SendTo,
			StartMenu = 11,
			Startup = 7,
			System = 37,
			Templates = 21,
			DesktopDirectory = 16,
			Personal = 5,
			MyDocuments = 5,
			ProgramFiles = 38,
			CommonProgramFiles = 43,
			AdminTools = 48,
			CDBurning = 59,
			CommonAdminTools = 47,
			CommonDocuments = 46,
			CommonMusic = 53,
			CommonOemLinks = 58,
			CommonPictures = 54,
			CommonStartMenu = 22,
			CommonPrograms,
			CommonStartup,
			CommonDesktopDirectory,
			CommonTemplates = 45,
			CommonVideos = 55,
			Fonts = 20,
			MyVideos = 14,
			NetworkShortcuts = 19,
			PrinterShortcuts = 27,
			UserProfile = 40,
			CommonProgramFilesX86 = 44,
			ProgramFilesX86 = 42,
			Resources = 56,
			LocalizedResources,
			SystemX86 = 41,
			Windows = 36
		}
		private const int MaxEnvVariableValueLength = 32767;
		private const int MaxSystemEnvVariableLength = 1024;
		private const int MaxUserEnvVariableLength = 255;
		private const int MaxMachineNameLength = 256;
		private static Environment.ResourceHelper m_resHelper;
		private static bool s_IsWindowsVista;
		private static bool s_CheckedOSType;
		private static bool s_IsW2k3;
		private static bool s_CheckedOSW2k3;
		private static object s_InternalSyncObject;
		private static OperatingSystem m_os;
		private static Environment.OSName m_osname;
		private static IntPtr processWinStation;
		private static bool isUserNonInteractive;
		private static object InternalSyncObject
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				if (Environment.s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange<object>(ref Environment.s_InternalSyncObject, value, null);
				}
				return Environment.s_InternalSyncObject;
			}
		}
		public static extern int TickCount
		{
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public static extern int ExitCode
		{
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}
		internal static bool IsCLRHosted
		{
			[SecuritySafeCritical]
			get
			{
				return Environment.GetIsCLRHosted();
			}
		}
		public static string CommandLine
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
				string result = null;
				Environment.GetCommandLine(JitHelpers.GetStringHandleOnStack(ref result));
				return result;
			}
		}
		public static string CurrentDirectory
		{
			get
			{
				return Directory.GetCurrentDirectory();
			}
			set
			{
				Directory.SetCurrentDirectory(value);
			}
		}
		public static string SystemDirectory
		{
			[SecuritySafeCritical]
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				if (Win32Native.GetSystemDirectory(stringBuilder, 260) == 0)
				{
					__Error.WinIOError();
				}
				string text = stringBuilder.ToString();
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
				return text;
			}
		}
		internal static string InternalWindowsDirectory
		{
			[SecurityCritical]
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				if (Win32Native.GetWindowsDirectory(stringBuilder, 260) == 0)
				{
					__Error.WinIOError();
				}
				return stringBuilder.ToString();
			}
		}
		public static string MachineName
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "COMPUTERNAME").Demand();
				StringBuilder stringBuilder = new StringBuilder(256);
				int num = 256;
				if (Win32Native.GetComputerName(stringBuilder, ref num) == 0)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ComputerName"));
				}
				return stringBuilder.ToString();
			}
		}
		public static int ProcessorCount
		{
			[SecuritySafeCritical]
			get
			{
				Win32Native.SYSTEM_INFO sYSTEM_INFO = default(Win32Native.SYSTEM_INFO);
				Win32Native.GetSystemInfo(ref sYSTEM_INFO);
				return sYSTEM_INFO.dwNumberOfProcessors;
			}
		}
		public static int SystemPageSize
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				Win32Native.SYSTEM_INFO sYSTEM_INFO = default(Win32Native.SYSTEM_INFO);
				Win32Native.GetSystemInfo(ref sYSTEM_INFO);
				return sYSTEM_INFO.dwPageSize;
			}
		}
		public static string NewLine
		{
			get
			{
				return "\r\n";
			}
		}
		public static Version Version
		{
			get
			{
				return new Version("4.0.30319.239");
			}
		}
		public static long WorkingSet
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				return Environment.GetWorkingSet();
			}
		}
		public static OperatingSystem OSVersion
		{
			[SecuritySafeCritical]
			get
			{
				if (Environment.m_os == null)
				{
					Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
					if (!Environment.GetVersion(oSVERSIONINFO))
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GetVersion"));
					}
					int platformId = oSVERSIONINFO.PlatformId;
					PlatformID platform;
					bool flag;
					if (platformId != 2)
					{
						switch (platformId)
						{
							case 10:
							{
								platform = PlatformID.Unix;
								flag = false;
								break;
							}
							case 11:
							{
								platform = PlatformID.MacOSX;
								flag = false;
								break;
							}
							default:
							{
								throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InvalidPlatformID"));
							}
						}
					}
					else
					{
						platform = PlatformID.Win32NT;
						flag = true;
					}
					Win32Native.OSVERSIONINFOEX oSVERSIONINFOEX = new Win32Native.OSVERSIONINFOEX();
					if (flag && !Environment.GetVersionEx(oSVERSIONINFOEX))
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GetVersion"));
					}
					Version version = new Version(oSVERSIONINFO.MajorVersion, oSVERSIONINFO.MinorVersion, oSVERSIONINFO.BuildNumber, (int)oSVERSIONINFOEX.ServicePackMajor << 16 | (int)oSVERSIONINFOEX.ServicePackMinor);
					Environment.m_os = new OperatingSystem(platform, version, oSVERSIONINFO.CSDVersion);
				}
				return Environment.m_os;
			}
		}
		internal static bool IsWindowsVista
		{
			get
			{
				if (!Environment.s_CheckedOSType)
				{
					OperatingSystem oSVersion = Environment.OSVersion;
					Environment.s_IsWindowsVista = (oSVersion.Platform == PlatformID.Win32NT && oSVersion.Version.Major >= 6);
					Environment.s_CheckedOSType = true;
				}
				return Environment.s_IsWindowsVista;
			}
		}
		internal static bool IsW2k3
		{
			get
			{
				if (!Environment.s_CheckedOSW2k3)
				{
					OperatingSystem oSVersion = Environment.OSVersion;
					Environment.s_IsW2k3 = (oSVersion.Platform == PlatformID.Win32NT && oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 2);
					Environment.s_CheckedOSW2k3 = true;
				}
				return Environment.s_IsW2k3;
			}
		}
		internal static bool RunningOnWinNT
		{
			get
			{
				return Environment.OSVersion.Platform == PlatformID.Win32NT;
			}
		}
		internal static Environment.OSName OSInfo
		{
			[SecuritySafeCritical]
			get
			{
				if (Environment.m_osname == Environment.OSName.Invalid)
				{
					lock (Environment.InternalSyncObject)
					{
						if (Environment.m_osname == Environment.OSName.Invalid)
						{
							Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
							if (!Environment.GetVersion(oSVERSIONINFO))
							{
								throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GetVersion"));
							}
							int platformId = oSVERSIONINFO.PlatformId;
							switch (platformId)
							{
								case 1:
								{
									Environment.m_osname = Environment.OSName.Unknown;
									break;
								}
								case 2:
								{
									switch (oSVERSIONINFO.MajorVersion)
									{
										case 4:
										{
											Environment.m_osname = Environment.OSName.Unknown;
											break;
										}
										case 5:
										{
											Environment.m_osname = Environment.OSName.Win2k;
											break;
										}
										default:
										{
											Environment.m_osname = Environment.OSName.WinNT;
											break;
										}
									}
									break;
								}
								default:
								{
									if (platformId != 11)
									{
										Environment.m_osname = Environment.OSName.Unknown;
									}
									else
									{
										if (oSVERSIONINFO.MajorVersion == 10)
										{
											switch (oSVERSIONINFO.MinorVersion)
											{
												case 4:
												{
													Environment.m_osname = Environment.OSName.Tiger;
													break;
												}
												case 5:
												{
													Environment.m_osname = Environment.OSName.Leopard;
													break;
												}
												default:
												{
													Environment.m_osname = Environment.OSName.MacOSX;
													break;
												}
											}
										}
										else
										{
											Environment.m_osname = Environment.OSName.MacOSX;
										}
									}
									break;
								}
							}
						}
					}
				}
				return Environment.m_osname;
			}
		}
		public static string StackTrace
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				return Environment.GetStackTrace(null, true);
			}
		}
		public static bool Is64BitProcess
		{
			get
			{
				return false;
			}
		}
		public static bool Is64BitOperatingSystem
		{
			[SecuritySafeCritical]
			get
			{
				bool flag;
				return Win32Native.DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && Win32Native.IsWow64Process(Win32Native.GetCurrentProcess(), out flag) && flag;
			}
		}
		public static extern bool HasShutdownStarted
		{
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public static string UserName
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserName").Demand();
				StringBuilder stringBuilder = new StringBuilder(256);
				int capacity = stringBuilder.Capacity;
				Win32Native.GetUserName(stringBuilder, ref capacity);
				return stringBuilder.ToString();
			}
		}
		public static bool UserInteractive
		{
			[SecuritySafeCritical]
			get
			{
				if ((Environment.OSInfo & Environment.OSName.WinNT) == Environment.OSName.WinNT)
				{
					IntPtr processWindowStation = Win32Native.GetProcessWindowStation();
					if (processWindowStation != IntPtr.Zero && Environment.processWinStation != processWindowStation)
					{
						int num = 0;
						Win32Native.USEROBJECTFLAGS uSEROBJECTFLAGS = new Win32Native.USEROBJECTFLAGS();
						if (Win32Native.GetUserObjectInformation(processWindowStation, 1, uSEROBJECTFLAGS, Marshal.SizeOf(uSEROBJECTFLAGS), ref num) && (uSEROBJECTFLAGS.dwFlags & 1) == 0)
						{
							Environment.isUserNonInteractive = true;
						}
						Environment.processWinStation = processWindowStation;
					}
				}
				return !Environment.isUserNonInteractive;
			}
		}
		public static string UserDomainName
		{
			[SecuritySafeCritical]
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserDomain").Demand();
				byte[] array = new byte[1024];
				int num = array.Length;
				StringBuilder stringBuilder = new StringBuilder(1024);
				int capacity = stringBuilder.Capacity;
				byte userNameEx = Win32Native.GetUserNameEx(2, stringBuilder, ref capacity);
				if (userNameEx == 1)
				{
					string text = stringBuilder.ToString();
					int num2 = text.IndexOf('\\');
					if (num2 != -1)
					{
						return text.Substring(0, num2);
					}
				}
				capacity = stringBuilder.Capacity;
				int num3;
				if (!Win32Native.LookupAccountName(null, Environment.UserName, array, ref num, stringBuilder, ref capacity, out num3))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					throw new InvalidOperationException(Win32Native.GetMessage(lastWin32Error));
				}
				return stringBuilder.ToString();
			}
		}
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		internal static extern void _Exit(int exitCode);
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Exit(int exitCode)
		{
			Environment._Exit(exitCode);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void FailFast(string message);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void FailFast(string message, Exception exception);
		[SecurityCritical, SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		internal static extern void TriggerCodeContractFailure(ContractFailureKind failureKind, string message, string condition, string exceptionAsString);
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetIsCLRHosted();
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void GetCommandLine(StringHandleOnStack retString);
		[SecuritySafeCritical]
		public static string ExpandEnvironmentVariables(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				return name;
			}
			bool flag = CodeAccessSecurityEngine.QuickCheckForAllDemands();
			string[] array = name.Split(new char[]
			{
				'%'
			});
			StringBuilder stringBuilder = flag ? null : new StringBuilder();
			int num = 100;
			StringBuilder stringBuilder2 = new StringBuilder(num);
			bool flag2 = false;
			int j;
			for (int i = 1; i < array.Length - 1; i++)
			{
				if (array[i].Length == 0 || flag2)
				{
					flag2 = false;
				}
				else
				{
					stringBuilder2.Length = 0;
					string text = "%" + array[i] + "%";
					j = Win32Native.ExpandEnvironmentStrings(text, stringBuilder2, num);
					if (j == 0)
					{
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					}
					while (j > num)
					{
						num = j;
						stringBuilder2.Capacity = num;
						stringBuilder2.Length = 0;
						j = Win32Native.ExpandEnvironmentStrings(text, stringBuilder2, num);
						if (j == 0)
						{
							Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
						}
					}
					if (!flag)
					{
						string a = stringBuilder2.ToString();
						flag2 = (a != text);
						if (flag2)
						{
							stringBuilder.Append(array[i]);
							stringBuilder.Append(';');
						}
					}
				}
			}
			if (!flag)
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, stringBuilder.ToString()).Demand();
			}
			stringBuilder2.Length = 0;
			j = Win32Native.ExpandEnvironmentStrings(name, stringBuilder2, num);
			if (j == 0)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			while (j > num)
			{
				num = j;
				stringBuilder2.Capacity = num;
				stringBuilder2.Length = 0;
				j = Win32Native.ExpandEnvironmentStrings(name, stringBuilder2, num);
				if (j == 0)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
			}
			return stringBuilder2.ToString();
		}
		[SecuritySafeCritical]
		public static string[] GetCommandLineArgs()
		{
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
			return Environment.GetCommandLineArgsNative();
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string[] GetCommandLineArgsNative();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetEnvironmentVariable(string variable);
		[SecuritySafeCritical]
		public static string GetEnvironmentVariable(string variable)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, variable).Demand();
			StringBuilder stringBuilder = new StringBuilder(128);
			int i = Win32Native.GetEnvironmentVariable(variable, stringBuilder, stringBuilder.Capacity);
			if (i == 0 && Marshal.GetLastWin32Error() == 203)
			{
				return null;
			}
			while (i > stringBuilder.Capacity)
			{
				stringBuilder.Capacity = i;
				stringBuilder.Length = 0;
				i = Win32Native.GetEnvironmentVariable(variable, stringBuilder, stringBuilder.Capacity);
			}
			return stringBuilder.ToString();
		}
		[SecuritySafeCritical]
		public static string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			if (target == EnvironmentVariableTarget.Process)
			{
				return Environment.GetEnvironmentVariable(variable);
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (target == EnvironmentVariableTarget.Machine)
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", false))
				{
					string result;
					if (registryKey == null)
					{
						result = null;
						return result;
					}
					string text = registryKey.GetValue(variable) as string;
					result = text;
					return result;
				}
			}
			if (target == EnvironmentVariableTarget.User)
			{
				using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("Environment", false))
				{
					string result;
					if (registryKey2 == null)
					{
						result = null;
						return result;
					}
					string text2 = registryKey2.GetValue(variable) as string;
					result = text2;
					return result;
				}
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
			{
				(int)target
			}));
		}
		[SecurityCritical]
		private unsafe static char[] GetEnvironmentCharArray()
		{
			char[] array = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				char* ptr = null;
				try
				{
					ptr = Win32Native.GetEnvironmentStrings();
					if (ptr == null)
					{
						throw new OutOfMemoryException();
					}
					char* ptr2 = ptr;
					while (*(ushort*)ptr2 != 0 || *(ushort*)(ptr2 + (IntPtr)2 / 2) != 0)
					{
						ptr2 += (IntPtr)2 / 2;
					}
					int num = (ptr2 - ptr / 2) / 2 + 1 / 2;
					array = new char[num];
					try
					{
						fixed (char* ptr3 = array)
						{
							Buffer.memcpy(ptr, 0, ptr3, 0, num);
						}
					}
					finally
					{
						char* ptr3 = null;
					}
				}
				finally
				{
					if (ptr != null)
					{
						Win32Native.FreeEnvironmentStrings(ptr);
					}
				}
			}
			return array;
		}
		[SecuritySafeCritical]
		public static IDictionary GetEnvironmentVariables()
		{
			bool flag = CodeAccessSecurityEngine.QuickCheckForAllDemands();
			char[] environmentCharArray = Environment.GetEnvironmentCharArray();
			Hashtable hashtable = new Hashtable(20);
			StringBuilder stringBuilder = flag ? null : new StringBuilder();
			bool flag2 = true;
			for (int i = 0; i < environmentCharArray.Length; i++)
			{
				int num = i;
				while (environmentCharArray[i] != '=' && environmentCharArray[i] != '\0')
				{
					i++;
				}
				if (environmentCharArray[i] != '\0')
				{
					if (i - num == 0)
					{
						while (environmentCharArray[i] != '\0')
						{
							i++;
						}
					}
					else
					{
						string text = new string(environmentCharArray, num, i - num);
						i++;
						int num2 = i;
						while (environmentCharArray[i] != '\0')
						{
							i++;
						}
						string value = new string(environmentCharArray, num2, i - num2);
						hashtable[text] = value;
						if (!flag)
						{
							if (flag2)
							{
								flag2 = false;
							}
							else
							{
								stringBuilder.Append(';');
							}
							stringBuilder.Append(text);
						}
					}
				}
			}
			if (!flag)
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, stringBuilder.ToString()).Demand();
			}
			return hashtable;
		}
		internal static IDictionary GetRegistryKeyNameValuePairs(RegistryKey registryKey)
		{
			Hashtable hashtable = new Hashtable(20);
			if (registryKey != null)
			{
				string[] valueNames = registryKey.GetValueNames();
				string[] array = valueNames;
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					string value = registryKey.GetValue(text, "").ToString();
					hashtable.Add(text, value);
				}
			}
			return hashtable;
		}
		[SecuritySafeCritical]
		public static IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target)
		{
			if (target == EnvironmentVariableTarget.Process)
			{
				return Environment.GetEnvironmentVariables();
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (target == EnvironmentVariableTarget.Machine)
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", false))
				{
					IDictionary registryKeyNameValuePairs = Environment.GetRegistryKeyNameValuePairs(registryKey);
					return registryKeyNameValuePairs;
				}
			}
			if (target == EnvironmentVariableTarget.User)
			{
				using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("Environment", false))
				{
					IDictionary registryKeyNameValuePairs = Environment.GetRegistryKeyNameValuePairs(registryKey2);
					return registryKeyNameValuePairs;
				}
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
			{
				(int)target
			}));
		}
		[SecuritySafeCritical]
		public static void SetEnvironmentVariable(string variable, string value)
		{
			Environment.CheckEnvironmentVariableName(variable);
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (string.IsNullOrEmpty(value) || value[0] == '\0')
			{
				value = null;
			}
			else
			{
				if (value.Length >= 32767)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_LongEnvVarValue"));
				}
			}
			if (Win32Native.SetEnvironmentVariable(variable, value))
			{
				return;
			}
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error == 203)
			{
				return;
			}
			if (lastWin32Error == 206)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_LongEnvVarValue"));
			}
			throw new ArgumentException(Win32Native.GetMessage(lastWin32Error));
		}
		private static void CheckEnvironmentVariableName(string variable)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			if (variable.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StringZeroLength"), "variable");
			}
			if (variable[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StringFirstCharIsZero"), "variable");
			}
			if (variable.Length >= 32767)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_LongEnvVarValue"));
			}
			if (variable.IndexOf('=') != -1)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_IllegalEnvVarName"));
			}
		}
		[SecuritySafeCritical]
		public static void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target)
		{
			if (target == EnvironmentVariableTarget.Process)
			{
				Environment.SetEnvironmentVariable(variable, value);
				return;
			}
			Environment.CheckEnvironmentVariableName(variable);
			if (variable.Length >= 1024)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_LongEnvVarName"));
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (string.IsNullOrEmpty(value) || value[0] == '\0')
			{
				value = null;
			}
			if (target == EnvironmentVariableTarget.Machine)
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", true))
				{
					if (registryKey != null)
					{
						if (value == null)
						{
							registryKey.DeleteValue(variable, false);
						}
						else
						{
							registryKey.SetValue(variable, value);
						}
					}
					goto IL_100;
				}
			}
			if (target == EnvironmentVariableTarget.User)
			{
				if (variable.Length >= 255)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_LongEnvVarValue"));
				}
				using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("Environment", true))
				{
					if (registryKey2 != null)
					{
						if (value == null)
						{
							registryKey2.DeleteValue(variable, false);
						}
						else
						{
							registryKey2.SetValue(variable, value);
						}
					}
					goto IL_100;
				}
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
			{
				(int)target
			}));
			IL_100:
			IntPtr value2 = Win32Native.SendMessageTimeout(new IntPtr(65535), 26, IntPtr.Zero, "Environment", 0u, 1000u, IntPtr.Zero);
			value2 == IntPtr.Zero;
		}
		[SecuritySafeCritical]
		public static string[] GetLogicalDrives()
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			int logicalDrives = Win32Native.GetLogicalDrives();
			if (logicalDrives == 0)
			{
				__Error.WinIOError();
			}
			uint num = (uint)logicalDrives;
			int num2 = 0;
			while (num != 0u)
			{
				if ((num & 1u) != 0u)
				{
					num2++;
				}
				num >>= 1;
			}
			string[] array = new string[num2];
			char[] array2 = new char[]
			{
				'A', 
				':', 
				'\\'
			};
			num = (uint)logicalDrives;
			num2 = 0;
			while (num != 0u)
			{
				if ((num & 1u) != 0u)
				{
					array[num2++] = new string(array2);
				}
				num >>= 1;
				char[] expr_6E_cp_0 = array2;
				int expr_6E_cp_1 = 0;
				expr_6E_cp_0[expr_6E_cp_1] += '\u0001';
			}
			return array;
		}
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern long GetWorkingSet();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetVersion(Win32Native.OSVERSIONINFO osVer);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetVersionEx(Win32Native.OSVERSIONINFOEX osVer);
		internal static string GetStackTrace(Exception e, bool needFileInfo)
		{
			StackTrace stackTrace;
			if (e == null)
			{
				stackTrace = new StackTrace(needFileInfo);
			}
			else
			{
				stackTrace = new StackTrace(e, needFileInfo);
			}
			return stackTrace.ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
		}
		[SecuritySafeCritical]
		private static void InitResourceHelper()
		{
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.Enter(Environment.InternalSyncObject, ref flag);
				if (Environment.m_resHelper == null)
				{
					Environment.ResourceHelper resHelper = new Environment.ResourceHelper("mscorlib");
					Thread.MemoryBarrier();
					Environment.m_resHelper = resHelper;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(Environment.InternalSyncObject);
				}
			}
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetResourceFromDefault(string key);
		internal static string GetResourceStringLocal(string key)
		{
			if (Environment.m_resHelper == null)
			{
				Environment.InitResourceHelper();
			}
			return Environment.m_resHelper.GetResourceString(key);
		}
		[SecuritySafeCritical]
		internal static string GetResourceString(string key)
		{
			return Environment.GetResourceFromDefault(key);
		}
		[SecuritySafeCritical]
		internal static string GetResourceString(string key, params object[] values)
		{
			string resourceFromDefault = Environment.GetResourceFromDefault(key);
			return string.Format(CultureInfo.CurrentCulture, resourceFromDefault, values);
		}
		[SecuritySafeCritical]
		internal static string GetRuntimeResourceString(string key)
		{
			return Environment.GetResourceFromDefault(key);
		}
		[SecuritySafeCritical]
		internal static string GetRuntimeResourceString(string key, params object[] values)
		{
			string resourceFromDefault = Environment.GetResourceFromDefault(key);
			return string.Format(CultureInfo.CurrentCulture, resourceFromDefault, values);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetCompatibilityFlag(CompatibilityFlag flag);
		[SecuritySafeCritical]
		public static string GetFolderPath(Environment.SpecialFolder folder)
		{
			return Environment.GetFolderPath(folder, Environment.SpecialFolderOption.None);
		}
		[SecuritySafeCritical]
		public static string GetFolderPath(Environment.SpecialFolder folder, Environment.SpecialFolderOption option)
		{
			if (!Enum.IsDefined(typeof(Environment.SpecialFolder), folder))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
				{
					(int)folder
				}));
			}
			if (!Enum.IsDefined(typeof(Environment.SpecialFolderOption), option))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
				{
					(int)option
				}));
			}
			if (option == Environment.SpecialFolderOption.Create)
			{
				new FileIOPermission(PermissionState.None)
				{
					AllFiles = FileIOPermissionAccess.Write
				}.Demand();
			}
			StringBuilder stringBuilder = new StringBuilder(260);
			int num = Win32Native.SHGetFolderPath(IntPtr.Zero, (int)(folder | (Environment.SpecialFolder)option), IntPtr.Zero, 0, stringBuilder);
			if (num < 0)
			{
				int num2 = num;
				if (num2 == -2146233031)
				{
					throw new PlatformNotSupportedException();
				}
			}
			string text = stringBuilder.ToString();
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
			return text;
		}
	}
}
