using System.Drawing;

namespace DefParser {
	/// <summary>Class for early-development to be used as an informative placeholder for stuff.</summary>
	/// <param name="RealName">String used to identify dummy instance.</param>
	public record Dummy(string RealName) {
		/// <summary>Construct a 1x1 <see cref="Bitmap"/> that is marked as a dummy image.</summary>
		public static Image Image() {
			Image img = new Bitmap(1, 1);
			img.SetTypeTag(0xFEEFFEEF);
			return img;
		}

		/// <summary>Check if <paramref name="img"/> is marked as a dummy image produced by <see cref="Image"/>.</summary>
		/// <param name="img">Image to check.</param>
		/// <returns><see langword="true"/> if <paramref name="img"/> is marked as a dummy image; <see langword="false"/> otherwise.</returns>
		public static bool IsDummyImage(Image img) {
			return img.GetTypeTag() == 0xFEEFFEEF;
		}

		public override string ToString() {
			return $"{{Dummy {RealName}}}";
		}
	}
}
