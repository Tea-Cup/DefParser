namespace DefParser.Defs {
	public class BuildingDef : ArmoredDef {
		public ResourceBundle Maintenance { get; init; } = new();
		public TileDef? Foundation { get; init; }

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("maintenance", Maintenance),
				("foundation", Foundation)
			);
		}
	}
}
