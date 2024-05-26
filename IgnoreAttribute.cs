using System;

namespace DefParser {
	/// <summary>Attribute used to remove property or class from <see cref="Parser"/> scanners.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class IgnoreAttribute : Attribute { }
}
