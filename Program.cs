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

Console.WriteLine();
Console.WriteLine("Blocks:");
foreach (var block in blocks) {
	Console.WriteLine($"""{block.Value.First,6:X} - {block.Value.Last,6:X}  {block.Key}""");
}

Dictionary<int, Character> chars = db.Root
	.Descendants()
	.Where(x => x.Name.LocalName == "char" && x.Attribute("cp") is not null)
	.Select(x => Character.Parse(x.ToString()))
	.ToDictionary(x => x.CodePoint);


string blockName = args.Length >= 1 ? args[0] : "Pictogram";
(string name, (int first, int last)) = blocks.Where(x => x.Key.Contains(blockName)).First();

Console.WriteLine();
Console.WriteLine($"{name}:");
foreach ((int _, Character character) in chars.Where(x => x.Key >= first && x.Key <= last)) {
	Console.WriteLine($"""{character.CodePoint,6:X} {character.CodePoint,6}   {character.String,-3} {character.Age,4}  {character.AllNames}""");
}

Console.WriteLine();
