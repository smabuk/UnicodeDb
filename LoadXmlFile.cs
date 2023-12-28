using System.IO.Compression;

namespace UnicodeDb;
internal static class XmlFile
{
	public static string Load(string name)
	{
		DirectoryInfo temp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "unicode_db"));
		string fileName = Path.Combine(temp.FullName, name + ".xml");

		if (File.Exists(fileName)) {
			return File.ReadAllText(fileName).ReplaceLineEndings();
		}

		string zipPath = Path.Combine("latest", "ucdxml", name + ".zip");

		if (!File.Exists(zipPath)) {
			throw new ApplicationException("No unicode data found.");
		}

		using ZipArchive archive = ZipFile.OpenRead(zipPath);

		foreach (ZipArchiveEntry entry in archive.Entries) {
			if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) {
				string destinationPath = Path.GetFullPath(Path.Combine(temp.FullName, entry.FullName));
				entry.ExtractToFile(destinationPath);
			}
		}

		if (File.Exists(fileName)) {
			return File.ReadAllText(fileName).ReplaceLineEndings();
		}

		throw new ApplicationException("No unicode data found.");
	}
}
