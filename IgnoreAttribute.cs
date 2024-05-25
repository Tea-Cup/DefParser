using System;

namespace DefParser {
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class IgnoreAttribute : Attribute { }
}
