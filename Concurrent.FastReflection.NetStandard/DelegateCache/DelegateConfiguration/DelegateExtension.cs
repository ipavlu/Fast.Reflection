using System;
using System.Reflection;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal static class DelegateExtension
	{
		public static ConstructorDelegateConfiguration<TTarget> ToConstructorDelegateConfig<TTarget>(this Delegate delegateSrc, Module module, Type type, Type[] args)
			=> new ConstructorDelegateConfiguration<TTarget>(delegateSrc, module, type, args)
		;
		public static PropertyDelegateConfiguration<TTarget, TReturn, TDirection> ToPropertyDelegateConfig<TTarget, TReturn, TDirection>(this Delegate delegateSrc, PropertyInfo property)
			=> new PropertyDelegateConfiguration<TTarget, TReturn, TDirection>(delegateSrc, property)
		;
		public static FieldDelegateConfiguration<TTarget, TReturn, TDirection> ToFieldDelegateConfig<TTarget, TReturn, TDirection>(this Delegate delegateSrc, FieldInfo field)
			=> new FieldDelegateConfiguration<TTarget, TReturn, TDirection>(delegateSrc, field)
		;
		public static MemberDelegateConfiguration<TTarget, TReturn> ToMemberDelegateConfig<TTarget, TReturn>(this Delegate delegateSrc, MemberInfo member)
			=> new MemberDelegateConfiguration<TTarget, TReturn>(delegateSrc, member)
		;
	}
}