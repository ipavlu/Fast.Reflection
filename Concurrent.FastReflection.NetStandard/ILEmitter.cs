using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Concurrent.FastReflection.NetCore
{
#pragma warning disable IDE1006 // Naming Styles
	public class ILEmitter
	{
		private ILGenerator il { get; }

		public ILEmitter(ILGenerator generator) => il = generator;

		public ILEmitter ret()                                 { il.Emit(OpCodes.Ret); return this; }
		public ILEmitter cast(Type type)                       { il.Emit(OpCodes.Castclass, type); return this; }
		public ILEmitter box(Type type)                        { il.Emit(OpCodes.Box, type); return this; }
		public ILEmitter unbox_any(Type type)                  { il.Emit(OpCodes.Unbox_Any, type); return this; }
		public ILEmitter unbox(Type type)                      { il.Emit(OpCodes.Unbox, type); return this; }
		public ILEmitter call(MethodInfo method)               { il.Emit(OpCodes.Call, method); return this; }
		public ILEmitter callvirt(MethodInfo method)           { il.Emit(OpCodes.Callvirt, method); return this; }
		public ILEmitter ldnull()                              { il.Emit(OpCodes.Ldnull); return this; }
		public ILEmitter bne_un(Label target)                  { il.Emit(OpCodes.Bne_Un, target); return this; }
		public ILEmitter beq(Label target)                     { il.Emit(OpCodes.Beq, target); return this; }
		public ILEmitter ldc_i4_0()                            { il.Emit(OpCodes.Ldc_I4_0); return this; }
		public ILEmitter ldc_i4_1()                            { il.Emit(OpCodes.Ldc_I4_1); return this; }
		public ILEmitter ldc_i4(int c)                         { il.Emit(OpCodes.Ldc_I4, c); return this; }
		public ILEmitter ldc_r4(float c)                       { il.Emit(OpCodes.Ldc_R4, c); return this; }
		public ILEmitter ldc_r8(double c)                      { il.Emit(OpCodes.Ldc_R8, c); return this; }
		public ILEmitter ldarg0()                              { il.Emit(OpCodes.Ldarg_0); return this; }
		public ILEmitter ldarg1()                              { il.Emit(OpCodes.Ldarg_1); return this; }
		public ILEmitter ldarg2()                              { il.Emit(OpCodes.Ldarg_2); return this; }
		public ILEmitter ldarga(int idx)                       { il.Emit(OpCodes.Ldarga, idx); return this; }
		public ILEmitter ldarga_s(int idx)                     { il.Emit(OpCodes.Ldarga_S, idx); return this; }
		public ILEmitter ldarg(int idx)                        { il.Emit(OpCodes.Ldarg, idx); return this; }
		public ILEmitter ldarg_s(int idx)                      { il.Emit(OpCodes.Ldarg_S, idx); return this; }
		public ILEmitter ldstr(string str)                     { il.Emit(OpCodes.Ldstr, str); return this; }
		public ILEmitter ifclass_ldind_ref(Type type)		   { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return this; }
		public ILEmitter ldloc0()                              { il.Emit(OpCodes.Ldloc_0); return this; }
		public ILEmitter ldloc1()                              { il.Emit(OpCodes.Ldloc_1); return this; }
		public ILEmitter ldloc2()                              { il.Emit(OpCodes.Ldloc_2); return this; }
		public ILEmitter ldloca_s(int idx)                     { il.Emit(OpCodes.Ldloca_S, idx); return this; }
		public ILEmitter ldloca_s(LocalBuilder local)          { il.Emit(OpCodes.Ldloca_S, local); return this; }
		public ILEmitter ldloc_s(int idx)                      { il.Emit(OpCodes.Ldloc_S, idx); return this; }
		public ILEmitter ldloc_s(LocalBuilder local)           { il.Emit(OpCodes.Ldloc_S, local); return this; }
		public ILEmitter ldloca(int idx)                       { il.Emit(OpCodes.Ldloca, idx); return this; }
		public ILEmitter ldloca(LocalBuilder local)            { il.Emit(OpCodes.Ldloca, local); return this; }
		public ILEmitter ldloc(int idx)                        { il.Emit(OpCodes.Ldloc, idx); return this; }
		public ILEmitter ldloc(LocalBuilder local)             { il.Emit(OpCodes.Ldloc, local); return this; }
		public ILEmitter initobj(Type type)                    { il.Emit(OpCodes.Initobj, type); return this; }
		public ILEmitter newobj(ConstructorInfo ctor)          { il.Emit(OpCodes.Newobj, ctor); return this; }
		public ILEmitter Throw()                               { il.Emit(OpCodes.Throw); return this; }
		public ILEmitter throw_new(Type type)                  { ConstructorInfo exp = type.GetConstructor(Type.EmptyTypes); newobj(exp).Throw(); return this; }
		public ILEmitter stelem_ref()                          { il.Emit(OpCodes.Stelem_Ref); return this; }
		public ILEmitter ldelem_ref()                          { il.Emit(OpCodes.Ldelem_Ref); return this; }
		public ILEmitter ldlen()                               { il.Emit(OpCodes.Ldlen); return this; }
		public ILEmitter stloc(int idx)                        { il.Emit(OpCodes.Stloc, idx); return this; }
		public ILEmitter stloc_s(int idx)                      { il.Emit(OpCodes.Stloc_S, idx); return this; }
		public ILEmitter stloc(LocalBuilder local)             { il.Emit(OpCodes.Stloc, local); return this; }
		public ILEmitter stloc_s(LocalBuilder local)           { il.Emit(OpCodes.Stloc_S, local); return this; }
		public ILEmitter stloc0()                              { il.Emit(OpCodes.Stloc_0); return this; }
		public ILEmitter stloc1()                              { il.Emit(OpCodes.Stloc_1); return this; }
		public ILEmitter mark(Label label)                     { il.MarkLabel(label); return this; }
		public ILEmitter ldfld(FieldInfo field)                { il.Emit(OpCodes.Ldfld, field); return this; }
		public ILEmitter ldsfld(FieldInfo field)               { il.Emit(OpCodes.Ldsfld, field); return this; }
		public ILEmitter lodfld(FieldInfo field)               => field.IsStatic ? ldsfld(field) : ldfld(field);
		public ILEmitter ifvaluetype_box(Type type)            { if (type.IsValueType) il.Emit(OpCodes.Box, type); return this; }
		public ILEmitter stfld(FieldInfo field)                { il.Emit(OpCodes.Stfld, field); return this; }
		public ILEmitter stsfld(FieldInfo field)               { il.Emit(OpCodes.Stsfld, field); return this; }
		public ILEmitter setfld(FieldInfo field)               => field.IsStatic ? stsfld(field) : stfld(field);
		public ILEmitter unboxorcast(Type type)                =>  type.IsValueType ? unbox(type) : cast(type);
		public ILEmitter callorvirt(MethodInfo method)         { if (method.IsVirtual) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return this; }
		public ILEmitter stind_ref()                           { il.Emit(OpCodes.Stind_Ref); return this; }
		public ILEmitter ldind_ref()                           { il.Emit(OpCodes.Ldind_Ref); return this; }
		public LocalBuilder declocal(Type type)                => il.DeclareLocal(type);
		public Label deflabel()                                => il.DefineLabel();
		public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type) => type.IsValueType ? ldarga(idx) : ldarg(idx);
		public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type) => type.IsValueType ? ldloca(idx) : ldloc(idx);
		public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member) { action(this, member); return this; }
		public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type) => type.IsByRef ? ldloca(local) : ldloc(local);
	}
#pragma warning restore IDE1006 // Naming Styles
}