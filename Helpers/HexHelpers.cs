namespace UnicodeDb.Helpers;
internal static class HexHelpers
{
	internal static int FromHex(this string hex) => int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
	internal static string ToHex(this string hex) => $"{hex:X}";
}
