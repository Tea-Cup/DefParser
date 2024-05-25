using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using DefParser.Defs;

namespace DefParser {
	public static class Helper {
		public static string Ellipsis(this string str, int length) {
			if (str.Length >= length + 3) return str[..length] + "...";
			return str;
		}
		public static string LocalName(this Type type) {
			string typename = type.FullName ?? type.Name;
			if (type.Namespace == typeof(Def).Namespace) return typename[(typeof(Def).Namespace!.Length + 1)..];
			return typename;
		}

		private const int PROPERTY_TAG_IMAGE_TITLE = 0x0320;
		private const int PROPERTY_TAG_NEW_SUBFILE_TYPE = 0x00FE;
		private const short PROPERTY_TAG_TYPE_ASCII = 2;
		private const short PROPERTY_TAG_TYPE_LONG = 4;
		private static PropertyItem CreatePropertyItem(int id, short type, int len, byte[] value) {
			PropertyItem prop = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true)!;
			prop.Id = id;
			prop.Type = type;
			prop.Len = len;
			prop.Value = value;
			return prop;
		}
		public static string GetImageFilename(this Image image) {
			try {
				PropertyItem? prop = image.GetPropertyItem(PROPERTY_TAG_IMAGE_TITLE);
				if (prop == null) return string.Empty;
				return Encoding.UTF8.GetString(prop.Value!);
			} catch (ArgumentException) {
				return string.Empty;
			}
		}
		public static void SetImageFilename(this Image image, string filename) {
			byte[] data = Encoding.UTF8.GetBytes(filename);
			image.SetPropertyItem(CreatePropertyItem(PROPERTY_TAG_IMAGE_TITLE, PROPERTY_TAG_TYPE_ASCII, data.Length, data));
		}
		public static uint GetTypeTag(this Image image) {
			try {
				PropertyItem? prop = image.GetPropertyItem(PROPERTY_TAG_NEW_SUBFILE_TYPE);
				if (prop == null) return 0;
				return BitConverter.ToUInt32(prop.Value!);
			} catch (ArgumentException) {
				return 0;
			}
		}
		public static void SetTypeTag(this Image image, uint tag) {
			image.SetPropertyItem(CreatePropertyItem(PROPERTY_TAG_NEW_SUBFILE_TYPE, PROPERTY_TAG_TYPE_LONG, 4, BitConverter.GetBytes(tag)));
		}
	}
}
