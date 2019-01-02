using System;
using System.Reflection;

namespace Concurrent.FastReflection.NetCore
{
	internal class PropertyDelegateConfiguration<TTarget, TReturn, TDirection> : ADelegateConfiguration<PropertyDelegateConfiguration<TTarget, TReturn, TDirection>>
	{
		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public Type DirectionType => typeof(TDirection);
		public PropertyInfo Property { get; }

		public PropertyDelegateConfiguration(PropertyInfo property) : this(null, property) { }

		public PropertyDelegateConfiguration(Delegate storeDelegate, PropertyInfo property)
			: base(storeDelegate)
		{
			Property = property ?? throw new ArgumentNullException(nameof(property));
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
				code ^= 397 * Property.GetHashCode();
				return code;
			}
		}

		protected override bool EqualsOfT(PropertyDelegateConfiguration<TTarget, TReturn, TDirection> other) =>
		other?.TargetType == TargetType &&
		other.ReturnType == ReturnType &&
		other.DirectionType == DirectionType &&
		other.Property == Property
		;
	}
}