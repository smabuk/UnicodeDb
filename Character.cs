namespace UnicodeDb;
/// <summary>
///  Representation of a char in the XML database
/// UNICODE CHARACTER DATABASE IN XML <see cref="https://www.unicode.org/reports/tr42/"/>
/// UNICODE EMOJI <seealso cref="https://www.unicode.org/reports/tr51/"/>
/// </summary>
internal sealed record Character(
	int CodePoint,
	string Name,
	string Age,
	string BlockName,
	string Script,
	bool IsEmoji,
	bool IsEmojiPresentation,
	bool IsEmojiModifier,
	bool IsEmojiBase,
	bool IsEmojiComponent,
	bool IsExtendedPictographic,
	string NumericType,
	string NumericValueString,
	float? NumericValue
	) : IParsable<Character>
{
	public required List<NameAlias> Aliases { get; init; }

	public string AllNames => (Name + (Aliases.Count == 0 ? "" :  " (" + string.Join(", ", Aliases.Select(a => a.Name)) + ")")).Trim();

	public string String        => char.ConvertFromUtf32(CodePoint);
	public string StringAsText  => char.ConvertFromUtf32(CodePoint) + char.ConvertFromUtf32(0xFE0E);   //VARIATION SELECTOR-15
	public string StringAsEmoji => char.ConvertFromUtf32(CodePoint) + char.ConvertFromUtf32(0xFE0F);   //VARIATION SELECTOR-16


	public static Character Parse(string s, IFormatProvider? provider)
	{
		XElement xel = XElement.Parse(s);

		string   name = xel.Attribute("na")?.Value ?? "";
		int codepoint = xel.Attribute("cp")!.Value.FromHex();
		string    age = xel.Attribute("age")?.Value ?? "";
		string    blk = xel.Attribute("blk")?.Value ?? "";
		string script = xel.Attribute("sc")?.Value ?? "";
		string     nt = xel.Attribute("nt")?.Value ?? "";
		string     nv = xel.Attribute("nv")?.Value ?? "";
		float? nValue = nt switch
		{
			"De" => float.Parse(nv),
			"Di" => float.Parse(nv),
			"Nu" when nv.Contains('/') => float.Parse(nv.Split('/')[0]) / float.Parse(nv.Split('/')[1]),
			"Nu" => float.TryParse(nv, null, out float nValue2) == true ? nValue2 : null,
			_ => null,
		};
		bool emoji = ((xel.Attribute("Emoji")  ?.Value ?? "") == "Y");
		bool ePres = ((xel.Attribute("EPres")  ?.Value ?? "") == "Y");
		bool  eMod = ((xel.Attribute("EMod")   ?.Value ?? "") == "Y");
		bool eBase = ((xel.Attribute("EBase")  ?.Value ?? "") == "Y");
		bool eComp = ((xel.Attribute("EComp")  ?.Value ?? "") == "Y");
		bool extnd = ((xel.Attribute("ExtPict")?.Value ?? "") == "Y");

		List<NameAlias> aliases = xel
			.Elements()
			.Select(x => new NameAlias(x.Attribute("alias")?.Value ?? "", x.Attribute("type")?.Value ?? ""))
			.ToList();

		return new(codepoint, name, age, blk, script, emoji, ePres, eMod, eBase, eComp, extnd, nt, nv, nValue) { Aliases = aliases };
	}

	public static Character Parse(string s) => Parse(s, null);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Character result) => throw new NotImplementedException();
}