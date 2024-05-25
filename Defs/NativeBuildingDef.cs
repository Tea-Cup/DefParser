namespace DefParser.Defs {
	public class NativeBuildingDef : BuildingDef {
		public ResourceBundle Storage { get; init; } = new();

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("storage", Storage)
			);
		}
	}
}
