using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace DefParser {
	/// <summary>Attribute that represents a link between a class and it's <see cref="TypeInitializer"/>.</summary>
	/// <typeparam name="T"></typeparam>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class InitializerAttribute<T> : Attribute where T : TypeInitializer { }

	/// <summary>Class used for custom parsing of a class marked with <see cref="InitializerAttribute{T}"/>.</summary>
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

		/// <summary>Retrieve a cached <see cref="TypeInitializer"/> or encache one for <paramref name="type"/> class.</summary>
		/// <param name="type">Type of a class to retrieve initializer for.</param>
		/// <returns><see cref="TypeInitializer"/> linked to <paramref name="type"/>.</returns>
		/// <exception cref="MissingMethodException">Activator failed to create an instance of <see cref="TypeInitializer"/>.</exception>
		public static TypeInitializer? GetFor(Type type) {
			if(cache.TryGetValue(type, out var initializer)) return initializer;
			Type? initType = GetInitializerType(type);
			if (initType is null) {
				cache.Add(type, null);
				return null;
			}
			initializer = (TypeInitializer?)Activator.CreateInstance(initType)
				// Shouldn't happen really.
				?? throw new MissingMethodException($"Failed to create instance of TypeInitializer: {initType.FullName}");
			cache.Add(type, initializer);
			return initializer;
		}

		/// <summary>Parse XML element into an object of specified type.</summary>
		/// <param name="type">Type that is expected to be returned.</param>
		/// <param name="name">Name of property passed from <see cref="Parser"/>.</param>
		/// <param name="root">XML element that represents serialized property.</param>
		/// <returns>An object of <paramref name="type"/> that was parsed from <paramref name="root"/>.</returns>
		public abstract object Create(Type type, string name, XmlElement root);
	}
}
