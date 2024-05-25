using System;

namespace DefParser.Defs {
	public class EffectDef : Def {
		public Type WorkerClass { get; init; } = null!;

		public override string? Validate() {
			if (WorkerClass is null) return "no worker class";
			return base.Validate();
		}

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("workerClass", WorkerClass)
			);
		}
	}
}
