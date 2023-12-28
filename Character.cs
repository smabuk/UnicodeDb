namespace UnicodeDb;
/// <summary>
///  Representation of a char in the XML database
/// </summary>
/// <see cref="https://www.unicode.org/reports/tr42/"/>
internal sealed record Character(int CodePoint, string Name, string Age, string BlockName, string Script, bool IsEmoji) : IParsable<Character>
{
	public required List<NameAlias> Aliases { get; init; }

	public string AllNames => (Name + (Aliases.Count == 0 ? "" :  " (" + string.Join(", ", Aliases.Select(a => a.Name)) + ")")).Trim();

	public string String => char.ConvertFromUtf32(CodePoint);

    public static Character Parse(string s, IFormatProvider? provider)
	{
		XElement xel = XElement.Parse(s);

		string   name = xel.Attribute("na")?.Value ?? "";
		int        cp = xel.Attribute("cp")!.Value.FromHex();
		string    age = xel.Attribute("age")?.Value ?? "";
		string    blk = xel.Attribute("blk")?.Value ?? "";
		string script = xel.Attribute("sc")?.Value ?? "";
		bool    emoji = ((xel.Attribute("Emoji")?.Value ?? "") == "Y");

		List<NameAlias> aliases = xel
			.Elements()
			.Select(x => new NameAlias(x.Attribute("alias")?.Value ?? "", x.Attribute("type")?.Value ?? ""))
			.ToList();

		//if (string.IsNullOrEmpty(name)) {
		//	name = xel
		//		.Elements()
		//		//.Where(x => x.Attribute("type")!.Value == "control")
		//		.First()
		//		.Attribute("alias")?.Value ?? "";
		//}

		return new(cp, name, age, blk, script, emoji) { Aliases = aliases };
	}

	public static Character Parse(string s) => Parse(s, null);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Character result) => throw new NotImplementedException();
}