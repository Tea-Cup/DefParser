using System.Drawing;

namespace DefParser.Defs {
	public class EntityDef : Def {
		public Image? Texture { get; init; }
		public int? Health { get; init; }
		public Vector2? Size { get; init; }

		public override string ToString() {
			return BuildToString(
				base.ToString(),
				("texture", Texture),
				("health", Health),
				("size", Size)
			);
		}
	}
}
