using System.Reflection.Emit;

namespace Consurrent.FastReflection.NetCore
{
	public static class ILGeneratorExtension
	{
		public static ILEmitter ToILEmitter(this ILGenerator il) => new ILEmitter(il);
	}
}