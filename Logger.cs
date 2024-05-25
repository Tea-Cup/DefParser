using System;

namespace DefParser {
	public static class Logger {
		public static LogLevel Level { get; set; } = LogLevel.Debug;
		private const string SEQ_RESET = "\u001b[0m";
		private const string PREFIX_DEBUG = "\u001b[4mD";
		private const string PREFIX_LOG = "L";
		private const string PREFIX_INFO = "\u001b[30m\u001b[104mI";
		private const string PREFIX_WARN= "\u001b[30m\u001b[43mW";
		private const string PREFIX_ERROR = "\x001B[37m\u001b[41mE";

		public static void Debug(object? message) {
			if (Level < LogLevel.Debug) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_DEBUG, msg, SEQ_RESET);
			System.Diagnostics.Debug.WriteLine("⚙️ " + msg);
		}
		public static void Log(object? message) {
			if (Level < LogLevel.Log) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_LOG, msg);
			System.Diagnostics.Debug.WriteLine("📋 " + msg);
		}
		public static void Info(object? message) {
			if (Level < LogLevel.Info) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_INFO, msg);
			System.Diagnostics.Debug.WriteLine("💬 " + msg);
		}
		public static void Warn(object? message) {
			if (Level < LogLevel.Warn) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_WARN, msg);
			System.Diagnostics.Debug.WriteLine("⚠️ " + msg);
		}
		public static void Error(object? message) {
			if (Level < LogLevel.Error) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_ERROR, msg);
			System.Diagnostics.Debug.WriteLine($"⛔ {msg}");
		}
		public static void Write(object? message) {
			string msg = message?.ToString() ?? "<null>";
			WriteLine("  ", msg);
			System.Diagnostics.Debug.WriteLine(msg);
		}
		
		private static void WriteLine(string pre, string message, string post = "") {
			Console.Write(pre);
			Console.Write(' ');
			Console.Write(message);
			Console.Write(post);
			int pad = Console.BufferWidth - Console.CursorLeft;
			for (int i = 0; i < pad; i++) Console.Write(' ');
			Console.WriteLine(SEQ_RESET);
		}
	}
	public enum LogLevel {
		None, Error, Warn, Info, Log, Debug
	}
}
