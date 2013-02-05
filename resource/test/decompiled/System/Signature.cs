using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
namespace System
{
	internal class Signature
	{
		internal enum MdSigCallingConvention : byte
		{
			Generics = 16,
			HasThis = 32,
			ExplicitThis = 64,
			CallConvMask = 15,
			Default = 0,
			C,
			StdCall,
			ThisCall,
			FastCall,
			Vararg,
			Field,
			LocalSig,
			Property,
			Unmgd,
			GenericInst,
			Max
		}
		internal SignatureStruct m_signature;
		internal CallingConventions CallingConvention
		{
			get
			{
				return this.m_signature.m_managedCallingConvention & (CallingConventions)255;
			}
		}
		internal RuntimeType[] Arguments
		{
			get
			{
				return this.m_signature.m_arguments;
			}
		}
		internal RuntimeType ReturnType
		{
			get
			{
				return this.m_signature.m_returnTypeORfieldType;
			}
		}
		internal RuntimeType FieldType
		{
			get
			{
				return this.m_signature.m_returnTypeORfieldType;
			}
		}
		public static implicit operator SignatureStruct(Signature pThis)
		{
			return pThis.m_signature;
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void GetSignature(ref SignatureStruct signature, void* pCorSig, int cCorSig, RuntimeFieldHandleInternal fieldHandle, IRuntimeMethodInfo methodHandle, RuntimeType declaringType);
		[SecuritySafeCritical]
		internal static void GetSignatureForDynamicMethod(ref SignatureStruct signature, IRuntimeMethodInfo methodHandle)
		{
			Signature.GetSignature(ref signature, null, 0, default(RuntimeFieldHandleInternal), methodHandle, null);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetCustomModifiers(ref SignatureStruct signature, int parameter, out Type[] required, out Type[] optional);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CompareSig(ref SignatureStruct left, ref SignatureStruct right);
		public Signature(IRuntimeMethodInfo method, RuntimeType[] arguments, RuntimeType returnType, CallingConventions callingConvention)
		{
			SignatureStruct signature = new SignatureStruct(method.Value, arguments, returnType, callingConvention);
			Signature.GetSignatureForDynamicMethod(ref signature, method);
			this.m_signature = signature;
		}
		[SecuritySafeCritical]
		public Signature(IRuntimeMethodInfo methodHandle, RuntimeType declaringType)
		{
			SignatureStruct signature = default(SignatureStruct);
			Signature.GetSignature(ref signature, null, 0, default(RuntimeFieldHandleInternal), methodHandle, declaringType);
			this.m_signature = signature;
		}
		[SecurityCritical]
		public Signature(IRuntimeFieldInfo fieldHandle, RuntimeType declaringType)
		{
			SignatureStruct signature = default(SignatureStruct);
			Signature.GetSignature(ref signature, null, 0, fieldHandle.Value, null, declaringType);
			GC.KeepAlive(fieldHandle);
			this.m_signature = signature;
		}
		[SecurityCritical]
		public unsafe Signature(void* pCorSig, int cCorSig, RuntimeType declaringType)
		{
			SignatureStruct signature = default(SignatureStruct);
			Signature.GetSignature(ref signature, pCorSig, cCorSig, default(RuntimeFieldHandleInternal), null, declaringType);
			this.m_signature = signature;
		}
		[SecuritySafeCritical]
		internal static bool DiffSigs(Signature sig1, Signature sig2)
		{
			SignatureStruct signatureStruct = sig1;
			SignatureStruct signatureStruct2 = sig2;
			return Signature.CompareSig(ref signatureStruct, ref signatureStruct2);
		}
		[SecuritySafeCritical]
		public Type[] GetCustomModifiers(int position, bool required)
		{
			Type[] result = null;
			Type[] result2 = null;
			SignatureStruct signatureStruct = this;
			Signature.GetCustomModifiers(ref signatureStruct, position, out result, out result2);
			if (!required)
			{
				return result2;
			}
			return result;
		}
	}
}
