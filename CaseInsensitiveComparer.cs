using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DefParser {
	/// <summary><see cref="IEqualityComparer{T}"/> for strings that uses <see cref="StringComparison.InvariantCultureIgnoreCase"/>.</summary>
	public class CaseInsensitiveComparer : IEqualityComparer<string> {
		public static CaseInsensitiveComparer Instance { get; } = new();

		/// <summary>Determines whether the specified objects are equal.</summary>
		/// <param name="x">The first string to compare.</param>
		/// <param name="y">The second string to compare.</param>
		/// <returns><see langword="true"/> if the specified strings are equal; otherwise, <see langword="false"/>.</returns>
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
