using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Concurrent.FastReflection.NetCore
{
	internal class TransactionalDelegateCache
	{
		private readonly object Lock = new object();
		private IDictionary<ADelegateConfiguration, ADelegateConfiguration> Cache { get; } = new Dictionary<ADelegateConfiguration, ADelegateConfiguration>();

		private Action<ADelegateConfiguration> StoreTransactionAction { get; }
		private Action<ADelegateConfiguration> ExitTransactionAction { get; }

		private void StoreTransaction(ADelegateConfiguration storeDelegate)
		{
			if (storeDelegate?.StoredDelegate != null) Cache[storeDelegate] = storeDelegate;
			ExitTransaction(storeDelegate);
		}

		private void ExitTransaction(ADelegateConfiguration ignoreDelegate)
		{
			Monitor.Exit(Lock);
			//used memory barrier, little less performant,
			//but ensures fairness on heavy loaded boxes
		}

		public TransactionalDelegateCache()
		{
			StoreTransactionAction = StoreTransaction;
			ExitTransactionAction = ExitTransaction;
		}

		public DelegateCacheTransaction<ConstructorDelegateConfiguration<TTarget>> Transaction<TTarget>(Type type, Type[] argsTypes = null)
		{
			ADelegateConfiguration search = new ConstructorDelegateConfiguration<TTarget>(type, argsTypes);

			bool gotLock = false;
			Monitor.Enter(Lock, ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(TransactionalDelegateCache)}.{nameof(Transaction)}: failed to acquire access");

			return
			Cache.TryGetValue(search, out ADelegateConfiguration result) && result?.StoredDelegate != null ?
			new DelegateCacheTransaction<ConstructorDelegateConfiguration<TTarget>>(result, ExitTransactionAction) :
			new DelegateCacheTransaction<ConstructorDelegateConfiguration<TTarget>>(search, StoreTransactionAction)
			;
		}

		public DelegateCacheTransaction<PropertyDelegateConfiguration<TTarget, TReturn, TDirection>> Transaction<TTarget, TReturn, TDirection>(PropertyInfo property)
		{
			ADelegateConfiguration search = new PropertyDelegateConfiguration<TTarget, TReturn, TDirection>(property);

			bool gotLock = false;
			Monitor.Enter(Lock, ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(TransactionalDelegateCache)}.{nameof(Transaction)}: failed to acquire access");

			return
			Cache.TryGetValue(search, out ADelegateConfiguration result) && result?.StoredDelegate != null ?
			new DelegateCacheTransaction<PropertyDelegateConfiguration<TTarget, TReturn, TDirection>>(result, ExitTransactionAction) :
			new DelegateCacheTransaction<PropertyDelegateConfiguration<TTarget, TReturn, TDirection>>(search, StoreTransactionAction)
			;
		}

		public DelegateCacheTransaction<FieldDelegateConfiguration<TTarget, TReturn, TDirection>> Transaction<TTarget, TReturn, TDirection>(FieldInfo field)
		{
			ADelegateConfiguration search = new FieldDelegateConfiguration<TTarget, TReturn, TDirection>(field);

			bool gotLock = false;
			Monitor.Enter(Lock, ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(TransactionalDelegateCache)}.{nameof(Transaction)}: failed to acquire access");

			return
			Cache.TryGetValue(search, out ADelegateConfiguration result) && result?.StoredDelegate != null ?
			new DelegateCacheTransaction<FieldDelegateConfiguration<TTarget, TReturn, TDirection>>(result, ExitTransactionAction) :
			new DelegateCacheTransaction<FieldDelegateConfiguration<TTarget, TReturn, TDirection>>(search, StoreTransactionAction)
			;
		}


		public DelegateCacheTransaction<MemberDelegateConfiguration<TTarget, TReturn>> Transaction<TTarget, TReturn>(MemberInfo member)
		{
			ADelegateConfiguration search = new MemberDelegateConfiguration<TTarget, TReturn>(member);

			bool gotLock = false;
			Monitor.Enter(Lock, ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(TransactionalDelegateCache)}.{nameof(Transaction)}: failed to acquire access");

			return
			Cache.TryGetValue(search, out ADelegateConfiguration result) && result?.StoredDelegate != null ?
			new DelegateCacheTransaction<MemberDelegateConfiguration<TTarget, TReturn>>(result, ExitTransactionAction) :
			new DelegateCacheTransaction<MemberDelegateConfiguration<TTarget, TReturn>>(search, StoreTransactionAction)
			;
		}
	}
}