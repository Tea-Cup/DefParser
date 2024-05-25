using System.Drawing;

namespace DefParser {
	public record Dummy(string RealName) {
		public static Image Image() {
			Image img = new Bitmap(1, 1);
			img.SetTypeTag(0xFEEFFEEF);
			return img;
		}
		public static bool IsDummyImage(Image img) {
			return img.GetTypeTag() == 0xFEEFFEEF;
		}
		public override string ToString() {
			return $"{{Dummy {RealName}}}";
		}
	}
}
