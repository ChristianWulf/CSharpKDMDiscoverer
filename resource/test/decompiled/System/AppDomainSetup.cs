using System;
using System.Collections.Generic;
using System.Deployment.Internal.Isolation.Manifest;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Text;
namespace System
{
	[ClassInterface(ClassInterfaceType.None), ComVisible(true)]
	[Serializable]
	public sealed class AppDomainSetup : IAppDomainSetup
	{
		[Serializable]
		internal enum LoaderInformation
		{
			ApplicationBaseValue,
			ConfigurationFileValue,
			DynamicBaseValue,
			DevPathValue,
			ApplicationNameValue,
			PrivateBinPathValue,
			PrivateBinPathProbeValue,
			ShadowCopyDirectoriesValue,
			ShadowCopyFilesValue,
			CachePathValue,
			LicenseFileValue,
			DisallowPublisherPolicyValue,
			DisallowCodeDownloadValue,
			DisallowBindingRedirectsValue,
			DisallowAppBaseProbingValue,
			ConfigurationBytesValue,
			ManifestFilePathValue,
			VersioningManifestBaseValue,
			LoaderMaximum
		}
		private string[] _Entries;
		private LoaderOptimization _LoaderOptimization;
		private string _AppBase;
		[OptionalField(VersionAdded = 2)]
		private AppDomainInitializer _AppDomainInitializer;
		[OptionalField(VersionAdded = 2)]
		private string[] _AppDomainInitializerArguments;
		[OptionalField(VersionAdded = 2)]
		private ActivationArguments _ActivationArguments;
		[OptionalField(VersionAdded = 2)]
		private string _ApplicationTrust;
		[OptionalField(VersionAdded = 2)]
		private byte[] _ConfigurationBytes;
		[OptionalField(VersionAdded = 3)]
		private bool _DisableInterfaceCache;
		[OptionalField(VersionAdded = 4)]
		private string _AppDomainManagerAssembly;
		[OptionalField(VersionAdded = 4)]
		private string _AppDomainManagerType;
		[OptionalField(VersionAdded = 4)]
		private string[] _AptcaVisibleAssemblies;
		[OptionalField(VersionAdded = 4)]
		private Dictionary<string, object> _CompatFlags;
		internal string[] Value
		{
			get
			{
				if (this._Entries == null)
				{
					this._Entries = new string[18];
				}
				return this._Entries;
			}
		}
		public string AppDomainManagerAssembly
		{
			get
			{
				return this._AppDomainManagerAssembly;
			}
			set
			{
				this._AppDomainManagerAssembly = value;
			}
		}
		public string AppDomainManagerType
		{
			get
			{
				return this._AppDomainManagerType;
			}
			set
			{
				this._AppDomainManagerType = value;
			}
		}
		public string[] PartialTrustVisibleAssemblies
		{
			get
			{
				return this._AptcaVisibleAssemblies;
			}
			set
			{
				if (value != null)
				{
					this._AptcaVisibleAssemblies = (string[])value.Clone();
					Array.Sort<string>(this._AptcaVisibleAssemblies, StringComparer.OrdinalIgnoreCase);
					return;
				}
				this._AptcaVisibleAssemblies = null;
			}
		}
		public string ApplicationBase
		{
			[SecuritySafeCritical]
			get
			{
				return this.VerifyDir(this.GetUnsecureApplicationBase(), false);
			}
			set
			{
				this.Value[0] = this.NormalizePath(value, false);
			}
		}
		internal static string ApplicationBaseKey
		{
			get
			{
				return "APPBASE";
			}
		}
		public string ConfigurationFile
		{
			[SecuritySafeCritical]
			get
			{
				return this.VerifyDir(this.Value[1], true);
			}
			set
			{
				this.Value[1] = value;
			}
		}
		internal string ConfigurationFileInternal
		{
			get
			{
				return this.NormalizePath(this.Value[1], true);
			}
		}
		internal static string ConfigurationFileKey
		{
			get
			{
				return "APP_CONFIG_FILE";
			}
		}
		private static string ConfigurationBytesKey
		{
			get
			{
				return "APP_CONFIG_BLOB";
			}
		}
		public string DynamicBase
		{
			[SecuritySafeCritical]
			get
			{
				return this.VerifyDir(this.Value[2], true);
			}
			[SecuritySafeCritical]
			set
			{
				if (value == null)
				{
					this.Value[2] = null;
					return;
				}
				if (this.ApplicationName == null)
				{
					throw new MemberAccessException(Environment.GetResourceString("AppDomain_RequireApplicationName"));
				}
				StringBuilder stringBuilder = new StringBuilder(this.NormalizePath(value, false));
				stringBuilder.Append('\\');
				string value2 = ParseNumbers.IntToString(this.ApplicationName.GetHashCode(), 16, 8, '0', 256);
				stringBuilder.Append(value2);
				this.Value[2] = stringBuilder.ToString();
			}
		}
		internal static string DynamicBaseKey
		{
			get
			{
				return "DYNAMIC_BASE";
			}
		}
		public bool DisallowPublisherPolicy
		{
			get
			{
				return this.Value[11] != null;
			}
			set
			{
				if (value)
				{
					this.Value[11] = "true";
					return;
				}
				this.Value[11] = null;
			}
		}
		public bool DisallowBindingRedirects
		{
			get
			{
				return this.Value[13] != null;
			}
			set
			{
				if (value)
				{
					this.Value[13] = "true";
					return;
				}
				this.Value[13] = null;
			}
		}
		public bool DisallowCodeDownload
		{
			get
			{
				return this.Value[12] != null;
			}
			set
			{
				if (value)
				{
					this.Value[12] = "true";
					return;
				}
				this.Value[12] = null;
			}
		}
		public bool DisallowApplicationBaseProbing
		{
			get
			{
				return this.Value[14] != null;
			}
			set
			{
				if (value)
				{
					this.Value[14] = "true";
					return;
				}
				this.Value[14] = null;
			}
		}
		internal string DeveloperPath
		{
			[SecurityCritical]
			get
			{
				string text = this.Value[3];
				this.VerifyDirList(text);
				return text;
			}
			set
			{
				if (value == null)
				{
					this.Value[3] = null;
					return;
				}
				string[] array = value.Split(new char[]
				{
					';'
				});
				int num = array.Length;
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					if (array[i].Length != 0)
					{
						if (flag)
						{
							stringBuilder.Append(";");
						}
						else
						{
							flag = true;
						}
						stringBuilder.Append(Path.GetFullPathInternal(array[i]));
					}
				}
				string text = stringBuilder.ToString();
				if (text.Length == 0)
				{
					this.Value[3] = null;
					return;
				}
				this.Value[3] = stringBuilder.ToString();
			}
		}
		internal static string DisallowPublisherPolicyKey
		{
			get
			{
				return "DISALLOW_APP";
			}
		}
		internal static string DisallowCodeDownloadKey
		{
			get
			{
				return "CODE_DOWNLOAD_DISABLED";
			}
		}
		internal static string DisallowBindingRedirectsKey
		{
			get
			{
				return "DISALLOW_APP_REDIRECTS";
			}
		}
		internal static string DeveloperPathKey
		{
			get
			{
				return "DEV_PATH";
			}
		}
		internal static string DisallowAppBaseProbingKey
		{
			get
			{
				return "DISALLOW_APP_BASE_PROBING";
			}
		}
		public string ApplicationName
		{
			get
			{
				return this.Value[4];
			}
			set
			{
				this.Value[4] = value;
			}
		}
		internal static string ApplicationNameKey
		{
			get
			{
				return "APP_NAME";
			}
		}
		[XmlIgnoreMember]
		public AppDomainInitializer AppDomainInitializer
		{
			get
			{
				return this._AppDomainInitializer;
			}
			set
			{
				this._AppDomainInitializer = value;
			}
		}
		public string[] AppDomainInitializerArguments
		{
			get
			{
				return this._AppDomainInitializerArguments;
			}
			set
			{
				this._AppDomainInitializerArguments = value;
			}
		}
		[XmlIgnoreMember]
		public ActivationArguments ActivationArguments
		{
			get
			{
				return this._ActivationArguments;
			}
			[SecuritySafeCritical]
			set
			{
				this._ActivationArguments = value;
			}
		}
		[XmlIgnoreMember]
		public ApplicationTrust ApplicationTrust
		{
			[SecuritySafeCritical]
			get
			{
				return this.InternalGetApplicationTrust();
			}
			[SecuritySafeCritical]
			set
			{
				this.InternalSetApplicationTrust(value);
			}
		}
		public string PrivateBinPath
		{
			[SecuritySafeCritical]
			get
			{
				string text = this.Value[5];
				this.VerifyDirList(text);
				return text;
			}
			set
			{
				this.Value[5] = value;
			}
		}
		internal static string PrivateBinPathKey
		{
			get
			{
				return "PRIVATE_BINPATH";
			}
		}
		public string PrivateBinPathProbe
		{
			get
			{
				return this.Value[6];
			}
			set
			{
				this.Value[6] = value;
			}
		}
		internal static string PrivateBinPathProbeKey
		{
			get
			{
				return "BINPATH_PROBE_ONLY";
			}
		}
		public string ShadowCopyDirectories
		{
			[SecuritySafeCritical]
			get
			{
				string text = this.Value[7];
				this.VerifyDirList(text);
				return text;
			}
			set
			{
				this.Value[7] = value;
			}
		}
		internal static string ShadowCopyDirectoriesKey
		{
			get
			{
				return "SHADOW_COPY_DIRS";
			}
		}
		public string ShadowCopyFiles
		{
			get
			{
				return this.Value[8];
			}
			set
			{
				if (value != null && string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
				{
					this.Value[8] = value;
					return;
				}
				this.Value[8] = null;
			}
		}
		internal static string ShadowCopyFilesKey
		{
			get
			{
				return "FORCE_CACHE_INSTALL";
			}
		}
		public string CachePath
		{
			[SecuritySafeCritical]
			get
			{
				return this.VerifyDir(this.Value[9], false);
			}
			set
			{
				this.Value[9] = this.NormalizePath(value, false);
			}
		}
		internal static string CachePathKey
		{
			get
			{
				return "CACHE_BASE";
			}
		}
		public string LicenseFile
		{
			[SecuritySafeCritical]
			get
			{
				return this.VerifyDir(this.Value[10], true);
			}
			set
			{
				this.Value[10] = value;
			}
		}
		public LoaderOptimization LoaderOptimization
		{
			get
			{
				return this._LoaderOptimization;
			}
			set
			{
				this._LoaderOptimization = value;
			}
		}
		internal static string LoaderOptimizationKey
		{
			get
			{
				return "LOADER_OPTIMIZATION";
			}
		}
		internal static string ConfigurationExtension
		{
			get
			{
				return ".config";
			}
		}
		internal static string PrivateBinPathEnvironmentVariable
		{
			get
			{
				return "RELPATH";
			}
		}
		internal static string RuntimeConfigurationFile
		{
			get
			{
				return "config\\machine.config";
			}
		}
		internal static string MachineConfigKey
		{
			get
			{
				return "MACHINE_CONFIG";
			}
		}
		internal static string HostBindingKey
		{
			get
			{
				return "HOST_CONFIG";
			}
		}
		public bool SandboxInterop
		{
			get
			{
				return this._DisableInterfaceCache;
			}
			set
			{
				this._DisableInterfaceCache = value;
			}
		}
		internal AppDomainSetup(AppDomainSetup copy, bool copyDomainBoundData)
		{
			string[] value = this.Value;
			if (copy != null)
			{
				string[] value2 = copy.Value;
				int num = this._Entries.Length;
				int num2 = value2.Length;
				int num3 = (num2 < num) ? num2 : num;
				for (int i = 0; i < num3; i++)
				{
					value[i] = value2[i];
				}
				if (num3 < num)
				{
					for (int j = num3; j < num; j++)
					{
						value[j] = null;
					}
				}
				this._LoaderOptimization = copy._LoaderOptimization;
				this._AppDomainInitializerArguments = copy.AppDomainInitializerArguments;
				this._ActivationArguments = copy.ActivationArguments;
				this._ApplicationTrust = copy._ApplicationTrust;
				if (copyDomainBoundData)
				{
					this._AppDomainInitializer = copy.AppDomainInitializer;
				}
				else
				{
					this._AppDomainInitializer = null;
				}
				this._ConfigurationBytes = copy.GetConfigurationBytes();
				this._DisableInterfaceCache = copy._DisableInterfaceCache;
				this._AppDomainManagerAssembly = copy.AppDomainManagerAssembly;
				this._AppDomainManagerType = copy.AppDomainManagerType;
				this._AptcaVisibleAssemblies = copy.PartialTrustVisibleAssemblies;
				if (copy._CompatFlags != null)
				{
					this._CompatFlags = new Dictionary<string, object>(copy._CompatFlags);
					return;
				}
			}
			else
			{
				this._LoaderOptimization = LoaderOptimization.NotSpecified;
			}
		}
		public AppDomainSetup()
		{
			this._LoaderOptimization = LoaderOptimization.NotSpecified;
		}
		[SecuritySafeCritical]
		public AppDomainSetup(ActivationContext activationContext) : this(new ActivationArguments(activationContext))
		{
		}
		[SecuritySafeCritical]
		public AppDomainSetup(ActivationArguments activationArguments)
		{
			if (activationArguments == null)
			{
				throw new ArgumentNullException("activationArguments");
			}
			this._LoaderOptimization = LoaderOptimization.NotSpecified;
			this.ActivationArguments = activationArguments;
			string entryPointFullPath = CmsUtils.GetEntryPointFullPath(activationArguments);
			if (!string.IsNullOrEmpty(entryPointFullPath))
			{
				this.SetupDefaults(entryPointFullPath);
				return;
			}
			this.ApplicationBase = activationArguments.ActivationContext.ApplicationDirectory;
		}
		internal void SetupDefaults(string imageLocation)
		{
			StringBuilder stringBuilder = null;
			char[] anyOf = new char[]
			{
				'\\', 
				'/'
			};
			int num = imageLocation.LastIndexOfAny(anyOf);
			string text = null;
			if (num == -1)
			{
				stringBuilder = new StringBuilder(imageLocation);
			}
			else
			{
				text = imageLocation.Substring(0, num + 1);
				stringBuilder = new StringBuilder(imageLocation.Substring(num + 1));
			}
			string text2;
			if (num == -1)
			{
				text2 = imageLocation;
			}
			else
			{
				text2 = imageLocation.Substring(num + 1);
			}
			stringBuilder.Append(AppDomainSetup.ConfigurationExtension);
			if (stringBuilder != null)
			{
				this.ConfigurationFile = stringBuilder.ToString();
			}
			if (text != null)
			{
				this.ApplicationBase = text;
			}
			if (text2 != null)
			{
				this.ApplicationName = text2;
			}
		}
		internal string GetUnsecureApplicationBase()
		{
			return this.Value[0];
		}
		private string NormalizePath(string path, bool useAppBase)
		{
			if (path == null)
			{
				return null;
			}
			if (!useAppBase)
			{
				path = URLString.PreProcessForExtendedPathRemoval(path, false);
			}
			int num = path.Length;
			if (num == 0)
			{
				return null;
			}
			bool flag = false;
			if (num > 7 && string.Compare(path, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
			{
				int num2;
				if (path[6] == '\\')
				{
					if (path[7] == '\\' || path[7] == '/')
					{
						if (num > 8 && (path[8] == '\\' || path[8] == '/'))
						{
							throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
						}
						num2 = 8;
					}
					else
					{
						num2 = 5;
						flag = true;
					}
				}
				else
				{
					if (path[7] == '/')
					{
						num2 = 8;
					}
					else
					{
						if (num > 8 && path[7] == '\\' && path[8] == '\\')
						{
							num2 = 7;
						}
						else
						{
							num2 = 5;
							StringBuilder stringBuilder = new StringBuilder(num);
							for (int i = 0; i < num; i++)
							{
								char c = path[i];
								if (c == '/')
								{
									stringBuilder.Append('\\');
								}
								else
								{
									stringBuilder.Append(c);
								}
							}
							path = stringBuilder.ToString();
						}
						flag = true;
					}
				}
				path = path.Substring(num2);
				num -= num2;
			}
			bool flag2;
			if (flag || (num > 1 && (path[0] == '/' || path[0] == '\\') && (path[1] == '/' || path[1] == '\\')))
			{
				flag2 = false;
			}
			else
			{
				int num3 = path.IndexOf(':') + 1;
				flag2 = (num3 == 0 || num <= num3 + 1 || (path[num3] != '/' && path[num3] != '\\') || (path[num3 + 1] != '/' && path[num3 + 1] != '\\'));
			}
			if (flag2)
			{
				if (useAppBase && (num == 1 || path[1] != ':'))
				{
					string text = this.Value[0];
					if (text == null || text.Length == 0)
					{
						throw new MemberAccessException(Environment.GetResourceString("AppDomain_AppBaseNotSet"));
					}
					StringBuilder stringBuilder2 = new StringBuilder();
					bool flag3 = false;
					if (path[0] == '/' || path[0] == '\\')
					{
						string text2 = Path.GetPathRoot(text);
						if (text2.Length == 0)
						{
							int j = text.IndexOf(":/", StringComparison.Ordinal);
							if (j == -1)
							{
								j = text.IndexOf(":\\", StringComparison.Ordinal);
							}
							int length = text.Length;
							for (j++; j < length; j++)
							{
								if (text[j] != '/' && text[j] != '\\')
								{
									break;
								}
							}
							while (j < length && text[j] != '/' && text[j] != '\\')
							{
								j++;
							}
							text2 = text.Substring(0, j);
						}
						stringBuilder2.Append(text2);
						flag3 = true;
					}
					else
					{
						stringBuilder2.Append(text);
					}
					int num4 = stringBuilder2.Length - 1;
					if (stringBuilder2[num4] != '/' && stringBuilder2[num4] != '\\')
					{
						if (!flag3)
						{
							if (text.IndexOf(":/", StringComparison.Ordinal) == -1)
							{
								stringBuilder2.Append('\\');
							}
							else
							{
								stringBuilder2.Append('/');
							}
						}
					}
					else
					{
						if (flag3)
						{
							stringBuilder2.Remove(num4, 1);
						}
					}
					stringBuilder2.Append(path);
					path = stringBuilder2.ToString();
				}
				else
				{
					path = Path.GetFullPathInternal(path);
				}
			}
			return path;
		}
		private bool IsFilePath(string path)
		{
			return path[1] == ':' || (path[0] == '\\' && path[1] == '\\');
		}
		public byte[] GetConfigurationBytes()
		{
			if (this._ConfigurationBytes == null)
			{
				return null;
			}
			return (byte[])this._ConfigurationBytes.Clone();
		}
		public void SetConfigurationBytes(byte[] value)
		{
			this._ConfigurationBytes = value;
		}
		internal Dictionary<string, object> GetCompatibilityFlags()
		{
			return this._CompatFlags;
		}
		public void SetCompatibilitySwitches(IEnumerable<string> switches)
		{
			if (switches != null)
			{
				this._CompatFlags = new Dictionary<string, object>();
				using (IEnumerator<string> enumerator = switches.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						this._CompatFlags.Add(current, null);
					}
					return;
				}
			}
			this._CompatFlags = null;
		}
		[SecurityCritical]
		private string VerifyDir(string dir, bool normalize)
		{
			if (dir != null)
			{
				if (dir.Length == 0)
				{
					dir = null;
				}
				else
				{
					if (normalize)
					{
						dir = this.NormalizePath(dir, true);
					}
					if (this.IsFilePath(dir))
					{
						new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dir).Demand();
					}
				}
			}
			return dir;
		}
		[SecurityCritical]
		private void VerifyDirList(string dirs)
		{
			if (dirs != null)
			{
				string[] array = dirs.Split(new char[]
				{
					';'
				});
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					this.VerifyDir(array[i], true);
				}
			}
		}
		internal ApplicationTrust InternalGetApplicationTrust()
		{
			if (this._ApplicationTrust == null)
			{
				return null;
			}
			SecurityElement element = SecurityElement.FromString(this._ApplicationTrust);
			ApplicationTrust applicationTrust = new ApplicationTrust();
			applicationTrust.FromXml(element);
			return applicationTrust;
		}
		internal void InternalSetApplicationTrust(ApplicationTrust value)
		{
			if (value != null)
			{
				this._ApplicationTrust = value.ToXml().ToString();
				return;
			}
			this._ApplicationTrust = null;
		}
		[SecurityCritical]
		internal bool UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation FieldValue, string FieldKey, string UpdatedField, IntPtr fusionContext, AppDomainSetup oldADS)
		{
			string text = this.Value[(int)FieldValue];
			string b = (oldADS == null) ? null : oldADS.Value[(int)FieldValue];
			if (text != b)
			{
				AppDomainSetup.UpdateContextProperty(fusionContext, FieldKey, (UpdatedField == null) ? text : UpdatedField);
				return true;
			}
			return false;
		}
		[SecurityCritical]
		internal void UpdateBooleanContextPropertyIfNeeded(AppDomainSetup.LoaderInformation FieldValue, string FieldKey, IntPtr fusionContext, AppDomainSetup oldADS)
		{
			if (this.Value[(int)FieldValue] != null)
			{
				AppDomainSetup.UpdateContextProperty(fusionContext, FieldKey, "true");
				return;
			}
			if (oldADS != null && oldADS.Value[(int)FieldValue] != null)
			{
				AppDomainSetup.UpdateContextProperty(fusionContext, FieldKey, "false");
			}
		}
		[SecurityCritical]
		internal static bool ByteArraysAreDifferent(byte[] A, byte[] B)
		{
			int num = A.Length;
			if (num != B.Length)
			{
				return true;
			}
			for (int i = 0; i < num; i++)
			{
				if (A[i] != B[i])
				{
					return true;
				}
			}
			return false;
		}
		[SecurityCritical]
		internal static void UpdateByteArrayContextPropertyIfNeeded(byte[] NewArray, byte[] OldArray, string FieldKey, IntPtr fusionContext)
		{
			if ((NewArray != null && OldArray == null) || (NewArray == null && OldArray != null) || (NewArray != null && OldArray != null && AppDomainSetup.ByteArraysAreDifferent(NewArray, OldArray)))
			{
				AppDomainSetup.UpdateContextProperty(fusionContext, FieldKey, NewArray);
			}
		}
		[SecurityCritical]
		internal void SetupFusionContext(IntPtr fusionContext, AppDomainSetup oldADS)
		{
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.ApplicationBaseValue, AppDomainSetup.ApplicationBaseKey, null, fusionContext, oldADS);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.PrivateBinPathValue, AppDomainSetup.PrivateBinPathKey, null, fusionContext, oldADS);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DevPathValue, AppDomainSetup.DeveloperPathKey, null, fusionContext, oldADS);
			this.UpdateBooleanContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DisallowPublisherPolicyValue, AppDomainSetup.DisallowPublisherPolicyKey, fusionContext, oldADS);
			this.UpdateBooleanContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DisallowCodeDownloadValue, AppDomainSetup.DisallowCodeDownloadKey, fusionContext, oldADS);
			this.UpdateBooleanContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DisallowBindingRedirectsValue, AppDomainSetup.DisallowBindingRedirectsKey, fusionContext, oldADS);
			this.UpdateBooleanContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DisallowAppBaseProbingValue, AppDomainSetup.DisallowAppBaseProbingKey, fusionContext, oldADS);
			if (this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.ShadowCopyFilesValue, AppDomainSetup.ShadowCopyFilesKey, this.ShadowCopyFiles, fusionContext, oldADS))
			{
				if (this.Value[7] == null)
				{
					this.ShadowCopyDirectories = this.BuildShadowCopyDirectories();
				}
				this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.ShadowCopyDirectoriesValue, AppDomainSetup.ShadowCopyDirectoriesKey, null, fusionContext, oldADS);
			}
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.CachePathValue, AppDomainSetup.CachePathKey, null, fusionContext, oldADS);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.PrivateBinPathProbeValue, AppDomainSetup.PrivateBinPathProbeKey, this.PrivateBinPathProbe, fusionContext, oldADS);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.ConfigurationFileValue, AppDomainSetup.ConfigurationFileKey, null, fusionContext, oldADS);
			AppDomainSetup.UpdateByteArrayContextPropertyIfNeeded(this._ConfigurationBytes, (oldADS == null) ? null : oldADS.GetConfigurationBytes(), AppDomainSetup.ConfigurationBytesKey, fusionContext);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.ApplicationNameValue, AppDomainSetup.ApplicationNameKey, this.ApplicationName, fusionContext, oldADS);
			this.UpdateContextPropertyIfNeeded(AppDomainSetup.LoaderInformation.DynamicBaseValue, AppDomainSetup.DynamicBaseKey, null, fusionContext, oldADS);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RuntimeEnvironment.GetRuntimeDirectoryImpl());
			stringBuilder.Append(AppDomainSetup.RuntimeConfigurationFile);
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.MachineConfigKey, stringBuilder.ToString());
			string hostBindingFile = RuntimeEnvironment.GetHostBindingFile();
			if (hostBindingFile != null || oldADS != null)
			{
				AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.HostBindingKey, hostBindingFile);
			}
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void UpdateContextProperty(IntPtr fusionContext, string key, object value);
		internal static int Locate(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return -1;
			}
			char c = s[0];
			if (c <= 'L')
			{
				switch (c)
				{
					case 'A':
					{
						if (s == "APP_CONFIG_FILE")
						{
							return 1;
						}
						if (s == "APP_NAME")
						{
							return 4;
						}
						if (s == "APPBASE")
						{
							return 0;
						}
						if (s == "APP_CONFIG_BLOB")
						{
							return 15;
						}
						break;
					}
					case 'B':
					{
						if (s == "BINPATH_PROBE_ONLY")
						{
							return 6;
						}
						break;
					}
					case 'C':
					{
						if (s == "CACHE_BASE")
						{
							return 9;
						}
						if (s == "CODE_DOWNLOAD_DISABLED")
						{
							return 12;
						}
						break;
					}
					case 'D':
					{
						if (s == "DEV_PATH")
						{
							return 3;
						}
						if (s == "DYNAMIC_BASE")
						{
							return 2;
						}
						if (s == "DISALLOW_APP")
						{
							return 11;
						}
						if (s == "DISALLOW_APP_REDIRECTS")
						{
							return 13;
						}
						if (s == "DISALLOW_APP_BASE_PROBING")
						{
							return 14;
						}
						break;
					}
					case 'E':
					{
						break;
					}
					case 'F':
					{
						if (s == "FORCE_CACHE_INSTALL")
						{
							return 8;
						}
						break;
					}
					default:
					{
						if (c == 'L')
						{
							if (s == "LICENSE_FILE")
							{
								return 10;
							}
						}
						break;
					}
				}
			}
			else
			{
				if (c != 'P')
				{
					if (c == 'S')
					{
						if (s == "SHADOW_COPY_DIRS")
						{
							return 7;
						}
					}
				}
				else
				{
					if (s == "PRIVATE_BINPATH")
					{
						return 5;
					}
				}
			}
			return -1;
		}
		private string BuildShadowCopyDirectories()
		{
			string text = this.Value[5];
			if (text == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text2 = this.Value[0];
			if (text2 != null)
			{
				char[] separator = new char[]
				{
					';'
				};
				string[] array = text.Split(separator);
				int num = array.Length;
				bool flag = text2[text2.Length - 1] != '/' && text2[text2.Length - 1] != '\\';
				if (num == 0)
				{
					stringBuilder.Append(text2);
					if (flag)
					{
						stringBuilder.Append('\\');
					}
					stringBuilder.Append(text);
				}
				else
				{
					for (int i = 0; i < num; i++)
					{
						stringBuilder.Append(text2);
						if (flag)
						{
							stringBuilder.Append('\\');
						}
						stringBuilder.Append(array[i]);
						if (i < num - 1)
						{
							stringBuilder.Append(';');
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
