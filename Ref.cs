using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using DefParser.Defs;

namespace DefParser {
	public static class Ref {
		public static MethodInfo GetPropertySetter(Type type, string name) {
			return type.GetProperty(name)?.SetMethod
				?? throw new InvalidProgramException($"No {name} setter in {type.LocalName()}");
		}

		private static Type? FindType(string typename, bool ignoreCase) {
			Type? type = Type.GetType(typename, false, ignoreCase);
			if (type != null) return type;
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				type = asm.GetType(typename, false, ignoreCase);
				if (type != null) return type;
			}
			return null;
		}
		public static Type? FindType(string typename) {
			Type? type = FindType(typename, false);
			if (type != null) return type;
			type = FindType(typename, true);
			if (type != null) {
				Logger.Error($"Type name \"{typename}\" found, but actual name is \"{type.FullName}\". Please respect type name casing.");
				return type;
			}
			return null;
		}

		public static object? Cast(object? obj, Type type) {
			if (obj is null) return null;
			if (obj.GetType() == type) return obj;
			var param = Expression.Parameter(typeof(object), "value");
			var body = Expression.Block(Expression.Convert(Expression.Convert(param, obj.GetType()), type));
			var exp = Expression.Lambda(body, param).Compile();
			return exp.DynamicInvoke(obj);
		}

		public static object Construct(Type type) {
			return Activator.CreateInstance(type) ?? throw new InvalidProgramException($"Failed to create instance of type: {type.FullName}");
		}

		private static readonly MethodInfo setDefID = GetPropertySetter(typeof(Def), nameof(Def.ID));
		public static void SetDefID(object def, string id) {
			setDefID.Invoke(def, new object[] { id });
		}

		private static readonly MethodInfo setDefAbstract = GetPropertySetter(typeof(Def), nameof(Def.Abstract));
		public static void SetDefAbstract(object def, bool abst) {
			setDefAbstract.Invoke(def, new object[] { abst });
		}

		public static void AddToDict(object dict, object key, object? value) {
			((IDictionary)dict).Add(key, value);
		}
	}
}
