namespace DefParser.Defs {
	public class ArmoredDef : EntityDef {
		public int Armor { get; init; }
		public ArmorType ArmorType { get; init; }

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("armor", Armor),
				("armorType", ArmorType)
			);
		}
	}
}
