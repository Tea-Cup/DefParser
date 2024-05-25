namespace DefParser.Defs {
	public class TowerDef : StructureDef {
		public float Range { get; init; } = 0;
		public float Rate { get; init; } = 0;
		public TargetKind TargetKind { get; init; } = TargetKind.Ground | TargetKind.Building;
		public ProjectileDef Projectile { get; init; } = null!;

		public override string? Validate() {
			if (Projectile is null) return "no projectile";
			return base.Validate();
		}

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("range", Range),
				("rate", Rate),
				("targetKind", TargetKind),
				("projectile", Projectile)
			);
		}
	}
}
