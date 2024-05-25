using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DefParser {
	[Initializer<VisionInitializer>]
	public record Vision(
		int? Optical = null,
		int? Digital = null
	) {
		public override string ToString() {
			List<string> strings = new();
			if (Optical.HasValue) strings.Add($"optical={Optical}");
			if (Digital.HasValue) strings.Add($"digital={Digital}");
			string contents = strings.Any() ? string.Join("; ", strings) : "none";
			return $"{{Vision {contents}}}";
		}
	}

	public class VisionInitializer : TypeInitializer {
		public override object Create(Type type, string name, XmlElement root) {
			if (type != typeof(Vision)) throw new InvalidOperationException("This initializer is only used for Vision type");
			int? optical = root.HasAttribute("optical") ? int.Parse(root.GetAttribute("optical")) : null;
			int? digital = root.HasAttribute("digital") ? int.Parse(root.GetAttribute("digital")) : null;
			return new Vision(optical, digital);
		}
	}
}
