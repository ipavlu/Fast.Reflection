using System;
using System.Reflection;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal class FieldDelegateConfiguration<TTarget, TReturn, TDirection> : ADelegateConfiguration<FieldDelegateConfiguration<TTarget, TReturn, TDirection>>
	{
		private int Code { get; }
		protected override int FactoryGetHashCode() => Code;

		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public Type DirectionType => typeof(TDirection);
		public FieldInfo Field { get; }

		public FieldDelegateConfiguration(FieldInfo field) : this(null, field) { }

		public FieldDelegateConfiguration(Delegate storeDelegate, FieldInfo field)
			: base(storeDelegate)
		{
			Field = field ?? throw new ArgumentNullException(nameof(field));

			unchecked
			{
				Code = TargetType.GetHashCode();
				Code ^= 397 * ReturnType.GetHashCode();
				Code ^= 397 * DirectionType.GetHashCode();
				Code ^= 397 * Field.GetHashCode();
			}
		}
		
		protected override bool EqualsOfT(FieldDelegateConfiguration<TTarget, TReturn, TDirection> other) =>
		other != null &&
		other.Code == Code &&
		other.TargetType == TargetType &&
		other.ReturnType == ReturnType &&
		other.DirectionType == DirectionType &&
		other.Field == Field
		;
	}
}