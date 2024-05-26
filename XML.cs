using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;

namespace DefParser {
	/// <summary>Set of XML extensions.</summary>
	public static class XML {
		/// <summary>Attempt to retrieve a child element of specified name.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <param name="name">Name of an element to find.</param>
		/// <param name="element">Found element or <see langword="null"/> if none found.</param>
		/// <returns><see langword="true"/> if element is found; <see langword="false"/> otherwise.</returns>
		public static bool TryGetElement(this XmlElement root, string name, [MaybeNullWhen(false)] out XmlElement element) {
			element = root[name];
			return element is not null;
		}

		/// <summary>Retrieve all <see cref="XmlElement"/> children.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <returns>All <see cref="XmlElement"/> items from <paramref name="root"/> child nodes collection.</returns>
		public static IEnumerable<XmlElement> GetChildren(this XmlElement root) {
			return root.ChildNodes.OfType<XmlElement>();
		}

		/// <summary>Get the value of an attribute.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <param name="name">Case-insensitive name of an attribute.</param>
		/// <returns>Attribute value or <see langword="null"/> if none found.</returns>
		public static string? FindAttribute(this XmlElement root, string name) {
			return root.Attributes.OfType<XmlAttribute>().FirstOrDefault(x => CaseInsensitiveComparer.IsEqual(x.Name, name))?.Value;
		}

		/// <summary>Get the value of an attribute parsed as <see cref="bool"/>.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <param name="name">Case-insensitive name of an attribute.</param>
		/// <returns>Attribute value parsed as <see cref="bool"/> or <see langword="null"/> if none found.</returns>
		public static bool? GetBoolAttribute(this XmlElement root, string name) {
			string? value = root.FindAttribute(name)?.ToLower();
			if (value is null) return null;
			if (value == "false") return false;
			if (value != "true") Logger.Error($"Expected a boolean value for attribute {root.Name}@{name}, got \"{value}\" instead. Assuming \"true\".");
			return true;
		}

		/// <summary>Retrieve child element with specified name.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <param name="name">Case-insensitive name of an element.</param>
		/// <returns>XML element or <see langword="null"/> if none found.</returns>
		public static XmlElement? FindChild(this XmlElement root, string name) {
			return root.GetChildren().FirstOrDefault(x => CaseInsensitiveComparer.IsEqual(x.Name, name));
		}

		/// <summary>Retrieve <see cref="XmlElement.InnerText"/> or <see cref="XmlElement.InnerXml"/> if element has child elements.</summary>
		/// <param name="root">XML element to look in.</param>
		/// <param name="name">Name of property for logging.</param>
		/// <param name="type">Type of property for logging.</param>
		/// <returns>Either <see cref="XmlElement.InnerText"/> or <see cref="XmlElement.InnerXml"/>.</returns>
		public static string GetStrictText(this XmlElement xml, string name, string type) {
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"{type}\" type but has children elements. Assuming CDATA block.");
				return xml.InnerXml;
			} else {
				return xml.InnerText;
			}
		}
	}
}
