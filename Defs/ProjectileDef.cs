namespace DefParser.Defs {
	public class ProjectileDef : EntityDef {
		public DamageType DamageType { get; init; } = DamageType.Bullet;
		public bool IgnoreTerrain { get; init; } = false;
		public float? Speed { get; init; }
		public TileDef? HitTile { get; init; }

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("damageType", DamageType),
				("ignoreTerrain", IgnoreTerrain),
				("speed", Speed),
				("hitTile", HitTile)
			);
		}
	}
}
