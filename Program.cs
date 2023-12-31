Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Unicode DB exploration");
Console.WriteLine("======================");

XDocument db = XDocument.Parse(XmlFile.Load("ucd.nounihan.flat"));

if (db.Root is null) {
	return;
}

Dictionary<string, (int First, int Last)> blocks = db.Root
	.Elements()
	.Where(x => x.Name.LocalName == "blocks")
	.Elements()
	.ToDictionary(x => x.Attribute("name")!.Value
		, x => (x.Attribute("first-cp")!.Value.FromHex(), x.Attribute("last-cp")!.Value.FromHex()));

if (args.Length == 0) {
	DisplayListOfBlocks(blocks);
	return;
}

Dictionary<int, UnicodeCharacter> chars = db.Root
	.Descendants()
	.Where(x => x.Name.LocalName == "char" && x.Attribute("cp") is not null)
	.Select(x => UnicodeCharacter.Parse(x.ToString()))
	.ToDictionary(x => x.CodePoint);



if (args.Length == 2 && args[0].ToLower() == "-c") {
	DisplayCharactersWhere(chars, args[1]);
	return;
}

string blockName = args[0];

if      (blockName == "numeric") { DisplayCharactersWithNumericValues(chars); }
else if (blockName == "emoji")   { DisplayAllEmoji(chars); }
else if (blockName == "pics")    { DisplayAllPictographs(chars); }
else if (blockName == "keycaps") { DisplayKeyCaps(chars); }
else {
	foreach ((string name, (int first, int last)) in blocks.Where(x => x.Key.Contains(blockName, StringComparison.OrdinalIgnoreCase))) {
		DisplayBlock(chars, name, first, last);
	}
}

Console.WriteLine();


static void DisplayListOfBlocks(Dictionary<string, (int First, int Last)> blocks)
{
	Console.WriteLine();
	Console.WriteLine("Blocks:");
	foreach ((string blockName, (int first, int last)) in blocks) {
		Console.WriteLine($"""{first,6:X} - {last,6:X}  {blockName}""");
	}
}

static void DisplayBlock(Dictionary<int, UnicodeCharacter> chars, string blockName, int first, int last)
{
	Console.WriteLine();
	Console.WriteLine($"{blockName}:");
	foreach ((int _, UnicodeCharacter character) in chars.Where(x => x.Key >= first && x.Key <= last)) {
		Console.WriteLine($"""{"0x" + character.CodePoint.ToString("X"),8} {character.CodePoint,6}   {character.String,-3} {character.Age,4}  {character.AllNames}""");
	}
}

static void DisplayAllEmoji(Dictionary<int, UnicodeCharacter> chars)
{
	Console.WriteLine();
	Console.WriteLine($"Emoji:");
	foreach (var character in chars.Values.Where(c => c.IsEmoji)) {
		string strEmoji = "";
		//strEmoji += ", " + (character.IsEmoji             ? "Emoji " : "" );
		strEmoji += (character.IsEmojiPresentation ? ", Presentation" : "");
		strEmoji += (character.IsEmojiBase ? ", Base" : "");
		strEmoji += (character.IsEmojiComponent ? ", Component" : "");
		strEmoji += (character.IsEmojiModifier ? ", Modifier" : "");
		Console.Write($"""{"0x" + character.CodePoint.ToString("X"),8} {character.CodePoint,6}  {character.StringAsEmoji,-2}  """);
		Console.Write($"{character.AllNames,-60}");
		Console.WriteLine($"     (Emoji: {strEmoji.Trim(',').Trim(' ')})");
	}
	int count = 0;
	int width = Console.WindowWidth / 4;
	foreach (var character in chars.Values.Where(c => c.IsEmoji && !c.IsEmojiComponent && c.IsEmojiPresentation)) {
		if (count++ % width == 0) {
			Console.WriteLine();
		}
		Console.Write(character.String);
	}
}

static void DisplayCharactersWithNumericValues(Dictionary<int, UnicodeCharacter> chars)
{
	Console.WriteLine();
	Console.WriteLine($"Characters with numeric values:");
	foreach (var character in chars.Values.Where(c => c.Numeric is not None)) {
		string type = character.Numeric.GetType().ToString()[10..];
		Console.WriteLine($"""{"0x" + character.CodePoint.ToString("X"),8} {character.CodePoint,6}   {character.String,-3}  => {character.Numeric.Value,8:0.###} {type,-8} {character.Numeric.ValueString,8}    {character.AllNames}""");
	}
}

static void DisplayAllPictographs(Dictionary<int, UnicodeCharacter> chars)
{
	Console.WriteLine();
	Console.WriteLine($"Extended Pictographic:");
	foreach (var character in chars.Values.Where(c => c.IsExtendedPictographic)) {
		Console.Write($"""{"0x" + character.CodePoint.ToString("X"),8} {character.CodePoint,6}  {character.StringAsEmoji,-2}  """);
		Console.WriteLine($" {character.Age,4} {character.AllNames,-60} ({character.BlockName})");
	}
	int count = 0;
	int width = Console.WindowWidth / 4;
	foreach (var character in chars.Values.Where(c => c.IsExtendedPictographic)) {
		if (count++ % width == 0) {
			Console.WriteLine();
		}
		Console.Write(character.String);
	}
}

static void DisplayCharactersWhere(Dictionary<int, UnicodeCharacter> chars, string searchTerm)
{
	Console.WriteLine();
	Console.WriteLine($"""Searching for ... "{searchTerm}":""");
	Console.WriteLine();
	foreach (var character in chars.Values.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || c.Aliases.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).Any())) {
		Console.Write($"""{"0x" + character.CodePoint.ToString("X"),8} {character.CodePoint,6}  {character.StringAsEmoji,-2}  """);
		Console.WriteLine($" {character.Age,4} {character.GeneralCategory,4} {character.AllNames,-60} ({character.BlockName})");
	}
}

static void DisplayKeyCaps(Dictionary<int, UnicodeCharacter> chars)
{
	Console.WriteLine();
	Console.WriteLine($"Keycaps:");
	for (int codePoint = 48; codePoint < 58; codePoint++) {
		Console.Write($"""{"0x" + codePoint.ToString("X"),8} {codePoint,6}""");
		Console.Write($"""  {char.ConvertFromUtf32(codePoint)}{char.ConvertFromUtf32(0x20E3)}""");
		Console.WriteLine($" {chars[codePoint].Age,4} {chars[codePoint].AllNames,-60} ({chars[codePoint].BlockName})");
	}
}