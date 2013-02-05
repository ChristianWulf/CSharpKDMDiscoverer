using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Cache;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
namespace System
{
	[Serializable]
	internal class RuntimeType : Type, ISerializable, ICloneable
	{
		[Serializable]
		internal class RuntimeTypeCache
		{
			internal enum WhatsCached
			{
				Nothing,
				EnclosingType
			}
			internal enum CacheType
			{
				Method,
				Constructor,
				Field,
				Property,
				Event,
				Interface,
				NestedType
			}
			private struct Filter
			{
				private Utf8String m_name;
				private MemberListType m_listType;
				private uint m_nameHash;
				[SecurityCritical]
				public unsafe Filter(byte* pUtf8Name, int cUtf8Name, MemberListType listType)
				{
					this.m_name = new Utf8String((void*)pUtf8Name, cUtf8Name);
					this.m_listType = listType;
					this.m_nameHash = 0u;
					if (this.RequiresStringComparison())
					{
						this.m_nameHash = this.m_name.HashCaseInsensitive();
					}
				}
				public bool Match(Utf8String name)
				{
					bool result = true;
					if (this.m_listType == MemberListType.CaseSensitive)
					{
						result = this.m_name.Equals(name);
					}
					else
					{
						if (this.m_listType == MemberListType.CaseInsensitive)
						{
							result = this.m_name.EqualsCaseInsensitive(name);
						}
					}
					return result;
				}
				[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
				public bool RequiresStringComparison()
				{
					return this.m_listType == MemberListType.CaseSensitive || this.m_listType == MemberListType.CaseInsensitive;
				}
				public uint GetHashToMatch()
				{
					return this.m_nameHash;
				}
			}
			[Serializable]
			private class MemberInfoCache<T> where T : MemberInfo
			{
				private CerHashtable<string, CerArrayList<T>> m_csMemberInfos;
				private CerHashtable<string, CerArrayList<T>> m_cisMemberInfos;
				private CerArrayList<T> m_root;
				private bool m_cacheComplete;
				private RuntimeType.RuntimeTypeCache m_runtimeTypeCache;
				internal RuntimeType ReflectedType
				{
					get
					{
						return this.m_runtimeTypeCache.GetRuntimeType();
					}
				}
				[SecuritySafeCritical]
				static MemberInfoCache()
				{
					RuntimeType.PrepareMemberInfoCache(typeof(RuntimeType.RuntimeTypeCache.MemberInfoCache<T>).TypeHandle);
				}
				[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
				internal MemberInfoCache(RuntimeType.RuntimeTypeCache runtimeTypeCache)
				{
					Mda.MemberInfoCacheCreation();
					this.m_runtimeTypeCache = runtimeTypeCache;
					this.m_cacheComplete = false;
				}
				[SecuritySafeCritical]
				internal MethodBase AddMethod(RuntimeType declaringType, RuntimeMethodHandleInternal method, RuntimeType.RuntimeTypeCache.CacheType cacheType)
				{
					object obj = null;
					MethodAttributes attributes = RuntimeMethodHandle.GetAttributes(method);
					bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
					bool isStatic = (attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
					bool isInherited = declaringType != this.ReflectedType;
					BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
					switch (cacheType)
					{
						case RuntimeType.RuntimeTypeCache.CacheType.Method:
						{
							obj = new List<RuntimeMethodInfo>(1)
							{
								new RuntimeMethodInfo(method, declaringType, this.m_runtimeTypeCache, attributes, bindingFlags, null)
							};
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Constructor:
						{
							obj = new List<RuntimeConstructorInfo>(1)
							{
								new RuntimeConstructorInfo(method, declaringType, this.m_runtimeTypeCache, attributes, bindingFlags)
							};
							break;
						}
					}
					CerArrayList<T> cerArrayList = new CerArrayList<T>((List<T>)obj);
					this.Insert(ref cerArrayList, null, MemberListType.HandleToInfo);
					return (MethodBase)cerArrayList[0];
				}
				[SecuritySafeCritical]
				internal FieldInfo AddField(RuntimeFieldHandleInternal field)
				{
					List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>(1);
					FieldAttributes attributes = RuntimeFieldHandle.GetAttributes(field);
					bool isPublic = (attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
					bool isStatic = (attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
					bool isInherited = RuntimeFieldHandle.GetApproxDeclaringType(field) != this.ReflectedType;
					BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
					list.Add(new RtFieldInfo(field, this.ReflectedType, this.m_runtimeTypeCache, bindingFlags));
					CerArrayList<T> cerArrayList = new CerArrayList<T>((List<T>)list);
					this.Insert(ref cerArrayList, null, MemberListType.HandleToInfo);
					return (FieldInfo)cerArrayList[0];
				}
				[SecuritySafeCritical]
				private unsafe CerArrayList<T> Populate(string name, MemberListType listType, RuntimeType.RuntimeTypeCache.CacheType cacheType)
				{
					List<T> list = null;
					if (name == null || name.Length == 0 || (cacheType == RuntimeType.RuntimeTypeCache.CacheType.Constructor && name.FirstChar != '.' && name.FirstChar != '*'))
					{
						list = this.GetListByName(null, 0, null, 0, listType, cacheType);
					}
					else
					{
						int length = name.Length;
						fixed (char* ptr = name)
						{
							int byteCount = Encoding.UTF8.GetByteCount(ptr, length);
							if (byteCount > 1024)
							{
								fixed (byte* ptr2 = new byte[byteCount])
								{
									list = this.GetListByName(ptr, length, ptr2, byteCount, listType, cacheType);
								}
							}
							else
							{
								byte* pUtf8Name = stackalloc byte[(UIntPtr)byteCount / 1];
								list = this.GetListByName(ptr, length, pUtf8Name, byteCount, listType, cacheType);
							}
						}
					}
					CerArrayList<T> result = new CerArrayList<T>(list);
					this.Insert(ref result, name, listType);
					return result;
				}
				[SecurityCritical]
				private unsafe List<T> GetListByName(char* pName, int cNameLen, byte* pUtf8Name, int cUtf8Name, MemberListType listType, RuntimeType.RuntimeTypeCache.CacheType cacheType)
				{
					if (cNameLen != 0)
					{
						Encoding.UTF8.GetBytes(pName, cNameLen, pUtf8Name, cUtf8Name);
					}
					RuntimeType.RuntimeTypeCache.Filter filter = new RuntimeType.RuntimeTypeCache.Filter(pUtf8Name, cUtf8Name, listType);
					List<T> result = null;
					switch (cacheType)
					{
						case RuntimeType.RuntimeTypeCache.CacheType.Method:
						{
							result = (this.PopulateMethods(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Constructor:
						{
							result = (this.PopulateConstructors(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Field:
						{
							result = (this.PopulateFields(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Property:
						{
							result = (this.PopulateProperties(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Event:
						{
							result = (this.PopulateEvents(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.Interface:
						{
							result = (this.PopulateInterfaces(filter) as List<T>);
							break;
						}
						case RuntimeType.RuntimeTypeCache.CacheType.NestedType:
						{
							result = (this.PopulateNestedClasses(filter) as List<T>);
							break;
						}
					}
					return result;
				}
				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
				internal void Insert(ref CerArrayList<T> list, string name, MemberListType listType)
				{
					bool flag = false;
					bool flag2 = false;
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
						Monitor.Enter(this, ref flag);
						if (listType == MemberListType.CaseSensitive)
						{
							if (this.m_csMemberInfos == null)
							{
								this.m_csMemberInfos = new CerHashtable<string, CerArrayList<T>>();
							}
							else
							{
								this.m_csMemberInfos.Preallocate(1);
							}
						}
						else
						{
							if (listType == MemberListType.CaseInsensitive)
							{
								if (this.m_cisMemberInfos == null)
								{
									this.m_cisMemberInfos = new CerHashtable<string, CerArrayList<T>>();
								}
								else
								{
									this.m_cisMemberInfos.Preallocate(1);
								}
							}
						}
						if (this.m_root == null)
						{
							this.m_root = new CerArrayList<T>(list.Count);
						}
						else
						{
							this.m_root.Preallocate(list.Count);
						}
						flag2 = true;
					}
					finally
					{
						try
						{
							if (flag2)
							{
								if (listType == MemberListType.CaseSensitive)
								{
									CerArrayList<T> cerArrayList = this.m_csMemberInfos[name];
									if (cerArrayList == null)
									{
										this.MergeWithGlobalList(list);
										this.m_csMemberInfos[name] = list;
									}
									else
									{
										list = cerArrayList;
									}
								}
								else
								{
									if (listType == MemberListType.CaseInsensitive)
									{
										CerArrayList<T> cerArrayList2 = this.m_cisMemberInfos[name];
										if (cerArrayList2 == null)
										{
											this.MergeWithGlobalList(list);
											this.m_cisMemberInfos[name] = list;
										}
										else
										{
											list = cerArrayList2;
										}
									}
									else
									{
										this.MergeWithGlobalList(list);
									}
								}
								if (listType == MemberListType.All)
								{
									this.m_cacheComplete = true;
								}
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
				}
				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
				private void MergeWithGlobalList(CerArrayList<T> list)
				{
					int count = this.m_root.Count;
					for (int i = 0; i < list.Count; i++)
					{
						T value = list[i];
						T t = default(T);
						bool flag = false;
						for (int j = 0; j < count; j++)
						{
							t = this.m_root[j];
							if (value.CacheEquals(t))
							{
								list.Replace(i, t);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							this.m_root.Add(value);
						}
					}
				}
				[SecuritySafeCritical]
				private unsafe List<RuntimeMethodInfo> PopulateMethods(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>();
					RuntimeType runtimeType = this.ReflectedType;
					bool flag = (RuntimeTypeHandle.GetAttributes(runtimeType) & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask;
					if (flag)
					{
						RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = RuntimeTypeHandle.GetIntroducedMethods(runtimeType).GetEnumerator();
						while (enumerator.MoveNext())
						{
							RuntimeMethodHandleInternal current = enumerator.Current;
							if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(current, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(current))))
							{
								MethodAttributes attributes = RuntimeMethodHandle.GetAttributes(current);
								bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
								bool isStatic = (attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
								bool isInherited = false;
								BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
								if ((attributes & MethodAttributes.RTSpecialName) == MethodAttributes.PrivateScope && !RuntimeMethodHandle.IsILStub(current))
								{
									RuntimeMethodHandleInternal stubIfNeeded = RuntimeMethodHandle.GetStubIfNeeded(current, runtimeType, null);
									RuntimeMethodInfo item = new RuntimeMethodInfo(stubIfNeeded, runtimeType, this.m_runtimeTypeCache, attributes, bindingFlags, null);
									list.Add(item);
								}
							}
						}
					}
					else
					{
						while (RuntimeTypeHandle.IsGenericVariable(runtimeType))
						{
							runtimeType = runtimeType.GetBaseType();
						}
						bool* ptr = stackalloc bool[(UIntPtr)RuntimeTypeHandle.GetNumVirtuals(runtimeType) / 1];
						bool isValueType = runtimeType.IsValueType;
						do
						{
							int numVirtuals = RuntimeTypeHandle.GetNumVirtuals(runtimeType);
							RuntimeTypeHandle.IntroducedMethodEnumerator enumerator2 = RuntimeTypeHandle.GetIntroducedMethods(runtimeType).GetEnumerator();
							while (enumerator2.MoveNext())
							{
								RuntimeMethodHandleInternal current2 = enumerator2.Current;
								if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(current2, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(current2))))
								{
									MethodAttributes attributes2 = RuntimeMethodHandle.GetAttributes(current2);
									MethodAttributes methodAttributes = attributes2 & MethodAttributes.MemberAccessMask;
									if ((attributes2 & MethodAttributes.RTSpecialName) == MethodAttributes.PrivateScope && !RuntimeMethodHandle.IsILStub(current2))
									{
										bool flag2 = false;
										int num = 0;
										if ((attributes2 & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
										{
											num = RuntimeMethodHandle.GetSlot(current2);
											flag2 = (num < numVirtuals);
										}
										bool flag3 = methodAttributes == MethodAttributes.Private;
										bool flag4 = flag2 & flag3;
										bool flag5 = runtimeType != this.ReflectedType;
										if (!flag5 || !flag3 || flag4)
										{
											if (flag2)
											{
												if (ptr[(IntPtr)num / 1])
												{
													continue;
												}
												ptr[(IntPtr)num / 1] = true;
											}
											else
											{
												if (isValueType && (attributes2 & (MethodAttributes.Virtual | MethodAttributes.Abstract)) != MethodAttributes.PrivateScope)
												{
													continue;
												}
											}
											bool isPublic2 = methodAttributes == MethodAttributes.Public;
											bool isStatic2 = (attributes2 & MethodAttributes.Static) != MethodAttributes.PrivateScope;
											BindingFlags bindingFlags2 = RuntimeType.FilterPreCalculate(isPublic2, flag5, isStatic2);
											RuntimeMethodHandleInternal stubIfNeeded2 = RuntimeMethodHandle.GetStubIfNeeded(current2, runtimeType, null);
											RuntimeMethodInfo item2 = new RuntimeMethodInfo(stubIfNeeded2, runtimeType, this.m_runtimeTypeCache, attributes2, bindingFlags2, null);
											list.Add(item2);
										}
									}
								}
							}
							runtimeType = RuntimeTypeHandle.GetBaseType(runtimeType);
						}
						while (runtimeType != null);
					}
					return list;
				}
				[SecuritySafeCritical]
				private List<RuntimeConstructorInfo> PopulateConstructors(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>();
					if (this.ReflectedType.IsGenericParameter)
					{
						return list;
					}
					RuntimeType reflectedType = this.ReflectedType;
					RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = RuntimeTypeHandle.GetIntroducedMethods(reflectedType).GetEnumerator();
					while (enumerator.MoveNext())
					{
						RuntimeMethodHandleInternal current = enumerator.Current;
						if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(current, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(current))))
						{
							MethodAttributes attributes = RuntimeMethodHandle.GetAttributes(current);
							if ((attributes & MethodAttributes.RTSpecialName) != MethodAttributes.PrivateScope && !RuntimeMethodHandle.IsILStub(current))
							{
								bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
								bool isStatic = (attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
								bool isInherited = false;
								BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
								RuntimeMethodHandleInternal stubIfNeeded = RuntimeMethodHandle.GetStubIfNeeded(current, reflectedType, null);
								RuntimeConstructorInfo item = new RuntimeConstructorInfo(stubIfNeeded, this.ReflectedType, this.m_runtimeTypeCache, attributes, bindingFlags);
								list.Add(item);
							}
						}
					}
					return list;
				}
				[SecuritySafeCritical]
				private List<RuntimeFieldInfo> PopulateFields(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>();
					RuntimeType runtimeType = this.ReflectedType;
					while (RuntimeTypeHandle.IsGenericVariable(runtimeType))
					{
						runtimeType = runtimeType.GetBaseType();
					}
					while (runtimeType != null)
					{
						this.PopulateRtFields(filter, runtimeType, list);
						this.PopulateLiteralFields(filter, runtimeType, list);
						runtimeType = RuntimeTypeHandle.GetBaseType(runtimeType);
					}
					if (this.ReflectedType.IsGenericParameter)
					{
						Type[] interfaces = this.ReflectedType.BaseType.GetInterfaces();
						for (int i = 0; i < interfaces.Length; i++)
						{
							this.PopulateLiteralFields(filter, (RuntimeType)interfaces[i], list);
							this.PopulateRtFields(filter, (RuntimeType)interfaces[i], list);
						}
					}
					else
					{
						Type[] interfaces2 = RuntimeTypeHandle.GetInterfaces(this.ReflectedType);
						if (interfaces2 != null)
						{
							for (int j = 0; j < interfaces2.Length; j++)
							{
								this.PopulateLiteralFields(filter, (RuntimeType)interfaces2[j], list);
								this.PopulateRtFields(filter, (RuntimeType)interfaces2[j], list);
							}
						}
					}
					return list;
				}
				[SecuritySafeCritical]
				private unsafe void PopulateRtFields(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, List<RuntimeFieldInfo> list)
				{
					IntPtr* ptr = stackalloc IntPtr[checked(unchecked((UIntPtr)64) * (UIntPtr)sizeof(IntPtr)) / sizeof(IntPtr)];
					int num = 64;
					if (!RuntimeTypeHandle.GetFields(declaringType, ptr, &num))
					{
						fixed (IntPtr* ptr2 = new IntPtr[num])
						{
							RuntimeTypeHandle.GetFields(declaringType, ptr2, &num);
							this.PopulateRtFields(filter, ptr2, num, declaringType, list);
						}
						return;
					}
					if (num > 0)
					{
						this.PopulateRtFields(filter, ptr, num, declaringType, list);
					}
				}
				[SecurityCritical]
				private unsafe void PopulateRtFields(RuntimeType.RuntimeTypeCache.Filter filter, IntPtr* ppFieldHandles, int count, RuntimeType declaringType, List<RuntimeFieldInfo> list)
				{
					bool flag = RuntimeTypeHandle.HasInstantiation(declaringType) && !RuntimeTypeHandle.ContainsGenericVariables(declaringType);
					bool flag2 = declaringType != this.ReflectedType;
					for (int i = 0; i < count; i++)
					{
						RuntimeFieldHandleInternal staticFieldForGenericType = new RuntimeFieldHandleInternal(ppFieldHandles[(IntPtr)i * (IntPtr)sizeof(IntPtr) / sizeof(IntPtr)]);
						if (!filter.RequiresStringComparison() || (RuntimeFieldHandle.MatchesNameHash(staticFieldForGenericType, filter.GetHashToMatch()) && filter.Match(RuntimeFieldHandle.GetUtf8Name(staticFieldForGenericType))))
						{
							FieldAttributes attributes = RuntimeFieldHandle.GetAttributes(staticFieldForGenericType);
							FieldAttributes fieldAttributes = attributes & FieldAttributes.FieldAccessMask;
							if (!flag2 || fieldAttributes != FieldAttributes.Private)
							{
								bool isPublic = fieldAttributes == FieldAttributes.Public;
								bool flag3 = (attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
								BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, flag2, flag3);
								if (flag && flag3)
								{
									staticFieldForGenericType = RuntimeFieldHandle.GetStaticFieldForGenericType(staticFieldForGenericType, declaringType);
								}
								RuntimeFieldInfo item = new RtFieldInfo(staticFieldForGenericType, declaringType, this.m_runtimeTypeCache, bindingFlags);
								list.Add(item);
							}
						}
					}
				}
				[SecuritySafeCritical]
				private unsafe void PopulateLiteralFields(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, List<RuntimeFieldInfo> list)
				{
					int token = RuntimeTypeHandle.GetToken(declaringType);
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return;
					}
					MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
					int num = metadataImport.EnumFieldsCount(token);
					int* ptr = stackalloc int[(UIntPtr)num];
					metadataImport.EnumFields(token, ptr, num);
					for (int i = 0; i < num; i++)
					{
						int num2 = ptr[(IntPtr)i];
						FieldAttributes fieldAttributes;
						metadataImport.GetFieldDefProps(num2, out fieldAttributes);
						FieldAttributes fieldAttributes2 = fieldAttributes & FieldAttributes.FieldAccessMask;
						if ((fieldAttributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
						{
							bool flag = declaringType != this.ReflectedType;
							if (!flag || fieldAttributes2 != FieldAttributes.Private)
							{
								if (filter.RequiresStringComparison())
								{
									Utf8String name = metadataImport.GetName(num2);
									if (!filter.Match(name))
									{
										goto IL_D9;
									}
								}
								bool isPublic = fieldAttributes2 == FieldAttributes.Public;
								bool isStatic = (fieldAttributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
								BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, flag, isStatic);
								RuntimeFieldInfo item = new MdFieldInfo(num2, fieldAttributes, declaringType.GetTypeHandleInternal(), this.m_runtimeTypeCache, bindingFlags);
								list.Add(item);
							}
						}
						IL_D9:;
					}
				}
				private static void AddElementTypes(Type template, IList<Type> types)
				{
					if (!template.HasElementType)
					{
						return;
					}
					RuntimeType.RuntimeTypeCache.MemberInfoCache<T>.AddElementTypes(template.GetElementType(), types);
					for (int i = 0; i < types.Count; i++)
					{
						if (template.IsArray)
						{
							if (template.IsSzArray)
							{
								types[i] = types[i].MakeArrayType();
							}
							else
							{
								types[i] = types[i].MakeArrayType(template.GetArrayRank());
							}
						}
						else
						{
							if (template.IsPointer)
							{
								types[i] = types[i].MakePointerType();
							}
						}
					}
				}
				[SecuritySafeCritical]
				private List<RuntimeType> PopulateInterfaces(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					List<RuntimeType> list = new List<RuntimeType>();
					RuntimeType reflectedType = this.ReflectedType;
					if (!RuntimeTypeHandle.IsGenericVariable(reflectedType))
					{
						Type[] interfaces = RuntimeTypeHandle.GetInterfaces(reflectedType);
						if (interfaces != null)
						{
							for (int i = 0; i < interfaces.Length; i++)
							{
								RuntimeType runtimeType = (RuntimeType)interfaces[i];
								if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(runtimeType)))
								{
									list.Add(runtimeType);
								}
							}
						}
						if (this.ReflectedType.IsSzArray)
						{
							RuntimeType runtimeType2 = (RuntimeType)this.ReflectedType.GetElementType();
							if (!runtimeType2.IsPointer)
							{
								RuntimeType runtimeType3 = (RuntimeType)typeof(IList<>).MakeGenericType(new Type[]
								{
									runtimeType2
								});
								if (runtimeType3.IsAssignableFrom(this.ReflectedType))
								{
									if (filter.Match(RuntimeTypeHandle.GetUtf8Name(runtimeType3)))
									{
										list.Add(runtimeType3);
									}
									Type[] interfaces2 = runtimeType3.GetInterfaces();
									for (int j = 0; j < interfaces2.Length; j++)
									{
										RuntimeType runtimeType4 = (RuntimeType)interfaces2[j];
										if (runtimeType4.IsGenericType && filter.Match(RuntimeTypeHandle.GetUtf8Name(runtimeType4)))
										{
											list.Add(runtimeType4);
										}
									}
								}
							}
						}
					}
					else
					{
						List<RuntimeType> list2 = new List<RuntimeType>();
						Type[] genericParameterConstraints = reflectedType.GetGenericParameterConstraints();
						for (int k = 0; k < genericParameterConstraints.Length; k++)
						{
							RuntimeType runtimeType5 = (RuntimeType)genericParameterConstraints[k];
							if (runtimeType5.IsInterface)
							{
								list2.Add(runtimeType5);
							}
							Type[] interfaces3 = runtimeType5.GetInterfaces();
							for (int l = 0; l < interfaces3.Length; l++)
							{
								list2.Add(interfaces3[l] as RuntimeType);
							}
						}
						Dictionary<RuntimeType, RuntimeType> dictionary = new Dictionary<RuntimeType, RuntimeType>();
						for (int m = 0; m < list2.Count; m++)
						{
							RuntimeType runtimeType6 = list2[m];
							if (!dictionary.ContainsKey(runtimeType6))
							{
								dictionary[runtimeType6] = runtimeType6;
							}
						}
						RuntimeType[] array = new RuntimeType[dictionary.Values.Count];
						dictionary.Values.CopyTo(array, 0);
						for (int n = 0; n < array.Length; n++)
						{
							if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(array[n])))
							{
								list.Add(array[n]);
							}
						}
					}
					return list;
				}
				[SecuritySafeCritical]
				private unsafe List<RuntimeType> PopulateNestedClasses(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					List<RuntimeType> list = new List<RuntimeType>();
					RuntimeType runtimeType = this.ReflectedType;
					while (RuntimeTypeHandle.IsGenericVariable(runtimeType))
					{
						runtimeType = runtimeType.GetBaseType();
					}
					int token = RuntimeTypeHandle.GetToken(runtimeType);
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return list;
					}
					RuntimeModule module = RuntimeTypeHandle.GetModule(runtimeType);
					MetadataImport metadataImport = ModuleHandle.GetMetadataImport(module);
					int num = metadataImport.EnumNestedTypesCount(token);
					int* ptr = stackalloc int[(UIntPtr)num];
					metadataImport.EnumNestedTypes(token, ptr, num);
					int i = 0;
					while (i < num)
					{
						RuntimeType runtimeType2 = null;
						try
						{
							runtimeType2 = ModuleHandle.ResolveTypeHandleInternal(module, ptr[(IntPtr)i], null, null);
						}
						catch (TypeLoadException)
						{
							goto IL_9E;
						}
						goto IL_7D;
						IL_9E:
						i++;
						continue;
						IL_7D:
						if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(runtimeType2)))
						{
							list.Add(runtimeType2);
							goto IL_9E;
						}
						goto IL_9E;
					}
					return list;
				}
				[SecuritySafeCritical]
				private List<RuntimeEventInfo> PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					Dictionary<string, RuntimeEventInfo> csEventInfos = new Dictionary<string, RuntimeEventInfo>();
					RuntimeType runtimeType = this.ReflectedType;
					List<RuntimeEventInfo> list = new List<RuntimeEventInfo>();
					if ((RuntimeTypeHandle.GetAttributes(runtimeType) & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
					{
						while (RuntimeTypeHandle.IsGenericVariable(runtimeType))
						{
							runtimeType = runtimeType.GetBaseType();
						}
						while (runtimeType != null)
						{
							this.PopulateEvents(filter, runtimeType, csEventInfos, list);
							runtimeType = RuntimeTypeHandle.GetBaseType(runtimeType);
						}
					}
					else
					{
						this.PopulateEvents(filter, runtimeType, csEventInfos, list);
					}
					return list;
				}
				[SecuritySafeCritical]
				private unsafe void PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, Dictionary<string, RuntimeEventInfo> csEventInfos, List<RuntimeEventInfo> list)
				{
					int token = RuntimeTypeHandle.GetToken(declaringType);
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return;
					}
					MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
					int num = metadataImport.EnumEventsCount(token);
					int* ptr = stackalloc int[(UIntPtr)num];
					metadataImport.EnumEvents(token, ptr, num);
					this.PopulateEvents(filter, declaringType, metadataImport, ptr, num, csEventInfos, list);
				}
				[SecurityCritical]
				private unsafe void PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, MetadataImport scope, int* tkEvents, int cAssociates, Dictionary<string, RuntimeEventInfo> csEventInfos, List<RuntimeEventInfo> list)
				{
					int i = 0;
					while (i < cAssociates)
					{
						int num = tkEvents[(IntPtr)i];
						if (!filter.RequiresStringComparison())
						{
							goto IL_29;
						}
						Utf8String name = scope.GetName(num);
						if (filter.Match(name))
						{
							goto IL_29;
						}
						IL_7F:
						i++;
						continue;
						IL_29:
						bool flag;
						RuntimeEventInfo runtimeEventInfo = new RuntimeEventInfo(num, declaringType, this.m_runtimeTypeCache, ref flag);
						if ((!(declaringType != this.m_runtimeTypeCache.GetRuntimeType()) || !flag) && !(csEventInfos.GetValueOrDefault(runtimeEventInfo.Name) != null))
						{
							csEventInfos[runtimeEventInfo.Name] = runtimeEventInfo;
							list.Add(runtimeEventInfo);
							goto IL_7F;
						}
						goto IL_7F;
					}
				}
				[SecuritySafeCritical]
				private List<RuntimePropertyInfo> PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter)
				{
					RuntimeType runtimeType = this.ReflectedType;
					List<RuntimePropertyInfo> list = new List<RuntimePropertyInfo>();
					if ((RuntimeTypeHandle.GetAttributes(runtimeType) & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
					{
						while (RuntimeTypeHandle.IsGenericVariable(runtimeType))
						{
							runtimeType = runtimeType.GetBaseType();
						}
						Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos = new Dictionary<string, List<RuntimePropertyInfo>>();
						bool[] usedSlots = new bool[RuntimeTypeHandle.GetNumVirtuals(runtimeType)];
						do
						{
							this.PopulateProperties(filter, runtimeType, csPropertyInfos, usedSlots, list);
							runtimeType = RuntimeTypeHandle.GetBaseType(runtimeType);
						}
						while (runtimeType != null);
					}
					else
					{
						this.PopulateProperties(filter, runtimeType, null, null, list);
					}
					return list;
				}
				[SecuritySafeCritical]
				private unsafe void PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos, bool[] usedSlots, List<RuntimePropertyInfo> list)
				{
					int token = RuntimeTypeHandle.GetToken(declaringType);
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return;
					}
					MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
					int num = metadataImport.EnumPropertiesCount(token);
					int* ptr = stackalloc int[(UIntPtr)num];
					metadataImport.EnumProperties(token, ptr, num);
					this.PopulateProperties(filter, declaringType, ptr, num, csPropertyInfos, usedSlots, list);
				}
				[SecurityCritical]
				private unsafe void PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, int* tkProperties, int cProperties, Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos, bool[] usedSlots, List<RuntimePropertyInfo> list)
				{
					RuntimeModule module = RuntimeTypeHandle.GetModule(declaringType);
					int numVirtuals = RuntimeTypeHandle.GetNumVirtuals(declaringType);
					int i = 0;
					while (i < cProperties)
					{
						int num = tkProperties[(IntPtr)i];
						if (!filter.RequiresStringComparison())
						{
							goto IL_5E;
						}
						if (ModuleHandle.ContainsPropertyMatchingHash(module, num, filter.GetHashToMatch()))
						{
							Utf8String name = declaringType.GetRuntimeModule().MetadataImport.GetName(num);
							if (filter.Match(name))
							{
								goto IL_5E;
							}
						}
						IL_167:
						i++;
						continue;
						IL_5E:
						bool flag;
						RuntimePropertyInfo runtimePropertyInfo = new RuntimePropertyInfo(num, declaringType, this.m_runtimeTypeCache, ref flag);
						if (usedSlots != null)
						{
							if (declaringType != this.ReflectedType && flag)
							{
								goto IL_167;
							}
							RuntimeMethodInfo runtimeMethodInfo = (RuntimeMethodInfo)runtimePropertyInfo.GetGetMethod();
							if (runtimeMethodInfo != null)
							{
								int slot = RuntimeMethodHandle.GetSlot(runtimeMethodInfo);
								if (slot < numVirtuals)
								{
									if (usedSlots[slot])
									{
										goto IL_167;
									}
									usedSlots[slot] = true;
								}
							}
							else
							{
								RuntimeMethodInfo runtimeMethodInfo2 = (RuntimeMethodInfo)runtimePropertyInfo.GetSetMethod();
								if (runtimeMethodInfo2 != null)
								{
									int slot2 = RuntimeMethodHandle.GetSlot(runtimeMethodInfo2);
									if (slot2 < numVirtuals)
									{
										if (usedSlots[slot2])
										{
											goto IL_167;
										}
										usedSlots[slot2] = true;
									}
								}
							}
							List<RuntimePropertyInfo> list2 = csPropertyInfos.GetValueOrDefault(runtimePropertyInfo.Name);
							if (list2 == null)
							{
								list2 = new List<RuntimePropertyInfo>(1);
								csPropertyInfos[runtimePropertyInfo.Name] = list2;
							}
							else
							{
								for (int j = 0; j < list2.Count; j++)
								{
									if (runtimePropertyInfo.EqualsSig(list2[j]))
									{
										list2 = null;
										break;
									}
								}
							}
							if (list2 == null)
							{
								goto IL_167;
							}
							list2.Add(runtimePropertyInfo);
						}
						list.Add(runtimePropertyInfo);
						goto IL_167;
					}
				}
				internal CerArrayList<T> GetMemberList(MemberListType listType, string name, RuntimeType.RuntimeTypeCache.CacheType cacheType)
				{
					switch (listType)
					{
						case MemberListType.All:
						{
							if (this.m_cacheComplete)
							{
								return this.m_root;
							}
							return this.Populate(null, listType, cacheType);
						}
						case MemberListType.CaseSensitive:
						{
							if (this.m_csMemberInfos == null)
							{
								return this.Populate(name, listType, cacheType);
							}
							CerArrayList<T> cerArrayList = this.m_csMemberInfos[name];
							if (cerArrayList == null)
							{
								return this.Populate(name, listType, cacheType);
							}
							return cerArrayList;
						}
						default:
						{
							if (this.m_cisMemberInfos == null)
							{
								return this.Populate(name, listType, cacheType);
							}
							CerArrayList<T> cerArrayList = this.m_cisMemberInfos[name];
							if (cerArrayList == null)
							{
								return this.Populate(name, listType, cacheType);
							}
							return cerArrayList;
						}
					}
				}
			}
			private const int MAXNAMELEN = 1024;
			private RuntimeType.RuntimeTypeCache.WhatsCached m_whatsCached;
			private RuntimeType m_runtimeType;
			private RuntimeType m_enclosingType;
			private TypeCode m_typeCode;
			private string m_name;
			private string m_fullname;
			private string m_toString;
			private string m_namespace;
			private bool m_isGlobal;
			private bool m_bIsDomainInitialized;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeMethodInfo> m_methodInfoCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeConstructorInfo> m_constructorInfoCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeFieldInfo> m_fieldInfoCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeType> m_interfaceCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeType> m_nestedClassesCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimePropertyInfo> m_propertyInfoCache;
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeEventInfo> m_eventInfoCache;
			private static CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> s_methodInstantiations;
			private static bool s_dontrunhack = false;
			internal bool DomainInitialized
			{
				get
				{
					return this.m_bIsDomainInitialized;
				}
				set
				{
					this.m_bIsDomainInitialized = value;
				}
			}
			internal TypeCode TypeCode
			{
				get
				{
					return this.m_typeCode;
				}
				set
				{
					this.m_typeCode = value;
				}
			}
			internal bool IsGlobal
			{
				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
				get
				{
					return this.m_isGlobal;
				}
			}
			internal RuntimeType RuntimeType
			{
				get
				{
					return this.m_runtimeType;
				}
			}
			[SecuritySafeCritical]
			internal static void Prejitinit_HACK()
			{
				if (!RuntimeType.RuntimeTypeCache.s_dontrunhack)
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeMethodInfo> memberInfoCache = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeMethodInfo>(null);
						CerArrayList<RuntimeMethodInfo> cerArrayList = null;
						memberInfoCache.Insert(ref cerArrayList, "dummy", MemberListType.All);
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeConstructorInfo> memberInfoCache2 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeConstructorInfo>(null);
						CerArrayList<RuntimeConstructorInfo> cerArrayList2 = null;
						memberInfoCache2.Insert(ref cerArrayList2, "dummy", MemberListType.All);
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeFieldInfo> memberInfoCache3 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeFieldInfo>(null);
						CerArrayList<RuntimeFieldInfo> cerArrayList3 = null;
						memberInfoCache3.Insert(ref cerArrayList3, "dummy", MemberListType.All);
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeType> memberInfoCache4 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeType>(null);
						CerArrayList<RuntimeType> cerArrayList4 = null;
						memberInfoCache4.Insert(ref cerArrayList4, "dummy", MemberListType.All);
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimePropertyInfo> memberInfoCache5 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimePropertyInfo>(null);
						CerArrayList<RuntimePropertyInfo> cerArrayList5 = null;
						memberInfoCache5.Insert(ref cerArrayList5, "dummy", MemberListType.All);
						RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeEventInfo> memberInfoCache6 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<RuntimeEventInfo>(null);
						CerArrayList<RuntimeEventInfo> cerArrayList6 = null;
						memberInfoCache6.Insert(ref cerArrayList6, "dummy", MemberListType.All);
					}
				}
			}
			internal RuntimeTypeCache(RuntimeType runtimeType)
			{
				this.m_typeCode = TypeCode.Empty;
				this.m_runtimeType = runtimeType;
				this.m_isGlobal = (RuntimeTypeHandle.GetModule(runtimeType).RuntimeType == runtimeType);
				RuntimeType.RuntimeTypeCache.s_dontrunhack = true;
				RuntimeType.RuntimeTypeCache.Prejitinit_HACK();
			}
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			private string ConstructName(ref string name, bool nameSpace, bool fullinst, bool assembly)
			{
				if (name == null)
				{
					name = new RuntimeTypeHandle(this.m_runtimeType).ConstructName(nameSpace, fullinst, assembly);
				}
				return name;
			}
			private CerArrayList<T> GetMemberList<T>(ref RuntimeType.RuntimeTypeCache.MemberInfoCache<T> m_cache, MemberListType listType, string name, RuntimeType.RuntimeTypeCache.CacheType cacheType) where T : MemberInfo
			{
				RuntimeType.RuntimeTypeCache.MemberInfoCache<T> memberCache = this.GetMemberCache<T>(ref m_cache);
				return memberCache.GetMemberList(listType, name, cacheType);
			}
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			private RuntimeType.RuntimeTypeCache.MemberInfoCache<T> GetMemberCache<T>(ref RuntimeType.RuntimeTypeCache.MemberInfoCache<T> m_cache) where T : MemberInfo
			{
				RuntimeType.RuntimeTypeCache.MemberInfoCache<T> memberInfoCache = m_cache;
				if (memberInfoCache == null)
				{
					RuntimeType.RuntimeTypeCache.MemberInfoCache<T> memberInfoCache2 = new RuntimeType.RuntimeTypeCache.MemberInfoCache<T>(this);
					memberInfoCache = Interlocked.CompareExchange<RuntimeType.RuntimeTypeCache.MemberInfoCache<T>>(ref m_cache, memberInfoCache2, null);
					if (memberInfoCache == null)
					{
						memberInfoCache = memberInfoCache2;
					}
				}
				return memberInfoCache;
			}
			internal string GetName()
			{
				return this.ConstructName(ref this.m_name, false, false, false);
			}
			[SecurityCritical]
			internal string GetNameSpace()
			{
				if (this.m_namespace == null)
				{
					Type type = this.m_runtimeType;
					type = type.GetRootElementType();
					while (type.IsNested)
					{
						type = type.DeclaringType;
					}
					this.m_namespace = RuntimeTypeHandle.GetMetadataImport((RuntimeType)type).GetNamespace(type.MetadataToken).ToString();
				}
				return this.m_namespace;
			}
			internal string GetToString()
			{
				return this.ConstructName(ref this.m_toString, true, false, false);
			}
			internal string GetFullName()
			{
				if (!this.m_runtimeType.IsGenericTypeDefinition && this.m_runtimeType.ContainsGenericParameters)
				{
					return null;
				}
				return this.ConstructName(ref this.m_fullname, true, true, false);
			}
			[SecuritySafeCritical]
			internal RuntimeType GetEnclosingType()
			{
				if ((this.m_whatsCached & RuntimeType.RuntimeTypeCache.WhatsCached.EnclosingType) == RuntimeType.RuntimeTypeCache.WhatsCached.Nothing)
				{
					this.m_enclosingType = RuntimeTypeHandle.GetDeclaringType(this.GetRuntimeType());
					this.m_whatsCached |= RuntimeType.RuntimeTypeCache.WhatsCached.EnclosingType;
				}
				return this.m_enclosingType;
			}
			internal RuntimeType GetRuntimeType()
			{
				return this.m_runtimeType;
			}
			internal void InvalidateCachedNestedType()
			{
				this.m_nestedClassesCache = null;
			}
			[SecurityCritical]
			internal MethodInfo GetGenericMethodInfo(RuntimeMethodHandleInternal genericMethod)
			{
				if (RuntimeType.RuntimeTypeCache.s_methodInstantiations == null)
				{
					Interlocked.CompareExchange<CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>>(ref RuntimeType.RuntimeTypeCache.s_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null);
				}
				CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> methodInstantiations = RuntimeType.RuntimeTypeCache.s_methodInstantiations;
				LoaderAllocator loaderAllocator = (LoaderAllocator)RuntimeMethodHandle.GetLoaderAllocator(genericMethod);
				if (loaderAllocator != null)
				{
					if (loaderAllocator.m_methodInstantiations == null)
					{
						Interlocked.CompareExchange<CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>>(ref loaderAllocator.m_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null);
					}
					methodInstantiations = loaderAllocator.m_methodInstantiations;
				}
				RuntimeMethodInfo runtimeMethodInfo = new RuntimeMethodInfo(genericMethod, RuntimeMethodHandle.GetDeclaringType(genericMethod), this, RuntimeMethodHandle.GetAttributes(genericMethod), (BindingFlags)(-1), loaderAllocator);
				RuntimeMethodInfo runtimeMethodInfo2 = methodInstantiations[runtimeMethodInfo];
				if (runtimeMethodInfo2 != null)
				{
					return runtimeMethodInfo2;
				}
				bool flag = false;
				bool flag2 = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.Enter(methodInstantiations, ref flag);
					runtimeMethodInfo2 = methodInstantiations[runtimeMethodInfo];
					if (runtimeMethodInfo2 != null)
					{
						return runtimeMethodInfo2;
					}
					methodInstantiations.Preallocate(1);
					flag2 = true;
				}
				finally
				{
					if (flag2)
					{
						methodInstantiations[runtimeMethodInfo] = runtimeMethodInfo;
					}
					if (flag)
					{
						Monitor.Exit(methodInstantiations);
					}
				}
				return runtimeMethodInfo;
			}
			internal CerArrayList<RuntimeMethodInfo> GetMethodList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeMethodInfo>(ref this.m_methodInfoCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Method);
			}
			internal CerArrayList<RuntimeConstructorInfo> GetConstructorList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeConstructorInfo>(ref this.m_constructorInfoCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Constructor);
			}
			internal CerArrayList<RuntimePropertyInfo> GetPropertyList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimePropertyInfo>(ref this.m_propertyInfoCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Property);
			}
			internal CerArrayList<RuntimeEventInfo> GetEventList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeEventInfo>(ref this.m_eventInfoCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Event);
			}
			internal CerArrayList<RuntimeFieldInfo> GetFieldList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeFieldInfo>(ref this.m_fieldInfoCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Field);
			}
			internal CerArrayList<RuntimeType> GetInterfaceList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeType>(ref this.m_interfaceCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.Interface);
			}
			internal CerArrayList<RuntimeType> GetNestedTypeList(MemberListType listType, string name)
			{
				return this.GetMemberList<RuntimeType>(ref this.m_nestedClassesCache, listType, name, RuntimeType.RuntimeTypeCache.CacheType.NestedType);
			}
			internal MethodBase GetMethod(RuntimeType declaringType, RuntimeMethodHandleInternal method)
			{
				this.GetMemberCache<RuntimeMethodInfo>(ref this.m_methodInfoCache);
				return this.m_methodInfoCache.AddMethod(declaringType, method, RuntimeType.RuntimeTypeCache.CacheType.Method);
			}
			internal MethodBase GetConstructor(RuntimeType declaringType, RuntimeMethodHandleInternal constructor)
			{
				this.GetMemberCache<RuntimeConstructorInfo>(ref this.m_constructorInfoCache);
				return this.m_constructorInfoCache.AddMethod(declaringType, constructor, RuntimeType.RuntimeTypeCache.CacheType.Constructor);
			}
			internal FieldInfo GetField(RuntimeFieldHandleInternal field)
			{
				this.GetMemberCache<RuntimeFieldInfo>(ref this.m_fieldInfoCache);
				return this.m_fieldInfoCache.AddField(field);
			}
		}
		private class TypeCacheQueue
		{
			private const int QUEUE_SIZE = 4;
			private object[] liveCache;
			internal TypeCacheQueue()
			{
				this.liveCache = new object[4];
			}
		}
		private class ActivatorCacheEntry
		{
			internal Type m_type;
			internal CtorDelegate m_ctor;
			internal RuntimeMethodHandleInternal m_hCtorMethodHandle;
			internal MethodAttributes m_ctorAttributes;
			internal bool m_bNeedSecurityCheck;
			internal bool m_bFullyInitialized;
			[SecurityCritical]
			internal ActivatorCacheEntry(Type t, RuntimeMethodHandleInternal rmh, bool bNeedSecurityCheck)
			{
				this.m_type = t;
				this.m_bNeedSecurityCheck = bNeedSecurityCheck;
				this.m_hCtorMethodHandle = rmh;
				if (!this.m_hCtorMethodHandle.IsNullHandle())
				{
					this.m_ctorAttributes = RuntimeMethodHandle.GetAttributes(this.m_hCtorMethodHandle);
				}
			}
		}
		private class ActivatorCache
		{
			private const int CACHE_SIZE = 16;
			private int hash_counter;
			private RuntimeType.ActivatorCacheEntry[] cache = new RuntimeType.ActivatorCacheEntry[16];
			private ConstructorInfo delegateCtorInfo;
			private PermissionSet delegateCreatePermissions;
			[SecuritySafeCritical]
			private void InitializeDelegateCreator()
			{
				PermissionSet permissionSet = new PermissionSet(PermissionState.None);
				permissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
				permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
				Thread.MemoryBarrier();
				this.delegateCreatePermissions = permissionSet;
				ConstructorInfo constructor = typeof(CtorDelegate).GetConstructor(new Type[]
				{
					typeof(object), 
					typeof(IntPtr)
				});
				Thread.MemoryBarrier();
				this.delegateCtorInfo = constructor;
			}
			[SecuritySafeCritical]
			private void InitializeCacheEntry(RuntimeType.ActivatorCacheEntry ace)
			{
				if (!ace.m_type.IsValueType)
				{
					if (this.delegateCtorInfo == null)
					{
						this.InitializeDelegateCreator();
					}
					this.delegateCreatePermissions.Assert();
					CtorDelegate ctor = (CtorDelegate)this.delegateCtorInfo.Invoke(new object[]
					{
						null, 
						RuntimeMethodHandle.GetFunctionPointer(ace.m_hCtorMethodHandle)
					});
					Thread.MemoryBarrier();
					ace.m_ctor = ctor;
				}
				ace.m_bFullyInitialized = true;
			}
			internal RuntimeType.ActivatorCacheEntry GetEntry(Type t)
			{
				int num = this.hash_counter;
				for (int i = 0; i < 16; i++)
				{
					RuntimeType.ActivatorCacheEntry activatorCacheEntry = this.cache[num];
					if (activatorCacheEntry != null && activatorCacheEntry.m_type == t)
					{
						if (!activatorCacheEntry.m_bFullyInitialized)
						{
							this.InitializeCacheEntry(activatorCacheEntry);
						}
						return activatorCacheEntry;
					}
					num = (num + 1 & 15);
				}
				return null;
			}
			internal void SetEntry(RuntimeType.ActivatorCacheEntry ace)
			{
				int num = this.hash_counter - 1 & 15;
				this.hash_counter = num;
				this.cache[num] = ace;
			}
		}
		[Flags]
		private enum DispatchWrapperType
		{
			Unknown = 1,
			Dispatch = 2,
			Record = 4,
			Error = 8,
			Currency = 16,
			BStr = 32,
			SafeArray = 65536
		}
		private const BindingFlags MemberBindingMask = (BindingFlags)255;
		private const BindingFlags InvocationMask = BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty;
		private const BindingFlags BinderNonCreateInstance = BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
		private const BindingFlags BinderGetSetProperty = BindingFlags.GetProperty | BindingFlags.SetProperty;
		private const BindingFlags BinderSetInvokeProperty = BindingFlags.InvokeMethod | BindingFlags.SetProperty;
		private const BindingFlags BinderGetSetField = BindingFlags.GetField | BindingFlags.SetField;
		private const BindingFlags BinderSetInvokeField = BindingFlags.InvokeMethod | BindingFlags.SetField;
		private const BindingFlags BinderNonFieldGetSet = (BindingFlags)16773888;
		private const BindingFlags ClassicBindingMask = BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty;
		private InternalCache m_cachedData;
		private object m_keepalive;
		private IntPtr m_cache;
		[ForceTokenStabilization]
		internal IntPtr m_handle;
		private static RuntimeType.TypeCacheQueue s_typeCache = null;
		internal static readonly RuntimeType ValueType = (RuntimeType)typeof(ValueType);
		internal static readonly RuntimeType EnumType = (RuntimeType)typeof(Enum);
		private static readonly RuntimeType ObjectType = (RuntimeType)typeof(object);
		private static readonly RuntimeType StringType = (RuntimeType)typeof(string);
		private static readonly RuntimeType DelegateType = (RuntimeType)typeof(Delegate);
		private static RuntimeType s_typedRef = (RuntimeType)typeof(TypedReference);
		private static RuntimeType.ActivatorCache s_ActivatorCache;
		private static OleAutBinder s_ForwardCallBinder;
		internal InternalCache RemotingCache
		{
			get
			{
				InternalCache internalCache = this.m_cachedData;
				if (internalCache == null)
				{
					internalCache = new InternalCache("MemberInfo");
					InternalCache internalCache2 = Interlocked.CompareExchange<InternalCache>(ref this.m_cachedData, internalCache, null);
					if (internalCache2 != null)
					{
						internalCache = internalCache2;
					}
					GC.ClearCache += new ClearCacheHandler(this.OnCacheClear);
				}
				return internalCache;
			}
		}
		internal bool DomainInitialized
		{
			get
			{
				return this.Cache.DomainInitialized;
			}
			set
			{
				this.Cache.DomainInitialized = value;
			}
		}
		private RuntimeType.RuntimeTypeCache Cache
		{
			[SecuritySafeCritical]
			get
			{
				if (this.m_cache.IsNull())
				{
					IntPtr gCHandle = new RuntimeTypeHandle(this).GetGCHandle(GCHandleType.WeakTrackResurrection);
					if (!Interlocked.CompareExchange(ref this.m_cache, gCHandle, (IntPtr)0).IsNull() && !new RuntimeTypeHandle(this).IsCollectible())
					{
						GCHandle.InternalFree(gCHandle);
					}
				}
				RuntimeType.RuntimeTypeCache runtimeTypeCache = GCHandle.InternalGet(this.m_cache) as RuntimeType.RuntimeTypeCache;
				if (runtimeTypeCache == null)
				{
					runtimeTypeCache = new RuntimeType.RuntimeTypeCache(this);
					RuntimeType.RuntimeTypeCache runtimeTypeCache2 = GCHandle.InternalCompareExchange(this.m_cache, runtimeTypeCache, null, false) as RuntimeType.RuntimeTypeCache;
					if (runtimeTypeCache2 != null)
					{
						runtimeTypeCache = runtimeTypeCache2;
					}
					if (RuntimeType.s_typeCache == null)
					{
						RuntimeType.s_typeCache = new RuntimeType.TypeCacheQueue();
					}
				}
				return runtimeTypeCache;
			}
		}
		public override Module Module
		{
			get
			{
				return this.GetRuntimeModule();
			}
		}
		public override Assembly Assembly
		{
			get
			{
				return this.GetRuntimeAssembly();
			}
		}
		public override RuntimeTypeHandle TypeHandle
		{
			get
			{
				return new RuntimeTypeHandle(this);
			}
		}
		internal override bool IsRuntimeType
		{
			get
			{
				return true;
			}
		}
		public override MethodBase DeclaringMethod
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				IRuntimeMethodInfo declaringMethod = RuntimeTypeHandle.GetDeclaringMethod(this);
				if (declaringMethod == null)
				{
					return null;
				}
				return RuntimeType.GetMethodBase(RuntimeMethodHandle.GetDeclaringType(declaringMethod), declaringMethod);
			}
		}
		public override Type BaseType
		{
			[SecuritySafeCritical]
			get
			{
				return this.GetBaseType();
			}
		}
		public override Type UnderlyingSystemType
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return this;
			}
		}
		public override string FullName
		{
			[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return this.Cache.GetFullName();
			}
		}
		public override string AssemblyQualifiedName
		{
			get
			{
				if (!this.IsGenericTypeDefinition && this.ContainsGenericParameters)
				{
					return null;
				}
				return Assembly.CreateQualifiedName(this.Assembly.FullName, this.FullName);
			}
		}
		public override string Namespace
		{
			[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				string nameSpace = this.Cache.GetNameSpace();
				if (nameSpace == null || nameSpace.Length == 0)
				{
					return null;
				}
				return nameSpace;
			}
		}
		public override Guid GUID
		{
			[SecuritySafeCritical]
			get
			{
				Guid result = default(Guid);
				this.GetGUID(ref result);
				return result;
			}
		}
		public override bool IsEnum
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return this.GetBaseType() == RuntimeType.EnumType;
			}
		}
		public override GenericParameterAttributes GenericParameterAttributes
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				GenericParameterAttributes result;
				RuntimeTypeHandle.GetMetadataImport(this).GetGenericParamProps(this.MetadataToken, out result);
				return result;
			}
		}
		public override bool IsSecurityCritical
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return new RuntimeTypeHandle(this).IsSecurityCritical();
			}
		}
		public override bool IsSecuritySafeCritical
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return new RuntimeTypeHandle(this).IsSecuritySafeCritical();
			}
		}
		public override bool IsSecurityTransparent
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return new RuntimeTypeHandle(this).IsSecurityTransparent();
			}
		}
		internal override bool IsSzArray
		{
			get
			{
				return RuntimeTypeHandle.IsSzArray(this);
			}
		}
		public override bool IsGenericTypeDefinition
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return RuntimeTypeHandle.IsGenericTypeDefinition(this);
			}
		}
		public override bool IsGenericParameter
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return RuntimeTypeHandle.IsGenericVariable(this);
			}
		}
		public override int GenericParameterPosition
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				return new RuntimeTypeHandle(this).GetGenericVariableIndex();
			}
		}
		public override bool IsGenericType
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return RuntimeTypeHandle.IsGenericType(this);
			}
		}
		public override bool ContainsGenericParameters
		{
			[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return this.GetRootElementType().GetTypeHandleInternal().ContainsGenericVariables();
			}
		}
		public override StructLayoutAttribute StructLayoutAttribute
		{
			[SecuritySafeCritical]
			get
			{
				return (StructLayoutAttribute)StructLayoutAttribute.GetCustomAttribute(this);
			}
		}
		public override string Name
		{
			[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return this.Cache.GetName();
			}
		}
		public override MemberTypes MemberType
		{
			get
			{
				if (base.IsPublic || base.IsNotPublic)
				{
					return MemberTypes.TypeInfo;
				}
				return MemberTypes.NestedType;
			}
		}
		public override Type DeclaringType
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
			get
			{
				return this.Cache.GetEnclosingType();
			}
		}
		public override Type ReflectedType
		{
			get
			{
				return this.DeclaringType;
			}
		}
		public override int MetadataToken
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
			get
			{
				return RuntimeTypeHandle.GetToken(this);
			}
		}
		private OleAutBinder ForwardCallBinder
		{
			get
			{
				if (RuntimeType.s_ForwardCallBinder == null)
				{
					RuntimeType.s_ForwardCallBinder = new OleAutBinder();
				}
				return RuntimeType.s_ForwardCallBinder;
			}
		}
		internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
		{
			this.m_cachedData = null;
		}
		internal static RuntimeType GetType(string typeName, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			return RuntimeTypeHandle.GetTypeByName(typeName, throwOnError, ignoreCase, reflectionOnly, ref stackMark, false);
		}
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		internal static extern void PrepareMemberInfoCache(RuntimeTypeHandle rt);
		internal static MethodBase GetMethodBase(RuntimeModule scope, int typeMetadataToken)
		{
			return RuntimeType.GetMethodBase(ModuleHandle.ResolveMethodHandleInternal(scope, typeMetadataToken));
		}
		internal static MethodBase GetMethodBase(IRuntimeMethodInfo methodHandle)
		{
			return RuntimeType.GetMethodBase(null, methodHandle);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		internal static MethodBase GetMethodBase(RuntimeType reflectedType, IRuntimeMethodInfo methodHandle)
		{
			MethodBase methodBase = RuntimeType.GetMethodBase(reflectedType, methodHandle.Value);
			GC.KeepAlive(methodHandle);
			return methodBase;
		}
		[SecurityCritical]
		internal static MethodBase GetMethodBase(RuntimeType reflectedType, RuntimeMethodHandleInternal methodHandle)
		{
			if (!RuntimeMethodHandle.IsDynamicMethod(methodHandle))
			{
				RuntimeType runtimeType = RuntimeMethodHandle.GetDeclaringType(methodHandle);
				RuntimeType[] array = null;
				if (reflectedType == null)
				{
					reflectedType = runtimeType;
				}
				if (reflectedType != runtimeType && !reflectedType.IsSubclassOf(runtimeType))
				{
					if (reflectedType.IsArray)
					{
						MethodBase[] array2 = reflectedType.GetMember(RuntimeMethodHandle.GetName(methodHandle), MemberTypes.Constructor | MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) as MethodBase[];
						bool flag = false;
						for (int i = 0; i < array2.Length; i++)
						{
							IRuntimeMethodInfo runtimeMethodInfo = (IRuntimeMethodInfo)array2[i];
							if (runtimeMethodInfo.Value.Value == methodHandle.Value)
							{
								flag = true;
							}
						}
						if (!flag)
						{
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[]
							{
								reflectedType.ToString(), 
								runtimeType.ToString()
							}));
						}
					}
					else
					{
						if (runtimeType.IsGenericType)
						{
							RuntimeType right = (RuntimeType)runtimeType.GetGenericTypeDefinition();
							RuntimeType runtimeType2 = reflectedType;
							while (runtimeType2 != null)
							{
								RuntimeType runtimeType3 = runtimeType2;
								if (runtimeType3.IsGenericType && !runtimeType2.IsGenericTypeDefinition)
								{
									runtimeType3 = (RuntimeType)runtimeType3.GetGenericTypeDefinition();
								}
								if (runtimeType3 == right)
								{
									break;
								}
								runtimeType2 = runtimeType2.GetBaseType();
							}
							if (runtimeType2 == null)
							{
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[]
								{
									reflectedType.ToString(), 
									runtimeType.ToString()
								}));
							}
							runtimeType = runtimeType2;
							if (!RuntimeMethodHandle.IsGenericMethodDefinition(methodHandle))
							{
								array = RuntimeMethodHandle.GetMethodInstantiationInternal(methodHandle);
							}
							methodHandle = RuntimeMethodHandle.GetMethodFromCanonical(methodHandle, runtimeType);
						}
						else
						{
							if (!runtimeType.IsAssignableFrom(reflectedType))
							{
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[]
								{
									reflectedType.ToString(), 
									runtimeType.ToString()
								}));
							}
						}
					}
				}
				methodHandle = RuntimeMethodHandle.GetStubIfNeeded(methodHandle, runtimeType, array);
				MethodBase result;
				if (RuntimeMethodHandle.IsConstructor(methodHandle))
				{
					result = reflectedType.Cache.GetConstructor(runtimeType, methodHandle);
				}
				else
				{
					if (RuntimeMethodHandle.HasMethodInstantiation(methodHandle) && !RuntimeMethodHandle.IsGenericMethodDefinition(methodHandle))
					{
						result = reflectedType.Cache.GetGenericMethodInfo(methodHandle);
					}
					else
					{
						result = reflectedType.Cache.GetMethod(runtimeType, methodHandle);
					}
				}
				GC.KeepAlive(array);
				return result;
			}
			Resolver resolver = RuntimeMethodHandle.GetResolver(methodHandle);
			if (resolver != null)
			{
				return resolver.GetDynamicMethod();
			}
			return null;
		}
		[SecuritySafeCritical]
		internal static FieldInfo GetFieldInfo(IRuntimeFieldInfo fieldHandle)
		{
			return RuntimeType.GetFieldInfo(RuntimeFieldHandle.GetApproxDeclaringType(fieldHandle), fieldHandle);
		}
		[SecuritySafeCritical]
		internal static FieldInfo GetFieldInfo(RuntimeType reflectedType, IRuntimeFieldInfo field)
		{
			RuntimeFieldHandleInternal value = field.Value;
			if (reflectedType == null)
			{
				reflectedType = RuntimeFieldHandle.GetApproxDeclaringType(value);
			}
			else
			{
				RuntimeType approxDeclaringType = RuntimeFieldHandle.GetApproxDeclaringType(value);
				if (reflectedType != approxDeclaringType && (!RuntimeFieldHandle.AcquiresContextFromThis(value) || !RuntimeTypeHandle.CompareCanonicalHandles(approxDeclaringType, reflectedType)))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveFieldHandle"), new object[]
					{
						reflectedType.ToString(), 
						approxDeclaringType.ToString()
					}));
				}
			}
			FieldInfo field2 = reflectedType.Cache.GetField(value);
			GC.KeepAlive(field);
			return field2;
		}
		private static PropertyInfo GetPropertyInfo(RuntimeType reflectedType, int tkProperty)
		{
			CerArrayList<RuntimePropertyInfo> propertyList = reflectedType.Cache.GetPropertyList(MemberListType.All, null);
			for (int i = 0; i < propertyList.Count; i++)
			{
				RuntimePropertyInfo runtimePropertyInfo = propertyList[i];
				if (runtimePropertyInfo.MetadataToken == tkProperty)
				{
					return runtimePropertyInfo;
				}
			}
			throw new SystemException();
		}
		private static void ThrowIfTypeNeverValidGenericArgument(RuntimeType type)
		{
			if (type.IsPointer || type.IsByRef || type == typeof(void))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeverValidGenericArgument", new object[]
				{
					type.ToString()
				}));
			}
		}
		internal static void SanityCheckGenericArguments(RuntimeType[] genericArguments, RuntimeType[] genericParamters)
		{
			if (genericArguments == null)
			{
				throw new ArgumentNullException();
			}
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] == null)
				{
					throw new ArgumentNullException();
				}
				RuntimeType.ThrowIfTypeNeverValidGenericArgument(genericArguments[i]);
			}
			if (genericArguments.Length != genericParamters.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughGenArguments", new object[]
				{
					genericArguments.Length, 
					genericParamters.Length
				}));
			}
		}
		[SecuritySafeCritical]
		internal static void ValidateGenericArguments(MemberInfo definition, RuntimeType[] genericArguments, Exception e)
		{
			RuntimeType[] typeContext = null;
			RuntimeType[] methodContext = null;
			RuntimeType[] array = null;
			if (definition is Type)
			{
				RuntimeType runtimeType = (RuntimeType)definition;
				array = runtimeType.GetGenericArgumentsInternal();
				typeContext = genericArguments;
			}
			else
			{
				RuntimeMethodInfo runtimeMethodInfo = (RuntimeMethodInfo)definition;
				array = runtimeMethodInfo.GetGenericArgumentsInternal();
				methodContext = genericArguments;
				RuntimeType runtimeType2 = (RuntimeType)runtimeMethodInfo.DeclaringType;
				if (runtimeType2 != null)
				{
					typeContext = runtimeType2.GetTypeHandleInternal().GetInstantiationInternal();
				}
			}
			for (int i = 0; i < genericArguments.Length; i++)
			{
				Type type = genericArguments[i];
				Type type2 = array[i];
				if (!RuntimeTypeHandle.SatisfiesConstraints(type2.GetTypeHandleInternal().GetTypeChecked(), typeContext, methodContext, type.GetTypeHandleInternal().GetTypeChecked()))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_GenConstraintViolation", new object[]
					{
						i.ToString(CultureInfo.CurrentCulture), 
						type.ToString(), 
						definition.ToString(), 
						type2.ToString()
					}), e);
				}
			}
		}
		private static void SplitName(string fullname, out string name, out string ns)
		{
			name = null;
			ns = null;
			if (fullname == null)
			{
				return;
			}
			int num = fullname.LastIndexOf(".", StringComparison.Ordinal);
			if (num == -1)
			{
				name = fullname;
				return;
			}
			ns = fullname.Substring(0, num);
			int num2 = fullname.Length - ns.Length - 1;
			if (num2 != 0)
			{
				name = fullname.Substring(num + 1, num2);
				return;
			}
			name = "";
		}
		internal static BindingFlags FilterPreCalculate(bool isPublic, bool isInherited, bool isStatic)
		{
			BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			if (isInherited)
			{
				bindingFlags |= BindingFlags.DeclaredOnly;
				if (isStatic)
				{
					bindingFlags |= (BindingFlags.Static | BindingFlags.FlattenHierarchy);
				}
				else
				{
					bindingFlags |= BindingFlags.Instance;
				}
			}
			else
			{
				if (isStatic)
				{
					bindingFlags |= BindingFlags.Static;
				}
				else
				{
					bindingFlags |= BindingFlags.Instance;
				}
			}
			return bindingFlags;
		}
		private static void FilterHelper(BindingFlags bindingFlags, ref string name, bool allowPrefixLookup, out bool prefixLookup, out bool ignoreCase, out MemberListType listType)
		{
			prefixLookup = false;
			ignoreCase = false;
			if (name != null)
			{
				if ((bindingFlags & BindingFlags.IgnoreCase) != BindingFlags.Default)
				{
					name = name.ToLower(CultureInfo.InvariantCulture);
					ignoreCase = true;
					listType = MemberListType.CaseInsensitive;
				}
				else
				{
					listType = MemberListType.CaseSensitive;
				}
				if (allowPrefixLookup && name.EndsWith("*", StringComparison.Ordinal))
				{
					name = name.Substring(0, name.Length - 1);
					prefixLookup = true;
					listType = MemberListType.All;
					return;
				}
			}
			else
			{
				listType = MemberListType.All;
			}
		}
		private static void FilterHelper(BindingFlags bindingFlags, ref string name, out bool ignoreCase, out MemberListType listType)
		{
			bool flag;
			RuntimeType.FilterHelper(bindingFlags, ref name, false, out flag, out ignoreCase, out listType);
		}
		private static bool FilterApplyPrefixLookup(MemberInfo memberInfo, string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				if (!memberInfo.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(name, StringComparison.Ordinal))
				{
					return false;
				}
			}
			else
			{
				if (!memberInfo.Name.StartsWith(name, StringComparison.Ordinal))
				{
					return false;
				}
			}
			return true;
		}
		private static bool FilterApplyBase(MemberInfo memberInfo, BindingFlags bindingFlags, bool isPublic, bool isNonProtectedInternal, bool isStatic, string name, bool prefixLookup)
		{
			if (isPublic)
			{
				if ((bindingFlags & BindingFlags.Public) == BindingFlags.Default)
				{
					return false;
				}
			}
			else
			{
				if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.Default)
				{
					return false;
				}
			}
			bool flag = !object.ReferenceEquals(memberInfo.DeclaringType, memberInfo.ReflectedType);
			if ((bindingFlags & BindingFlags.DeclaredOnly) != BindingFlags.Default && flag)
			{
				return false;
			}
			if (memberInfo.MemberType != MemberTypes.TypeInfo && memberInfo.MemberType != MemberTypes.NestedType)
			{
				if (isStatic)
				{
					if ((bindingFlags & BindingFlags.FlattenHierarchy) == BindingFlags.Default && flag)
					{
						return false;
					}
					if ((bindingFlags & BindingFlags.Static) == BindingFlags.Default)
					{
						return false;
					}
				}
				else
				{
					if ((bindingFlags & BindingFlags.Instance) == BindingFlags.Default)
					{
						return false;
					}
				}
			}
			if (prefixLookup && !RuntimeType.FilterApplyPrefixLookup(memberInfo, name, (bindingFlags & BindingFlags.IgnoreCase) != BindingFlags.Default))
			{
				return false;
			}
			if ((bindingFlags & BindingFlags.DeclaredOnly) == BindingFlags.Default && flag && isNonProtectedInternal && (bindingFlags & BindingFlags.NonPublic) != BindingFlags.Default && !isStatic && (bindingFlags & BindingFlags.Instance) != BindingFlags.Default)
			{
				MethodInfo methodInfo = memberInfo as MethodInfo;
				if (methodInfo == null)
				{
					return false;
				}
				if (!methodInfo.IsVirtual && !methodInfo.IsAbstract)
				{
					return false;
				}
			}
			return true;
		}
		private static bool FilterApplyType(Type type, BindingFlags bindingFlags, string name, bool prefixLookup, string ns)
		{
			bool isPublic = type.IsNestedPublic || type.IsPublic;
			bool isStatic = false;
			return RuntimeType.FilterApplyBase(type, bindingFlags, isPublic, type.IsNestedAssembly, isStatic, name, prefixLookup) && (ns == null || type.Namespace.Equals(ns));
		}
		private static bool FilterApplyMethodInfo(RuntimeMethodInfo method, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
		{
			return RuntimeType.FilterApplyMethodBase(method, method.BindingFlags, bindingFlags, callConv, argumentTypes);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		private static bool FilterApplyConstructorInfo(RuntimeConstructorInfo constructor, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
		{
			return RuntimeType.FilterApplyMethodBase(constructor, constructor.BindingFlags, bindingFlags, callConv, argumentTypes);
		}
		private static bool FilterApplyMethodBase(MethodBase methodBase, BindingFlags methodFlags, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
		{
			bindingFlags ^= BindingFlags.DeclaredOnly;
			if ((bindingFlags & methodFlags) != methodFlags)
			{
				return false;
			}
			if ((callConv & CallingConventions.Any) == (CallingConventions)0)
			{
				if ((callConv & CallingConventions.VarArgs) != (CallingConventions)0 && (methodBase.CallingConvention & CallingConventions.VarArgs) == (CallingConventions)0)
				{
					return false;
				}
				if ((callConv & CallingConventions.Standard) != (CallingConventions)0 && (methodBase.CallingConvention & CallingConventions.Standard) == (CallingConventions)0)
				{
					return false;
				}
			}
			if (argumentTypes != null)
			{
				ParameterInfo[] parametersNoCopy = methodBase.GetParametersNoCopy();
				if (argumentTypes.Length != parametersNoCopy.Length)
				{
					if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty)) == BindingFlags.Default)
					{
						return false;
					}
					bool flag = false;
					bool flag2 = argumentTypes.Length > parametersNoCopy.Length;
					if (flag2)
					{
						if ((methodBase.CallingConvention & CallingConventions.VarArgs) == (CallingConventions)0)
						{
							flag = true;
						}
					}
					else
					{
						if ((bindingFlags & BindingFlags.OptionalParamBinding) == BindingFlags.Default)
						{
							flag = true;
						}
						else
						{
							if (!parametersNoCopy[argumentTypes.Length].IsOptional)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						if (parametersNoCopy.Length == 0)
						{
							return false;
						}
						bool flag3 = argumentTypes.Length < parametersNoCopy.Length - 1;
						if (flag3)
						{
							return false;
						}
						ParameterInfo parameterInfo = parametersNoCopy[parametersNoCopy.Length - 1];
						if (!parameterInfo.ParameterType.IsArray)
						{
							return false;
						}
						if (!parameterInfo.IsDefined(typeof(ParamArrayAttribute), false))
						{
							return false;
						}
					}
				}
				else
				{
					if ((bindingFlags & BindingFlags.ExactBinding) != BindingFlags.Default && (bindingFlags & BindingFlags.InvokeMethod) == BindingFlags.Default)
					{
						for (int i = 0; i < parametersNoCopy.Length; i++)
						{
							if (argumentTypes[i] != null && !object.ReferenceEquals(parametersNoCopy[i].ParameterType, argumentTypes[i]))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}
		internal RuntimeType()
		{
			throw new NotSupportedException();
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			RuntimeType runtimeType = o as RuntimeType;
			return !(runtimeType == null) && runtimeType.m_handle.Equals(this.m_handle);
		}
		internal bool IsSpecialSerializableType()
		{
			RuntimeType runtimeType = this;
			while (!(runtimeType == RuntimeType.DelegateType) && !(runtimeType == RuntimeType.EnumType))
			{
				runtimeType = runtimeType.GetBaseType();
				if (!(runtimeType != null))
				{
					return false;
				}
			}
			return true;
		}
		private MethodInfo[] GetMethodCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
		{
			bool flag;
			bool ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out ignoreCase, out listType);
			CerArrayList<RuntimeMethodInfo> methodList = this.Cache.GetMethodList(listType, name);
			List<MethodInfo> list = new List<MethodInfo>(methodList.Count);
			for (int i = 0; i < methodList.Count; i++)
			{
				RuntimeMethodInfo runtimeMethodInfo = methodList[i];
				if (RuntimeType.FilterApplyMethodInfo(runtimeMethodInfo, bindingAttr, callConv, types) && (!flag || RuntimeType.FilterApplyPrefixLookup(runtimeMethodInfo, name, ignoreCase)))
				{
					list.Add(runtimeMethodInfo);
				}
			}
			return list.ToArray();
		}
		private ConstructorInfo[] GetConstructorCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
		{
			bool flag;
			bool ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out ignoreCase, out listType);
			CerArrayList<RuntimeConstructorInfo> constructorList = this.Cache.GetConstructorList(listType, name);
			List<ConstructorInfo> list = new List<ConstructorInfo>(constructorList.Count);
			for (int i = 0; i < constructorList.Count; i++)
			{
				RuntimeConstructorInfo runtimeConstructorInfo = constructorList[i];
				if (RuntimeType.FilterApplyConstructorInfo(runtimeConstructorInfo, bindingAttr, callConv, types) && (!flag || RuntimeType.FilterApplyPrefixLookup(runtimeConstructorInfo, name, ignoreCase)))
				{
					list.Add(runtimeConstructorInfo);
				}
			}
			return list.ToArray();
		}
		private PropertyInfo[] GetPropertyCandidates(string name, BindingFlags bindingAttr, Type[] types, bool allowPrefixLookup)
		{
			bool flag;
			bool ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out ignoreCase, out listType);
			CerArrayList<RuntimePropertyInfo> propertyList = this.Cache.GetPropertyList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			List<PropertyInfo> list = new List<PropertyInfo>(propertyList.Count);
			for (int i = 0; i < propertyList.Count; i++)
			{
				RuntimePropertyInfo runtimePropertyInfo = propertyList[i];
				if ((bindingAttr & runtimePropertyInfo.BindingFlags) == runtimePropertyInfo.BindingFlags && (!flag || RuntimeType.FilterApplyPrefixLookup(runtimePropertyInfo, name, ignoreCase)) && (types == null || runtimePropertyInfo.GetIndexParameters().Length == types.Length))
				{
					list.Add(runtimePropertyInfo);
				}
			}
			return list.ToArray();
		}
		private EventInfo[] GetEventCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			bool flag;
			bool ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out ignoreCase, out listType);
			CerArrayList<RuntimeEventInfo> eventList = this.Cache.GetEventList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			List<EventInfo> list = new List<EventInfo>(eventList.Count);
			for (int i = 0; i < eventList.Count; i++)
			{
				RuntimeEventInfo runtimeEventInfo = eventList[i];
				if ((bindingAttr & runtimeEventInfo.BindingFlags) == runtimeEventInfo.BindingFlags && (!flag || RuntimeType.FilterApplyPrefixLookup(runtimeEventInfo, name, ignoreCase)))
				{
					list.Add(runtimeEventInfo);
				}
			}
			return list.ToArray();
		}
		[SecuritySafeCritical]
		private FieldInfo[] GetFieldCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			bool flag;
			bool ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out ignoreCase, out listType);
			CerArrayList<RuntimeFieldInfo> fieldList = this.Cache.GetFieldList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			List<FieldInfo> list = new List<FieldInfo>(fieldList.Count);
			for (int i = 0; i < fieldList.Count; i++)
			{
				RuntimeFieldInfo runtimeFieldInfo = fieldList[i];
				if ((bindingAttr & runtimeFieldInfo.BindingFlags) == runtimeFieldInfo.BindingFlags && (!flag || RuntimeType.FilterApplyPrefixLookup(runtimeFieldInfo, name, ignoreCase)))
				{
					list.Add(runtimeFieldInfo);
				}
			}
			return list.ToArray();
		}
		private Type[] GetNestedTypeCandidates(string fullname, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			bindingAttr &= ~BindingFlags.Static;
			string name;
			string ns;
			RuntimeType.SplitName(fullname, out name, out ns);
			bool prefixLookup;
			bool flag;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out flag, out listType);
			CerArrayList<RuntimeType> nestedTypeList = this.Cache.GetNestedTypeList(listType, name);
			List<Type> list = new List<Type>(nestedTypeList.Count);
			for (int i = 0; i < nestedTypeList.Count; i++)
			{
				RuntimeType runtimeType = nestedTypeList[i];
				if (RuntimeType.FilterApplyType(runtimeType, bindingAttr, name, prefixLookup, ns))
				{
					list.Add(runtimeType);
				}
			}
			return list.ToArray();
		}
		[SecuritySafeCritical]
		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			return this.GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, false);
		}
		[ComVisible(true), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			return this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, false);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			return this.GetPropertyCandidates(null, bindingAttr, null, false);
		}
		[SecuritySafeCritical]
		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			return this.GetEventCandidates(null, bindingAttr, false);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			return this.GetFieldCandidates(null, bindingAttr, false);
		}
		[SecuritySafeCritical]
		public override Type[] GetInterfaces()
		{
			CerArrayList<RuntimeType> interfaceList = this.Cache.GetInterfaceList(MemberListType.All, null);
			Type[] array = new Type[interfaceList.Count];
			for (int i = 0; i < interfaceList.Count; i++)
			{
				JitHelpers.UnsafeSetArrayElement(array, i, interfaceList[i]);
			}
			return array;
		}
		[SecuritySafeCritical]
		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			return this.GetNestedTypeCandidates(null, bindingAttr, false);
		}
		[SecuritySafeCritical]
		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			MethodInfo[] methodCandidates = this.GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, false);
			ConstructorInfo[] constructorCandidates = this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, false);
			PropertyInfo[] propertyCandidates = this.GetPropertyCandidates(null, bindingAttr, null, false);
			EventInfo[] eventCandidates = this.GetEventCandidates(null, bindingAttr, false);
			FieldInfo[] fieldCandidates = this.GetFieldCandidates(null, bindingAttr, false);
			Type[] nestedTypeCandidates = this.GetNestedTypeCandidates(null, bindingAttr, false);
			MemberInfo[] array = new MemberInfo[methodCandidates.Length + constructorCandidates.Length + propertyCandidates.Length + eventCandidates.Length + fieldCandidates.Length + nestedTypeCandidates.Length];
			int num = 0;
			Array.Copy(methodCandidates, 0, array, num, methodCandidates.Length);
			num += methodCandidates.Length;
			Array.Copy(constructorCandidates, 0, array, num, constructorCandidates.Length);
			num += constructorCandidates.Length;
			Array.Copy(propertyCandidates, 0, array, num, propertyCandidates.Length);
			num += propertyCandidates.Length;
			Array.Copy(eventCandidates, 0, array, num, eventCandidates.Length);
			num += eventCandidates.Length;
			Array.Copy(fieldCandidates, 0, array, num, fieldCandidates.Length);
			num += fieldCandidates.Length;
			Array.Copy(nestedTypeCandidates, 0, array, num, nestedTypeCandidates.Length);
			num += nestedTypeCandidates.Length;
			return array;
		}
		[SecuritySafeCritical]
		public override InterfaceMapping GetInterfaceMap(Type ifaceType)
		{
			if (this.IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
			}
			if (ifaceType == null)
			{
				throw new ArgumentNullException("ifaceType");
			}
			RuntimeType runtimeType = ifaceType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "ifaceType");
			}
			RuntimeTypeHandle typeHandleInternal = runtimeType.GetTypeHandleInternal();
			this.GetTypeHandleInternal().VerifyInterfaceIsImplemented(typeHandleInternal);
			if (this.IsSzArray && ifaceType.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ArrayGetInterfaceMap"));
			}
			int interfaceMethodSlots = RuntimeTypeHandle.GetInterfaceMethodSlots(runtimeType);
			int num = 0;
			for (int i = 0; i < interfaceMethodSlots; i++)
			{
				if ((RuntimeMethodHandle.GetAttributes(RuntimeTypeHandle.GetMethodAt(runtimeType, i)) & MethodAttributes.Static) != MethodAttributes.PrivateScope)
				{
					num++;
				}
			}
			int num2 = interfaceMethodSlots - num;
			InterfaceMapping result;
			result.InterfaceType = ifaceType;
			result.TargetType = this;
			result.InterfaceMethods = new MethodInfo[num2];
			result.TargetMethods = new MethodInfo[num2];
			for (int j = 0; j < interfaceMethodSlots; j++)
			{
				RuntimeMethodHandleInternal methodAt = RuntimeTypeHandle.GetMethodAt(runtimeType, j);
				if (num > 0 && (RuntimeMethodHandle.GetAttributes(methodAt) & MethodAttributes.Static) != MethodAttributes.PrivateScope)
				{
					num--;
				}
				else
				{
					MethodBase methodBase = RuntimeType.GetMethodBase(runtimeType, methodAt);
					result.InterfaceMethods[j] = (MethodInfo)methodBase;
					int interfaceMethodImplementationSlot = this.GetTypeHandleInternal().GetInterfaceMethodImplementationSlot(typeHandleInternal, methodAt);
					if (interfaceMethodImplementationSlot != -1)
					{
						RuntimeMethodHandleInternal methodAt2 = RuntimeTypeHandle.GetMethodAt(this, interfaceMethodImplementationSlot);
						MethodBase methodBase2 = RuntimeType.GetMethodBase(this, methodAt2);
						result.TargetMethods[j] = (MethodInfo)methodBase2;
					}
				}
			}
			return result;
		}
		[SecuritySafeCritical]
		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
		{
			MethodInfo[] methodCandidates = this.GetMethodCandidates(name, bindingAttr, callConv, types, false);
			if (methodCandidates.Length == 0)
			{
				return null;
			}
			if (types == null || types.Length == 0)
			{
				if (methodCandidates.Length == 1)
				{
					return methodCandidates[0];
				}
				if (types == null)
				{
					for (int i = 1; i < methodCandidates.Length; i++)
					{
						MethodInfo m = methodCandidates[i];
						if (!System.DefaultBinder.CompareMethodSigAndName(m, methodCandidates[0]))
						{
							throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
						}
					}
					return System.DefaultBinder.FindMostDerivedNewSlotMeth(methodCandidates, methodCandidates.Length) as MethodInfo;
				}
			}
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			return binder.SelectMethod(bindingAttr, methodCandidates, types, modifiers) as MethodInfo;
		}
		[SecuritySafeCritical]
		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			ConstructorInfo[] constructorCandidates = this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, types, false);
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			if (constructorCandidates.Length == 0)
			{
				return null;
			}
			if (types.Length == 0 && constructorCandidates.Length == 1)
			{
				ParameterInfo[] parametersNoCopy = constructorCandidates[0].GetParametersNoCopy();
				if (parametersNoCopy == null || parametersNoCopy.Length == 0)
				{
					return constructorCandidates[0];
				}
			}
			if ((bindingAttr & BindingFlags.ExactBinding) != BindingFlags.Default)
			{
				return System.DefaultBinder.ExactBinding(constructorCandidates, types, modifiers) as ConstructorInfo;
			}
			return binder.SelectMethod(bindingAttr, constructorCandidates, types, modifiers) as ConstructorInfo;
		}
		[SecuritySafeCritical]
		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			PropertyInfo[] propertyCandidates = this.GetPropertyCandidates(name, bindingAttr, types, false);
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			if (propertyCandidates.Length == 0)
			{
				return null;
			}
			if (types == null || types.Length == 0)
			{
				if (propertyCandidates.Length == 1)
				{
					if (returnType != null && !returnType.IsEquivalentTo(propertyCandidates[0].PropertyType))
					{
						return null;
					}
					return propertyCandidates[0];
				}
				else
				{
					if (returnType == null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
					}
				}
			}
			if ((bindingAttr & BindingFlags.ExactBinding) != BindingFlags.Default)
			{
				return System.DefaultBinder.ExactPropertyBinding(propertyCandidates, returnType, types, modifiers);
			}
			return binder.SelectProperty(bindingAttr, propertyCandidates, returnType, types, modifiers);
		}
		[SecuritySafeCritical]
		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			bool flag;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, out flag, out listType);
			CerArrayList<RuntimeEventInfo> eventList = this.Cache.GetEventList(listType, name);
			EventInfo eventInfo = null;
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < eventList.Count; i++)
			{
				RuntimeEventInfo runtimeEventInfo = eventList[i];
				if ((bindingAttr & runtimeEventInfo.BindingFlags) == runtimeEventInfo.BindingFlags)
				{
					if (eventInfo != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
					}
					eventInfo = runtimeEventInfo;
				}
			}
			return eventInfo;
		}
		[SecuritySafeCritical]
		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			bool flag;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, out flag, out listType);
			CerArrayList<RuntimeFieldInfo> fieldList = this.Cache.GetFieldList(listType, name);
			FieldInfo fieldInfo = null;
			bindingAttr ^= BindingFlags.DeclaredOnly;
			bool flag2 = false;
			for (int i = 0; i < fieldList.Count; i++)
			{
				RuntimeFieldInfo runtimeFieldInfo = fieldList[i];
				if ((bindingAttr & runtimeFieldInfo.BindingFlags) == runtimeFieldInfo.BindingFlags)
				{
					if (fieldInfo != null)
					{
						if (object.ReferenceEquals(runtimeFieldInfo.DeclaringType, fieldInfo.DeclaringType))
						{
							throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
						}
						if (fieldInfo.DeclaringType.IsInterface && runtimeFieldInfo.DeclaringType.IsInterface)
						{
							flag2 = true;
						}
					}
					if (fieldInfo == null || runtimeFieldInfo.DeclaringType.IsSubclassOf(fieldInfo.DeclaringType) || fieldInfo.DeclaringType.IsInterface)
					{
						fieldInfo = runtimeFieldInfo;
					}
				}
			}
			if (flag2 && fieldInfo.DeclaringType.IsInterface)
			{
				throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
			}
			return fieldInfo;
		}
		[SecuritySafeCritical]
		public override Type GetInterface(string fullname, bool ignoreCase)
		{
			if (fullname == null)
			{
				throw new ArgumentNullException();
			}
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
			bindingFlags &= ~BindingFlags.Static;
			if (ignoreCase)
			{
				bindingFlags |= BindingFlags.IgnoreCase;
			}
			string name;
			string ns;
			RuntimeType.SplitName(fullname, out name, out ns);
			MemberListType listType;
			RuntimeType.FilterHelper(bindingFlags, ref name, out ignoreCase, out listType);
			CerArrayList<RuntimeType> interfaceList = this.Cache.GetInterfaceList(listType, name);
			RuntimeType runtimeType = null;
			for (int i = 0; i < interfaceList.Count; i++)
			{
				RuntimeType runtimeType2 = interfaceList[i];
				if (RuntimeType.FilterApplyType(runtimeType2, bindingFlags, name, false, ns))
				{
					if (runtimeType != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
					}
					runtimeType = runtimeType2;
				}
			}
			return runtimeType;
		}
		[SecuritySafeCritical]
		public override Type GetNestedType(string fullname, BindingFlags bindingAttr)
		{
			if (fullname == null)
			{
				throw new ArgumentNullException();
			}
			bindingAttr &= ~BindingFlags.Static;
			string name;
			string ns;
			RuntimeType.SplitName(fullname, out name, out ns);
			bool flag;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, out flag, out listType);
			CerArrayList<RuntimeType> nestedTypeList = this.Cache.GetNestedTypeList(listType, name);
			RuntimeType runtimeType = null;
			for (int i = 0; i < nestedTypeList.Count; i++)
			{
				RuntimeType runtimeType2 = nestedTypeList[i];
				if (RuntimeType.FilterApplyType(runtimeType2, bindingAttr, name, false, ns))
				{
					if (runtimeType != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
					}
					runtimeType = runtimeType2;
				}
			}
			return runtimeType;
		}
		[SecuritySafeCritical]
		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			MethodInfo[] array = new MethodInfo[0];
			ConstructorInfo[] array2 = new ConstructorInfo[0];
			PropertyInfo[] array3 = new PropertyInfo[0];
			EventInfo[] array4 = new EventInfo[0];
			FieldInfo[] array5 = new FieldInfo[0];
			Type[] array6 = new Type[0];
			if ((type & MemberTypes.Method) != (MemberTypes)0)
			{
				array = this.GetMethodCandidates(name, bindingAttr, CallingConventions.Any, null, true);
			}
			if ((type & MemberTypes.Constructor) != (MemberTypes)0)
			{
				array2 = this.GetConstructorCandidates(name, bindingAttr, CallingConventions.Any, null, true);
			}
			if ((type & MemberTypes.Property) != (MemberTypes)0)
			{
				array3 = this.GetPropertyCandidates(name, bindingAttr, null, true);
			}
			if ((type & MemberTypes.Event) != (MemberTypes)0)
			{
				array4 = this.GetEventCandidates(name, bindingAttr, true);
			}
			if ((type & MemberTypes.Field) != (MemberTypes)0)
			{
				array5 = this.GetFieldCandidates(name, bindingAttr, true);
			}
			if ((type & (MemberTypes.TypeInfo | MemberTypes.NestedType)) != (MemberTypes)0)
			{
				array6 = this.GetNestedTypeCandidates(name, bindingAttr, true);
			}
			if (type <= MemberTypes.Property)
			{
				switch (type)
				{
					case MemberTypes.Constructor:
					{
						return array2;
					}
					case MemberTypes.Event:
					{
						return array4;
					}
					case MemberTypes.Constructor | MemberTypes.Event:
					case MemberTypes.Constructor | MemberTypes.Field:
					case MemberTypes.Event | MemberTypes.Field:
					case MemberTypes.Constructor | MemberTypes.Event | MemberTypes.Field:
					{
						break;
					}
					case MemberTypes.Field:
					{
						return array5;
					}
					case MemberTypes.Method:
					{
						return array;
					}
					case MemberTypes.Constructor | MemberTypes.Method:
					{
						MethodBase[] array7 = new MethodBase[array.Length + array2.Length];
						Array.Copy(array, array7, array.Length);
						Array.Copy(array2, 0, array7, array.Length, array2.Length);
						return array7;
					}
					default:
					{
						if (type == MemberTypes.Property)
						{
							return array3;
						}
						break;
					}
				}
			}
			else
			{
				if (type == MemberTypes.TypeInfo)
				{
					return array6;
				}
				if (type == MemberTypes.NestedType)
				{
					return array6;
				}
			}
			MemberInfo[] array8 = new MemberInfo[array.Length + array2.Length + array3.Length + array4.Length + array5.Length + array6.Length];
			int num = 0;
			if (array.Length > 0)
			{
				Array.Copy(array, 0, array8, num, array.Length);
			}
			num += array.Length;
			if (array2.Length > 0)
			{
				Array.Copy(array2, 0, array8, num, array2.Length);
			}
			num += array2.Length;
			if (array3.Length > 0)
			{
				Array.Copy(array3, 0, array8, num, array3.Length);
			}
			num += array3.Length;
			if (array4.Length > 0)
			{
				Array.Copy(array4, 0, array8, num, array4.Length);
			}
			num += array4.Length;
			if (array5.Length > 0)
			{
				Array.Copy(array5, 0, array8, num, array5.Length);
			}
			num += array5.Length;
			if (array6.Length > 0)
			{
				Array.Copy(array6, 0, array8, num, array6.Length);
			}
			num += array6.Length;
			return array8;
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		internal RuntimeModule GetRuntimeModule()
		{
			return RuntimeTypeHandle.GetModule(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		internal RuntimeAssembly GetRuntimeAssembly()
		{
			return RuntimeTypeHandle.GetAssembly(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal sealed override RuntimeTypeHandle GetTypeHandleInternal()
		{
			return new RuntimeTypeHandle(this);
		}
		[SecuritySafeCritical]
		protected override TypeCode GetTypeCodeImpl()
		{
			TypeCode typeCode = this.Cache.TypeCode;
			if (typeCode != TypeCode.Empty)
			{
				return typeCode;
			}
			switch (RuntimeTypeHandle.GetCorElementType(this))
			{
				case CorElementType.Boolean:
				{
					typeCode = TypeCode.Boolean;
					goto IL_12B;
				}
				case CorElementType.Char:
				{
					typeCode = TypeCode.Char;
					goto IL_12B;
				}
				case CorElementType.I1:
				{
					typeCode = TypeCode.SByte;
					goto IL_12B;
				}
				case CorElementType.U1:
				{
					typeCode = TypeCode.Byte;
					goto IL_12B;
				}
				case CorElementType.I2:
				{
					typeCode = TypeCode.Int16;
					goto IL_12B;
				}
				case CorElementType.U2:
				{
					typeCode = TypeCode.UInt16;
					goto IL_12B;
				}
				case CorElementType.I4:
				{
					typeCode = TypeCode.Int32;
					goto IL_12B;
				}
				case CorElementType.U4:
				{
					typeCode = TypeCode.UInt32;
					goto IL_12B;
				}
				case CorElementType.I8:
				{
					typeCode = TypeCode.Int64;
					goto IL_12B;
				}
				case CorElementType.U8:
				{
					typeCode = TypeCode.UInt64;
					goto IL_12B;
				}
				case CorElementType.R4:
				{
					typeCode = TypeCode.Single;
					goto IL_12B;
				}
				case CorElementType.R8:
				{
					typeCode = TypeCode.Double;
					goto IL_12B;
				}
				case CorElementType.String:
				{
					typeCode = TypeCode.String;
					goto IL_12B;
				}
				case CorElementType.ValueType:
				{
					if (this == Convert.ConvertTypes[15])
					{
						typeCode = TypeCode.Decimal;
						goto IL_12B;
					}
					if (this == Convert.ConvertTypes[16])
					{
						typeCode = TypeCode.DateTime;
						goto IL_12B;
					}
					if (this.IsEnum)
					{
						typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(this));
						goto IL_12B;
					}
					typeCode = TypeCode.Object;
					goto IL_12B;
				}
			}
			if (this == Convert.ConvertTypes[2])
			{
				typeCode = TypeCode.DBNull;
			}
			else
			{
				if (this == Convert.ConvertTypes[18])
				{
					typeCode = TypeCode.String;
				}
				else
				{
					typeCode = TypeCode.Object;
				}
			}
			IL_12B:
			this.Cache.TypeCode = typeCode;
			return typeCode;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public override bool IsInstanceOfType(object o)
		{
			return RuntimeTypeHandle.IsInstanceOfType(this, o);
		}
		[ComVisible(true)]
		public override bool IsSubclassOf(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			RuntimeType runtimeType = type as RuntimeType;
			if (runtimeType == null)
			{
				return false;
			}
			RuntimeType baseType = this.GetBaseType();
			while (baseType != null)
			{
				if (baseType == runtimeType)
				{
					return true;
				}
				baseType = baseType.GetBaseType();
			}
			return runtimeType == RuntimeType.ObjectType && runtimeType != this;
		}
		[SecuritySafeCritical]
		public override bool IsAssignableFrom(Type c)
		{
			if (c == null)
			{
				return false;
			}
			if (object.ReferenceEquals(c, this))
			{
				return true;
			}
			RuntimeType runtimeType = c.UnderlyingSystemType as RuntimeType;
			if (runtimeType != null)
			{
				return RuntimeTypeHandle.CanCastTo(runtimeType, this);
			}
			if (c is TypeBuilder)
			{
				if (c.IsSubclassOf(this))
				{
					return true;
				}
				if (base.IsInterface)
				{
					return c.ImplementInterface(this);
				}
				if (this.IsGenericParameter)
				{
					Type[] genericParameterConstraints = this.GetGenericParameterConstraints();
					for (int i = 0; i < genericParameterConstraints.Length; i++)
					{
						if (!genericParameterConstraints[i].IsAssignableFrom(c))
						{
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
		public override bool IsEquivalentTo(Type other)
		{
			RuntimeType runtimeType = other as RuntimeType;
			return runtimeType != null && (runtimeType == this || RuntimeTypeHandle.IsEquivalentTo(this, runtimeType));
		}
		private RuntimeType GetBaseType()
		{
			if (base.IsInterface)
			{
				return null;
			}
			if (RuntimeTypeHandle.IsGenericVariable(this))
			{
				Type[] genericParameterConstraints = this.GetGenericParameterConstraints();
				RuntimeType runtimeType = RuntimeType.ObjectType;
				for (int i = 0; i < genericParameterConstraints.Length; i++)
				{
					RuntimeType runtimeType2 = (RuntimeType)genericParameterConstraints[i];
					if (!runtimeType2.IsInterface)
					{
						if (runtimeType2.IsGenericParameter)
						{
							GenericParameterAttributes genericParameterAttributes = runtimeType2.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
							if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.None && (genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.None)
							{
								goto IL_55;
							}
						}
						runtimeType = runtimeType2;
					}
					IL_55:;
				}
				if (runtimeType == RuntimeType.ObjectType)
				{
					GenericParameterAttributes genericParameterAttributes2 = this.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
					if ((genericParameterAttributes2 & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
					{
						runtimeType = RuntimeType.ValueType;
					}
				}
				return runtimeType;
			}
			return RuntimeTypeHandle.GetBaseType(this);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			return RuntimeTypeHandle.GetAttributes(this);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void GetGUID(ref Guid result);
		[SecuritySafeCritical]
		protected override bool IsContextfulImpl()
		{
			return RuntimeTypeHandle.IsContextful(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override bool IsByRefImpl()
		{
			return RuntimeTypeHandle.IsByRef(this);
		}
		protected override bool IsPrimitiveImpl()
		{
			return RuntimeTypeHandle.IsPrimitive(this);
		}
		protected override bool IsPointerImpl()
		{
			return RuntimeTypeHandle.IsPointer(this);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override bool IsCOMObjectImpl()
		{
			return RuntimeTypeHandle.IsComObject(this, false);
		}
		[SecuritySafeCritical]
		internal override bool HasProxyAttributeImpl()
		{
			return RuntimeTypeHandle.HasProxyAttribute(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override bool IsValueTypeImpl()
		{
			return !(this == typeof(ValueType)) && !(this == typeof(Enum)) && this.IsSubclassOf(typeof(ValueType));
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override bool HasElementTypeImpl()
		{
			return RuntimeTypeHandle.HasElementType(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected override bool IsArrayImpl()
		{
			return RuntimeTypeHandle.IsArray(this);
		}
		[SecuritySafeCritical]
		public override int GetArrayRank()
		{
			if (!this.IsArrayImpl())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_HasToBeArrayClass"));
			}
			return RuntimeTypeHandle.GetArrayRank(this);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override Type GetElementType()
		{
			return RuntimeTypeHandle.GetElementType(this);
		}
		public override string[] GetEnumNames()
		{
			if (!this.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			string[] array = Enum.InternalGetNames(this);
			string[] array2 = new string[array.Length];
			Array.Copy(array, array2, array.Length);
			return array2;
		}
		[SecuritySafeCritical]
		public override Array GetEnumValues()
		{
			if (!this.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			ulong[] array = Enum.InternalGetValues(this);
			Array array2 = Array.UnsafeCreateInstance(this, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				object value = Enum.ToObject(this, array[i]);
				array2.SetValue(value, i);
			}
			return array2;
		}
		[SecuritySafeCritical]
		public override Type GetEnumUnderlyingType()
		{
			if (!this.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return Enum.InternalGetUnderlyingType(this);
		}
		public override bool IsEnumDefined(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			RuntimeType runtimeType = (RuntimeType)value.GetType();
			if (runtimeType.IsEnum)
			{
				if (!runtimeType.IsEquivalentTo(this))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType", new object[]
					{
						runtimeType.ToString(), 
						this.ToString()
					}));
				}
				runtimeType = (RuntimeType)runtimeType.GetEnumUnderlyingType();
			}
			if (runtimeType == RuntimeType.StringType)
			{
				string[] array = Enum.InternalGetNames(this);
				return Array.IndexOf<object>(array, value) >= 0;
			}
			if (!Type.IsIntegerType(runtimeType))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
			}
			RuntimeType runtimeType2 = Enum.InternalGetUnderlyingType(this);
			if (runtimeType2 != runtimeType)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumUnderlyingTypeAndObjectMustBeSameType", new object[]
				{
					runtimeType.ToString(), 
					runtimeType2.ToString()
				}));
			}
			ulong[] array2 = Enum.InternalGetValues(this);
			ulong value2 = Enum.ToUInt64(value);
			return Array.BinarySearch<ulong>(array2, value2) >= 0;
		}
		public override string GetEnumName(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			Type type = value.GetType();
			if (!type.IsEnum && !Type.IsIntegerType(type))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value");
			}
			ulong[] array = Enum.InternalGetValues(this);
			ulong value2 = Enum.ToUInt64(value);
			int num = Array.BinarySearch<ulong>(array, value2);
			if (num >= 0)
			{
				string[] array2 = Enum.InternalGetNames(this);
				return array2[num];
			}
			return null;
		}
		internal RuntimeType[] GetGenericArgumentsInternal()
		{
			return this.GetRootElementType().GetTypeHandleInternal().GetInstantiationInternal();
		}
		[SecuritySafeCritical]
		public override Type[] GetGenericArguments()
		{
			Type[] array = this.GetRootElementType().GetTypeHandleInternal().GetInstantiationPublic();
			if (array == null)
			{
				array = new Type[0];
			}
			return array;
		}
		[SecuritySafeCritical]
		public override Type MakeGenericType(params Type[] instantiation)
		{
			if (instantiation == null)
			{
				throw new ArgumentNullException("instantiation");
			}
			RuntimeType[] array = new RuntimeType[instantiation.Length];
			if (!this.IsGenericTypeDefinition)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericTypeDefinition", new object[]
				{
					this
				}));
			}
			if (this.GetGenericArguments().Length != instantiation.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_GenericArgsCount"), "instantiation");
			}
			for (int i = 0; i < instantiation.Length; i++)
			{
				Type type = instantiation[i];
				if (type == null)
				{
					throw new ArgumentNullException();
				}
				RuntimeType runtimeType = type as RuntimeType;
				if (runtimeType == null)
				{
					Type[] array2 = new Type[instantiation.Length];
					for (int j = 0; j < instantiation.Length; j++)
					{
						array2[j] = instantiation[j];
					}
					instantiation = array2;
					return TypeBuilderInstantiation.MakeGenericType(this, instantiation);
				}
				array[i] = runtimeType;
			}
			RuntimeType[] genericArgumentsInternal = this.GetGenericArgumentsInternal();
			RuntimeType.SanityCheckGenericArguments(array, genericArgumentsInternal);
			Type result = null;
			try
			{
				result = new RuntimeTypeHandle(this).Instantiate(array);
			}
			catch (TypeLoadException ex)
			{
				RuntimeType.ValidateGenericArguments(this, array, ex);
				throw ex;
			}
			return result;
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override Type GetGenericTypeDefinition()
		{
			if (!this.IsGenericType)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotGenericType"));
			}
			return RuntimeTypeHandle.GetGenericTypeDefinition(this);
		}
		[SecuritySafeCritical]
		public override Type[] GetGenericParameterConstraints()
		{
			if (!this.IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
			}
			Type[] array = new RuntimeTypeHandle(this).GetConstraints();
			if (array == null)
			{
				array = new Type[0];
			}
			return array;
		}
		[SecuritySafeCritical]
		public override Type MakePointerType()
		{
			return new RuntimeTypeHandle(this).MakePointer();
		}
		[SecuritySafeCritical]
		public override Type MakeByRefType()
		{
			return new RuntimeTypeHandle(this).MakeByRef();
		}
		[SecuritySafeCritical]
		public override Type MakeArrayType()
		{
			return new RuntimeTypeHandle(this).MakeSZArray();
		}
		[SecuritySafeCritical]
		public override Type MakeArrayType(int rank)
		{
			if (rank <= 0)
			{
				throw new IndexOutOfRangeException();
			}
			return new RuntimeTypeHandle(this).MakeArray(rank);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CanValueSpecialCast(RuntimeType valueType, RuntimeType targetType);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object AllocateValueType(RuntimeType type, object value, bool fForceTypeChange);
		[SecuritySafeCritical]
		internal object CheckValue(object value, Binder binder, CultureInfo culture, BindingFlags invokeAttr)
		{
			if (this.IsInstanceOfType(value))
			{
				if (!object.ReferenceEquals(value.GetType(), this) && RuntimeTypeHandle.IsValueType(this))
				{
					return RuntimeType.AllocateValueType(this.TypeHandle.GetRuntimeType(), value, true);
				}
				return value;
			}
			else
			{
				bool isByRef = base.IsByRef;
				if (isByRef)
				{
					Type elementType = this.GetElementType();
					if (elementType.IsInstanceOfType(value) || value == null)
					{
						return RuntimeType.AllocateValueType(elementType.TypeHandle.GetRuntimeType(), value, false);
					}
				}
				else
				{
					if (value == null)
					{
						return value;
					}
					if (this == RuntimeType.s_typedRef)
					{
						return value;
					}
				}
				bool flag = base.IsPointer || this.IsEnum || base.IsPrimitive;
				if (flag)
				{
					Pointer pointer = value as Pointer;
					RuntimeType valueType;
					if (pointer != null)
					{
						valueType = pointer.GetPointerType();
					}
					else
					{
						valueType = (RuntimeType)value.GetType();
					}
					if (RuntimeType.CanValueSpecialCast(valueType, this))
					{
						if (pointer != null)
						{
							return pointer.GetPointerValue();
						}
						return value;
					}
				}
				if ((invokeAttr & BindingFlags.ExactBinding) == BindingFlags.ExactBinding)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), new object[]
					{
						value.GetType(), 
						this
					}));
				}
				return this.TryChangeType(value, binder, culture, flag);
			}
		}
		[SecurityCritical]
		private object TryChangeType(object value, Binder binder, CultureInfo culture, bool needsSpecialCast)
		{
			if (binder != null && binder != Type.DefaultBinder)
			{
				value = binder.ChangeType(value, this, culture);
				if (this.IsInstanceOfType(value))
				{
					return value;
				}
				if (base.IsByRef)
				{
					Type elementType = this.GetElementType();
					if (elementType.IsInstanceOfType(value) || value == null)
					{
						return RuntimeType.AllocateValueType(elementType.TypeHandle.GetRuntimeType(), value, false);
					}
				}
				else
				{
					if (value == null)
					{
						return value;
					}
				}
				if (needsSpecialCast)
				{
					Pointer pointer = value as Pointer;
					RuntimeType valueType;
					if (pointer != null)
					{
						valueType = pointer.GetPointerType();
					}
					else
					{
						valueType = (RuntimeType)value.GetType();
					}
					if (RuntimeType.CanValueSpecialCast(valueType, this))
					{
						if (pointer != null)
						{
							return pointer.GetPointerValue();
						}
						return value;
					}
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), new object[]
			{
				value.GetType(), 
				this
			}));
		}
		[SecurityCritical]
		internal virtual string GetDefaultMemberName()
		{
			string text = (string)this.RemotingCache[CacheObjType.DefaultMember];
			if (text == null)
			{
				object[] customAttributes = this.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
				if (customAttributes.Length > 1)
				{
					throw new InvalidProgramException(Environment.GetResourceString("ExecutionEngine_InvalidAttribute"));
				}
				if (customAttributes.Length == 0)
				{
					return null;
				}
				text = ((DefaultMemberAttribute)customAttributes[0]).MemberName;
				this.RemotingCache[CacheObjType.DefaultMember] = text;
			}
			return text;
		}
		[SecuritySafeCritical]
		public override MemberInfo[] GetDefaultMembers()
		{
			string text = (string)this.RemotingCache[CacheObjType.DefaultMember];
			if (text == null)
			{
				CustomAttributeData customAttributeData = null;
				Type typeFromHandle = typeof(DefaultMemberAttribute);
				RuntimeType runtimeType = this;
				while (runtimeType != null)
				{
					IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(runtimeType);
					for (int i = 0; i < customAttributes.Count; i++)
					{
						if (object.ReferenceEquals(customAttributes[i].Constructor.DeclaringType, typeFromHandle))
						{
							customAttributeData = customAttributes[i];
							break;
						}
					}
					if (customAttributeData != null)
					{
						break;
					}
					runtimeType = runtimeType.GetBaseType();
				}
				if (customAttributeData == null)
				{
					return new MemberInfo[0];
				}
				text = (customAttributeData.ConstructorArguments[0].Value as string);
				this.RemotingCache[CacheObjType.DefaultMember] = text;
			}
			MemberInfo[] array = base.GetMember(text);
			if (array == null)
			{
				array = new MemberInfo[0];
			}
			return array;
		}
		[DebuggerStepThrough, SecuritySafeCritical, DebuggerHidden]
		public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
		{
			if (this.IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
			}
			if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) == BindingFlags.Default)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NoAccessSpec"), "bindingFlags");
			}
			if ((bindingFlags & (BindingFlags)255) == BindingFlags.Default)
			{
				bindingFlags |= (BindingFlags.Instance | BindingFlags.Public);
				if ((bindingFlags & BindingFlags.CreateInstance) == BindingFlags.Default)
				{
					bindingFlags |= BindingFlags.Static;
				}
			}
			if (namedParams != null)
			{
				if (providedArgs != null)
				{
					if (namedParams.Length > providedArgs.Length)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
					}
				}
				else
				{
					if (namedParams.Length != 0)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
					}
				}
			}
			if (target != null && target.GetType().IsCOMObject)
			{
				if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) == BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMAccess"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty) & ~(BindingFlags.InvokeMethod | BindingFlags.GetProperty)) != BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty) & ~(BindingFlags.InvokeMethod | BindingFlags.GetProperty)) != BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty) & ~BindingFlags.SetProperty) != BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.PutDispProperty) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty) & ~BindingFlags.PutDispProperty) != BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.PutRefDispProperty) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty) & ~BindingFlags.PutRefDispProperty) != BindingFlags.Default)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if (RemotingServices.IsTransparentProxy(target))
				{
					return ((MarshalByRefObject)target).InvokeMember(name, bindingFlags, binder, providedArgs, modifiers, culture, namedParams);
				}
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				bool[] byrefModifiers = (modifiers == null) ? null : modifiers[0].IsByRefArray;
				int culture2 = (culture == null) ? 1033 : culture.LCID;
				return this.InvokeDispMethod(name, bindingFlags, target, providedArgs, byrefModifiers, culture2, namedParams);
			}
			else
			{
				if (namedParams != null && Array.IndexOf<string>(namedParams, null) != -1)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamNull"), "namedParams");
				}
				int num = (providedArgs != null) ? providedArgs.Length : 0;
				if (binder == null)
				{
					binder = Type.DefaultBinder;
				}
				Binder arg_253_0 = Type.DefaultBinder;
				if ((bindingFlags & BindingFlags.CreateInstance) != BindingFlags.Default)
				{
					if ((bindingFlags & BindingFlags.CreateInstance) != BindingFlags.Default && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty)) != BindingFlags.Default)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_CreatInstAccess"), "bindingFlags");
					}
					return Activator.CreateInstance(this, bindingFlags, binder, providedArgs, culture);
				}
				else
				{
					if ((bindingFlags & (BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) != BindingFlags.Default)
					{
						bindingFlags |= BindingFlags.SetProperty;
					}
					if (name == null)
					{
						throw new ArgumentNullException("name");
					}
					if (name.Length == 0 || name.Equals("[DISPID=0]"))
					{
						name = this.GetDefaultMemberName();
						if (name == null)
						{
							name = "ToString";
						}
					}
					bool flag = (bindingFlags & BindingFlags.GetField) != BindingFlags.Default;
					bool flag2 = (bindingFlags & BindingFlags.SetField) != BindingFlags.Default;
					if (flag || flag2)
					{
						if (flag)
						{
							if (flag2)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_FldSetGet"), "bindingFlags");
							}
							if ((bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_FldGetPropSet"), "bindingFlags");
							}
						}
						else
						{
							if (providedArgs == null)
							{
								throw new ArgumentNullException("providedArgs");
							}
							if ((bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_FldSetPropGet"), "bindingFlags");
							}
							if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_FldSetInvoke"), "bindingFlags");
							}
						}
						FieldInfo fieldInfo = null;
						FieldInfo[] array = this.GetMember(name, MemberTypes.Field, bindingFlags) as FieldInfo[];
						if (array.Length == 1)
						{
							fieldInfo = array[0];
						}
						else
						{
							if (array.Length > 0)
							{
								fieldInfo = binder.BindToField(bindingFlags, array, flag ? Empty.Value : providedArgs[0], culture);
							}
						}
						if (fieldInfo != null)
						{
							if (fieldInfo.FieldType.IsArray || object.ReferenceEquals(fieldInfo.FieldType, typeof(Array)))
							{
								int num2;
								if ((bindingFlags & BindingFlags.GetField) != BindingFlags.Default)
								{
									num2 = num;
								}
								else
								{
									num2 = num - 1;
								}
								if (num2 > 0)
								{
									int[] array2 = new int[num2];
									for (int i = 0; i < num2; i++)
									{
										try
										{
											array2[i] = ((IConvertible)providedArgs[i]).ToInt32(null);
										}
										catch (InvalidCastException)
										{
											throw new ArgumentException(Environment.GetResourceString("Arg_IndexMustBeInt"));
										}
									}
									Array array3 = (Array)fieldInfo.GetValue(target);
									if ((bindingFlags & BindingFlags.GetField) != BindingFlags.Default)
									{
										return array3.GetValue(array2);
									}
									array3.SetValue(providedArgs[num2], array2);
									return null;
								}
							}
							if (flag)
							{
								if (num != 0)
								{
									throw new ArgumentException(Environment.GetResourceString("Arg_FldGetArgErr"), "bindingFlags");
								}
								return fieldInfo.GetValue(target);
							}
							else
							{
								if (num != 1)
								{
									throw new ArgumentException(Environment.GetResourceString("Arg_FldSetArgErr"), "bindingFlags");
								}
								fieldInfo.SetValue(target, providedArgs[0], bindingFlags, binder, culture);
								return null;
							}
						}
						else
						{
							if ((bindingFlags & (BindingFlags)16773888) == BindingFlags.Default)
							{
								throw new MissingFieldException(this.FullName, name);
							}
						}
					}
					bool flag3 = (bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default;
					bool flag4 = (bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default;
					if (flag3 || flag4)
					{
						if (flag3)
						{
							if (flag4)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
							}
						}
						else
						{
							if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
							}
						}
					}
					MethodInfo[] array4 = null;
					MethodInfo methodInfo = null;
					if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
					{
						MethodInfo[] array5 = this.GetMember(name, MemberTypes.Method, bindingFlags) as MethodInfo[];
						List<MethodInfo> list = null;
						for (int j = 0; j < array5.Length; j++)
						{
							MethodInfo methodInfo2 = array5[j];
							if (RuntimeType.FilterApplyMethodInfo((RuntimeMethodInfo)methodInfo2, bindingFlags, CallingConventions.Any, new Type[num]))
							{
								if (methodInfo == null)
								{
									methodInfo = methodInfo2;
								}
								else
								{
									if (list == null)
									{
										list = new List<MethodInfo>(array5.Length);
										list.Add(methodInfo);
									}
									list.Add(methodInfo2);
								}
							}
						}
						if (list != null)
						{
							array4 = new MethodInfo[list.Count];
							list.CopyTo(array4);
						}
					}
					if ((methodInfo == null && flag3) || flag4)
					{
						PropertyInfo[] array6 = this.GetMember(name, MemberTypes.Property, bindingFlags) as PropertyInfo[];
						List<MethodInfo> list2 = null;
						for (int k = 0; k < array6.Length; k++)
						{
							MethodInfo methodInfo3 = null;
							if (flag4)
							{
								methodInfo3 = array6[k].GetSetMethod(true);
							}
							else
							{
								methodInfo3 = array6[k].GetGetMethod(true);
							}
							if (!(methodInfo3 == null) && RuntimeType.FilterApplyMethodInfo((RuntimeMethodInfo)methodInfo3, bindingFlags, CallingConventions.Any, new Type[num]))
							{
								if (methodInfo == null)
								{
									methodInfo = methodInfo3;
								}
								else
								{
									if (list2 == null)
									{
										list2 = new List<MethodInfo>(array6.Length);
										list2.Add(methodInfo);
									}
									list2.Add(methodInfo3);
								}
							}
						}
						if (list2 != null)
						{
							array4 = new MethodInfo[list2.Count];
							list2.CopyTo(array4);
						}
					}
					if (!(methodInfo != null))
					{
						throw new MissingMethodException(this.FullName, name);
					}
					if (array4 == null && num == 0 && methodInfo.GetParametersNoCopy().Length == 0 && (bindingFlags & BindingFlags.OptionalParamBinding) == BindingFlags.Default)
					{
						return methodInfo.Invoke(target, bindingFlags, binder, providedArgs, culture);
					}
					if (array4 == null)
					{
						array4 = new MethodInfo[]
						{
							methodInfo
						};
					}
					if (providedArgs == null)
					{
						providedArgs = new object[0];
					}
					object obj = null;
					MethodBase methodBase = null;
					try
					{
						methodBase = binder.BindToMethod(bindingFlags, array4, ref providedArgs, modifiers, culture, namedParams, out obj);
					}
					catch (MissingMethodException)
					{
					}
					if (methodBase == null)
					{
						throw new MissingMethodException(this.FullName, name);
					}
					object result = ((MethodInfo)methodBase).Invoke(target, bindingFlags, binder, providedArgs, culture);
					if (obj != null)
					{
						binder.ReorderArgumentArray(ref providedArgs, obj);
					}
					return result;
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override bool Equals(object obj)
		{
			return obj == this;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}
		public static bool operator ==(RuntimeType left, RuntimeType right)
		{
			return object.ReferenceEquals(left, right);
		}
		public static bool operator !=(RuntimeType left, RuntimeType right)
		{
			return !object.ReferenceEquals(left, right);
		}
		[SecuritySafeCritical]
		public override string ToString()
		{
			return this.Cache.GetToString();
		}
		public object Clone()
		{
			return this;
		}
		[SecurityCritical]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			UnitySerializationHolder.GetUnitySerializationInfo(info, this);
		}
		[SecuritySafeCritical]
		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, RuntimeType.ObjectType, inherit);
		}
		[SecuritySafeCritical]
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(this, runtimeType, inherit);
		}
		[SecuritySafeCritical]
		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.IsDefined(this, runtimeType, inherit);
		}
		public override IList<CustomAttributeData> GetCustomAttributesData()
		{
			return CustomAttributeData.GetCustomAttributesInternal(this);
		}
		private void CreateInstanceCheckThis()
		{
			if (this is ReflectionOnlyType)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
			}
			if (this.ContainsGenericParameters)
			{
				throw new ArgumentException(Environment.GetResourceString("Acc_CreateGenericEx", new object[]
				{
					this
				}));
			}
			Type rootElementType = this.GetRootElementType();
			if (object.ReferenceEquals(rootElementType, typeof(ArgIterator)))
			{
				throw new NotSupportedException(Environment.GetResourceString("Acc_CreateArgIterator"));
			}
			if (object.ReferenceEquals(rootElementType, typeof(void)))
			{
				throw new NotSupportedException(Environment.GetResourceString("Acc_CreateVoid"));
			}
		}
		[SecurityCritical]
		internal object CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			this.CreateInstanceCheckThis();
			object result = null;
			try
			{
				try
				{
					if (activationAttributes != null)
					{
						ActivationServices.PushActivationAttributes(this, activationAttributes);
					}
					if (args == null)
					{
						args = new object[0];
					}
					int num = args.Length;
					if (binder == null)
					{
						binder = Type.DefaultBinder;
					}
					if (num == 0 && (bindingAttr & BindingFlags.Public) != BindingFlags.Default && (bindingAttr & BindingFlags.Instance) != BindingFlags.Default && (this.IsGenericCOMObjectImpl() || base.IsValueType))
					{
						result = this.CreateInstanceDefaultCtor((bindingAttr & BindingFlags.NonPublic) == BindingFlags.Default);
					}
					else
					{
						ConstructorInfo[] constructors = this.GetConstructors(bindingAttr);
						List<MethodBase> list = new List<MethodBase>(constructors.Length);
						Type[] array = new Type[num];
						for (int i = 0; i < num; i++)
						{
							if (args[i] != null)
							{
								array[i] = args[i].GetType();
							}
						}
						for (int j = 0; j < constructors.Length; j++)
						{
							if (RuntimeType.FilterApplyConstructorInfo((RuntimeConstructorInfo)constructors[j], bindingAttr, CallingConventions.Any, array))
							{
								list.Add(constructors[j]);
							}
						}
						MethodBase[] array2 = new MethodBase[list.Count];
						list.CopyTo(array2);
						if (array2 != null && array2.Length == 0)
						{
							array2 = null;
						}
						if (array2 == null)
						{
							if (activationAttributes != null)
							{
								ActivationServices.PopActivationAttributes(this);
								activationAttributes = null;
							}
							throw new MissingMethodException(Environment.GetResourceString("MissingConstructor_Name", new object[]
							{
								this.FullName
							}));
						}
						object obj = null;
						MethodBase methodBase;
						try
						{
							methodBase = binder.BindToMethod(bindingAttr, array2, ref args, null, culture, null, out obj);
						}
						catch (MissingMethodException)
						{
							methodBase = null;
						}
						if (methodBase == null)
						{
							if (activationAttributes != null)
							{
								ActivationServices.PopActivationAttributes(this);
								activationAttributes = null;
							}
							throw new MissingMethodException(Environment.GetResourceString("MissingConstructor_Name", new object[]
							{
								this.FullName
							}));
						}
						if (RuntimeType.DelegateType.IsAssignableFrom(methodBase.DeclaringType))
						{
							new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
						}
						if (methodBase.GetParametersNoCopy().Length == 0)
						{
							if (args.Length != 0)
							{
								throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("NotSupported_CallToVarArg"), new object[0]));
							}
							result = Activator.CreateInstance(this, true);
						}
						else
						{
							result = ((ConstructorInfo)methodBase).Invoke(bindingAttr, binder, args, culture);
							if (obj != null)
							{
								binder.ReorderArgumentArray(ref args, obj);
							}
						}
					}
				}
				finally
				{
					if (activationAttributes != null)
					{
						ActivationServices.PopActivationAttributes(this);
						activationAttributes = null;
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return result;
		}
		[SecuritySafeCritical]
		private object CreateInstanceSlow(bool publicOnly, bool skipCheckThis, bool fillCache)
		{
			RuntimeMethodHandleInternal rmh = default(RuntimeMethodHandleInternal);
			bool bNeedSecurityCheck = true;
			bool flag = false;
			bool noCheck = false;
			if (!skipCheckThis)
			{
				this.CreateInstanceCheckThis();
			}
			if (!fillCache)
			{
				noCheck = true;
			}
			object result = RuntimeTypeHandle.CreateInstance(this, publicOnly, noCheck, ref flag, ref rmh, ref bNeedSecurityCheck);
			if (flag && fillCache)
			{
				RuntimeType.ActivatorCache activatorCache = RuntimeType.s_ActivatorCache;
				if (activatorCache == null)
				{
					activatorCache = new RuntimeType.ActivatorCache();
					Thread.MemoryBarrier();
					RuntimeType.s_ActivatorCache = activatorCache;
				}
				RuntimeType.ActivatorCacheEntry entry = new RuntimeType.ActivatorCacheEntry(this, rmh, bNeedSecurityCheck);
				Thread.MemoryBarrier();
				activatorCache.SetEntry(entry);
			}
			return result;
		}
		[DebuggerStepThrough, DebuggerHidden]
		internal object CreateInstanceDefaultCtor(bool publicOnly)
		{
			return this.CreateInstanceDefaultCtor(publicOnly, false, false, true);
		}
		[DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
		internal object CreateInstanceDefaultCtor(bool publicOnly, bool skipVisibilityChecks, bool skipCheckThis, bool fillCache)
		{
			if (base.GetType() == typeof(ReflectionOnlyType))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
			}
			RuntimeType.ActivatorCache activatorCache = RuntimeType.s_ActivatorCache;
			if (activatorCache != null)
			{
				RuntimeType.ActivatorCacheEntry entry = activatorCache.GetEntry(this);
				if (entry != null)
				{
					if (publicOnly && entry.m_ctor != null && (entry.m_ctorAttributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
					{
						throw new MissingMethodException(Environment.GetResourceString("Arg_NoDefCTor"));
					}
					object obj = RuntimeTypeHandle.Allocate(this);
					if (entry.m_ctor != null)
					{
						if (!skipVisibilityChecks && entry.m_bNeedSecurityCheck)
						{
							RuntimeMethodHandle.PerformSecurityCheck(obj, entry.m_hCtorMethodHandle, this, 268435456u);
						}
						try
						{
							entry.m_ctor(obj);
						}
						catch (Exception inner)
						{
							throw new TargetInvocationException(inner);
						}
					}
					return obj;
				}
			}
			return this.CreateInstanceSlow(publicOnly, skipCheckThis, fillCache);
		}
		internal void InvalidateCachedNestedType()
		{
			this.Cache.InvalidateCachedNestedType();
		}
		[SecuritySafeCritical]
		internal bool IsGenericCOMObjectImpl()
		{
			return RuntimeTypeHandle.IsComObject(this, true);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object _CreateEnum(RuntimeType enumType, long value);
		[SecuritySafeCritical]
		internal static object CreateEnum(RuntimeType enumType, long value)
		{
			return RuntimeType._CreateEnum(enumType, value);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object InvokeDispMethod(string name, BindingFlags invokeAttr, object target, object[] args, bool[] byrefModifiers, int culture, string[] namedParameters);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Type GetTypeFromProgIDImpl(string progID, string server, bool throwOnError);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Type GetTypeFromCLSIDImpl(Guid clsid, string server, bool throwOnError);
		[SecuritySafeCritical]
		private object ForwardCallToInvokeMember(string memberName, BindingFlags flags, object target, int[] aWrapperTypes, ref MessageData msgData)
		{
			ParameterModifier[] array = null;
			object obj = null;
			Message message = new Message();
			message.InitFields(msgData);
			MethodInfo methodInfo = (MethodInfo)message.GetMethodBase();
			object[] args = message.Args;
			int num = args.Length;
			ParameterInfo[] parametersNoCopy = methodInfo.GetParametersNoCopy();
			if (num > 0)
			{
				ParameterModifier parameterModifier = new ParameterModifier(num);
				for (int i = 0; i < num; i++)
				{
					if (parametersNoCopy[i].ParameterType.IsByRef)
					{
						parameterModifier[i] = true;
					}
				}
				array = new ParameterModifier[]
				{
					parameterModifier
				};
				if (aWrapperTypes != null)
				{
					this.WrapArgsForInvokeCall(args, aWrapperTypes);
				}
			}
			if (object.ReferenceEquals(methodInfo.ReturnType, typeof(void)))
			{
				flags |= BindingFlags.IgnoreReturn;
			}
			try
			{
				obj = this.InvokeMember(memberName, flags, null, target, args, array, null, null);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			for (int j = 0; j < num; j++)
			{
				if (array[0][j] && args[j] != null)
				{
					Type elementType = parametersNoCopy[j].ParameterType.GetElementType();
					if (!object.ReferenceEquals(elementType, args[j].GetType()))
					{
						args[j] = this.ForwardCallBinder.ChangeType(args[j], elementType, null);
					}
				}
			}
			if (obj != null)
			{
				Type returnType = methodInfo.ReturnType;
				if (!object.ReferenceEquals(returnType, obj.GetType()))
				{
					obj = this.ForwardCallBinder.ChangeType(obj, returnType, null);
				}
			}
			RealProxy.PropagateOutParameters(message, args, obj);
			return obj;
		}
		[SecuritySafeCritical]
		private void WrapArgsForInvokeCall(object[] aArgs, int[] aWrapperTypes)
		{
			int num = aArgs.Length;
			for (int i = 0; i < num; i++)
			{
				if (aWrapperTypes[i] != 0)
				{
					if ((aWrapperTypes[i] & 65536) != 0)
					{
						Type type = null;
						bool flag = false;
						RuntimeType.DispatchWrapperType dispatchWrapperType = (RuntimeType.DispatchWrapperType)(aWrapperTypes[i] & -65537);
						if (dispatchWrapperType <= RuntimeType.DispatchWrapperType.Error)
						{
							switch (dispatchWrapperType)
							{
								case RuntimeType.DispatchWrapperType.Unknown:
								{
									type = typeof(UnknownWrapper);
									break;
								}
								case RuntimeType.DispatchWrapperType.Dispatch:
								{
									type = typeof(DispatchWrapper);
									break;
								}
								default:
								{
									if (dispatchWrapperType == RuntimeType.DispatchWrapperType.Error)
									{
										type = typeof(ErrorWrapper);
									}
									break;
								}
							}
						}
						else
						{
							if (dispatchWrapperType != RuntimeType.DispatchWrapperType.Currency)
							{
								if (dispatchWrapperType == RuntimeType.DispatchWrapperType.BStr)
								{
									type = typeof(BStrWrapper);
									flag = true;
								}
							}
							else
							{
								type = typeof(CurrencyWrapper);
							}
						}
						Array array = (Array)aArgs[i];
						int length = array.Length;
						object[] array2 = (object[])Array.UnsafeCreateInstance(type, length);
						ConstructorInfo constructor;
						if (flag)
						{
							constructor = type.GetConstructor(new Type[]
							{
								typeof(string)
							});
						}
						else
						{
							constructor = type.GetConstructor(new Type[]
							{
								typeof(object)
							});
						}
						for (int j = 0; j < length; j++)
						{
							if (flag)
							{
								array2[j] = constructor.Invoke(new object[]
								{
									(string)array.GetValue(j)
								});
							}
							else
							{
								array2[j] = constructor.Invoke(new object[]
								{
									array.GetValue(j)
								});
							}
						}
						aArgs[i] = array2;
					}
					else
					{
						RuntimeType.DispatchWrapperType dispatchWrapperType2 = (RuntimeType.DispatchWrapperType)aWrapperTypes[i];
						if (dispatchWrapperType2 <= RuntimeType.DispatchWrapperType.Error)
						{
							switch (dispatchWrapperType2)
							{
								case RuntimeType.DispatchWrapperType.Unknown:
								{
									aArgs[i] = new UnknownWrapper(aArgs[i]);
									break;
								}
								case RuntimeType.DispatchWrapperType.Dispatch:
								{
									aArgs[i] = new DispatchWrapper(aArgs[i]);
									break;
								}
								default:
								{
									if (dispatchWrapperType2 == RuntimeType.DispatchWrapperType.Error)
									{
										aArgs[i] = new ErrorWrapper(aArgs[i]);
									}
									break;
								}
							}
						}
						else
						{
							if (dispatchWrapperType2 != RuntimeType.DispatchWrapperType.Currency)
							{
								if (dispatchWrapperType2 == RuntimeType.DispatchWrapperType.BStr)
								{
									aArgs[i] = new BStrWrapper((string)aArgs[i]);
								}
							}
							else
							{
								aArgs[i] = new CurrencyWrapper(aArgs[i]);
							}
						}
					}
				}
			}
		}
	}
}
