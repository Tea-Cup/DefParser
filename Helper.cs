using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using DefParser.Defs;

namespace DefParser {
	/// <summary>Set of general extensions.</summary>
	public static class Helper {
		/// <summary>Truncate and add an ellipsis (...) if the string is longer that <paramref name="length"/>.</summary>
		/// <param name="str">String to operate on.</param>
		/// <param name="length">Maximum allowed length of a string without ellipsis length (3).</param>
		/// <returns>Either an original string, or a string truncated to <paramref name="length"/> with an ellipsis.</returns>
		public static string Ellipsis(this string str, int length) {
			if (str.Length >= length + 3) return str[..length] + "...";
			return str;
		}

		/// <summary>Attempts to shorten the <paramref name="type"/> name if it's in <see cref="Def"/> namespace.</summary>
		/// <param name="type">Type to shorten the full name of.</param>
		/// <returns><see cref="Type.FullName"/> of <paramref name="type"/> with <see cref="Def"/> namespace cut off if any.</returns>
		public static string LocalName(this Type type) {
			string typename = type.FullName ?? type.Name;
			if (type.Namespace == typeof(Def).Namespace) return typename[(typeof(Def).Namespace!.Length + 1)..];
			return typename;
		}

		private const int PROPERTY_TAG_IMAGE_TITLE = 0x0320;
		private const int PROPERTY_TAG_NEW_SUBFILE_TYPE = 0x00FE;
		private const short PROPERTY_TAG_TYPE_ASCII = 2; // Really just a null-terminated set of bytes
		private const short PROPERTY_TAG_TYPE_LONG = 4; // long = int32 in c++ world

		// PropertyItem constructor is marked as internal, so you can't access it without reflection
		private static PropertyItem CreatePropertyItem(int id, short type, int len, byte[] value) {
			PropertyItem prop = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true)!;
			prop.Id = id;
			prop.Type = type;
			prop.Len = len;
			prop.Value = value;
			return prop;
		}

		/// <summary>Get <paramref name="image"/> title property string.</summary>
		/// <param name="image">Image to get title from.</param>
		/// <returns><paramref name="image"/> title property string, or an empty string if none found.</returns>
		public static string GetImageFilename(this Image image) {
			try {
				PropertyItem? prop = image.GetPropertyItem(PROPERTY_TAG_IMAGE_TITLE);
				if (prop == null) return string.Empty;
				return Encoding.UTF8.GetString(prop.Value!);
			} catch (ArgumentException) {
				return string.Empty;
			}
		}

		/// <summary>Set <paramref name="image"/> title property string.</summary>
		/// <param name="image">Image to set title on.</param>
		/// <param name="filename">New title for <paramref name="image"/>.</param>
		public static void SetImageFilename(this Image image, string filename) {
			byte[] data = Encoding.UTF8.GetBytes(filename);
			image.SetPropertyItem(CreatePropertyItem(PROPERTY_TAG_IMAGE_TITLE, PROPERTY_TAG_TYPE_ASCII, data.Length, data));
		}

		/// <summary>Get <paramref name="image"/> subfile type property number.</summary>
		/// <param name="image">Image to get number from.</param>
		/// <returns><paramref name="image"/> subfile type property number, or 0 if none found.</returns>
		public static uint GetTypeTag(this Image image) {
			try {
				PropertyItem? prop = image.GetPropertyItem(PROPERTY_TAG_NEW_SUBFILE_TYPE);
				if (prop == null) return 0;
				return BitConverter.ToUInt32(prop.Value!);
			} catch (ArgumentException) {
				return 0;
			}
		}

		/// <summary>Set <paramref name="image"/> subfile type property number.</summary>
		/// <param name="image">Image to set number on.</param>
		/// <param name="filename">New subfile type for <paramref name="image"/>.</param>
		public static void SetTypeTag(this Image image, uint tag) {
			image.SetPropertyItem(CreatePropertyItem(PROPERTY_TAG_NEW_SUBFILE_TYPE, PROPERTY_TAG_TYPE_LONG, 4, BitConverter.GetBytes(tag)));
		}
	}
}
