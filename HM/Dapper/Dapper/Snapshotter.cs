using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HM.Framework.Dapper
{
	public static class Snapshotter
	{
		public class Snapshot<T>
		{
			public class Change
			{
				public string Name
				{
					get;
					set;
				}

				public object NewValue
				{
					get;
					set;
				}
			}

			private static Func<T, T> cloner;

			private static Func<T, T, List<Change>> differ;

			private readonly T memberWiseClone;

			private readonly T trackedObject;

			public Snapshot(T original)
			{
				memberWiseClone = Clone(original);
				trackedObject = original;
			}

			public DynamicParameters Diff()
			{
				return Diff(memberWiseClone, trackedObject);
			}

			private static T Clone(T myObject)
			{
				cloner = (cloner ?? GenerateCloner());
				return cloner(myObject);
			}

			private static DynamicParameters Diff(T original, T current)
			{
				DynamicParameters dynamicParameters = new DynamicParameters();
				differ = (differ ?? GenerateDiffer());
				foreach (Change item in differ(original, current))
				{
					dynamicParameters.Add(item.Name, item.NewValue);
				}
				return dynamicParameters;
			}

			private static List<PropertyInfo> RelevantProperties()
			{
				return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate(PropertyInfo p)
				{
					if (p.GetSetMethod(nonPublic: true) != null && p.GetGetMethod(nonPublic: true) != null)
					{
						if (!(p.PropertyType == typeof(string)) && !p.PropertyType.IsValueType())
						{
							if (p.PropertyType.IsGenericType())
							{
								return p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
							}
							return false;
						}
						return true;
					}
					return false;
				}).ToList();
			}

			private static bool AreEqual<U>(U first, U second)
			{
				if (EqualityComparer<U>.Default.Equals(first, default(U)) && EqualityComparer<U>.Default.Equals(second, default(U)))
				{
					return true;
				}
				if (EqualityComparer<U>.Default.Equals(first, default(U)))
				{
					return false;
				}
				return first.Equals(second);
			}

			private static Func<T, T, List<Change>> GenerateDiffer()
			{
				DynamicMethod dynamicMethod = new DynamicMethod("DoDiff", typeof(List<Change>), new Type[2]
				{
					typeof(T),
					typeof(T)
				}, restrictedSkipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.DeclareLocal(typeof(List<Change>));
				iLGenerator.DeclareLocal(typeof(Change));
				iLGenerator.DeclareLocal(typeof(object));
				iLGenerator.Emit(OpCodes.Newobj, typeof(List<Change>).GetConstructor(Type.EmptyTypes));
				iLGenerator.Emit(OpCodes.Stloc_0);
				foreach (PropertyInfo item in RelevantProperties())
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Callvirt, item.GetGetMethod(nonPublic: true));
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Callvirt, item.GetGetMethod(nonPublic: true));
					iLGenerator.Emit(OpCodes.Dup);
					if (item.PropertyType != typeof(string))
					{
						iLGenerator.Emit(OpCodes.Box, item.PropertyType);
					}
					iLGenerator.Emit(OpCodes.Stloc_2);
					iLGenerator.EmitCall(OpCodes.Call, typeof(Snapshot<T>).GetMethod("AreEqual", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(item.PropertyType), null);
					Label label = iLGenerator.DefineLabel();
					iLGenerator.Emit(OpCodes.Brtrue_S, label);
					iLGenerator.Emit(OpCodes.Newobj, typeof(Change).GetConstructor(Type.EmptyTypes));
					iLGenerator.Emit(OpCodes.Dup);
					iLGenerator.Emit(OpCodes.Stloc_1);
					iLGenerator.Emit(OpCodes.Ldstr, item.Name);
					iLGenerator.Emit(OpCodes.Callvirt, typeof(Change).GetMethod("set_Name"));
					iLGenerator.Emit(OpCodes.Ldloc_1);
					iLGenerator.Emit(OpCodes.Ldloc_2);
					iLGenerator.Emit(OpCodes.Callvirt, typeof(Change).GetMethod("set_NewValue"));
					iLGenerator.Emit(OpCodes.Ldloc_0);
					iLGenerator.Emit(OpCodes.Ldloc_1);
					iLGenerator.Emit(OpCodes.Callvirt, typeof(List<Change>).GetMethod("Add"));
					iLGenerator.MarkLabel(label);
				}
				iLGenerator.Emit(OpCodes.Ldloc_0);
				iLGenerator.Emit(OpCodes.Ret);
				return (Func<T, T, List<Change>>)dynamicMethod.CreateDelegate(typeof(Func<T, T, List<Change>>));
			}

			private static Func<T, T> GenerateCloner()
			{
				DynamicMethod dynamicMethod = new DynamicMethod("DoClone", typeof(T), new Type[1]
				{
					typeof(T)
				}, restrictedSkipVisibility: true);
				ConstructorInfo constructor = typeof(T).GetConstructor(new Type[0]);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.DeclareLocal(typeof(T));
				iLGenerator.Emit(OpCodes.Newobj, constructor);
				iLGenerator.Emit(OpCodes.Stloc_0);
				foreach (PropertyInfo item in RelevantProperties())
				{
					iLGenerator.Emit(OpCodes.Ldloc_0);
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Callvirt, item.GetGetMethod(nonPublic: true));
					iLGenerator.Emit(OpCodes.Callvirt, item.GetSetMethod(nonPublic: true));
				}
				iLGenerator.Emit(OpCodes.Ldloc_0);
				iLGenerator.Emit(OpCodes.Ret);
				return (Func<T, T>)dynamicMethod.CreateDelegate(typeof(Func<T, T>));
			}
		}

		public static Snapshot<T> Start<T>(T obj)
		{
			return new Snapshot<T>(obj);
		}
	}
}
