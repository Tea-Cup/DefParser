namespace DefParser {
	[System.Flags]
	public enum TargetKind {
		Ground = 0b00001,
		Air = 0b00010,
		Underground = 0b00100,
		Building = 0b01000,
		Tile = 0b10000
	}
}
