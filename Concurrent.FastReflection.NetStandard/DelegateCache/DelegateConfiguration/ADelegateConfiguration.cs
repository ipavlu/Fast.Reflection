using System;
using System.Collections.Generic;
using System.Threading;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal abstract class ADelegateConfiguration<T> : ADelegateConfiguration where T : ADelegateConfiguration
	{
		protected abstract bool EqualsOfT(T other);
		protected override bool ProtectedEquals(ADelegateConfiguration other) => other is T otherT && EqualsOfT(otherT);
		protected ADelegateConfiguration(Delegate storeDelegate) : base(storeDelegate) { }
		public T StoreDelegateConfiguration(DelegateCacheTransaction<T> transaction)
		=> transaction?.StoreDelegate(this as T) ?? this as T
		;
	}

	internal abstract class ADelegateConfiguration : IEquatable<ADelegateConfiguration>
	{
		private SpinLock _spinLock = new SpinLock();

		private int? StoredHashCode { get; set; }

		private int? GetStoredHashCode()
		{
			bool entry = false;
			try
			{
				_spinLock.Enter(ref entry);
				return StoredHashCode;
			}
			finally
			{
				if (entry) _spinLock.Exit(true);
			}
		}

		private int GenerateAndStoreHashCode()
		{
			int code = FactoryGetHashCode();
			bool entry = false;
			try
			{
				_spinLock.Enter(ref entry);
				if (StoredHashCode.HasValue && StoredHashCode.Value != code) throw new ArgumentException($"{nameof(FactoryGetHashCode)} is supposed to generate same in each run for immutable data!");
				StoredHashCode = code;
				return StoredHashCode.Value;
			}
			finally
			{
				if (entry) _spinLock.Exit(true);
			}
		}

		protected int GetOrSetHashCode() => GetStoredHashCode() ?? GenerateAndStoreHashCode();

		protected abstract int FactoryGetHashCode();

		public int HashCode => GetOrSetHashCode();

		public Delegate StoredDelegate { get; protected set; }

		protected ADelegateConfiguration(Delegate storeDelegate) => StoredDelegate = storeDelegate;

		public override int GetHashCode() => GetOrSetHashCode();

		public override bool Equals(object o) => o is ADelegateConfiguration other && Equals(other);

		public virtual bool Equals(ADelegateConfiguration other) => other != null && other.HashCode == HashCode && ProtectedEquals(other);

		protected abstract bool ProtectedEquals(ADelegateConfiguration other);

		protected static bool CompareTypeSequences(IList<Type> a, IList<Type> b)
		{
			if (a == null || b == null) return false;
			if (a.Count != b.Count) return false;
			int max = a.Count;
			for (int i = 0; i < max; ++i)
			{
				if (a[i] != b[i]) return false;
			}
			return true;
		}
	}
}
