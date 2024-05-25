using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace DefParser {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class InitializerAttribute<T> : Attribute where T : TypeInitializer {
		public static Type Type => typeof(T);
	}

	public abstract class TypeInitializer {
		private static readonly Dictionary<Type, TypeInitializer?> cache = new();
		private static Type? GetInitializerType(Type type) {
			foreach (Attribute attr in type.GetCustomAttributes()) {
				Type at = attr.GetType();
				if (!at.IsConstructedGenericType) continue;
				if (at.GetGenericTypeDefinition() != typeof(InitializerAttribute<>)) continue;
				return at.GenericTypeArguments[0];
			}
			return null;
		}
		public static TypeInitializer? GetFor(Type type) {
			if(cache.TryGetValue(type, out var initializer)) return initializer;
			Type? initType = GetInitializerType(type);
			if (initType is null) {
				cache.Add(type, null);
				return null;
			}
			initializer = (TypeInitializer?)Activator.CreateInstance(initType)
				?? throw new MissingMethodException($"Failed to create instance of TypeInitializer: {initType.FullName}");
			cache.Add(type, initializer);
			return initializer;
		}

		public abstract object Create(Type type, string name, XmlElement root);
	}
}
