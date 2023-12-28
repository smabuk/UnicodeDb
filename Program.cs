Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Unicode DB exploration");
Console.WriteLine("======================");

string xml = File.ReadAllText("ucd.all.flat.xml").ReplaceLineEndings();


XDocument db = XDocument.Parse(xml);
xml = "";
if (db.Root is null ) {
	return;
}

Dictionary<string, (int First, int Last)> blocks = db.Root
	.Elements()
	.Where(x => x.Name.LocalName == "blocks")
	.Elements()
	.ToDictionary(x => x.Attribute("name")!.Value
		, x => (x.Attribute("first-cp")!.Value.FromHex() ,x.Attribute("last-cp")!.Value.FromHex()));

Console.WriteLine("Blocks:");
foreach (var block in blocks) {
	Console.WriteLine($"""{block.Value.First,6:X} - {block.Value.Last,6:X}  {block.Key}""");
}

Dictionary<int, Character> chars = db.Root
	.Descendants()
	.Where(x => x.Name.LocalName == "char" && x.Attribute("cp") is not null)
	.Select(x => Character.Parse(x.ToString(), null))
	.ToDictionary(x => x.Cp);


(int first, int last) = blocks.Where(x => x.Key.Contains("Dingbats")).First().Value;

Console.WriteLine("Dingbats:");
foreach ((int _, Character character) in chars.Where(x => x.Key >= first && x.Key <= last)) {
	Console.WriteLine($"""{character.Cp,6:X} {character.Cp,6}   {char.ConvertFromUtf32(character.Cp),-3} {character.Name}""");
}

Console.WriteLine(xml);

record Character(int Cp, string Name, string Age) : IParsable<Character>
{
	public static Character Parse(string s, IFormatProvider? provider)
	{
		XElement xel = XElement.Parse(s);
		string name = xel.Attribute("na")?.Value ?? "";
		if (string.IsNullOrEmpty(name)) {
			name = xel
				.Elements()
				//.Where(x => x.Attribute("type")!.Value == "control")
				.First()
				.Attribute("alias")?.Value ?? "";
		}
		int cp = xel.Attribute("cp")!.Value.FromHex();
		string age = xel.Attribute("age")?.Value ?? "";
		return new(cp, name, age);
	}

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Character result) => throw new NotImplementedException();
}