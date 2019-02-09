using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal class ConstructorDelegateConfiguration<TTarget> : ADelegateConfiguration<ConstructorDelegateConfiguration<TTarget>>
	{
		private int Code { get; }
		protected override int FactoryGetHashCode() => Code;

		public Type RequestType => typeof(TTarget);
		public Type Type { get; }
		public Module Module { get; }
		public ImmutableArray<Type> ArgsTypes { get; }

		public ConstructorDelegateConfiguration(Module module, Type type, Type[] argsTypes = null) : this(null, module, type, argsTypes) { }
		public ConstructorDelegateConfiguration(Delegate storeDelegate, Module module, Type type, Type[] argsTypes = null)
			: base(storeDelegate)
		{
			Type = type;
			ArgsTypes = (argsTypes ?? Array.Empty<Type>()).ToImmutableArray();
			Module = module;

			unchecked
			{
				Code = RequestType.GetHashCode();
				Code ^= 397 * Type.GetHashCode();
				Code = Module == null ? Code : Code ^ (397 * Module.GetHashCode());
				for (int i = 0; i < ArgsTypes.Length; ++i)
				{
					Code ^= 397 * ArgsTypes[i].GetHashCode();
				}
			}
		}

		protected override bool EqualsOfT(ConstructorDelegateConfiguration<TTarget> other) =>
		other != null &&
		other.Code == Code &&
		other.RequestType == RequestType &&
		other.Type == Type &&
		other.Module == Module &&
		CompareTypeSequences(other.ArgsTypes, ArgsTypes)
		;
	}
}