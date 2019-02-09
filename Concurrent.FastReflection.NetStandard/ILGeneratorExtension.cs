using System.Reflection.Emit;

// ReSharper disable InconsistentNaming

namespace Concurrent.FastReflection.NetStandard
{
	public static class ILGeneratorExtension
	{
		public static ILEmitter ToILEmitter(this ILGenerator il) => new ILEmitter(il);
	}
}