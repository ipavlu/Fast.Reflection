using System;
using System.Reflection;

namespace Consurrent.FastReflection.NetCore
{
	internal class MemberDelegateConfiguration<TTarget, TReturn> : ADelegateConfiguration<MemberDelegateConfiguration<TTarget, TReturn>>
	{
		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public MemberInfo Member { get; }

		public MemberDelegateConfiguration(MemberInfo member) : this(null, member) { }

		public MemberDelegateConfiguration(Delegate storeDelegate, MemberInfo member)
			: base(storeDelegate)
		{
			Member = member ?? throw new ArgumentNullException(nameof(member));
			HashCode = GetHashCode();
		}

		public override int GetHashCode()
		{
			int code;
			unchecked
			{
				code = TargetType.GetHashCode();
				code ^= 397 * ReturnType.GetHashCode();
				code ^= 397 * ReturnType.GetHashCode();
				code ^= 397 * Member.GetHashCode();
				return code;
			}
		}

		protected override bool EqualsOfT(MemberDelegateConfiguration<TTarget, TReturn> other) =>
			other?.TargetType == TargetType &&
			other.ReturnType == ReturnType &&
			other.Member == Member
		;
	}
}