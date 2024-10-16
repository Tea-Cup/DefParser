namespace DefParser.Defs {
	public class FactoryDef : StructureDef {
		public int Range { get; init; } = 0;
		public int SpawnRate { get; init; } = 0;
		public UnitDef Unit { get; init; } = null!;

		public override string? Validate() {
			if (Unit is null) return "no unit";
			return base.Validate();
		}

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("range", Range),
				("spawnrate", SpawnRate),
				("unit", Unit)
			);
		}
	}
}
