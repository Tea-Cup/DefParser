using System;
using System.Collections;
using System.Drawing;
using System.Text;

namespace DefParser.Defs {
	public abstract class Def {
		public static bool MultilineToString { get; set; } = false;
		[Ignore] public bool Abstract { get; init; } = false;
		[Ignore] public string ID { get; init; } = "<no id>";
		public string Name { get; init; } = "";
		public string Description { get; init; } = "";
		public Image? Icon { get; init; }
		public string[] Tags { get; init; } = Array.Empty<string>();

		public virtual string? Validate() { return null; }

		public override string ToString() {
			return BuildToString(
				$"{GetType().LocalName()} {ID};",
				("name", Name),
				("description", Description),
				("icon", Icon),
				("tags", Tags)
			);
		}

		protected static string BuildToString(string baseString, params (string name, object? value)[] values) {
			StringBuilder sb = new();
			sb.Append(baseString);
			foreach (var (name, value) in values) {
				sb.Append(MultilineToString ? '\n' : ' ');
				if (MultilineToString) sb.Append("    ");
				sb.Append(name).Append('=').Append(ToRefString(value, !MultilineToString)).Append(';');
			}
			return sb.ToString();

			static string ToRefString(object? value, bool shorten) => value switch {
				null => "null",
				Def def => $"{{{def.GetType().LocalName()} {def.ID}}}",
				Image bmp => $"{{Bitmap{(Dummy.IsDummyImage(bmp) ? " (Dummy)" : "")} \"{bmp.GetImageFilename()}\"}}",
				Type type => $"{{class {type.LocalName()}}}",
				string s => $"\"{(shorten ? s.Ellipsis(25) : s)}\"",
				_ when value.GetType().IsArray => EnumerableToString((Array)value, shorten),
				_ when value.GetType().IsAssignableTo(typeof(IEnumerable)) => EnumerableToString((IEnumerable)value, shorten),
				_ => value.ToString() ?? $"{{{value.GetType().LocalName()}}}"
			};
			static string EnumerableToString(IEnumerable en, bool shorten) {
				IEnumerator e = en.GetEnumerator();
				StringBuilder sb = new();
				bool first = true;
				int count = 0;
				while (e.MoveNext() && (!shorten || sb.Length < 25)) {
					if (!first) sb.Append(',');
					first = false;
					sb.Append(ToRefString(e.Current, shorten));
					count++;
				}
				while (e.MoveNext()) count++;
				return $"({count}) [{sb}]";
			}
		}
	}
}
