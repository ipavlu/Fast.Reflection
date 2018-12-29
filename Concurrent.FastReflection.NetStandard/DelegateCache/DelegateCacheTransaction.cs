using System;

namespace Consurrent.FastReflection.NetCore
{
	internal class DelegateCacheTransaction<TDelegateConfig> : IDisposable
		where TDelegateConfig : ADelegateConfiguration
	{
		private Action<ADelegateConfiguration> ExitTransaction { get; set; }
		public TDelegateConfig Config { get; private set; }
		public Delegate Delegate => Config?.StoredDelegate;
		public bool HasDelegate => Delegate != default(Delegate);

		public TDelegateConfig StoreDelegate(TDelegateConfig delegateConfig) => Config = delegateConfig;

		public void Dispose()
		{
			if (ExitTransaction == null) throw new InvalidOperationException("An attempt to reuse disposed transaction has failed!");
			ExitTransaction.Invoke(Config);
			ExitTransaction = null;
		}

		public DelegateCacheTransaction(ADelegateConfiguration config, Action<ADelegateConfiguration> exitTransaction)
		: this((TDelegateConfig)config, exitTransaction) { }

		public DelegateCacheTransaction(TDelegateConfig config, Action<ADelegateConfiguration> exitTransaction)
		{
			ExitTransaction = exitTransaction ?? throw new ArgumentNullException(nameof(exitTransaction));
			Config = config;
		}
	}
}