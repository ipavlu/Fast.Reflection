using System;
using System.Reflection;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal class MemberDelegateConfiguration<TTarget, TReturn> : ADelegateConfiguration<MemberDelegateConfiguration<TTarget, TReturn>>
	{
		private int Code { get; }
		protected override int FactoryGetHashCode() => Code;

		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public MemberInfo Member { get; }

		public MemberDelegateConfiguration(MemberInfo member) : this(null, member) { }

		public MemberDelegateConfiguration(Delegate storeDelegate, MemberInfo member)
			: base(storeDelegate)
		{
			Member = member ?? throw new ArgumentNullException(nameof(member));
			unchecked
			{
				Code = TargetType.GetHashCode();
				Code ^= 397 * ReturnType.GetHashCode();
				Code ^= 397 * Member.GetHashCode();
			}
		}

		protected override bool EqualsOfT(MemberDelegateConfiguration<TTarget, TReturn> other) =>
		other != null &&
		other.Code == Code &&
		other.TargetType == TargetType &&
		other.ReturnType == ReturnType &&
		other.Member == Member
		;
	}
}