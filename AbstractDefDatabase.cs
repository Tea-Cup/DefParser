using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DefParser.Defs;

namespace DefParser {
	public static class AbstractDefDatabase<T> where T : Def {
		public static string DefClassName => typeof(T).Name;
		private static readonly Dictionary<string, T> items = new(CaseInsensitiveComparer.Instance);
		public static IEnumerable<T> All => items.Values;

		static AbstractDefDatabase() {
			AbstractDefDatabase.All(typeof(T));
		}

		public static T Get(string id) {
			return items[id];
		}
		public static bool TryGet(string id, [MaybeNullWhen(false)] out T def) {
			return items.TryGetValue(id, out def);
		}

		public static void Add(T def) {
			AddInternal(def);
			Logger.Log($"Registered <{def.ID}> as abstract {DefClassName}");
		}
		private static void AddInternal(T def) {
			if (items.ContainsKey(def.ID)) throw new DuplicateDefException($"<{def.ID}> is already registered as abstract");
			items[def.ID] = def;
			if (typeof(T) != typeof(Def)) AbstractDefDatabase.Add(typeof(T).BaseType!, def, false);
		}
	}

	public static class AbstractDefDatabase {
		private static readonly Dictionary<Type, Type> dbs = new();

		private static Type GetDatabase(Type type) {
			if (dbs.TryGetValue(type, out var db)) return db;
			if (!type.IsAssignableTo(typeof(Def))) throw new ArgumentException($"Type {type.FullName} is not assignable to type Def");
			return dbs[type] = typeof(AbstractDefDatabase<>).MakeGenericType(type);
		}
		private static PropertyInfo GetProperty(Type type, string name) {
			return GetDatabase(type).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
		}
		private static MethodInfo GetMethod(Type type, string name) {
			return GetDatabase(type).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
		}

		public static IEnumerable<Def> All(Type type) {
			IEnumerable all = (IEnumerable)GetProperty(type, nameof(AbstractDefDatabase<Def>.All)).GetValue(null)!;
			return all.Cast<Def>();
		}

		public static Def Get(Type type, string id) {
			return (Def)GetMethod(type, nameof(AbstractDefDatabase<Def>.Add)).Invoke(null, new[] { id })!;
		}

		public static bool TryGet(Type type, string id, [MaybeNullWhen(false)] out Def def) {
			object?[] param = new[] { id, null };
			bool result = (bool)GetMethod(type, nameof(AbstractDefDatabase<Def>.Add)).Invoke(null, param)!;
			def = (Def)param[1]!;
			return result;
		}

		public static void Add(Type type, Def def, bool log = true) {
			if (log) GetMethod(type, nameof(AbstractDefDatabase<Def>.Add)).Invoke(null, new[] { def });
			else GetMethod(type, "AddInternal").Invoke(null, new[] { def });
		}
	}
}
