using System;
using System.Collections.Immutable;
using System.Linq;

namespace Concurrent.FastReflection.NetCore
{
	internal class ConstructorDelegateConfiguration<TTarget> : ADelegateConfiguration<ConstructorDelegateConfiguration<TTarget>>
	{
		public Type RequestType => typeof(TTarget);
		public Type Type { get; }
		public ImmutableArray<Type> ArgsTypes { get; }
		private ImmutableArray<Type> Configuration { get; }

		public ConstructorDelegateConfiguration(Type type, Type[] argsTypes = null) : this(null, type, argsTypes) { }

		public ConstructorDelegateConfiguration(Delegate storeDelegate, Type type, Type[] argsTypes = null)
			: base(storeDelegate)
		{
			Type = type;
			ArgsTypes = (argsTypes = argsTypes ?? Array.Empty<Type>()).ToImmutableArray();
			Configuration = ImmutableArray.Create<Type>().Add(RequestType).Add(Type).AddRange(ArgsTypes);

			HashCode =
				Configuration
					.Select(x => x.GetHashCode())
					.Aggregate((c, n) => { unchecked { return c ^ (397 * n); } })
				;
		}

		public override int GetHashCode() => HashCode;

		protected override bool EqualsOfT(ConstructorDelegateConfiguration<TTarget> other) =>
			other?.Configuration.Length == Configuration.Length &&
			Enumerable
				.Range(0, Configuration.Length)
				.Select(i => other.Configuration[i] == Configuration[i])
				.All(x => x)
		;
	}
}