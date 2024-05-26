using System;

namespace DefParser {
	/// <summary>Printer to debug and console output with colors and icons.</summary>
	public static class Logger {
		/// <summary>Maximum level of logs to print. Logs below this level will be ignored.</summary>
		public static LogLevel Level { get; set; } = LogLevel.Debug;
		// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
		private const string SEQ_RESET = "\u001b[0m";
		private const string PREFIX_DEBUG = "\u001b[4mD";
		private const string PREFIX_LOG = "L";
		private const string PREFIX_INFO = "\u001b[30m\u001b[104mI";
		private const string PREFIX_WARN= "\u001b[30m\u001b[43mW";
		private const string PREFIX_ERROR = "\x001B[37m\u001b[41mE";

		/// <summary>Print message with <see cref="LogLevel.Debug"/> level.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
		public static void Debug(object? message) {
			if (Level < LogLevel.Debug) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_DEBUG, msg, SEQ_RESET);
			System.Diagnostics.Debug.WriteLine("⚙️ " + msg);
		}

		/// <summary>Print message with <see cref="LogLevel.Log"/> level.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
		public static void Log(object? message) {
			if (Level < LogLevel.Log) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_LOG, msg);
			System.Diagnostics.Debug.WriteLine("📋 " + msg);
		}

		/// <summary>Print message with <see cref="LogLevel.Info"/> level.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
		public static void Info(object? message) {
			if (Level < LogLevel.Info) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_INFO, msg);
			System.Diagnostics.Debug.WriteLine("💬 " + msg);
		}

		/// <summary>Print message with <see cref="LogLevel.Warn"/> level.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
		public static void Warn(object? message) {
			if (Level < LogLevel.Warn) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_WARN, msg);
			System.Diagnostics.Debug.WriteLine("⚠️ " + msg);
		}

		/// <summary>Print message with <see cref="LogLevel.Error"/> level.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
		public static void Error(object? message) {
			if (Level < LogLevel.Error) return;
			string msg = message?.ToString() ?? "<null>";
			WriteLine(PREFIX_ERROR, msg);
			System.Diagnostics.Debug.WriteLine($"⛔ {msg}");
		}
		/// <summary>Print message without respect to <see cref="Level"/>.</summary>
		/// <param name="message">Object to print with <see langword="null"/> conversion.</param>
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
			// Fill the rest of console line with spaces to apply color to the entire line
			int pad = Console.BufferWidth - Console.CursorLeft;
			for (int i = 0; i < pad; i++) Console.Write(' ');
			// Reset attributes
			Console.WriteLine(SEQ_RESET);
		}
	}
	public enum LogLevel {
		None, Error, Warn, Info, Log, Debug
	}
}
