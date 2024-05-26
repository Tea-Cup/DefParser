using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using DefParser.Defs;

namespace DefParser {
	/// <summary>Set of helper methods for reflection things.</summary>
	public static class Ref {
		/// <summary>Retrieve a property setter or throw <see cref="InvalidProgramException"/> if none found.</summary>
		/// <param name="type">Type to search for property in.</param>
		/// <param name="name">Name of target property.</param>
		/// <returns>Reference to a setter method of a property.</returns>
		/// <exception cref="InvalidProgramException">Setter for property was not found.</exception>
		public static MethodInfo GetPropertySetter(Type type, string name) {
			return type.GetProperty(name)?.SetMethod
				?? throw new InvalidProgramException($"No {name} setter in {type.LocalName()}");
		}

		// Internal method for use in public FindType
		private static Type? FindType(string typename, bool ignoreCase) {
			Type? type = Type.GetType(typename, false, ignoreCase);
			if (type != null) return type;
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				type = asm.GetType(typename, false, ignoreCase);
				if (type != null) return type;
			}
			return null;
		}

		/// <summary>Search for a type with specified name in the entire AppDomain.</summary>
		/// <param name="typename">Full Name of a type to search for.</param>
		/// <returns></returns>
		public static Type? FindType(string typename) {
			Type? type = FindType(typename, false);
			if (type != null) return type;
			type = FindType(typename, true);
			if (type != null) {
				// This is just for discipline
				Logger.Error($"Type name \"{typename}\" found, but actual name is \"{type.FullName}\". Please respect type name casing.");
				return type;
			}
			return null;
		}

		/// <summary>Perform explicit casting of object to specified type.</summary>
		/// <param name="obj">Object to cast.</param>
		/// <param name="type">Type to cast to.</param>
		/// <returns><paramref name="obj"/> explicitly casted into <paramref name="type"/>.</returns>
		public static object? Cast(object? obj, Type type) {
			if (obj is null) return null;
			if (obj.GetType() == type) return obj;
			// Create a dynamic lambda with explicit casting to force the use of explicit cast operator if any.
			var param = Expression.Parameter(typeof(object), "value");
			var body = Expression.Block(Expression.Convert(Expression.Convert(param, obj.GetType()), type));
			var exp = Expression.Lambda(body, param).Compile();
			return exp.DynamicInvoke(obj);
		}

		/// <summary>Create instance of an object of specified type. Parameterless constructor is used.</summary>
		/// <param name="type">Type to create instance of.</param>
		/// <returns>Instance of <paramref name="type"/>.</returns>
		/// <exception cref="InvalidProgramException">If method is triggered for <see cref="Nullable{T}"/> type.</exception>
		public static object Construct(Type type) {
			return Activator.CreateInstance(type) ?? throw new InvalidProgramException($"Failed to create instance of type: {type.FullName}");
		}

		private static readonly MethodInfo setDefID = GetPropertySetter(typeof(Def), nameof(Def.ID));
		/// <summary>Set value of a <see cref="Def.ID"/>.</summary>
		/// <param name="def">Def to set ID on.</param>
		/// <param name="id">New value for def ID.</param>
		public static void SetDefID(object def, string id) {
			setDefID.Invoke(def, new object[] { id });
		}

		private static readonly MethodInfo setDefAbstract = GetPropertySetter(typeof(Def), nameof(Def.Abstract));
		/// <summary>Set value of a <see cref="Def.Abstract"/>.</summary>
		/// <param name="def">Def to set abstract flag on.</param>
		/// <param name="abst">New value for def abstract flag.</param>
		public static void SetDefAbstract(object def, bool abst) {
			setDefAbstract.Invoke(def, new object[] { abst });
		}

		/// <summary>Invoke a <see cref="IDictionary.Add(object, object?)"/> method on object.</summary>
		/// <param name="dict">Dictionary object.</param>
		/// <param name="key">Key to pass to method.</param>
		/// <param name="value">Value to pass to method.</param>
		public static void AddToDict(object dict, object key, object? value) {
			// This used to be bigger, but explicit casting turned out much easier than reflection.
			((IDictionary)dict).Add(key, value);
		}
	}
}
