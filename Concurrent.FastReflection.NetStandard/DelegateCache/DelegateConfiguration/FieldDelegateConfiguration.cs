using System;
using System.Reflection;

namespace Consurrent.FastReflection.NetCore
{
	internal class FieldDelegateConfiguration<TTarget, TReturn, TDirection> : ADelegateConfiguration<FieldDelegateConfiguration<TTarget, TReturn, TDirection>>
	{
		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public Type DirectionType => typeof(TDirection);
		public FieldInfo Field { get; }

		public FieldDelegateConfiguration(FieldInfo field) : this(null, field) { }

		public FieldDelegateConfiguration(Delegate storeDelegate, FieldInfo field)
			: base(storeDelegate)
		{
			Field = field ?? throw new ArgumentNullException(nameof(field));
			HashCode = GetHashCode();
		}

		public override int GetHashCode()
		{
			int code;
			unchecked
			{
				code = TargetType.GetHashCode();
				code ^= 397 * TargetType.GetHashCode();
				code ^= 397 * ReturnType.GetHashCode();
				code ^= 397 * DirectionType.GetHashCode();
				code ^= 397 * Field.GetHashCode();
				return code;
			}
		}

		protected override bool EqualsOfT(FieldDelegateConfiguration<TTarget, TReturn, TDirection> other) =>
			other?.TargetType == TargetType &&
			other.ReturnType == ReturnType &&
			other.DirectionType == DirectionType &&
			other.Field == Field
		;
	}
}