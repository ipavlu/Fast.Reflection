using System;

namespace Consurrent.FastReflection.NetCore
{
	internal abstract class ADelegateConfiguration<T> : ADelegateConfiguration where T : ADelegateConfiguration
	{
		protected virtual bool EqualsOfT(T other) => false;
		protected override bool ProtectedEquals(ADelegateConfiguration other) => EqualsOfT(other as T);
		protected ADelegateConfiguration(Delegate storeDelegate) : base(storeDelegate) { }

		
		public T StoreDelegateConfiguration(DelegateCacheTransaction<T> transaction)
		=> transaction?.StoreDelegate(this as T) ?? this as T
		;
	}

	internal abstract class ADelegateConfiguration : IEquatable<ADelegateConfiguration>
	{
		public int HashCode { get; protected set; }

		public Delegate StoredDelegate { get; protected set; }

		protected ADelegateConfiguration(Delegate storeDelegate) => StoredDelegate = storeDelegate;

		public override int GetHashCode() => HashCode;

		public override bool Equals(object o) => Equals(o as ADelegateConfiguration);

		public virtual bool Equals(ADelegateConfiguration other) => other?.HashCode == HashCode && ProtectedEquals(other);

		protected virtual bool ProtectedEquals(ADelegateConfiguration other) => false;
	}
}
