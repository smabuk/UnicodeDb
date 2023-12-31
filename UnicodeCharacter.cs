namespace UnicodeDb;

/// <summary>
///  Representation of a char in the XML database
/// UNICODE CHARACTER DATABASE IN XML <see cref="https://www.unicode.org/reports/tr42/"/>
/// UNICODE EMOJI <seealso cref="https://www.unicode.org/reports/tr51/"/>
/// </summary>
internal sealed record UnicodeCharacter(
	int    CodePoint,
	string Name,
	string Age,
	string BlockName,
	string Script,
	string GeneralCategory,
	bool   IsEmoji,
	bool   IsEmojiPresentation,
	bool   IsEmojiModifier,
	bool   IsEmojiBase,
	bool   IsEmojiComponent,
	bool   IsExtendedPictographic,
	Numeric Numeric
	) : IParsable<UnicodeCharacter>
{
	public required List<NameAlias> Aliases { get; init; }

	public string AllNames => (Name + (Aliases.Count == 0 ? "" :  " (" + string.Join(", ", Aliases.Select(a => a.Name)) + ")")).Trim();

	public string String        => char.ConvertFromUtf32(CodePoint);
	public string StringAsText  => char.ConvertFromUtf32(CodePoint) + char.ConvertFromUtf32(0xFE0E);   //VARIATION SELECTOR-15
	public string StringAsEmoji => char.ConvertFromUtf32(CodePoint) + char.ConvertFromUtf32(0xFE0F);   //VARIATION SELECTOR-16


	public string GeneralCategoryFull => GeneralCategory switch
	{
		"Lu" => "Letter, uppercase",
		"Ll" => "Letter, lowercase",
		"Lt" => "Letter, titlecase",
		"Lo" => "Letter, other",

		"Mn" => "Mark, nonspacing",
		"Mc" => "Mark, spacing combining",
		"Me" => "Mark, enclosing",

		"Nd" => "Number, decimal digit",
		"Nl" => "Number, decimal letter",
		"No" => "Number, other",

		"Pc" => "Punctuation, connector",
		"Pd" => "Punctuation, dash",
		"Ps" => "Punctuation, open",
		"Pe" => "Punctuation, close",
		"Pi" => "Punctuation, initial quote",
		"Pf" => "Punctuation, final quote",
		"Po" => "Punctuation, other",

		"Sm" => "Symbol, math",
		"Sc" => "Symbol, currency",
		"Sk" => "Symbol, modifier",
		"So" => "Symbol, other",

		"Zs" => "Separator, space",
		"Zl" => "Separator, line",
		"Zp" => "Separator, paragraph",

		"Cc" => "Other, control",
		"Cf" => "Other, format",
		"Cs" => "Other, surrogate",
		"Co" => "Other, private use",
		"Cn" => "Other, not assigned",

		_ => "Unknown",
	};



	public static UnicodeCharacter Parse(string s, IFormatProvider? provider)
	{
		XElement xel = XElement.Parse(s);

		string   name = xel.Attribute("na")?.Value ?? "";
		int codepoint = xel.Attribute("cp")!.Value.FromHex();
		string    age = xel.Attribute("age")?.Value ?? "";
		string    blk = xel.Attribute("blk")?.Value ?? "";
		string script = xel.Attribute("sc")?.Value ?? "";
		string     gc = xel.Attribute("gc")?.Value ?? "";

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

		return new(codepoint, name, age, blk, script, gc, emoji, ePres, eMod, eBase, eComp, extnd, Numeric.Parse(s)) { Aliases = aliases };
	}

	public static UnicodeCharacter Parse(string s) => Parse(s, null);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UnicodeCharacter result) => throw new NotImplementedException();




}
