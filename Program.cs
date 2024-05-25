using System;
using System.IO;
using DefParser.Defs;

namespace DefParser {
	internal class Program {
		static void Main(string[] args) {
			Def.MultilineToString = true;
			string root = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\source\repos\DefParser\content\";
			string defs = Path.Combine(root, "defs");
			string assets = Path.Combine(root, "assets");
			Logger.Write("");
			Parser.ParseDefs(defs, new[] { assets });
			foreach (Def def in DefDatabase<Def>.All) {
				Logger.Info(def);
			}
		}
	}
}