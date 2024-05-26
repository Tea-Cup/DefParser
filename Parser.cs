using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DefParser.Defs;

namespace DefParser {
	// TODO: Comments
	public class Parser {
		private static Dictionary<string, Type> DefTypes { get; set; } = new();
		private static Dictionary<string, Dictionary<string, PropertyInfo>> PropsCache { get; set; } = new();

		private readonly string[] content;
		private readonly Dictionary<string, Def> all = new();
		private readonly HashSet<string> incomplete = new();
		private readonly Dictionary<string, HashSet<(string, XmlElement)>> inheritance = new();

		static Parser() {
			Logger.Info("Loading Def types");
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				Logger.Log($"Scanning {asm.FullName}");
				int count = 0;
				foreach (Type type in asm.GetTypes()) {
					if (type.IsInterface) continue;
					if (!type.IsAssignableTo(typeof(Def))) continue;
					if (type.GetCustomAttribute<IgnoreAttribute>() != null) continue;

					string typename = type.FullName ?? type.Name;
					if (type.Namespace == typeof(Def).Namespace) typename = type.Name;

					DefTypes.Add(typename.ToUpperInvariant(), type);
					Logger.Log($"{typename} : {type.BaseType?.Name}");
					count++;
				}
				if (count > 0) {
					Logger.Info($"Loaded {count} Def types from {asm.FullName}");
				} else {
					Logger.Debug($"Loaded {count} Def types from {asm.FullName}");
				}
			}
		}

		private Parser(string[] content) {
			this.content = content;
		}

		private void MakeDefKnown(Def def) {
			string id = def.ID.ToUpperInvariant();
			all.Add(id, def);
			incomplete.Add(id);
		}
		private void MakeDefComplete(string id) {
			id = id.ToUpperInvariant();
			incomplete.Remove(id);
		}
		private bool TryGetDef(string id, [NotNullWhen(true)] out Def? def) {
			if (all.TryGetValue(id.ToUpperInvariant(), out def)) return true;
			return false;
		}
		private Def ConstructDef(Type type, string id, bool isAbstract) {
			Def def = (Def)Ref.Construct(type);
			Ref.SetDefID(def, id);
			Ref.SetDefAbstract(def, isAbstract);
			MakeDefKnown(def);
			return def;
		}

		private Type? ParseDefType(string typename) {
			if (!DefTypes.TryGetValue(typename.ToUpperInvariant(), out Type? type)) {
				Logger.Error($"Unknown Def Type: {typename}");
				return null;
			}
			return type;
		}

		private bool RegisterInheritance(string parent, XmlElement child, string id) {
			if (!inheritance.TryGetValue(parent, out var set)) {
				inheritance[parent] = set = new();
			}
			return set.Add((id, child));
		}

		private static bool IsPropertyValid(PropertyInfo prop) {
			return prop.GetCustomAttribute<IgnoreAttribute>() == null && prop.GetMethod != null && prop.SetMethod != null;
		}

		private static Dictionary<string, PropertyInfo> GetProperties(Type type) {
			string typename = (type.FullName ?? type.Name).ToUpperInvariant();
			if (PropsCache.TryGetValue(typename, out var props)) return props;
			props = new();
			foreach (PropertyInfo prop in type.GetRuntimeProperties()) {
				if (!IsPropertyValid(prop)) continue;
				props.Add(prop.Name.ToUpperInvariant(), prop);
			}
			PropsCache[typename] = props;
			return props;
		}

		private object? ParseValueNull(XmlElement xml, Type type, string name) {
			if (xml.ChildNodes.Count > 0) {
				Logger.Warn($"Property {name} has \"null\"=\"true\", but is not empty. Assuming null value.");
			}
			object? value = null;
			if (type.IsValueType) {
				Logger.Warn($"Property {name} has null value, but is not nullable. Assuming default value for type.");
				value = Ref.Construct(type);
			}
			return value;
		}
		private object? ParseValueString(XmlElement xml, Type type, string name) {
			string value;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"string\" type but has children elements. Assuming CDATA block.");
				value = xml.InnerXml;
			} else {
				value = xml.InnerText;
			}
			if (xml.GetBoolAttribute("normalize") != false) {
				string[] lines = value.Trim('\n').Replace("\r", "").Split('\n');
				int pad = int.MaxValue;
				foreach (string line in lines) {
					pad = Math.Min(pad, line.Length - line.Trim().Length);
				}
				value = string.Join('\n', lines.Select(x => x[pad..]));
			}
			return value;
		}
		private object? ParseValueBool(XmlElement xml, Type type, string name) {
			string str;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"bool\" type but has children elements. Assuming CDATA block.");
				str = xml.InnerXml;
			} else {
				str = xml.InnerText;
			}
			str = str.ToLowerInvariant().Trim();
			bool value = true;
			if (str == "false") value = false;
			else if (str != "true") Logger.Error($"Property {name} is of \"bool\" type but not \"true\" or \"false\". Assuming \"true\".");
			return value;
		}

		private static (MethodInfo?, MethodInfo?) GetParseableMethods(Type type) {
			MethodInfo? mTryParse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), type.MakeByRefType() });
			MethodInfo? mParse = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string) });
			if (mTryParse?.ReturnParameter.ParameterType != typeof(bool)) mTryParse = null;
			if (mParse?.ReturnParameter.ParameterType != type) mParse = null;
			return (mTryParse, mParse);
		}
		private static bool IsParseableType(Type type) {
			var (mTryParse, mParse) = GetParseableMethods(type);
			return mTryParse != null || mParse != null;
		}
		private object? ParseValueParseable(XmlElement xml, Type type, string name) {
			string str;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"{type.Name}\" type but has children elements. Assuming CDATA block.");
				str = xml.InnerXml;
			} else {
				str = xml.InnerText;
			}
			str = str.ToLowerInvariant().Trim();
			object? value = null!;
			if (type.IsValueType) value = Ref.Construct(type);
			var (mTryParse, mParse) = GetParseableMethods(type);
			if (mTryParse != null) {
				object?[] pars = new object?[] { str, value };
				bool result = (bool)mTryParse.Invoke(null, pars)!;
				if (result != true) {
					Logger.Error($"Failed to parse {name} with {type.Name}::TryParse: \"{str}\"");
				} else {
					value = pars[1];
				}
			} else if (mParse != null) {
				try {
					value = mParse.Invoke(null, new object[] { str })!;
				} catch (Exception ex) {
					Logger.Error($"Failed to parse {name} with {type.Name}::Parse: \"{str}\"");
					Logger.Error(ex);
				}
			}
			return value;
		}

		private object? ParseValueDef(XmlElement xml, Type type, string name) {
			string id;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"{type.Name}\" type but has children elements. Assuming CDATA block.");
				id = xml.InnerXml;
			} else {
				id = xml.InnerText;
			}
			id = id.Trim().ToUpperInvariant();
			if (!TryGetDef(id, out var def)) def = ConstructDef(type, id, false);
			return def;
		}

		private object? ParseValueInitializer(XmlElement xml, Type type, string name) {
			TypeInitializer init = TypeInitializer.GetFor(type) ?? throw new ArgumentException($"No [Initializer] on type {type.FullName}");
			return init.Create(type, name, xml);
		}

		private static readonly Dictionary<string, string> typeMap = new() {
			{ "bool", typeof(bool).FullName! },
			{ "byte", typeof(byte).FullName! },
			{ "sbyte", typeof(sbyte).FullName! },
			{ "char", typeof(char).FullName! },
			{ "decimal", typeof(decimal).FullName! },
			{ "double", typeof(double).FullName! },
			{ "float", typeof(float).FullName! },
			{ "int", typeof(int).FullName! },
			{ "uint", typeof(uint).FullName! },
			{ "long", typeof(long).FullName! },
			{ "ulong", typeof(ulong).FullName! },
			{ "short", typeof(short).FullName! },
			{ "ushort", typeof(ushort).FullName! },
			{ "string", typeof(string).FullName! },
		};
		private bool TryParseValueElement(XmlElement xml, Type type, out object? value) {
			string typename = xml.Name;
			if (typeMap.ContainsKey(typename.ToLowerInvariant())) typename = typeMap[typename.ToLowerInvariant()];
			Type? eltype = Ref.FindType(typename);
			if (eltype == null) {
				Logger.Error($"Type named \"{typename}\" not found for element <{xml.Name}>");
				value = null;
				return false;
			}
			try {
				value = ParseValue(xml, eltype, xml.Name);
				try {
					value = Ref.Cast(value, type);
					return true;
				} catch (Exception e) {
					Logger.Error($"Failed to cast element <{xml.Name}> from type {eltype.FullName} to target type {type.FullName}");
					Logger.Error(e);
				}
			} catch (Exception e) {
				Logger.Error($"Failed to parse element <{xml.Name}> as a value of type {eltype.FullName}");
				Logger.Error(e);
			}
			value = null;
			return false;
		}
		private IEnumerable<object?> ParseValueEnumerableInner(XmlElement xml, Type type) {
			foreach (XmlElement el in xml.GetChildren()) {
				if (TryParseValueElement(el, type, out var value)) yield return value;
			}
		}
		private IList ParseValueEnumerable(XmlElement xml, Type type) {
			IList list = (IList)Ref.Construct(typeof(List<>).MakeGenericType(type));
			foreach (object? obj in ParseValueEnumerableInner(xml, type)) list.Add(Ref.Cast(obj, type));
			return list;
		}

		private object? ParseValueArray(XmlElement xml, Type type, string name) {
			Type? eltype = type.GetElementType();
			if (eltype == null) throw new ArgumentException($"Array parsing called on a type without element type.");
			IList list = ParseValueEnumerable(xml, eltype);
			Array arr = Array.CreateInstance(eltype, list.Count);
			list.CopyTo(arr, 0);
			return arr;
		}

		private object? ParseValueList(XmlElement xml, Type type, string name) {
			if (!type.IsConstructedGenericType) throw new ArgumentException($"List parsing called on an incomplete generic type.");
			return ParseValueEnumerable(xml, type.GenericTypeArguments[0]);
		}

		private object? ParseValueDictionary(XmlElement xml, Type type, string name) {
			if (!type.IsConstructedGenericType) throw new ArgumentException($"Dictionary parsing called on an incomplete generic type.");
			Type typeKey = type.GenericTypeArguments[0];
			Type typeValue = type.GenericTypeArguments[1];
			List<object> keys = new();
			List<object?> values = new();
			foreach (XmlElement el in xml.GetChildren()) {
				if (el.Name.ToUpperInvariant() != "PAIR") {
					Logger.Error($"Expected <pair> element, got <{el.Name}> instead.");
					continue;
				}

				XmlElement? elKey = el.FindChild("key");
				XmlElement? elValue = el.FindChild("value");
				if (elKey is null) {
					Logger.Error($"<{el.Name}> in <{xml.Name}> does not have a <key>.");
					continue;
				}
				if (elValue is null) {
					Logger.Error($"<{el.Name}> in <{xml.Name}> does not have a <value>.");
					continue;
				}

				object? key;
				try {
					key = ParseValue(elKey, typeKey, xml.Name);
					if (key is null) {
						Logger.Error($"<{elKey.Name}> element of <{xml.Name}> was parsed as a null. Dictionary key must not be null.");
						continue;
					}
				} catch (Exception e) {
					Logger.Error($"Failed to parse <{elKey.Name}> element of <{xml.Name}> as a value of type {typeKey.FullName}");
					Logger.Error(e);
					continue;
				}

				object? value;
				try {
					value = ParseValue(elValue, typeValue, xml.Name);
				} catch (Exception e) {
					Logger.Error($"Failed to parse <{elValue.Name}> element of <{xml.Name}> as a value of type {typeValue.FullName}");
					Logger.Error(e);
					continue;
				}

				keys.Add(key);
				values.Add(value);
			}

			object dict = Ref.Construct(type);
			for (int i = 0; i < keys.Count; i++) Ref.AddToDict(dict, keys[i], values[i]);
			return dict;
		}

		private object? ParseValueImage(XmlElement xml, Type type, string name) {
			string path;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"{type.Name}\" type but has children elements. Assuming CDATA block.");
				path = xml.InnerXml;
			} else {
				path = xml.InnerText;
			}
			string filename = Path.ChangeExtension(path.Trim(), ".png");
			foreach (string content in content) {
				string fullpath = Path.Combine(content, filename);
				if (!File.Exists(fullpath)) continue;
				try {
					Image img = Image.FromFile(fullpath);
					img.SetImageFilename(filename);
					return img;
				} catch (Exception ex) {
					Logger.Error($"Failed to read file as a Bitmap image: \"{fullpath}\"");
					Logger.Error(ex);
				}
			}

			// TODO: remove dummies
			Image dum = Dummy.Image();
			dum.SetImageFilename(filename);
			return dum;

			Logger.Error($"Failed to find suitable Bitmap image file for path: \"{filename}\"");
			return null;
		}

		private object? ParseValueType(XmlElement xml, Type type, string name) {
			// TODO: remove dummies
			string typename;
			if (xml.GetChildren().Any()) {
				Logger.Warn($"Property {name} is of \"{type.Name}\" type but has children elements. Assuming CDATA block.");
				typename = xml.InnerXml;
			} else {
				typename = xml.InnerText;
			}
			typename = typename.Trim();
			return new Dummy(typename);
		}

		private object? ParseValueNullable(XmlElement xml, Type type, string name) {
			return ParseValue(xml, Nullable.GetUnderlyingType(type)!, name);
		}

		private object? ParseValueDeep(XmlElement xml, Type type, string name) {
			object obj = Ref.Construct(type);
			foreach (XmlElement el in xml.GetChildren()) {
				PropertyInfo? prop = type.GetRuntimeProperty(el.Name);
				if (prop is null) {
					Logger.Error($"Property not found: {type.FullName}::{el.Name}");
					continue;
				}
				if (!IsPropertyValid(prop)) {
					Logger.Error($"Property is not valid for parsing: {type.FullName}::{el.Name}");
					continue;
				}

				object? value;
				try {
					value = ParseValue(el, prop.PropertyType, el.Name);
				} catch {
					Logger.Error($"Failed to parse <{el.Name}> as a value of type {prop.PropertyType.FullName}");
					continue;
				}
				prop.SetMethod?.Invoke(obj, new[] { value });
			}
			return obj;
		}

		private object? ParseValueEnum(XmlElement xml, Type type, string name) {
			string text = xml.GetStrictText(name, "enum");
			return Enum.Parse(type, text, true);
		}

		private object? ParseValue(XmlElement xml, Type type, string name) {
			if (xml.GetBoolAttribute("null") == true) return ParseValueNull(xml, type, name);
			else if (type == typeof(Image)) return ParseValueImage(xml, type, name);
			else if (type == typeof(Type)) return ParseValueType(xml, type, name);
			else if (type.IsAssignableTo(typeof(Def))) return ParseValueDef(xml, type, name);
			else if (type.IsEnum) return ParseValueEnum(xml, type, name);
			else if (type.IsArray) return ParseValueArray(xml, type, name);
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return ParseValueNullable(xml, type, name);
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return ParseValueList(xml, type, name);
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return ParseValueDictionary(xml, type, name);
			else if (type == typeof(string)) return ParseValueString(xml, type, name);
			else if (type == typeof(bool)) return ParseValueBool(xml, type, name);
			else if (IsParseableType(type)) return ParseValueParseable(xml, type, name);
			else if (TypeInitializer.GetFor(type) != null) return ParseValueInitializer(xml, type, name);
			return ParseValueDeep(xml, type, name);
		}

		private Def? ParseDef(XmlElement xml, Def? def = null) {
			Type? type;
			if (def == null) {
				type = ParseDefType(xml.Name);
				if (type == null) return null;

				XmlElement idel = xml.FindChild(nameof(Def.ID)) ?? throw new InvalidDataException($"<{xml.Name}> element does not have an <id> child.");
				string id = idel.InnerText.ToUpperInvariant();

				string? extends = xml.FindAttribute("extends")?.ToUpperInvariant();
				if (extends != null) {
					if (RegisterInheritance(extends, xml, id)) {
						Logger.Info($"Registered <{id}> waiting for <{extends}>");
					}
					return null;
				}

				bool abst = xml.FindAttribute("abstract")?.ToLowerInvariant() == "true";

				if (!TryGetDef(id, out def)) def = ConstructDef(type, id, abst);
				else Ref.SetDefAbstract(def, abst);
			} else {
				type = def.GetType();
			}

			Dictionary<string, PropertyInfo> props = GetProperties(type);
			foreach (XmlElement element in xml.GetChildren()) {
				string propname = element.Name.ToUpperInvariant();
				if (propname == "ID") continue;
				if (!props.TryGetValue(propname, out var prop)) {
					Logger.Error($"Unknown property: {type.LocalName()}::{element.Name}");
					continue;
				}
				try {
					object? value = ParseValue(element, prop.PropertyType, prop.Name);
					prop.SetMethod!.Invoke(def, new object?[] { value });
				} catch (Exception e) {
					Logger.Error($"Failed to parse element <{element.Name}> as a property {type.FullName}::{prop.Name} of type {prop.PropertyType.FullName}");
					Logger.Error(e);
				}
			}

			MakeDefComplete(def.ID);
			if (def.Abstract) AbstractDefDatabase.Add(def.GetType(), def);
			else DefDatabase.Add(def.GetType(), def);
			string? error = def.Validate();
			if (error != null) Logger.Error($"{def.GetType().LocalName()} \"{def.ID}\" failed to validate: {error}");
			return def;
		}

		private int? ParseXml(XmlElement root) {
			if (root.Name.ToUpperInvariant() != "DEFS") return null;
			int count = 0;
			foreach (XmlElement element in root.GetChildren()) {
				if (ParseDef(element) != null) count++;
			}
			return count;
		}

		private void ParseFile(FileInfo file) {
			Logger.Debug($"Parsing {file.FullName}");
			XmlDocument doc = new();
			doc.LoadXml(File.ReadAllText(file.FullName));
			if (doc.DocumentElement is not XmlElement root) return; // Not valid XML

			int? count = ParseXml(root);
			if (!count.HasValue) return; // No <Defs>

			Logger.Log($"Parsed {file.FullName}");
		}

		private void ParseDirectory(DirectoryInfo root) {
			foreach (FileInfo fi in root.EnumerateFiles()) {
				ParseFile(fi);
			}
			foreach (DirectoryInfo di in root.EnumerateDirectories()) {
				ParseDirectory(di);
			}
		}

		private void InheritDef(Def parent, Def child) {
			var props = GetProperties(child.GetType());
			foreach (var (name, propParent) in GetProperties(parent.GetType())) {
				if(!props.TryGetValue(name, out var propChild)) continue;
				propChild.SetMethod?.Invoke(child, new[] { propParent.GetValue(parent) });
			}
		}

		private bool Resolve(string parentId, HashSet<(string, XmlElement)> children) {
			if (!TryGetDef(parentId, out var parent)) return false;
			foreach (var (id, xml) in children) {
				Type? type = ParseDefType(xml.Name);
				if (type == null) continue;

				Def child = (Def)Ref.Construct(type);
				try {
					Ref.SetDefID(child, id);
					InheritDef(parent, child);
					Logger.Log($"Inherited <{child.ID}> from <{parent.ID}>");
					ParseDef(xml, child);
				} catch (Exception ex) {
					Logger.Error($"Failed to inherit <{child.ID}> from <{parent.ID}>");
					Logger.Error(ex);
				}
			}
			return true;
		}
		private bool InheritanceLoop() {
			bool changed = false;
			foreach (var (parent, children) in inheritance.ToList()) {
				if (Resolve(parent, children)) {
					inheritance.Remove(parent);
					changed = true;
				}
			}
			return changed;
		}
		private void ResolveInheritance() {
			bool changed;
			do {
				changed = InheritanceLoop();
			} while (inheritance.Any() && changed);

			if (inheritance.Any()) {
				Logger.Error("Failed to resolve all of inheritance chains");
				foreach (var (parent, children) in inheritance) {
					Logger.Error($"<{parent}>:");
					foreach (var (id, _) in children) {
						Logger.Error($"  <{id}>");
					}
				}
			}
		}

		public static void ParseDefs(string path, string[] contentRoots) {
			DirectoryInfo root = new(path);
			Logger.Info($"Recursively looking for Defs in \"{root.FullName}\"");
			Parser p = new(contentRoots);
			p.ParseDirectory(root);
			Logger.Info("Resolving inheritance chains");
			p.ResolveInheritance();
			if (p.incomplete.Count > 0) {
				Logger.Error("Incomplete defs found:");
				foreach (string id in p.incomplete) Logger.Error($"{p.all[id.ToUpperInvariant()].GetType().FullName} \"{id}\"");
			}
		}
	}
}
