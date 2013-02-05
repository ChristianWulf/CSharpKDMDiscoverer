using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Deployment.Internal.Isolation.Manifest;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Security.Util;
using System.Text;
using System.Threading;
namespace System
{
	[ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_AppDomain))]
	public sealed class AppDomain : MarshalByRefObject, _AppDomain, IEvidenceFactory
	{
		[Serializable]
		private class EvidenceCollection
		{
			public Evidence ProvidedSecurityInfo;
			public Evidence CreatorsSecurityInfo;
		}
		private class CAPTCASearcher : IComparer
		{
			int IComparer.Compare(object lhs, object rhs)
			{
				AssemblyName assemblyName = new AssemblyName((string)lhs);
				AssemblyName assemblyName2 = (AssemblyName)rhs;
				int num = string.Compare(assemblyName.Name, assemblyName2.Name, StringComparison.OrdinalIgnoreCase);
				if (num != 0)
				{
					return num;
				}
				byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
				byte[] publicKeyToken2 = assemblyName2.GetPublicKeyToken();
				if (publicKeyToken == null)
				{
					return -1;
				}
				if (publicKeyToken2 == null)
				{
					return 1;
				}
				if (publicKeyToken.Length < publicKeyToken2.Length)
				{
					return -1;
				}
				if (publicKeyToken.Length > publicKeyToken2.Length)
				{
					return 1;
				}
				for (int i = 0; i < publicKeyToken.Length; i++)
				{
					byte b = publicKeyToken[i];
					byte b2 = publicKeyToken2[i];
					if (b < b2)
					{
						return -1;
					}
					if (b > b2)
					{
						return 1;
					}
				}
				return 0;
			}
		}
		internal const int DefaultADID = 1;
		[SecurityCritical]
		private AppDomainManager _domainManager;
		private Dictionary<string, object[]> _LocalStore;
		private AppDomainSetup _FusionStore;
		private Evidence _SecurityIdentity;
		private object[] _Policies;
		private Context _DefaultContext;
		private ActivationContext _activationContext;
		private ApplicationIdentity _applicationIdentity;
		private ApplicationTrust _applicationTrust;
		private IPrincipal _DefaultPrincipal;
		private DomainSpecificRemotingData _RemotingData;
		private EventHandler _processExit;
		private EventHandler _domainUnload;
		private UnhandledExceptionEventHandler _unhandledException;
		private string[] _aptcaVisibleAssemblies;
		private Dictionary<string, object> _compatFlags;
		private EventHandler<FirstChanceExceptionEventArgs> _firstChanceException;
		private IntPtr _pDomain;
		private PrincipalPolicy _PrincipalPolicy;
		private bool _HasSetPolicy;
		public event AssemblyLoadEventHandler AssemblyLoad
		{
			[SecurityCritical]
			add
			{
				AssemblyLoadEventHandler assemblyLoadEventHandler = this.AssemblyLoad;
				AssemblyLoadEventHandler assemblyLoadEventHandler2;
				do
				{
					assemblyLoadEventHandler2 = assemblyLoadEventHandler;
					AssemblyLoadEventHandler value2 = (AssemblyLoadEventHandler)Delegate.Combine(assemblyLoadEventHandler2, value);
					assemblyLoadEventHandler = Interlocked.CompareExchange<AssemblyLoadEventHandler>(ref this.AssemblyLoad, value2, assemblyLoadEventHandler2);
				}
				while (assemblyLoadEventHandler != assemblyLoadEventHandler2);
			}
			[SecurityCritical]
			remove
			{
				AssemblyLoadEventHandler assemblyLoadEventHandler = this.AssemblyLoad;
				AssemblyLoadEventHandler assemblyLoadEventHandler2;
				do
				{
					assemblyLoadEventHandler2 = assemblyLoadEventHandler;
					AssemblyLoadEventHandler value2 = (AssemblyLoadEventHandler)Delegate.Remove(assemblyLoadEventHandler2, value);
					assemblyLoadEventHandler = Interlocked.CompareExchange<AssemblyLoadEventHandler>(ref this.AssemblyLoad, value2, assemblyLoadEventHandler2);
				}
				while (assemblyLoadEventHandler != assemblyLoadEventHandler2);
			}
		}
		public event ResolveEventHandler TypeResolve
		{
			[SecurityCritical]
			add
			{
				ResolveEventHandler resolveEventHandler = this.TypeResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Combine(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.TypeResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
			[SecurityCritical]
			remove
			{
				ResolveEventHandler resolveEventHandler = this.TypeResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Remove(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.TypeResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
		}
		public event ResolveEventHandler ResourceResolve
		{
			[SecurityCritical]
			add
			{
				ResolveEventHandler resolveEventHandler = this.ResourceResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Combine(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.ResourceResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
			[SecurityCritical]
			remove
			{
				ResolveEventHandler resolveEventHandler = this.ResourceResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Remove(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.ResourceResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
		}
		public event ResolveEventHandler AssemblyResolve
		{
			[SecurityCritical]
			add
			{
				ResolveEventHandler resolveEventHandler = this.AssemblyResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Combine(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.AssemblyResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
			[SecurityCritical]
			remove
			{
				ResolveEventHandler resolveEventHandler = this.AssemblyResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Remove(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.AssemblyResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
		}
		public event ResolveEventHandler ReflectionOnlyAssemblyResolve
		{
			[SecurityCritical]
			add
			{
				ResolveEventHandler resolveEventHandler = this.ReflectionOnlyAssemblyResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Combine(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.ReflectionOnlyAssemblyResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
			[SecurityCritical]
			remove
			{
				ResolveEventHandler resolveEventHandler = this.ReflectionOnlyAssemblyResolve;
				ResolveEventHandler resolveEventHandler2;
				do
				{
					resolveEventHandler2 = resolveEventHandler;
					ResolveEventHandler value2 = (ResolveEventHandler)Delegate.Remove(resolveEventHandler2, value);
					resolveEventHandler = Interlocked.CompareExchange<ResolveEventHandler>(ref this.ReflectionOnlyAssemblyResolve, value2, resolveEventHandler2);
				}
				while (resolveEventHandler != resolveEventHandler2);
			}
		}
		public event EventHandler ProcessExit
		{
			[SecuritySafeCritical]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareContractedDelegate(value);
					bool flag = false;
					try
					{
						Monitor.Enter(this, ref flag);
						this._processExit = (EventHandler)Delegate.Combine(this._processExit, value);
					}
					finally
					{
						if (flag)
						{
							Monitor.Exit(this);
						}
					}
				}
			}
			[SecuritySafeCritical]
			remove
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._processExit = (EventHandler)Delegate.Remove(this._processExit, value);
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		public event EventHandler DomainUnload
		{
			[SecuritySafeCritical]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareContractedDelegate(value);
					bool flag = false;
					try
					{
						Monitor.Enter(this, ref flag);
						this._domainUnload = (EventHandler)Delegate.Combine(this._domainUnload, value);
					}
					finally
					{
						if (flag)
						{
							Monitor.Exit(this);
						}
					}
				}
			}
			[SecuritySafeCritical]
			remove
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._domainUnload = (EventHandler)Delegate.Remove(this._domainUnload, value);
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		public event UnhandledExceptionEventHandler UnhandledException
		{
			[SecurityCritical]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareContractedDelegate(value);
					bool flag = false;
					try
					{
						Monitor.Enter(this, ref flag);
						this._unhandledException = (UnhandledExceptionEventHandler)Delegate.Combine(this._unhandledException, value);
					}
					finally
					{
						if (flag)
						{
							Monitor.Exit(this);
						}
					}
				}
			}
			[SecurityCritical]
			remove
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._unhandledException = (UnhandledExceptionEventHandler)Delegate.Remove(this._unhandledException, value);
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		public event EventHandler<FirstChanceExceptionEventArgs> FirstChanceException
		{
			[SecurityCritical]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareContractedDelegate(value);
					bool flag = false;
					try
					{
						Monitor.Enter(this, ref flag);
						this._firstChanceException = (EventHandler<FirstChanceExceptionEventArgs>)Delegate.Combine(this._firstChanceException, value);
					}
					finally
					{
						if (flag)
						{
							Monitor.Exit(this);
						}
					}
				}
			}
			[SecurityCritical]
			remove
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._firstChanceException = (EventHandler<FirstChanceExceptionEventArgs>)Delegate.Remove(this._firstChanceException, value);
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		internal string[] PartialTrustVisibleAssemblies
		{
			get
			{
				return this._aptcaVisibleAssemblies;
			}
			[SecuritySafeCritical]
			set
			{
				this._aptcaVisibleAssemblies = value;
				string canonicalConditionalAptcaList = null;
				if (value != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < value.Length; i++)
					{
						if (value[i] != null)
						{
							stringBuilder.Append(value[i].ToUpperInvariant());
							if (i != value.Length - 1)
							{
								stringBuilder.Append(';');
							}
						}
					}
					canonicalConditionalAptcaList = stringBuilder.ToString();
				}
				this.SetCanonicalConditionalAptcaList(canonicalConditionalAptcaList);
			}
		}
		public AppDomainManager DomainManager
		{
			[SecurityCritical]
			get
			{
				return this._domainManager;
			}
		}
		internal HostSecurityManager HostSecurityManager
		{
			[SecurityCritical]
			get
			{
				HostSecurityManager hostSecurityManager = null;
				AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
				if (domainManager != null)
				{
					hostSecurityManager = domainManager.HostSecurityManager;
				}
				if (hostSecurityManager == null)
				{
					hostSecurityManager = new HostSecurityManager();
				}
				return hostSecurityManager;
			}
		}
		public static AppDomain CurrentDomain
		{
			get
			{
				return Thread.GetDomain();
			}
		}
		public Evidence Evidence
		{
			[SecuritySafeCritical]
			[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
			get
			{
				return this.EvidenceNoDemand;
			}
		}
		internal Evidence EvidenceNoDemand
		{
			[SecurityCritical]
			get
			{
				if (this._SecurityIdentity != null)
				{
					return this._SecurityIdentity.Clone();
				}
				if (!this.IsDefaultAppDomain() && this.nIsDefaultAppDomainForEvidence())
				{
					return AppDomain.GetDefaultDomain().Evidence;
				}
				return new Evidence(new AppDomainEvidenceFactory(this));
			}
		}
		internal Evidence InternalEvidence
		{
			get
			{
				return this._SecurityIdentity;
			}
		}
		public string FriendlyName
		{
			[SecuritySafeCritical]
			get
			{
				return this.nGetFriendlyName();
			}
		}
		public string BaseDirectory
		{
			[SecuritySafeCritical]
			get
			{
				return this.FusionStore.ApplicationBase;
			}
		}
		public string RelativeSearchPath
		{
			[SecuritySafeCritical]
			get
			{
				return this.FusionStore.PrivateBinPath;
			}
		}
		public bool ShadowCopyFiles
		{
			get
			{
				string shadowCopyFiles = this.FusionStore.ShadowCopyFiles;
				return shadowCopyFiles != null && string.Compare(shadowCopyFiles, "true", StringComparison.OrdinalIgnoreCase) == 0;
			}
		}
		public ActivationContext ActivationContext
		{
			[SecurityCritical]
			get
			{
				return this._activationContext;
			}
		}
		public ApplicationIdentity ApplicationIdentity
		{
			[SecurityCritical]
			get
			{
				return this._applicationIdentity;
			}
		}
		public ApplicationTrust ApplicationTrust
		{
			[SecurityCritical]
			get
			{
				return this._applicationTrust;
			}
		}
		public string DynamicDirectory
		{
			[SecuritySafeCritical]
			get
			{
				string dynamicDir = this.GetDynamicDir();
				if (dynamicDir != null)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dynamicDir).Demand();
				}
				return dynamicDir;
			}
		}
		internal DomainSpecificRemotingData RemotingData
		{
			get
			{
				if (this._RemotingData == null)
				{
					this.CreateRemotingData();
				}
				return this._RemotingData;
			}
		}
		internal AppDomainSetup FusionStore
		{
			get
			{
				return this._FusionStore;
			}
		}
		private Dictionary<string, object[]> LocalStore
		{
			get
			{
				if (this._LocalStore != null)
				{
					return this._LocalStore;
				}
				this._LocalStore = new Dictionary<string, object[]>();
				return this._LocalStore;
			}
		}
		public AppDomainSetup SetupInformation
		{
			get
			{
				return new AppDomainSetup(this.FusionStore, true);
			}
		}
		public PermissionSet PermissionSet
		{
			[SecurityCritical]
			get
			{
				PermissionSet permissionSet = null;
				AppDomain.GetGrantSet(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref permissionSet));
				if (permissionSet != null)
				{
					return permissionSet.Copy();
				}
				return new PermissionSet(PermissionState.Unrestricted);
			}
		}
		public bool IsFullyTrusted
		{
			[SecuritySafeCritical]
			get
			{
				PermissionSet permissionSet = null;
				AppDomain.GetGrantSet(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref permissionSet));
				return permissionSet == null || permissionSet.IsUnrestricted();
			}
		}
		public bool IsHomogenous
		{
			get
			{
				return this._applicationTrust != null;
			}
		}
		internal bool IsLegacyCasPolicyEnabled
		{
			[SecuritySafeCritical]
			get
			{
				return AppDomain.GetIsLegacyCasPolicyEnabled(this.GetNativeHandle());
			}
		}
		public int Id
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
			get
			{
				return this.GetId();
			}
		}
		public static bool MonitoringIsEnabled
		{
			[SecurityCritical]
			get
			{
				return AppDomain.nMonitoringIsEnabled();
			}
			[SecurityCritical]
			set
			{
				if (!value)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_MustBeTrue"));
				}
				AppDomain.nEnableMonitoring();
			}
		}
		public TimeSpan MonitoringTotalProcessorTime
		{
			[SecurityCritical]
			get
			{
				long num = this.nGetTotalProcessorTime();
				if (num == -1L)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
				}
				return new TimeSpan(num);
			}
		}
		public long MonitoringTotalAllocatedMemorySize
		{
			[SecurityCritical]
			get
			{
				long num = this.nGetTotalAllocatedMemorySize();
				if (num == -1L)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
				}
				return num;
			}
		}
		public long MonitoringSurvivedMemorySize
		{
			[SecurityCritical]
			get
			{
				long num = this.nGetLastSurvivedMemorySize();
				if (num == -1L)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
				}
				return num;
			}
		}
		public static long MonitoringSurvivedProcessMemorySize
		{
			[SecurityCritical]
			get
			{
				long num = AppDomain.nGetLastSurvivedProcessMemorySize();
				if (num == -1L)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
				}
				return num;
			}
		}
		public new Type GetType()
		{
			return base.GetType();
		}
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DisableFusionUpdatesFromADManager(AppDomainHandle domain);
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void GetAppDomainManagerType(AppDomainHandle domain, StringHandleOnStack retAssembly, StringHandleOnStack retType);
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void SetAppDomainManagerType(AppDomainHandle domain, string assembly, string type);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nSetHostSecurityManagerFlags(HostSecurityManagerOptions flags);
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void SetSecurityHomogeneousFlag(AppDomainHandle domain, [MarshalAs(UnmanagedType.Bool)] bool runtimeSuppliedHomogenousGrantSet);
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void SetLegacyCasPolicyEnabled(AppDomainHandle domain);
		[SecurityCritical]
		private void SetLegacyCasPolicyEnabled()
		{
			AppDomain.SetLegacyCasPolicyEnabled(this.GetNativeHandle());
		}
		internal AppDomainHandle GetNativeHandle()
		{
			if (this._pDomain.IsNull())
			{
				throw new InvalidOperationException(Environment.GetResourceString("Argument_InvalidHandle"));
			}
			return new AppDomainHandle(this._pDomain);
		}
		[SecuritySafeCritical]
		private void CreateAppDomainManager()
		{
			AppDomainSetup arg_06_0 = this.FusionStore;
			string text;
			string text2;
			this.GetAppDomainManagerType(out text, out text2);
			if (text != null && text2 != null)
			{
				try
				{
					new PermissionSet(PermissionState.Unrestricted).Assert();
					this._domainManager = (this.CreateInstanceAndUnwrap(text, text2) as AppDomainManager);
					CodeAccessPermission.RevertAssert();
				}
				catch (FileNotFoundException inner)
				{
					throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), inner);
				}
				catch (SecurityException inner2)
				{
					throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), inner2);
				}
				catch (TypeLoadException inner3)
				{
					throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), inner3);
				}
				if (this._domainManager == null)
				{
					throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"));
				}
				this.FusionStore.AppDomainManagerAssembly = text;
				this.FusionStore.AppDomainManagerType = text2;
				bool flag = this._domainManager.GetType() != typeof(AppDomainManager) && !this.DisableFusionUpdatesFromADManager();
				AppDomainSetup oldInfo = null;
				if (flag)
				{
					oldInfo = new AppDomainSetup(this.FusionStore, true);
				}
				this._domainManager.InitializeNewDomain(this.FusionStore);
				if (flag)
				{
					this.SetupFusionStore(this._FusionStore, oldInfo);
				}
				AppDomainManagerInitializationOptions initializationFlags = this._domainManager.InitializationFlags;
				if ((initializationFlags & AppDomainManagerInitializationOptions.RegisterWithHost) == AppDomainManagerInitializationOptions.RegisterWithHost)
				{
					this._domainManager.RegisterWithHost();
				}
			}
			this.InitializeCompatibilityFlags();
		}
		[SecuritySafeCritical]
		private void InitializeCompatibilityFlags()
		{
			AppDomainSetup fusionStore = this.FusionStore;
			if (fusionStore.GetCompatibilityFlags() != null)
			{
				this._compatFlags = new Dictionary<string, object>(fusionStore.GetCompatibilityFlags(), StringComparer.OrdinalIgnoreCase);
				return;
			}
			this._compatFlags = new Dictionary<string, object>();
		}
		[SecuritySafeCritical]
		internal bool DisableFusionUpdatesFromADManager()
		{
			return AppDomain.DisableFusionUpdatesFromADManager(this.GetNativeHandle());
		}
		[SecuritySafeCritical]
		internal void GetAppDomainManagerType(out string assembly, out string type)
		{
			string text = null;
			string text2 = null;
			AppDomain.GetAppDomainManagerType(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref text), JitHelpers.GetStringHandleOnStack(ref text2));
			assembly = text;
			type = text2;
		}
		[SecuritySafeCritical]
		private void SetAppDomainManagerType(string assembly, string type)
		{
			AppDomain.SetAppDomainManagerType(this.GetNativeHandle(), assembly, type);
		}
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void SetCanonicalConditionalAptcaList(AppDomainHandle appDomain, string canonicalList);
		[SecurityCritical]
		private void SetCanonicalConditionalAptcaList(string canonicalList)
		{
			AppDomain.SetCanonicalConditionalAptcaList(this.GetNativeHandle(), canonicalList);
		}
		private void SetupDefaultClickOnceDomain(string fullName, string[] manifestPaths, string[] activationData)
		{
			this.FusionStore.ActivationArguments = new ActivationArguments(fullName, manifestPaths, activationData);
		}
		[SecurityCritical]
		private void InitializeDomainSecurity(Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, bool generateDefaultEvidence, IntPtr parentSecurityDescriptor, bool publishAppDomain)
		{
			AppDomainSetup fusionStore = this.FusionStore;
			bool runtimeSuppliedHomogenousGrantSet = false;
			bool? flag = this.IsCompatibilitySwitchSet("NetFx40_LegacySecurityPolicy");
			if (flag.HasValue && flag.Value)
			{
				this.SetLegacyCasPolicyEnabled();
			}
			if (fusionStore.ApplicationTrust == null && !this.IsLegacyCasPolicyEnabled)
			{
				fusionStore.ApplicationTrust = new ApplicationTrust(new PermissionSet(PermissionState.Unrestricted));
				runtimeSuppliedHomogenousGrantSet = true;
			}
			if (fusionStore.ActivationArguments != null)
			{
				ActivationContext activationContext = null;
				ApplicationIdentity applicationIdentity = null;
				CmsUtils.CreateActivationContext(fusionStore.ActivationArguments.ApplicationFullName, fusionStore.ActivationArguments.ApplicationManifestPaths, fusionStore.ActivationArguments.UseFusionActivationContext, out applicationIdentity, out activationContext);
				string[] activationData = fusionStore.ActivationArguments.ActivationData;
				providedSecurityInfo = CmsUtils.MergeApplicationEvidence(providedSecurityInfo, applicationIdentity, activationContext, activationData, fusionStore.ApplicationTrust);
				this.SetupApplicationHelper(providedSecurityInfo, creatorsSecurityInfo, applicationIdentity, activationContext, activationData);
			}
			else
			{
				ApplicationTrust applicationTrust = fusionStore.ApplicationTrust;
				if (applicationTrust != null)
				{
					this.SetupDomainSecurityForHomogeneousDomain(applicationTrust, runtimeSuppliedHomogenousGrantSet);
				}
			}
			Evidence evidence = (providedSecurityInfo != null) ? providedSecurityInfo : creatorsSecurityInfo;
			if (evidence == null && generateDefaultEvidence)
			{
				evidence = new Evidence(new AppDomainEvidenceFactory(this));
			}
			if (this._domainManager != null)
			{
				HostSecurityManager hostSecurityManager = this._domainManager.HostSecurityManager;
				if (hostSecurityManager != null)
				{
					AppDomain.nSetHostSecurityManagerFlags(hostSecurityManager.Flags);
					if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) == HostSecurityManagerOptions.HostAppDomainEvidence)
					{
						evidence = hostSecurityManager.ProvideAppDomainEvidence(evidence);
						if (evidence != null && evidence.Target == null)
						{
							evidence.Target = new AppDomainEvidenceFactory(this);
						}
					}
				}
			}
			this._SecurityIdentity = evidence;
			this.SetupDomainSecurity(evidence, parentSecurityDescriptor, publishAppDomain);
			if (this._domainManager != null)
			{
				this.RunDomainManagerPostInitialization(this._domainManager);
			}
		}
		[SecurityCritical]
		private void RunDomainManagerPostInitialization(AppDomainManager domainManager)
		{
			HostExecutionContextManager arg_06_0 = domainManager.HostExecutionContextManager;
			if (this.IsLegacyCasPolicyEnabled)
			{
				HostSecurityManager hostSecurityManager = domainManager.HostSecurityManager;
				if (hostSecurityManager != null && (hostSecurityManager.Flags & HostSecurityManagerOptions.HostPolicyLevel) == HostSecurityManagerOptions.HostPolicyLevel)
				{
					PolicyLevel domainPolicy = hostSecurityManager.DomainPolicy;
					if (domainPolicy != null)
					{
						this.SetAppDomainPolicy(domainPolicy);
					}
				}
			}
		}
		[SecurityCritical]
		private void SetupApplicationHelper(Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, ApplicationIdentity appIdentity, ActivationContext activationContext, string[] activationData)
		{
			HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.HostSecurityManager;
			ApplicationTrust applicationTrust = hostSecurityManager.DetermineApplicationTrust(providedSecurityInfo, creatorsSecurityInfo, new TrustManagerContext());
			if (applicationTrust == null || !applicationTrust.IsApplicationTrustedToRun)
			{
				throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, null);
			}
			if (activationContext != null)
			{
				this.SetupDomainForApplication(activationContext, activationData);
			}
			this.SetupDomainSecurityForApplication(appIdentity, applicationTrust);
		}
		[SecurityCritical]
		private void SetupDomainForApplication(ActivationContext activationContext, string[] activationData)
		{
			if (this.IsDefaultAppDomain())
			{
				AppDomainSetup fusionStore = this.FusionStore;
				fusionStore.ActivationArguments = new ActivationArguments(activationContext, activationData);
				string entryPointFullPath = CmsUtils.GetEntryPointFullPath(activationContext);
				if (!string.IsNullOrEmpty(entryPointFullPath))
				{
					fusionStore.SetupDefaults(entryPointFullPath);
				}
				else
				{
					fusionStore.ApplicationBase = activationContext.ApplicationDirectory;
				}
				this.SetupFusionStore(fusionStore, null);
			}
			activationContext.PrepareForExecution();
			activationContext.SetApplicationState(ActivationContext.ApplicationState.Starting);
			activationContext.SetApplicationState(ActivationContext.ApplicationState.Running);
			IPermission permission = null;
			string dataDirectory = activationContext.DataDirectory;
			if (dataDirectory != null && dataDirectory.Length > 0)
			{
				permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dataDirectory);
			}
			this.SetData("DataDirectory", dataDirectory, permission);
			this._activationContext = activationContext;
		}
		[SecurityCritical]
		private void SetupDomainSecurityForApplication(ApplicationIdentity appIdentity, ApplicationTrust appTrust)
		{
			this._applicationIdentity = appIdentity;
			this.SetupDomainSecurityForHomogeneousDomain(appTrust, false);
		}
		[SecurityCritical]
		private void SetupDomainSecurityForHomogeneousDomain(ApplicationTrust appTrust, bool runtimeSuppliedHomogenousGrantSet)
		{
			if (runtimeSuppliedHomogenousGrantSet)
			{
				this._FusionStore.ApplicationTrust = null;
			}
			this._applicationTrust = appTrust;
			AppDomain.SetSecurityHomogeneousFlag(this.GetNativeHandle(), runtimeSuppliedHomogenousGrantSet);
		}
		[SecuritySafeCritical]
		private int ActivateApplication()
		{
			ObjectHandle objectHandle = Activator.CreateInstance(AppDomain.CurrentDomain.ActivationContext);
			return (int)objectHandle.Unwrap();
		}
		private Assembly ResolveAssemblyForIntrospection(object sender, ResolveEventArgs args)
		{
			return Assembly.ReflectionOnlyLoad(this.ApplyPolicy(args.Name));
		}
		[SecuritySafeCritical]
		private void EnableResolveAssembliesForIntrospection()
		{
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(this.ResolveAssemblyForIntrospection);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref stackCrawlMark, assemblyAttributes, SecurityContextSource.CurrentAssembly);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref stackCrawlMark, assemblyAttributes, securityContextSource);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, null, null, null, null, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, Evidence evidence)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, evidence, null, null, null, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, null, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of DefineDynamicAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, evidence, null, null, null, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, null, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, null, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  Please see http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, null, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackCrawlMark, assemblyAttributes, SecurityContextSource.CurrentAssembly);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return this.InternalDefineDynamicAssembly(name, access, dir, null, null, null, null, ref stackCrawlMark, assemblyAttributes, SecurityContextSource.CurrentAssembly);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		private AssemblyBuilder InternalDefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, ref StackCrawlMark stackMark, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
		{
			return AssemblyBuilder.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, assemblyAttributes, securityContextSource);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string nApplyPolicy(AssemblyName an);
		[ComVisible(false)]
		public string ApplyPolicy(string assemblyName)
		{
			AssemblyName assemblyName2 = new AssemblyName(assemblyName);
			byte[] array = assemblyName2.GetPublicKeyToken();
			if (array == null)
			{
				array = assemblyName2.GetPublicKey();
			}
			if (array == null || array.Length == 0)
			{
				return assemblyName;
			}
			return this.nApplyPolicy(assemblyName2);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstance(string assemblyName, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName);
		}
		[SecurityCritical]
		internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName)
		{
			PermissionSet.s_fullTrust.Assert();
			return this.CreateInstance(assemblyName, typeName);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName);
		}
		[SecurityCritical]
		internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName)
		{
			PermissionSet.s_fullTrust.Assert();
			return this.CreateInstanceFrom(assemblyName, typeName);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateComInstanceFrom(assemblyName, typeName);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateComInstanceFrom(string assemblyFile, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateComInstanceFrom(assemblyFile, typeName, hashValue, hashAlgorithm);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName, activationAttributes);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName, activationAttributes);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (securityAttributes != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			return Activator.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
		}
		[SecurityCritical]
		internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			PermissionSet.s_fullTrust.Assert();
			return this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (securityAttributes != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}
		[SecuritySafeCritical]
		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
		}
		[SecurityCritical]
		internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			PermissionSet.s_fullTrust.Assert();
			return this.CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(AssemblyName assemblyRef)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, null, ref stackCrawlMark, false, false);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(string assemblyString)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.InternalLoad(assemblyString, null, ref stackCrawlMark, false);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(byte[] rawAssembly)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.nLoadImage(rawAssembly, null, null, ref stackCrawlMark, false, SecurityContextSource.CurrentAssembly);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, null, ref stackCrawlMark, false, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			if (securityEvidence != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, securityEvidence, ref stackCrawlMark, false, SecurityContextSource.CurrentAssembly);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, assemblySecurity, ref stackCrawlMark, false, false);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(string assemblyString, Evidence assemblySecurity)
		{
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			return RuntimeAssembly.InternalLoad(assemblyString, assemblySecurity, ref stackCrawlMark, false);
		}
		[SecuritySafeCritical]
		public int ExecuteAssembly(string assemblyFile)
		{
			return this.ExecuteAssembly(assemblyFile, null);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity)
		{
			return this.ExecuteAssembly(assemblyFile, assemblySecurity, null);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity, string[] args)
		{
			if (assemblySecurity != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.LoadFrom(assemblyFile, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical]
		public int ExecuteAssembly(string assemblyFile, string[] args)
		{
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.LoadFrom(assemblyFile);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (assemblySecurity != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.LoadFrom(assemblyFile, assemblySecurity, hashValue, hashAlgorithm);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical]
		public int ExecuteAssembly(string assemblyFile, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.LoadFrom(assemblyFile, hashValue, hashAlgorithm);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical]
		public int ExecuteAssemblyByName(string assemblyName)
		{
			return this.ExecuteAssemblyByName(assemblyName, null);
		}
		[SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
		public int ExecuteAssemblyByName(string assemblyName, Evidence assemblySecurity)
		{
			return this.ExecuteAssemblyByName(assemblyName, assemblySecurity, null);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public int ExecuteAssemblyByName(string assemblyName, Evidence assemblySecurity, params string[] args)
		{
			if (assemblySecurity != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.Load(assemblyName, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical]
		public int ExecuteAssemblyByName(string assemblyName, params string[] args)
		{
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.Load(assemblyName);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public int ExecuteAssemblyByName(AssemblyName assemblyName, Evidence assemblySecurity, params string[] args)
		{
			if (assemblySecurity != null && !this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
			}
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.Load(assemblyName, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		[SecuritySafeCritical]
		public int ExecuteAssemblyByName(AssemblyName assemblyName, params string[] args)
		{
			RuntimeAssembly assembly = (RuntimeAssembly)Assembly.Load(assemblyName);
			if (args == null)
			{
				args = new string[0];
			}
			return this.nExecuteAssembly(assembly, args);
		}
		internal EvidenceBase GetHostEvidence(Type type)
		{
			if (this._SecurityIdentity != null)
			{
				return this._SecurityIdentity.GetHostEvidence(type);
			}
			return new Evidence(new AppDomainEvidenceFactory(this)).GetHostEvidence(type);
		}
		[SecuritySafeCritical]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = this.nGetFriendlyName();
			if (text != null)
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_Name") + text);
				stringBuilder.Append(Environment.NewLine);
			}
			if (this._Policies == null || this._Policies.Length == 0)
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_NoContextPolicies") + Environment.NewLine);
			}
			else
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_ContextPolicies") + Environment.NewLine);
				for (int i = 0; i < this._Policies.Length; i++)
				{
					stringBuilder.Append(this._Policies[i]);
					stringBuilder.Append(Environment.NewLine);
				}
			}
			return stringBuilder.ToString();
		}
		public Assembly[] GetAssemblies()
		{
			return this.nGetAssemblies(false);
		}
		public Assembly[] ReflectionOnlyGetAssemblies()
		{
			return this.nGetAssemblies(true);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Assembly[] nGetAssemblies(bool forIntrospection);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsUnloadingForcedFinalize();
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool IsFinalizingForUnload();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void PublishAnonymouslyHostedDynamicMethodsAssembly(RuntimeAssembly assemblyHandle);
		[SecurityCritical, Obsolete("AppDomain.AppendPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void AppendPrivatePath(string path)
		{
			if (path == null || path.Length == 0)
			{
				return;
			}
			string text = this.FusionStore.Value[5];
			StringBuilder stringBuilder = new StringBuilder();
			if (text != null && text.Length > 0)
			{
				stringBuilder.Append(text);
				if (text[text.Length - 1] != Path.PathSeparator && path[0] != Path.PathSeparator)
				{
					stringBuilder.Append(Path.PathSeparator);
				}
			}
			stringBuilder.Append(path);
			string path2 = stringBuilder.ToString();
			this.InternalSetPrivateBinPath(path2);
		}
		[SecurityCritical, Obsolete("AppDomain.ClearPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void ClearPrivatePath()
		{
			this.InternalSetPrivateBinPath(string.Empty);
		}
		[SecurityCritical, Obsolete("AppDomain.ClearShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void ClearShadowCopyPath()
		{
			this.InternalSetShadowCopyPath(string.Empty);
		}
		[Obsolete("AppDomain.SetCachePath has been deprecated. Please investigate the use of AppDomainSetup.CachePath instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
		public void SetCachePath(string path)
		{
			this.InternalSetCachePath(path);
		}
		[SecurityCritical]
		public void SetData(string name, object data)
		{
			this.SetDataHelper(name, data, null);
		}
		[SecurityCritical]
		public void SetData(string name, object data, IPermission permission)
		{
			this.SetDataHelper(name, data, permission);
		}
		[SecurityCritical]
		private void SetDataHelper(string name, object data, IPermission permission)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Equals("IgnoreSystemPolicy"))
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					if (!this._HasSetPolicy)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
					}
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
				new PermissionSet(PermissionState.Unrestricted).Demand();
			}
			int num = AppDomainSetup.Locate(name);
			if (num == -1)
			{
				lock (((ICollection)this.LocalStore).SyncRoot)
				{
					this.LocalStore[name] = new object[]
					{
						data, 
						permission
					};
					return;
				}
			}
			if (permission != null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
			}
			switch (num)
			{
				case 2:
				{
					this.FusionStore.DynamicBase = (string)data;
					return;
				}
				case 3:
				{
					this.FusionStore.DeveloperPath = (string)data;
					return;
				}
				case 7:
				{
					this.FusionStore.ShadowCopyDirectories = (string)data;
					return;
				}
				case 11:
				{
					if (data != null)
					{
						this.FusionStore.DisallowPublisherPolicy = true;
						return;
					}
					this.FusionStore.DisallowPublisherPolicy = false;
					return;
				}
				case 12:
				{
					if (data != null)
					{
						this.FusionStore.DisallowCodeDownload = true;
						return;
					}
					this.FusionStore.DisallowCodeDownload = false;
					return;
				}
				case 13:
				{
					if (data != null)
					{
						this.FusionStore.DisallowBindingRedirects = true;
						return;
					}
					this.FusionStore.DisallowBindingRedirects = false;
					return;
				}
				case 14:
				{
					if (data != null)
					{
						this.FusionStore.DisallowApplicationBaseProbing = true;
						return;
					}
					this.FusionStore.DisallowApplicationBaseProbing = false;
					return;
				}
				case 15:
				{
					this.FusionStore.SetConfigurationBytes((byte[])data);
					return;
				}
			}
			this.FusionStore.Value[num] = (string)data;
		}
		[SecuritySafeCritical]
		public object GetData(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			int num = AppDomainSetup.Locate(name);
			if (num == -1)
			{
				if (name.Equals(AppDomainSetup.LoaderOptimizationKey))
				{
					return this.FusionStore.LoaderOptimization;
				}
				object[] array;
				lock (((ICollection)this.LocalStore).SyncRoot)
				{
					this.LocalStore.TryGetValue(name, out array);
				}
				if (array == null)
				{
					return null;
				}
				if (array[1] != null)
				{
					IPermission permission = (IPermission)array[1];
					permission.Demand();
				}
				return array[0];
			}
			else
			{
				switch (num)
				{
					case 0:
					{
						return this.FusionStore.ApplicationBase;
					}
					case 1:
					{
						return this.FusionStore.ConfigurationFile;
					}
					case 2:
					{
						return this.FusionStore.DynamicBase;
					}
					case 3:
					{
						return this.FusionStore.DeveloperPath;
					}
					case 4:
					{
						return this.FusionStore.ApplicationName;
					}
					case 5:
					{
						return this.FusionStore.PrivateBinPath;
					}
					case 6:
					{
						return this.FusionStore.PrivateBinPathProbe;
					}
					case 7:
					{
						return this.FusionStore.ShadowCopyDirectories;
					}
					case 8:
					{
						return this.FusionStore.ShadowCopyFiles;
					}
					case 9:
					{
						return this.FusionStore.CachePath;
					}
					case 10:
					{
						return this.FusionStore.LicenseFile;
					}
					case 11:
					{
						return this.FusionStore.DisallowPublisherPolicy;
					}
					case 12:
					{
						return this.FusionStore.DisallowCodeDownload;
					}
					case 13:
					{
						return this.FusionStore.DisallowBindingRedirects;
					}
					case 14:
					{
						return this.FusionStore.DisallowApplicationBaseProbing;
					}
					case 15:
					{
						return this.FusionStore.GetConfigurationBytes();
					}
					default:
					{
						return null;
					}
				}
			}
		}
		public bool? IsCompatibilitySwitchSet(string value)
		{
			bool? result;
			if (this._compatFlags == null)
			{
				result = null;
			}
			else
			{
				result = new bool?(this._compatFlags.ContainsKey(value));
			}
			return result;
		}
		[Obsolete("AppDomain.GetCurrentThreadId has been deprecated because it does not provide a stable Id when managed threads are running on fibers (aka lightweight threads). To get a stable identifier for a managed thread, use the ManagedThreadId property on Thread.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();
		[SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail)]
		[SecurityPermission(SecurityAction.Demand, ControlAppDomain = true)]
		public static void Unload(AppDomain domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			try
			{
				int idForUnload = AppDomain.GetIdForUnload(domain);
				if (idForUnload == 0)
				{
					throw new CannotUnloadAppDomainException();
				}
				AppDomain.nUnload(idForUnload);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		[SecurityCritical, Obsolete("AppDomain policy levels are obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
		public void SetAppDomainPolicy(PolicyLevel domainPolicy)
		{
			if (domainPolicy == null)
			{
				throw new ArgumentNullException("domainPolicy");
			}
			if (!this.IsLegacyCasPolicyEnabled)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
			}
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._HasSetPolicy)
				{
					throw new PolicyException(Environment.GetResourceString("Policy_PolicyAlreadySet"));
				}
				this._HasSetPolicy = true;
				this.nChangeSecurityPolicy();
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
			SecurityManager.PolicyManager.AddLevel(domainPolicy);
		}
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void SetThreadPrincipal(IPrincipal principal)
		{
			if (principal == null)
			{
				throw new ArgumentNullException("principal");
			}
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._DefaultPrincipal != null)
				{
					throw new PolicyException(Environment.GetResourceString("Policy_PrincipalTwice"));
				}
				this._DefaultPrincipal = principal;
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void SetPrincipalPolicy(PrincipalPolicy policy)
		{
			this._PrincipalPolicy = policy;
		}
		[SecurityCritical]
		public override object InitializeLifetimeService()
		{
			return null;
		}
		public void DoCallBack(CrossAppDomainDelegate callBackDelegate)
		{
			if (callBackDelegate == null)
			{
				throw new ArgumentNullException("callBackDelegate");
			}
			callBackDelegate();
		}
		[SecuritySafeCritical]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo)
		{
			return AppDomain.CreateDomain(friendlyName, securityInfo, null);
		}
		[SecuritySafeCritical]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = appBasePath;
			appDomainSetup.PrivateBinPath = appRelativeSearchPath;
			if (shadowCopyFiles)
			{
				appDomainSetup.ShadowCopyFiles = "true";
			}
			return AppDomain.CreateDomain(friendlyName, securityInfo, appDomainSetup);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string GetDynamicDir();
		[SecuritySafeCritical]
		public static AppDomain CreateDomain(string friendlyName)
		{
			return AppDomain.CreateDomain(friendlyName, null, null);
		}
		[SecurityCritical]
		private static byte[] MarshalObject(object o)
		{
			CodeAccessPermission.Assert(true);
			return AppDomain.Serialize(o);
		}
		[SecurityCritical]
		private static byte[] MarshalObjects(object o1, object o2, out byte[] blob2)
		{
			CodeAccessPermission.Assert(true);
			byte[] result = AppDomain.Serialize(o1);
			blob2 = AppDomain.Serialize(o2);
			return result;
		}
		[SecurityCritical]
		private static object UnmarshalObject(byte[] blob)
		{
			CodeAccessPermission.Assert(true);
			return AppDomain.Deserialize(blob);
		}
		[SecurityCritical]
		private static object UnmarshalObjects(byte[] blob1, byte[] blob2, out object o2)
		{
			CodeAccessPermission.Assert(true);
			object result = AppDomain.Deserialize(blob1);
			o2 = AppDomain.Deserialize(blob2);
			return result;
		}
		[SecurityCritical]
		private static byte[] Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}
			if (o is ISecurityEncodable)
			{
				SecurityElement securityElement = ((ISecurityEncodable)o).ToXml();
				MemoryStream memoryStream = new MemoryStream(4096);
				memoryStream.WriteByte(0);
				StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
				securityElement.ToWriter(streamWriter);
				streamWriter.Flush();
				return memoryStream.ToArray();
			}
			MemoryStream memoryStream2 = new MemoryStream();
			memoryStream2.WriteByte(1);
			CrossAppDomainSerializer.SerializeObject(o, memoryStream2);
			return memoryStream2.ToArray();
		}
		[SecurityCritical]
		private static object Deserialize(byte[] blob)
		{
			if (blob == null)
			{
				return null;
			}
			if (blob[0] != 0)
			{
				object result = null;
				using (MemoryStream memoryStream = new MemoryStream(blob, 1, blob.Length - 1))
				{
					result = CrossAppDomainSerializer.DeserializeObject(memoryStream);
				}
				return result;
			}
			Parser parser = new Parser(blob, Tokenizer.ByteTokenEncoding.UTF8Tokens, 1);
			SecurityElement topElement = parser.GetTopElement();
			if (topElement.Tag.Equals("IPermission") || topElement.Tag.Equals("Permission"))
			{
				IPermission permission = XMLUtil.CreatePermission(topElement, PermissionState.None, false);
				if (permission == null)
				{
					return null;
				}
				permission.FromXml(topElement);
				return permission;
			}
			else
			{
				if (topElement.Tag.Equals("PermissionSet"))
				{
					PermissionSet permissionSet = new PermissionSet();
					permissionSet.FromXml(topElement, false, false);
					return permissionSet;
				}
				if (topElement.Tag.Equals("PermissionToken"))
				{
					PermissionToken permissionToken = new PermissionToken();
					permissionToken.FromXml(topElement);
					return permissionToken;
				}
				return null;
			}
		}
		private AppDomain()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Constructor"));
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nExecuteAssembly(RuntimeAssembly assembly, string[] args);
		internal int nExecuteAssembly(RuntimeAssembly assembly, string[] args)
		{
			return this._nExecuteAssembly(assembly, args);
		}
		internal void CreateRemotingData()
		{
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._RemotingData == null)
				{
					this._RemotingData = new DomainSpecificRemotingData();
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string nGetFriendlyName();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool nIsDefaultAppDomainForEvidence();
		private void OnAssemblyLoadEvent(RuntimeAssembly LoadedAssembly)
		{
			AssemblyLoadEventHandler assemblyLoad = this.AssemblyLoad;
			if (assemblyLoad != null)
			{
				AssemblyLoadEventArgs args = new AssemblyLoadEventArgs(LoadedAssembly);
				assemblyLoad(this, args);
			}
		}
		private RuntimeAssembly OnResourceResolveEvent(RuntimeAssembly assembly, string resourceName)
		{
			ResolveEventHandler resourceResolve = this.ResourceResolve;
			if (resourceResolve == null)
			{
				return null;
			}
			Delegate[] invocationList = resourceResolve.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Assembly asm = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(resourceName, assembly));
				RuntimeAssembly runtimeAssembly = AppDomain.GetRuntimeAssembly(asm);
				if (runtimeAssembly != null)
				{
					return runtimeAssembly;
				}
			}
			return null;
		}
		private RuntimeAssembly OnTypeResolveEvent(RuntimeAssembly assembly, string typeName)
		{
			ResolveEventHandler typeResolve = this.TypeResolve;
			if (typeResolve == null)
			{
				return null;
			}
			Delegate[] invocationList = typeResolve.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Assembly asm = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(typeName, assembly));
				RuntimeAssembly runtimeAssembly = AppDomain.GetRuntimeAssembly(asm);
				if (runtimeAssembly != null)
				{
					return runtimeAssembly;
				}
			}
			return null;
		}
		private RuntimeAssembly OnAssemblyResolveEvent(RuntimeAssembly assembly, string assemblyFullName)
		{
			ResolveEventHandler assemblyResolve = this.AssemblyResolve;
			if (assemblyResolve != null)
			{
				Delegate[] invocationList = assemblyResolve.GetInvocationList();
				int num = invocationList.Length;
				for (int i = 0; i < num; i++)
				{
					Assembly asm = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(assemblyFullName, assembly));
					RuntimeAssembly runtimeAssembly = AppDomain.GetRuntimeAssembly(asm);
					if (runtimeAssembly != null)
					{
						return runtimeAssembly;
					}
				}
			}
			return null;
		}
		private RuntimeAssembly OnReflectionOnlyAssemblyResolveEvent(RuntimeAssembly assembly, string assemblyFullName)
		{
			ResolveEventHandler reflectionOnlyAssemblyResolve = this.ReflectionOnlyAssemblyResolve;
			if (reflectionOnlyAssemblyResolve != null)
			{
				Delegate[] invocationList = reflectionOnlyAssemblyResolve.GetInvocationList();
				int num = invocationList.Length;
				for (int i = 0; i < num; i++)
				{
					Assembly asm = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(assemblyFullName, assembly));
					RuntimeAssembly runtimeAssembly = AppDomain.GetRuntimeAssembly(asm);
					if (runtimeAssembly != null)
					{
						return runtimeAssembly;
					}
				}
			}
			return null;
		}
		private static RuntimeAssembly GetRuntimeAssembly(Assembly asm)
		{
			if (asm == null)
			{
				return null;
			}
			RuntimeAssembly runtimeAssembly = asm as RuntimeAssembly;
			if (runtimeAssembly != null)
			{
				return runtimeAssembly;
			}
			AssemblyBuilder assemblyBuilder = asm as AssemblyBuilder;
			if (assemblyBuilder != null)
			{
				return assemblyBuilder.InternalAssembly;
			}
			return null;
		}
		private void TurnOnBindingRedirects()
		{
			this._FusionStore.DisallowBindingRedirects = false;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
		internal static int GetIdForUnload(AppDomain domain)
		{
			if (RemotingServices.IsTransparentProxy(domain))
			{
				return RemotingServices.GetServerDomainIdForProxy(domain);
			}
			return domain.Id;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsDomainIdValid(int id);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern AppDomain GetDefaultDomain();
		internal IPrincipal GetThreadPrincipal()
		{
			IPrincipal principal = null;
			bool flag = false;
			IPrincipal result;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._DefaultPrincipal == null)
				{
					switch (this._PrincipalPolicy)
					{
						case PrincipalPolicy.UnauthenticatedPrincipal:
						{
							principal = new GenericPrincipal(new GenericIdentity("", ""), new string[]
							{
								""
							});
							break;
						}
						case PrincipalPolicy.NoPrincipal:
						{
							principal = null;
							break;
						}
						case PrincipalPolicy.WindowsPrincipal:
						{
							principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
							break;
						}
						default:
						{
							principal = null;
							break;
						}
					}
				}
				else
				{
					principal = this._DefaultPrincipal;
				}
				result = principal;
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
			return result;
		}
		[SecurityCritical]
		internal void CreateDefaultContext()
		{
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._DefaultContext == null)
				{
					this._DefaultContext = Context.CreateDefaultContext();
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		[SecurityCritical]
		internal Context GetDefaultContext()
		{
			if (this._DefaultContext == null)
			{
				this.CreateDefaultContext();
			}
			return this._DefaultContext;
		}
		[SecuritySafeCritical]
		internal static void CheckDomainCreationEvidence(AppDomainSetup creationDomainSetup, Evidence creationEvidence)
		{
			if (creationEvidence != null && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled && (creationDomainSetup == null || creationDomainSetup.ApplicationTrust == null))
			{
				Zone hostEvidence = AppDomain.CurrentDomain.EvidenceNoDemand.GetHostEvidence<Zone>();
				SecurityZone securityZone = (hostEvidence != null) ? hostEvidence.SecurityZone : SecurityZone.MyComputer;
				Zone hostEvidence2 = creationEvidence.GetHostEvidence<Zone>();
				if (hostEvidence2 != null && hostEvidence2.SecurityZone != securityZone && hostEvidence2.SecurityZone != SecurityZone.MyComputer)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
				}
			}
		}
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, ControlAppDomain = true)]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup info)
		{
			return AppDomain.InternalCreateDomain(friendlyName, securityInfo, info);
		}
		[SecurityCritical]
		internal static AppDomain InternalCreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup info)
		{
			if (friendlyName == null)
			{
				throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
			}
			AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
			if (domainManager != null)
			{
				return domainManager.CreateDomain(friendlyName, securityInfo, info);
			}
			if (securityInfo != null)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
				AppDomain.CheckDomainCreationEvidence(info, securityInfo);
			}
			return AppDomain.nCreateDomain(friendlyName, info, securityInfo, (securityInfo == null) ? AppDomain.CurrentDomain.InternalEvidence : null, AppDomain.CurrentDomain.GetSecurityDescriptor());
		}
		[SecuritySafeCritical]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (info.ApplicationBase == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AppDomainSandboxAPINeedsExplicitAppBase"));
			}
			if (fullTrustAssemblies == null)
			{
				fullTrustAssemblies = new StrongName[0];
			}
			info.ApplicationTrust = new ApplicationTrust(grantSet, fullTrustAssemblies);
			return AppDomain.CreateDomain(friendlyName, securityInfo, info);
		}
		[SecuritySafeCritical]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles, AppDomainInitializer adInit, string[] adInitArgs)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = appBasePath;
			appDomainSetup.PrivateBinPath = appRelativeSearchPath;
			appDomainSetup.AppDomainInitializer = adInit;
			appDomainSetup.AppDomainInitializerArguments = adInitArgs;
			if (shadowCopyFiles)
			{
				appDomainSetup.ShadowCopyFiles = "true";
			}
			return AppDomain.CreateDomain(friendlyName, securityInfo, appDomainSetup);
		}
		[SecurityCritical]
		private void SetupFusionStore(AppDomainSetup info, AppDomainSetup oldInfo)
		{
			if (oldInfo == null)
			{
				if (info.Value[0] == null || info.Value[1] == null)
				{
					AppDomain defaultDomain = AppDomain.GetDefaultDomain();
					if (this == defaultDomain)
					{
						info.SetupDefaults(RuntimeEnvironment.GetModuleFileName());
					}
					else
					{
						if (info.Value[1] == null)
						{
							info.ConfigurationFile = defaultDomain.FusionStore.Value[1];
						}
						if (info.Value[0] == null)
						{
							info.ApplicationBase = defaultDomain.FusionStore.Value[0];
						}
						if (info.Value[4] == null)
						{
							info.ApplicationName = defaultDomain.FusionStore.Value[4];
						}
					}
				}
				if (info.Value[5] == null)
				{
					info.PrivateBinPath = Environment.nativeGetEnvironmentVariable(AppDomainSetup.PrivateBinPathEnvironmentVariable);
				}
				if (info.DeveloperPath == null)
				{
					info.DeveloperPath = RuntimeEnvironment.GetDeveloperPath();
				}
			}
			IntPtr fusionContext = this.GetFusionContext();
			info.SetupFusionContext(fusionContext, oldInfo);
			if (info.LoaderOptimization != LoaderOptimization.NotSpecified || (oldInfo != null && info.LoaderOptimization != oldInfo.LoaderOptimization))
			{
				this.UpdateLoaderOptimization(info.LoaderOptimization);
			}
			this._FusionStore = info;
		}
		private static void RunInitializer(AppDomainSetup setup)
		{
			if (setup.AppDomainInitializer != null)
			{
				string[] args = null;
				if (setup.AppDomainInitializerArguments != null)
				{
					args = (string[])setup.AppDomainInitializerArguments.Clone();
				}
				setup.AppDomainInitializer(args);
			}
		}
		[SecurityCritical]
		private static object PrepareDataForSetup(string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor, string securityZone, string[] propertyNames, string[] propertyValues)
		{
			byte[] array = null;
			bool flag = false;
			AppDomain.EvidenceCollection evidenceCollection = null;
			if (providedSecurityInfo != null || creatorsSecurityInfo != null)
			{
				HostSecurityManager hostSecurityManager = (AppDomain.CurrentDomain.DomainManager != null) ? AppDomain.CurrentDomain.DomainManager.HostSecurityManager : null;
				if (hostSecurityManager == null || !(hostSecurityManager.GetType() != typeof(HostSecurityManager)) || (hostSecurityManager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) != HostSecurityManagerOptions.HostAppDomainEvidence)
				{
					if (providedSecurityInfo != null && providedSecurityInfo.IsUnmodified && providedSecurityInfo.Target != null && providedSecurityInfo.Target is AppDomainEvidenceFactory)
					{
						providedSecurityInfo = null;
						flag = true;
					}
					if (creatorsSecurityInfo != null && creatorsSecurityInfo.IsUnmodified && creatorsSecurityInfo.Target != null && creatorsSecurityInfo.Target is AppDomainEvidenceFactory)
					{
						creatorsSecurityInfo = null;
						flag = true;
					}
				}
			}
			if (providedSecurityInfo != null || creatorsSecurityInfo != null)
			{
				evidenceCollection = new AppDomain.EvidenceCollection();
				evidenceCollection.ProvidedSecurityInfo = providedSecurityInfo;
				evidenceCollection.CreatorsSecurityInfo = creatorsSecurityInfo;
			}
			if (evidenceCollection != null)
			{
				array = CrossAppDomainSerializer.SerializeObject(evidenceCollection).GetBuffer();
			}
			AppDomainInitializerInfo appDomainInitializerInfo = null;
			if (setup != null && setup.AppDomainInitializer != null)
			{
				appDomainInitializerInfo = new AppDomainInitializerInfo(setup.AppDomainInitializer);
			}
			AppDomainSetup appDomainSetup = new AppDomainSetup(setup, false);
			return new object[]
			{
				friendlyName, 
				appDomainSetup, 
				parentSecurityDescriptor, 
				flag, 
				array, 
				appDomainInitializerInfo, 
				securityZone, 
				propertyNames, 
				propertyValues
			};
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static object Setup(object arg)
		{
			object[] array = (object[])arg;
			string friendlyName = (string)array[0];
			AppDomainSetup appDomainSetup = (AppDomainSetup)array[1];
			IntPtr parentSecurityDescriptor = (IntPtr)array[2];
			bool generateDefaultEvidence = (bool)array[3];
			byte[] array2 = (byte[])array[4];
			AppDomainInitializerInfo appDomainInitializerInfo = (AppDomainInitializerInfo)array[5];
			string arg_48_0 = (string)array[6];
			string[] array3 = (string[])array[7];
			string[] array4 = (string[])array[8];
			Evidence evidence = null;
			Evidence creatorsSecurityInfo = null;
			AppDomain currentDomain = AppDomain.CurrentDomain;
			AppDomainSetup appDomainSetup2 = new AppDomainSetup(appDomainSetup, false);
			if (array3 != null && array4 != null)
			{
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i] == "APPBASE")
					{
						if (array4[i] == null)
						{
							throw new ArgumentNullException("APPBASE");
						}
						if (Path.IsRelative(array4[i]))
						{
							throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
						}
						appDomainSetup2.ApplicationBase = Path.NormalizePath(array4[i], true);
					}
					else
					{
						if (array3[i] == "LOCATION_URI" && evidence == null)
						{
							evidence = new Evidence();
							evidence.AddHostEvidence<Url>(new Url(array4[i]));
							currentDomain.SetDataHelper(array3[i], array4[i], null);
						}
						else
						{
							if (array3[i] == "LOADER_OPTIMIZATION")
							{
								if (array4[i] == null)
								{
									throw new ArgumentNullException("LOADER_OPTIMIZATION");
								}
								string a;
								if ((a = array4[i]) != null)
								{
									if (a == "SingleDomain")
									{
										appDomainSetup2.LoaderOptimization = LoaderOptimization.SingleDomain;
										goto IL_1CA;
									}
									if (a == "MultiDomain")
									{
										appDomainSetup2.LoaderOptimization = LoaderOptimization.MultiDomain;
										goto IL_1CA;
									}
									if (a == "MultiDomainHost")
									{
										appDomainSetup2.LoaderOptimization = LoaderOptimization.MultiDomainHost;
										goto IL_1CA;
									}
									if (a == "NotSpecified")
									{
										appDomainSetup2.LoaderOptimization = LoaderOptimization.NotSpecified;
										goto IL_1CA;
									}
								}
								throw new ArgumentException(Environment.GetResourceString("Argument_UnrecognizedLoaderOptimization"), "LOADER_OPTIMIZATION");
							}
						}
					}
					IL_1CA:;
				}
			}
			currentDomain.SetupFusionStore(appDomainSetup2, null);
			AppDomainSetup fusionStore = currentDomain.FusionStore;
			if (array2 != null)
			{
				AppDomain.EvidenceCollection evidenceCollection = (AppDomain.EvidenceCollection)CrossAppDomainSerializer.DeserializeObject(new MemoryStream(array2));
				evidence = evidenceCollection.ProvidedSecurityInfo;
				creatorsSecurityInfo = evidenceCollection.CreatorsSecurityInfo;
			}
			currentDomain.nSetupFriendlyName(friendlyName);
			if (appDomainSetup != null && appDomainSetup.SandboxInterop)
			{
				currentDomain.nSetDisableInterfaceCache();
			}
			if (fusionStore.AppDomainManagerAssembly != null && fusionStore.AppDomainManagerType != null)
			{
				currentDomain.SetAppDomainManagerType(fusionStore.AppDomainManagerAssembly, fusionStore.AppDomainManagerType);
			}
			currentDomain.PartialTrustVisibleAssemblies = fusionStore.PartialTrustVisibleAssemblies;
			currentDomain.CreateAppDomainManager();
			currentDomain.InitializeDomainSecurity(evidence, creatorsSecurityInfo, generateDefaultEvidence, parentSecurityDescriptor, true);
			if (appDomainInitializerInfo != null)
			{
				fusionStore.AppDomainInitializer = appDomainInitializerInfo.Unwrap();
			}
			AppDomain.RunInitializer(fusionStore);
			ObjectHandle obj = null;
			if (fusionStore.ActivationArguments != null && fusionStore.ActivationArguments.ActivateInstance)
			{
				obj = Activator.CreateInstance(currentDomain.ActivationContext);
			}
			return RemotingServices.MarshalInternal(obj, null, null);
		}
		[SecuritySafeCritical]
		[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
		private bool IsAssemblyOnAptcaVisibleList(RuntimeAssembly assembly)
		{
			if (this._aptcaVisibleAssemblies == null)
			{
				return false;
			}
			AssemblyName name = assembly.GetName();
			string text = name.GetNameWithPublicKey();
			text = text.ToUpperInvariant();
			int num = Array.BinarySearch<string>(this._aptcaVisibleAssemblies, text, StringComparer.OrdinalIgnoreCase);
			return num >= 0;
		}
		[SecurityCritical]
		private unsafe bool IsAssemblyOnAptcaVisibleListRaw(char* namePtr, int nameLen, byte* keyTokenPtr, int keyTokenLen)
		{
			if (this._aptcaVisibleAssemblies == null)
			{
				return false;
			}
			string name = new string(namePtr, 0, nameLen);
			byte[] array = new byte[keyTokenLen];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = keyTokenPtr[(IntPtr)i / 1];
			}
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = name;
			assemblyName.SetPublicKeyToken(array);
			bool result;
			try
			{
				int num = Array.BinarySearch(this._aptcaVisibleAssemblies, assemblyName, new AppDomain.CAPTCASearcher());
				result = (num >= 0);
			}
			catch (InvalidOperationException)
			{
				result = false;
			}
			return result;
		}
		[SecurityCritical]
		private void SetupDomain(bool allowRedirects, string path, string configFile, string[] propertyNames, string[] propertyValues)
		{
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this._FusionStore == null)
				{
					AppDomainSetup appDomainSetup = new AppDomainSetup();
					appDomainSetup.SetupDefaults(RuntimeEnvironment.GetModuleFileName());
					if (path != null)
					{
						appDomainSetup.Value[0] = path;
					}
					if (configFile != null)
					{
						appDomainSetup.Value[1] = configFile;
					}
					if (!allowRedirects)
					{
						appDomainSetup.DisallowBindingRedirects = true;
					}
					if (propertyNames != null)
					{
						for (int i = 0; i < propertyNames.Length; i++)
						{
							if (string.Equals(propertyNames[i], "PARTIAL_TRUST_VISIBLE_ASSEMBLIES", StringComparison.Ordinal) && propertyValues[i] != null)
							{
								if (propertyValues[i].Length > 0)
								{
									appDomainSetup.PartialTrustVisibleAssemblies = propertyValues[i].Split(new char[]
									{
										';'
									});
								}
								else
								{
									appDomainSetup.PartialTrustVisibleAssemblies = new string[0];
								}
							}
						}
					}
					this.PartialTrustVisibleAssemblies = appDomainSetup.PartialTrustVisibleAssemblies;
					this.SetupFusionStore(appDomainSetup, null);
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		[SecurityCritical]
		private void SetupLoaderOptimization(LoaderOptimization policy)
		{
			if (policy != LoaderOptimization.NotSpecified)
			{
				this.FusionStore.LoaderOptimization = policy;
				this.UpdateLoaderOptimization(this.FusionStore.LoaderOptimization);
			}
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetFusionContext();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetSecurityDescriptor();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern AppDomain nCreateDomain(string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern ObjRef nCreateInstance(string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);
		[SecurityCritical]
		private void SetupDomainSecurity(Evidence appDomainEvidence, IntPtr creatorsSecurityDescriptor, bool publishAppDomain)
		{
			Evidence evidence = appDomainEvidence;
			AppDomain.SetupDomainSecurity(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<Evidence>(ref evidence), creatorsSecurityDescriptor, publishAppDomain);
		}
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void SetupDomainSecurity(AppDomainHandle appDomain, ObjectHandleOnStack appDomainEvidence, IntPtr creatorsSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool publishAppDomain);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nSetupFriendlyName(string friendlyName);
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nSetDisableInterfaceCache();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void UpdateLoaderOptimization(LoaderOptimization optimization);
		[SecurityCritical, Obsolete("AppDomain.SetShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void SetShadowCopyPath(string path)
		{
			this.InternalSetShadowCopyPath(path);
		}
		[Obsolete("AppDomain.SetShadowCopyFiles has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyFiles instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
		public void SetShadowCopyFiles()
		{
			this.InternalSetShadowCopyFiles();
		}
		[Obsolete("AppDomain.SetDynamicBase has been deprecated. Please investigate the use of AppDomainSetup.DynamicBase instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
		public void SetDynamicBase(string path)
		{
			this.InternalSetDynamicBase(path);
		}
		[SecurityCritical]
		internal void InternalSetShadowCopyPath(string path)
		{
			if (path != null)
			{
				IntPtr fusionContext = this.GetFusionContext();
				AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.ShadowCopyDirectoriesKey, path);
			}
			this.FusionStore.ShadowCopyDirectories = path;
		}
		[SecurityCritical]
		internal void InternalSetShadowCopyFiles()
		{
			IntPtr fusionContext = this.GetFusionContext();
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.ShadowCopyFilesKey, "true");
			this.FusionStore.ShadowCopyFiles = "true";
		}
		[SecurityCritical]
		internal void InternalSetCachePath(string path)
		{
			this.FusionStore.CachePath = path;
			if (this.FusionStore.Value[9] != null)
			{
				IntPtr fusionContext = this.GetFusionContext();
				AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.CachePathKey, this.FusionStore.Value[9]);
			}
		}
		[SecurityCritical]
		internal void InternalSetPrivateBinPath(string path)
		{
			IntPtr fusionContext = this.GetFusionContext();
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.PrivateBinPathKey, path);
			this.FusionStore.PrivateBinPath = path;
		}
		[SecurityCritical]
		internal void InternalSetDynamicBase(string path)
		{
			this.FusionStore.DynamicBase = path;
			if (this.FusionStore.Value[2] != null)
			{
				IntPtr fusionContext = this.GetFusionContext();
				AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.DynamicBaseKey, this.FusionStore.Value[2]);
			}
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string IsStringInterned(string str);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string GetOrInternString(string str);
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void GetGrantSet(AppDomainHandle domain, ObjectHandleOnStack retGrantSet);
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetIsLegacyCasPolicyEnabled(AppDomainHandle domain);
		internal PermissionSet GetHomogenousGrantSet(Evidence evidence)
		{
			if (evidence.GetDelayEvaluatedHostEvidence<StrongName>() != null)
			{
				foreach (StrongName current in this._applicationTrust.FullTrustAssemblies)
				{
					StrongNameMembershipCondition strongNameMembershipCondition = new StrongNameMembershipCondition(current.PublicKey, current.Name, current.Version);
					object obj = null;
					if (((IReportMatchMembershipCondition)strongNameMembershipCondition).Check(evidence, out obj))
					{
						IDelayEvaluatedEvidence delayEvaluatedEvidence = obj as IDelayEvaluatedEvidence;
						if (obj != null)
						{
							delayEvaluatedEvidence.MarkUsed();
						}
						return new PermissionSet(PermissionState.Unrestricted);
					}
				}
			}
			return this._applicationTrust.DefaultGrantSet.PermissionSet.Copy();
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nChangeSecurityPolicy();
		[SecurityCritical, ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nUnload(int domainInternal);
		[SecuritySafeCritical]
		public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
		{
			ObjectHandle objectHandle = this.CreateInstance(assemblyName, typeName);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[SecuritySafeCritical]
		public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstance(assemblyName, typeName, activationAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceAndUnwrap which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[SecuritySafeCritical]
		public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[SecuritySafeCritical]
		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName)
		{
			ObjectHandle objectHandle = this.CreateInstanceFrom(assemblyName, typeName);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[SecuritySafeCritical]
		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstanceFrom(assemblyName, typeName, activationAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFromAndUnwrap which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[SecuritySafeCritical]
		public object CreateInstanceFromAndUnwrap(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			ObjectHandle objectHandle = this.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
			if (objectHandle == null)
			{
				return null;
			}
			return objectHandle.Unwrap();
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetId();
		public bool IsDefaultAppDomain()
		{
			return this.GetId() == 1;
		}
		private static AppDomainSetup InternalCreateDomainSetup(string imageLocation)
		{
			int num = imageLocation.LastIndexOf('\\');
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = imageLocation.Substring(0, num + 1);
			StringBuilder stringBuilder = new StringBuilder(imageLocation.Substring(num + 1));
			stringBuilder.Append(AppDomainSetup.ConfigurationExtension);
			appDomainSetup.ConfigurationFile = stringBuilder.ToString();
			return appDomainSetup;
		}
		private static AppDomain InternalCreateDomain(string imageLocation)
		{
			AppDomainSetup info = AppDomain.InternalCreateDomainSetup(imageLocation);
			return AppDomain.CreateDomain("Validator", null, info);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nEnableMonitoring();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nMonitoringIsEnabled();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern long nGetTotalProcessorTime();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern long nGetTotalAllocatedMemorySize();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern long nGetLastSurvivedMemorySize();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern long nGetLastSurvivedProcessMemorySize();
		[SecurityCritical]
		private void InternalSetDomainContext(string imageLocation)
		{
			this.SetupFusionStore(AppDomain.InternalCreateDomainSetup(imageLocation), null);
		}
		void _AppDomain.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}
		void _AppDomain.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}
		void _AppDomain.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}
		void _AppDomain.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
