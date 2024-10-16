namespace DefParser {
	[System.Flags]
	public enum TargetKind {
		Ground = 0b000001,
		Air = 0b000010,
		Underground = 0b000100,
		Building = 0b001000,
		Tile = 0b010000,
		Operator = 0b100000,
	}
}
