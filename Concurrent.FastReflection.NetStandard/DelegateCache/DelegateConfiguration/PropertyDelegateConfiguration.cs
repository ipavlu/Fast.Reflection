using System;
using System.Reflection;

namespace Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration
{
	internal class PropertyDelegateConfiguration<TTarget, TReturn, TDirection> : ADelegateConfiguration<PropertyDelegateConfiguration<TTarget, TReturn, TDirection>>
	{
		private int Code { get; }
		protected override int FactoryGetHashCode() => Code;

		public Type TargetType => typeof(TTarget);
		public Type ReturnType => typeof(TReturn);
		public Type DirectionType => typeof(TDirection);
		public PropertyInfo Property { get; }

		public PropertyDelegateConfiguration(PropertyInfo property) : this(null, property) { }

		public PropertyDelegateConfiguration(Delegate storeDelegate, PropertyInfo property)
			: base(storeDelegate)
		{
			Property = property ?? throw new ArgumentNullException(nameof(property));
			unchecked
			{
				Code = TargetType.GetHashCode();
				Code ^= 397 * ReturnType.GetHashCode();
				Code ^= 397 * DirectionType.GetHashCode();
				Code ^= 397 * Property.GetHashCode();
			}
		}

		protected override bool EqualsOfT(PropertyDelegateConfiguration<TTarget, TReturn, TDirection> other) =>
		other != null &&
		other.Code == Code &&
		other.TargetType == TargetType &&
		other.ReturnType == ReturnType &&
		other.DirectionType == DirectionType &&
		other.Property == Property
		;
	}
}