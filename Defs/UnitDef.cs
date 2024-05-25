namespace DefParser.Defs {
	public class UnitDef : ArmoredDef {
		public float Speed { get; init; } = 0;
		public float Range { get; init; } = 0;
		public float Rate { get; init; } = 0;
		public TargetKind TargetKind { get; init; } = TargetKind.Ground | TargetKind.Building;
		public UnitKind UnitKind { get; init; } = UnitKind.Ground;
		public Vision Vision { get; init; } = new();
		public ProjectileDef Projectile { get; init; } = null!;

		public override string? Validate() {
			if (Projectile is null) return "no projectile";
			return base.Validate();
		}

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("speed", Speed),
				("range", Range),
				("rate", Rate),
				("targetKind", TargetKind),
				("unitKind", UnitKind),
				("vision", Vision),
				("projectile", Projectile)
			);
		}
	}
}
