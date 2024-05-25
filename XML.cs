using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace DefParser {
	public static class XML {
		public static bool EqualsInCase(this string s, string str) {
			return s.Equals(str, System.StringComparison.InvariantCultureIgnoreCase);
		}
		public static bool TryGetElement(this XmlElement root, string name, [MaybeNullWhen(false)] out XmlElement element) {
			element = root[name];
			return element is not null;
		}
		public static IEnumerable<XmlElement> GetChildren(this XmlElement root) {
			return root.ChildNodes.OfType<XmlElement>();
		}
		public static string? FindAttribute(this XmlElement root, string name) {
			return root.Attributes.OfType<XmlAttribute>().FirstOrDefault(x => x.Name.EqualsInCase(name))?.Value;
		}
		public static bool? GetBoolAttribute(this XmlElement root, string name) {
			string? value = root.FindAttribute(name)?.ToLower();
			if (value is null) return null;
			if (value == "false") return false;
			if (value != "true") Logger.Error($"Expected a boolean value for attribute {root.Name}@{name}, got \"{value}\" instead. Assuming \"true\".");
			return true;
		}

		public static XmlElement? FindChild(this XmlElement root, string name) {
			return root.GetChildren().FirstOrDefault(x => x.Name.EqualsInCase(name));
		}
		public static (string name, XmlElement? element)[] FindChildren(this XmlElement root, params string[] names) {
			var result = new (string, XmlElement?)[names.Length];
			for(int i = 0; i < names.Length; ++i) {
				result[i] = (names[i], root.FindChild(names[i]));
			}
			return result;
		}

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
