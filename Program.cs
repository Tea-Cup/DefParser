using System.IO;
using DefParser.Defs;

namespace DefParser {
	internal class Program {
		static void Main(string[] args) {
			Def.MultilineToString = true;
			string root = File.ReadAllText("SolutionDir.env").Trim();
			string content = Path.Combine(root, "content");
			string defs = Path.Combine(content, "defs");
			string assets = Path.Combine(content, "assets");
			Logger.Write("");
			Parser.ParseDefs(defs, new[] { assets });
			foreach (Def def in DefDatabase<Def>.All) {
				Logger.Info(def);
			}
		}
	}
}