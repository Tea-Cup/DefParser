using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DefParser.Defs;

namespace DefParser {
	/// <summary>Type-specific database class to store all parsed and finalized defs.</summary>
	/// <typeparam name="T">Specific type of <see cref="Def"/> this database stores.</typeparam>
	public static class DefDatabase<T> where T : Def {
		private static readonly Dictionary<string, T> items = new(CaseInsensitiveComparer.Instance);
		/// <summary>Local name of this database items.</summary>
		public static string DefClassName => typeof(T).LocalName();
		/// <summary>Enumeration of all items in this database.</summary>
		public static IEnumerable<T> All => items.Values;

		static DefDatabase() {
			// Trigger typed db registration in general class for caching
			DefDatabase.All(typeof(T));
		}

		/// <summary>
		/// <para>Retrieve stored def identified by <paramref name="id"/>.</para>
		/// <para>Throws <see cref="KeyNotFoundException"/> if not found.</para>
		/// </summary>
		/// <param name="id">Identifier of the def to retrieve.</param>
		/// <exception cref="KeyNotFoundException">Def identified by <paramref name="id"/> is not found in this database.</exception>
		/// <returns>Def identified by <paramref name="id"/>.</returns>
		public static T Get(string id) {
			return items[id];
		}

		/// <summary>Attempt to retrieve stored def identified by <paramref name="id"/> without throwing exceptions.</summary>
		/// <param name="id">Identifier of the def to retrieve.</param>
		/// <param name="def">Def identified by <paramref name="id"/> or <see langword="null"/> if not found.</param>
		/// <returns><see langword="true"/> if def is found; <see langword="false"/> otherwise.</returns>
		public static bool TryGet(string id, [MaybeNullWhen(false)] out T def) {
			return items.TryGetValue(id, out def);
		}

		/// <summary>
		/// <para>Register a new def to this database.</para>
		/// <para>This also registers def in every database of it's ancestors.</para>
		/// <para>Throws <see cref="DuplicateDefException"/> if def is already registered.</para>
		/// </summary>
		/// <param name="def">Def to register.</param>
		public static void Add(T def) {
			AddInternal(def);
			Logger.Log($"Registered <{def.ID}> as {DefClassName}");
		}

		// Register def, but without logging
		private static void AddInternal(T def) {
			if (items.ContainsKey(def.ID)) throw new DuplicateDefException($"<{def.ID}> is already registered");
			items[def.ID] = def;
			// Keep ascending the inheritance chain until Def
			if (typeof(T) != typeof(Def)) DefDatabase.Add(typeof(T).BaseType!, def, false);
		}
	}

	/// <summary>
	/// <para>Type-agnostic database class to store all parsed and finalized defs.</para>
	/// <para>Used by <see cref="Parser"/> for reflection-level access to <see cref="DefDatabase{T}"/>.</para>
	/// </summary>
	public static class DefDatabase {
		private static readonly Dictionary<Type, Type> dbs = new();

		// Retrieve from cache or encache and return a constructed generic database type
		private static Type GetDatabase(Type type) {
			if (dbs.TryGetValue(type, out var db)) return db;
			if (!type.IsAssignableTo(typeof(Def))) throw new ArgumentException($"Type {type.FullName} is not assignable to type Def");
			return dbs[type] = typeof(DefDatabase<>).MakeGenericType(type);
		}
		// Retrieve a property from database of specified type
		private static PropertyInfo GetProperty(Type type, string name) {
			return GetDatabase(type).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
		}
		// Retrieve a method from database of specified type
		private static MethodInfo GetMethod(Type type, string name) {
			return GetDatabase(type).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
		}

		/// <summary>Enumeration of all items in <see cref="DefDatabase{T}"/> of <paramref name="type"/>.</summary>
		/// <seealso cref="DefDatabase{T}.All"/>
		/// <param name="type">Type of <see cref="DefDatabase{T}"/> to access.</param>
		public static IEnumerable<Def> All(Type type) {
			IEnumerable all = (IEnumerable)GetProperty(type, nameof(DefDatabase<Def>.All)).GetValue(null)!;
			return all.Cast<Def>();
		}

		/// <summary>
		/// <para>Retrieve stored def identified by <paramref name="id"/> from <see cref="DefDatabase{T}"/> of <paramref name="type"/>.</para>
		/// <para>Throws <see cref="KeyNotFoundException"/> if not found.</para>
		/// </summary>
		/// <seealso cref="DefDatabase{T}.Get(string)"/>
		/// <param name="type">Type of <see cref="DefDatabase{T}"/> to access.</param>
		/// <param name="id">Identifier of the def to retrieve.</param>
		/// <exception cref="KeyNotFoundException">Def identified by <paramref name="id"/> is not found in this database.</exception>
		/// <returns>Def identified by <paramref name="id"/>.</returns>
		public static Def Get(Type type, string id) {
			return (Def)GetMethod(type, nameof(DefDatabase<Def>.Add)).Invoke(null, new[] { id })!;
		}

		/// <summary>
		/// Attempt to retrieve stored def identified by <paramref name="id"/>
		/// from <see cref="DefDatabase{T}"/> of <paramref name="type"/> without throwing exceptions.
		/// </summary>
		/// <seealso cref="DefDatabase{T}.TryGet(string, out T)"/>
		/// <param name="type">Type of <see cref="DefDatabase{T}"/> to access.</param>
		/// <param name="id">Identifier of the def to retrieve.</param>
		/// <param name="def">Def identified by <paramref name="id"/> or <see langword="null"/> if not found.</param>
		/// <returns><see langword="true"/> if def is found; <see langword="false"/> otherwise.</returns>
		public static bool TryGet(Type type, string id, [MaybeNullWhen(false)] out Def def) {
			object?[] param = new[] { id, null };
			bool result = (bool)GetMethod(type, nameof(DefDatabase<Def>.Add)).Invoke(null, param)!;
			def = (Def)param[1]!;
			return result;
		}

		/// <summary>
		/// <para>Register a new def to <see cref="DefDatabase{T}"/> of <paramref name="type"/>.</para>
		/// <para>This also registers def in every database of it's ancestors.</para>
		/// <para>Throws <see cref="DuplicateDefException"/> if def is already registered.</para>
		/// </summary>
		/// <seealso cref="DefDatabase{T}.Add(T)"/>
		/// <param name="type">Type of <see cref="DefDatabase{T}"/> to access.</param>
		/// <param name="def">Def to register.</param>
		/// <param name="log">If <see langword="true"/>, log message will be printed.</param>
		public static void Add(Type type, Def def, bool log = true) {
			if(log) GetMethod(type, nameof(DefDatabase<Def>.Add)).Invoke(null, new[] { def });
			else GetMethod(type, "AddInternal").Invoke(null, new[] { def });
		}
	}
}
