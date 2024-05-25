using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DefParser {
	public class CaseInsensitiveComparer : IEqualityComparer<string> {
		public static CaseInsensitiveComparer Instance { get; } = new();

		public static bool IsEqual(string? x, string? y) {
			return string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool Equals(string? x, string? y) {
			return IsEqual(x, y);
		}

		public int GetHashCode([DisallowNull] string obj) {
			return obj.ToUpperInvariant().GetHashCode();
		}
	}
}
