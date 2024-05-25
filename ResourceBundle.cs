using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DefParser {
	[Initializer<ResourceBundleInitializer>]
	public record ResourceBundle(
		int? Energy = null,
		int? Metal = null
	) {
		public override string ToString() {
			List<string> strings = new();
			if (Energy.HasValue) strings.Add($"energy={Energy}");
			if (Metal.HasValue) strings.Add($"metal={Metal}");
			string contents = strings.Any() ? string.Join("; ", strings) : "empty";
			return $"{{ResourceBundle {contents}}}";
		}
	}

	public class ResourceBundleInitializer : TypeInitializer {
		public override object Create(Type type, string name, XmlElement root) {
			if (type != typeof(ResourceBundle)) throw new InvalidOperationException("This initializer is only used for ResourceBundle type");
			int? energy = root.HasAttribute("energy") ? int.Parse(root.GetAttribute("energy")) : null;
			int? metal = root.HasAttribute("metal") ? int.Parse(root.GetAttribute("metal")) : null;
			return new ResourceBundle(energy, metal);
		}
	}
}

