namespace DefParser {
	/// <summary>Exception signaling that an attempt to register an already registered def was made.</summary>
	public class DuplicateDefException : System.ApplicationException {
		public DuplicateDefException(string message) : base(message) { }
	}
}
