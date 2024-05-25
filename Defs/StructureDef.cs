namespace DefParser.Defs {
	public class StructureDef : BuildingDef {
		public Vision Vision { get; init; } = new();

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("vision", Vision)
			);
		}
	}
}
