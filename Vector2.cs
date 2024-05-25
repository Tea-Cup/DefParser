using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace DefParser {
	[Initializer<Vector2Initializer>]
	public record Vector2(float X, float Y) {
		public override string ToString() {
			return $"{{Vector2 {X}, {Y}}}";
		}
	}

	public partial class Vector2Initializer : TypeInitializer {
		[GeneratedRegex("\\((-?\\d+(?:\\.\\d+)?), *(-?\\d+(?:\\.\\d+)?)\\)", RegexOptions.Compiled)]
		private static partial Regex TextRegex();
		private static readonly Regex rx = TextRegex();

		public override object Create(Type type, string name, XmlElement root) {
			if (type != typeof(Vector2)) throw new InvalidOperationException("This initializer is only used for Vector2 type");
			if (root["x"] is null && root["y"] is null) {
				Match m = rx.Match(root.InnerText);
				if (!m.Success) throw new FormatException($"Invalid Vector2: \"{root.InnerText}\"");
				return new Vector2(float.Parse(m.Groups[1].Value), float.Parse(m.Groups[2].Value));
			}
			return new Vector2(
				float.Parse(root["x"]?.InnerText ?? "0"),
				float.Parse(root["y"]?.InnerText ?? "0")
			);
		}
	}
}
