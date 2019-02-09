using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Concurrent.FastReflection.NetStandard.DelegateCache;
using Concurrent.FastReflection.NetStandard.DelegateCache.DelegateConfiguration;

namespace Concurrent.FastReflection.NetStandard
{
	public delegate void MemberSetter<TTarget, TValue>(ref TTarget target, TValue value);
	public delegate TReturn MemberGetter<TTarget, TReturn>(TTarget target);
	public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);
	public delegate T CtorInvoker<T>(object[] parameters);

	/// <summary>
	/// A dynamic reflection extensions library that emits IL to set/get fields/properties, call methods and invoke constructors
	/// Once the delegate is created, it can be stored and reused resulting in much faster access times than using regular reflection
	/// The results are cached. Once a delegate is generated, any subsequent call to generate the same delegate on the same field/property/method will return the previously generated delegate
	/// Note: Since this generates IL, it won't work on AOT platforms such as iOS an Android. But is useful and works very well in editor codes and standalone targets
	/// </summary>
	public static class FastReflection
	{
		private static TransactionalDelegateCache Cache { get; } = new TransactionalDelegateCache();

		private const string KCtorInvokerName = "CI<>";
		private const string KMethodCallerName = "MC<>";
		private const string KFieldSetterName = "FS<>";
		private const string KFieldGetterName = "FG<>";
		private const string KPropertySetterName = "PS<>";
		private const string KPropertyGetterName = "PG<>";

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static CtorInvoker<T> DelegateForCtor<T>(this Type type, params Type[] paramTypes) => type.DelegateForCtor<T>(null, paramTypes);

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static CtorInvoker<T> DelegateForCtor<T>(this Type type, Module module, params Type[] paramTypes)
		{
			using (var transaction = Cache.Transaction<T>(module, type, paramTypes))
			{
				if (transaction.HasDelegate) return (CtorInvoker<T>)transaction.Delegate;

				var dynMethod = module == null
					? new DynamicMethod(KCtorInvokerName, typeof(T), new[] { typeof(object[]) })
					: new DynamicMethod(KCtorInvokerName, typeof(T), new[] { typeof(object[]) }, module);

				var emit = dynMethod.GetILGenerator().ToILEmitter();
				GenCtor<T>(emit, type, paramTypes);

				return
				(CtorInvoker<T>)
				dynMethod
				.CreateDelegate(typeof(CtorInvoker<T>))
				.ToConstructorDelegateConfig<T>(module, type, paramTypes)
				.StoreDelegateConfiguration(transaction)
				.StoredDelegate
				;
			}
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static CtorInvoker<object> DelegateForCtor(this Type type, params Type[] ctorParamTypes)
		=> DelegateForCtor<object>(type, ctorParamTypes)
		;

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static MemberGetter<TTarget, TValue> DelegateForGet<TTarget, TValue>(this PropertyInfo property)
		{
			if (!property.CanRead) throw new InvalidOperationException($"Property is not readable {property.Name}");

			using (var transaction = Cache.Transaction<TTarget, TValue, MemberGetter<TTarget, TValue>>(property))
			{
				if (transaction.HasDelegate) return (MemberGetter<TTarget, TValue>)transaction.Delegate;

				return
				GenDelegateForMember<MemberGetter<TTarget, TValue>, PropertyInfo>(
					property,
					KPropertyGetterName,
					GenPropertyGetter<TTarget>,
					typeof(TValue),
					typeof(TTarget))
				.ToPropertyDelegateConfig<TTarget, TValue, MemberGetter<TTarget, TValue>>(property)
				.StoreDelegateConfiguration(transaction)
				.StoredDelegate as MemberGetter<TTarget, TValue>
				;
			}
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static MemberGetter<object, object> DelegateForGet(this PropertyInfo property) => DelegateForGet<object, object>(property);

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static MemberSetter<TTarget, TValue> DelegateForSet<TTarget, TValue>(this PropertyInfo property)
		{
			if (!property.CanWrite) throw new InvalidOperationException($"Property is not writable {property.Name}");

			using (var transaction = Cache.Transaction<TTarget, TValue, MemberSetter<TTarget, TValue>>(property))
			{
				if (transaction.HasDelegate) return (MemberSetter<TTarget, TValue>)transaction.Delegate;

				return
				GenDelegateForMember<MemberSetter<TTarget, TValue>, PropertyInfo>(
						property,
						KPropertySetterName,
						GenPropertySetter<TTarget>,
						typeof(void),
						typeof(TTarget).MakeByRefType(),
						typeof(TValue))
				.ToPropertyDelegateConfig<TTarget, TValue, MemberSetter<TTarget, TValue>>(property)
				.StoreDelegateConfiguration(transaction)
				.StoredDelegate as MemberSetter<TTarget, TValue>
				;
			}
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static MemberSetter<object, object> DelegateForSet(this PropertyInfo property) => DelegateForSet<object, object>(property);

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static MemberGetter<TTarget, TValue> DelegateForGet<TTarget, TValue>(this FieldInfo field)
		{
			using (var transaction = Cache.Transaction<TTarget, TValue, MemberGetter<TTarget, TValue>>(field))
			{
				if (transaction.HasDelegate) return (MemberGetter<TTarget, TValue>)transaction.Delegate;

				return
					GenDelegateForMember<MemberGetter<TTarget, TValue>, FieldInfo>(
							field,
							KFieldGetterName,
							GenFieldGetter<TTarget>,
							typeof(TValue),
							typeof(TTarget))
						.ToFieldDelegateConfig<TTarget, TValue, MemberGetter<TTarget, TValue>>(field)
						.StoreDelegateConfiguration(transaction)
						.StoredDelegate as MemberGetter<TTarget, TValue>
					;
			}
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static MemberGetter<object, object> DelegateForGet(this FieldInfo field) => DelegateForGet<object, object>(field);

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static MemberSetter<TTarget, TValue> DelegateForSet<TTarget, TValue>(this FieldInfo field)
		{
			using (var transaction = Cache.Transaction<TTarget, TValue, MemberSetter<TTarget, TValue>>(field))
			{
				if (transaction.HasDelegate) return (MemberSetter<TTarget, TValue>)transaction.Delegate;

				return
				GenDelegateForMember<MemberSetter<TTarget, TValue>, FieldInfo>(
						field,
						KFieldSetterName,
						GenFieldSetter<TTarget>,
						typeof(void),
						typeof(TTarget).MakeByRefType(),
						typeof(TValue))
				.ToFieldDelegateConfig<TTarget, TValue, MemberSetter<TTarget, TValue>>(field)
				.StoreDelegateConfiguration(transaction)
				.StoredDelegate as MemberSetter<TTarget, TValue>
				;
			}
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static MemberSetter<object, object> DelegateForSet(this FieldInfo field) => DelegateForSet<object, object>(field);

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static MethodCaller<TTarget, TReturn> DelegateForCall<TTarget, TReturn>(this MethodInfo method)
		{
			using (var transaction = Cache.Transaction<TTarget, TReturn>(method))
			{
				if (transaction.HasDelegate) return (MethodCaller<TTarget, TReturn>)transaction.Delegate;

				return
				GenDelegateForMember<MethodCaller<TTarget, TReturn>, MethodInfo>(
						method,
						KMethodCallerName,
						GenMethodInvocation<TTarget>,
						typeof(TReturn),
						typeof(TTarget),
						typeof(object[]))
				.ToMemberDelegateConfig<TTarget, TReturn>(method)
				.StoreDelegateConfiguration(transaction)
				.StoredDelegate as MethodCaller<TTarget, TReturn>
				;
			}
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static MethodCaller<object, object> DelegateForCall(this MethodInfo method) => DelegateForCall<object, object>(method);

		/// <summary>
		/// Executes the delegate on the specified target and arguments but only if it's not null
		/// </summary>
		public static void SafeInvoke<TTarget, TValue>(this MethodCaller<TTarget, TValue> caller, TTarget target, params object[] args)
		=> caller?.Invoke(target, args)
		;

		/// <summary>
		/// Executes the delegate on the specified target and value but only if it's not null
		/// </summary>
		public static void SafeInvoke<TTarget, TValue>(this MemberSetter<TTarget, TValue> setter, ref TTarget target, TValue value)
		=> setter?.Invoke(ref target, value)
		;

		/// <summary>
		/// Executes the delegate on the specified target only if it's not null, returns default(TReturn) otherwise
		/// </summary>
		public static TReturn SafeInvoke<TTarget, TReturn>(this MemberGetter<TTarget, TReturn> getter, TTarget target)
		=> getter == null ? default(TReturn) : getter(target);

		///// <summary>
		///// Obsolete because .NetCore can not save assembly.
		///// Generates a assembly called 'fileName' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
		///// If 'field' is not null, it generates a setter and getter for that field
		///// If 'property' is not null, it generates a setter and getter for that property
		///// If 'method' is not null, it generates a call for that method
		///// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
		///// </summary>
		//[Obsolete("Obsolete because .NetCore can not save assembly.", true)]
		//public static void GenDebugAssembly(string fileName, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
		//=> GenDebugAssembly<object>(fileName, field, property, method, targetType, ctorParamTypes)
		//;

		///// <summary>
		///// Not implemented [disabled]  because .NetCore can not save assembly.
		///// Generates a assembly called 'fileName' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
		///// If 'field' is not null, it generates a setter and getter for that field
		///// If 'property' is not null, it generates a setter and getter for that property
		///// If 'method' is not null, it generates a call for that method
		///// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
		///// </summary>
		//[Obsolete("Obsolete because .NetCore can not save assembly.", true)]
		//public static void GenDebugAssembly<TTarget>(string fileName, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
		//{
		//	//TODO RunAndSave???
		//	//var asmName = new AssemblyName("Asm");
		//	//var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
		//	var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

		//	//Todo: filename
		//	//var modBuilder = asmBuilder.DefineDynamicModule("Mod", fileName);
		//	var modBuilder = asmBuilder.DefineDynamicModule(fileName);

		//	var typeBuilder = modBuilder.DefineType("Test", TypeAttributes.Public);

		//	var weakTyping = typeof(TTarget) == typeof(object);

		//	Func<string, Type, Type[], ILGenerator> buildMethod = (methodName, returnType, parameterTypes) =>
		//	{
		//		var methodBuilder = typeBuilder.DefineMethod(methodName,
		//		MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
		//		CallingConventions.Standard,
		//		returnType, parameterTypes);
		//		return methodBuilder.GetILGenerator();
		//	};

		//	if (field != null)
		//	{
		//		var fieldType = weakTyping ? typeof(object) : field.FieldType;
		//		var emit =  buildMethod("FieldSetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), fieldType }).ToILEmitter();
		//		GenFieldSetter<TTarget>(emit, field);
		//		emit = buildMethod("FieldGetter", fieldType, new Type[] {typeof(TTarget)}).ToILEmitter();
		//		GenFieldGetter<TTarget>(emit, field);
		//	}

		//	if (property != null)
		//	{
		//		var propType = weakTyping ? typeof(object) : property.PropertyType;
		//		var emit = buildMethod("PropertySetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), propType }).ToILEmitter();
		//		GenPropertySetter<TTarget>(emit, property);
		//		emit = buildMethod("PropertyGetter", propType, new Type[] { typeof(TTarget) }).ToILEmitter();
		//		GenPropertyGetter<TTarget>(emit, property);
		//	}

		//	if (method != null)
		//	{
		//		var returnType = (weakTyping || method.ReturnType == typeof(void)) ? typeof(object) : method.ReturnType;
		//		var emit = buildMethod("MethodCaller", returnType, new Type[] { typeof(TTarget), typeof(object[]) }).ToILEmitter();
		//		GenMethodInvocation<TTarget>(emit, method);
		//	}

		//	if (targetType != null)
		//	{
		//		var emit = buildMethod("Ctor", typeof(TTarget), new Type[] {typeof(object[])}).ToILEmitter();
		//		GenCtor<TTarget>(emit, targetType, ctorParamTypes);
		//	}

		//	typeBuilder.CreateType();

		//	///ToDo: Save
		//	//smBuilder.Save(fileName);
		//}

		//static int GetKey<T, R>(MemberInfo member, string dynMethodName)
		//=> member.GetHashCode() ^ dynMethodName.GetHashCode() ^ typeof(T).GetHashCode() ^ typeof(R).GetHashCode()
		//;

		private
		static
		TDelegate
		GenDelegateForMember<TDelegate, TMember>(	TMember member,
													string dynMethodName,
													Action<ILEmitter, TMember> generator,
													Type returnType,
													params Type[] paramTypes)
		where TMember : MemberInfo
		where TDelegate : class
		{
			var dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);

			generator(dynMethod.GetILGenerator().ToILEmitter(), member);

			var result = dynMethod.CreateDelegate(typeof(TDelegate));
			return (TDelegate)(object)result;
		}

		static void GenCtor<T>(ILEmitter emit, Type type, Type[] paramTypes)
		{
			// arg0: object[] arguments
			// goal: return new T(arguments)
			Type targetType = typeof(T) == typeof(object) ? type : typeof(T);

			if (targetType.IsValueType && paramTypes.Length == 0)
			{
				var tmp = emit.declocal(targetType);
				emit
				.ldloca(tmp)
				.initobj(targetType)
				.ldloc(0);
			}
			else
			{
				var ctor = targetType.GetConstructor(paramTypes);
				if (ctor == null)
				{
					string paramsTypesText =
					paramTypes
					.Select(x => x.Name)
					.Aggregate(	string.Empty,
								(c,n) => string.IsNullOrEmpty(c) ? n : $"{c},{n}",
								rslt => string.IsNullOrEmpty(rslt) ? "No empty constructor found!" : rslt)
					;
					throw new Exception($"Generating constructor for type: {targetType} {paramsTypesText}");
				}

				// push parameters in order to then call ctor
				for (int i = 0, imax = paramTypes.Length; i < imax; i++)
				{
					emit
					.ldarg0()					// push args array
					.ldc_i4(i)					// push index
					.ldelem_ref()				// push array[index]
					.unbox_any(paramTypes[i])	// cast
					;
				}

				emit.newobj(ctor);
			}

			if (typeof(T) == typeof(object) && targetType.IsValueType) emit.box(targetType);

			emit.ret();
		}

		static void GenMethodInvocation<TTarget>(ILEmitter emit, MethodInfo method)
		{
			var weaklyTyped = typeof(TTarget) == typeof(object);

			// push target if not static (instance-method. in that case first arg is always 'this')
			if (!method.IsStatic)
			{
				var targetType = weaklyTyped ? method.DeclaringType : typeof(TTarget);
				emit.declocal(targetType);
				emit.ldarg0();
				if (weaklyTyped) emit.unbox_any(targetType);
				emit.stloc0().ifclass_ldloc_else_ldloca(0, targetType);
			}

			// push arguments in order to call method
			var prams = method.GetParameters();
			for (int i = 0, imax = prams.Length; i < imax; i++)
			{
				emit
				.ldarg1()		// push array
				.ldc_i4(i)		// push index
				.ldelem_ref()
				;	// pop array, index and push array[index]

				var param = prams[i];
				var dataType = param.ParameterType;

				if (dataType.IsByRef) dataType = dataType.GetElementType();

				var tmp = emit.declocal(dataType);

				emit
				.unbox_any(dataType)
				.stloc(tmp)
				.ifbyref_ldloca_else_ldloc(tmp, param.ParameterType)
				;
			}

			// perform the correct call (pushes the result)
			emit.callorvirt(method);

			// if method wasn't static that means we declared a temp local to load the target
			// that means our local variables index for the arguments start from 1
			int localVarStart = method.IsStatic ? 0 : 1;

			for (int i = 0; i < prams.Length; i++)
			{
				var paramType = prams[i].ParameterType;
				if (paramType.IsByRef)
				{
					var byRefType = paramType.GetElementType();
					emit.ldarg1()
					.ldc_i4(i)
					.ldloc(i + localVarStart);
					if (byRefType != null && byRefType.IsValueType) emit.box(byRefType);
					emit.stelem_ref();
				}
			}

			if (method.ReturnType == typeof(void)) emit.ldnull();
			else if (weaklyTyped) emit.ifvaluetype_box(method.ReturnType);

			emit.ret();
		}

		static void GenFieldGetter<TTarget>(ILEmitter emit, FieldInfo field)
		=>	GenMemberGetter<TTarget>(emit, field, field.FieldType, field.IsStatic, (e, f) =>
			{
				if (field.IsLiteral)
				{
					if (field.FieldType == typeof(bool)) e.ldc_i4_1();
					else if(field.FieldType == typeof(int)) e.ldc_i4((int) field.GetRawConstantValue());
					else if (field.FieldType == typeof(float)) e.ldc_r4((float) field.GetRawConstantValue());
					else if (field.FieldType == typeof(double)) e.ldc_r8((double)field.GetRawConstantValue());
					else if (field.FieldType == typeof(string)) e.ldstr((string) field.GetRawConstantValue());
					else throw new NotSupportedException($"Creating a FieldGetter for type: {field.FieldType.Name} is unsupported.");
					return;
				}
				e.lodfld((FieldInfo)f);
			})
		;

		static void GenPropertyGetter<TTarget>(ILEmitter emit, PropertyInfo property)
		=>	GenMemberGetter<TTarget>(	emit,
										property,
										property.PropertyType,
										property.GetGetMethod(true).IsStatic,
										(e, p) => e.callorvirt(((PropertyInfo)p).GetGetMethod(true)))
		;

		static void GenMemberGetter<TTarget>(	ILEmitter emit,
												MemberInfo member,
												Type memberType,
												bool isStatic,
												Action<ILEmitter, MemberInfo> get)
		{
			if (typeof(TTarget) == typeof(object)) // weakly-typed?
			{
				// if we're static immediately load member and return value
				// otherwise load and cast target, get the member value and box it if neccessary:
				// return ((DeclaringType)target).member;
				if (!isStatic) emit.ldarg0().unboxorcast(member.DeclaringType);

				emit.perform(get, member).ifvaluetype_box(memberType);
			}
			else // we're strongly-typed, don't need any casting or boxing
			{
				// if we're static return member value immediately
				// otherwise load target and get member value immeidately
				// return target.member;
				if (!isStatic) emit.ifclass_ldarg_else_ldarga(0, typeof(TTarget));
				emit.perform(get, member);
			}

			emit.ret();
		}

		static void GenFieldSetter<TTarget>(ILEmitter emit, FieldInfo field)
		=>	GenMemberSetter<TTarget>(	emit,
										field,
										field.FieldType,
										field.IsStatic,
										(e, f) => e.setfld((FieldInfo)f))
		;

		static void GenPropertySetter<TTarget>(ILEmitter emit, PropertyInfo property)
		=>	GenMemberSetter<TTarget>(	emit,
										property,
										property.PropertyType,
										property.GetSetMethod(true).IsStatic,
										(e, p) => e.callorvirt(((PropertyInfo)p).GetSetMethod(true))	)
		;

		public static void GenMemberSetter<TTarget>(ILEmitter emit,
													MemberInfo member,
													Type memberType,
													bool isStatic,
													Action<ILEmitter, MemberInfo> set)
		{
			var targetType = typeof(TTarget);
			var stronglyTyped = targetType != typeof(object);

			// if we're static set member immediately
			if (isStatic)
			{
				emit.ldarg1();
				if (!stronglyTyped) emit.unbox_any(memberType);
				emit.perform(set, member).ret();
				return;
			}

			if (stronglyTyped)
			{
				// push target and value argument, set member immediately
				// target.member = value;
				emit
				.ldarg0()
				.ifclass_ldind_ref(targetType)
				.ldarg1()
				.perform(set, member)
				.ret()
				;

				return;
			}

			// we're weakly-typed
			targetType = member.DeclaringType;
			if (targetType != null && !targetType.IsValueType) // are we a reference-type?
			{
				// load and cast target, load and cast value and set
				// ((TargetType)target).member = (MemberType)value;
				emit
				.ldarg0()
				.ldind_ref()
				.cast(targetType)
				.ldarg1()
				.unbox_any(memberType)
				.perform(set, member)
				.ret()
				;

				return;
			}

			// we're a value-type
			// handle boxing/unboxing for the user so he doesn't have to do it himself
			// here's what we're basically generating (remember, we're weakly typed, so
			// the target argument is of type object here):
			// TargetType tmp = (TargetType)target; // unbox
			// tmp.member = (MemberField)value;		// set member value
			// target = tmp;						// box back

			emit.declocal(targetType);

			emit
			.ldarg0()
			.ldind_ref()
			.unbox_any(targetType)
			.stloc0()
			.ldloca(0)
			.ldarg1()
			.unbox_any(memberType)
			.perform(set, member)
			.ldarg0()
			.ldloc0()
			.box(targetType)
			.stind_ref()
			.ret()
			;
		}
	}
}