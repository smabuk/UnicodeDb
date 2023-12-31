
namespace UnicodeDb;

/// <summary>
/// Numeric values and types
/// Decimal:
///    Characters are classified with a Numeric type.
///    Characters such as fractions, subscripts, superscripts, Roman numerals, currency numerators, encircled numbers,
///    and script-specific digits are type Numeric.
///    They have a numeric value that can be decimal, including zero and negatives, or a vulgar fraction.
///    If there is not such a value, as with most of the characters, the numeric type is "None".
///
///    The characters that do have a numeric value are separated in three groups:
///        Decimal (De), Digit (Di) and Numeric (Nu, i.e.all other).
///
///        "Decimal" means the character is a straight decimal digit.
///        Only characters that are part of a contiguous encoded range 0..9 have numeric type Decimal.
///        Other digits, like superscripts, have numeric type Digit.
///        All numeric characters like fractions and Roman numerals end up with the type "Numeric".
///        The intended effect is that a simple parser can use these decimal numeric values,
///        without being distracted by say a numeric superscript or a fraction.
///
///        Eighty-three CJK Ideographs that represent a number, including those used for accounting, are typed Numeric.
///
/// On the other hand, characters that could have a numeric value as a second meaning are still marked Numeric type "None",
/// and have no numeric value (""). E.g.Latin letters can be used in paragraph numbering like "II.A.1.b",
/// but the letters "I", "A" and "b" are not numeric(type "None") and have no numeric value.
///
/// Hexadecimal digits:
///    Hexadecimal characters are those in the series with hexadecimal values 0...9ABCDEF(sixteen characters, decimal value 0–15).
///    The character property Hex_Digit is set to Yes when a character is in such a series:
///    Characters in Unicode marked Hex_Digit=Yes
///       0123456789ABCDEF Basic Latin, capitals Also ASCII_Hex_Digit=Yes
///       0123456789abcdef Basic Latin, small letters  Also ASCII_Hex_Digit = Yes
///       ０１２３４５６７８９ＡＢＣＤＥＦ Fullwidth forms, capitals	
///       ０１２３４５６７８９ａｂｃｄｅｆ Fullwidth forms, small letters
///
/// Forty-four characters are marked as Hex_Digit.The ones in the Basic Latin block are also marked as ASCII_Hex_Digit.
/// Unicode has no separate characters for hexadecimal values.
/// A consequence is, that when using regular characters it is not possible to determine whether hexadecimal value is intended,
/// or even whether a value is intended at all.
/// That should be determined at a higher level, e.g.by prepending "0x" to a hexadecimal number or by context.
/// The only feature is that Unicode can note that a sequence can or can not be a hexadecimal value.
/// 
/// <see cref="https://en.wikipedia.org/wiki/Unicode_character_property"/> 
/// 
/// </summary>

internal record Numeric(double Value, string ValueString) : IParsable<Numeric>
{
	public static Numeric Parse(string s, IFormatProvider? provider)
	{
		XElement xel = XElement.Parse(s);
		string nt = xel.Attribute("nt")?.Value ?? "";
		string nv = xel.Attribute("nv")?.Value ?? "";
		float nValue = nt switch
		{
			"De" => float.Parse(nv),
			"Di" => float.Parse(nv),
			"Nu" when nv.Contains('/') => float.Parse(nv.Split('/')[0]) / float.Parse(nv.Split('/')[1]),
			"Nu" => float.TryParse(nv, null, out float nValue2) == true ? nValue2 : 0,
			_ => 0,
		};
		return nt switch
		{
			"De" => new Decimal(nValue, nv),
			"Di" => new Digit(nValue, nv),
			"Nu" => new Numeric(nValue, nv),
			   _ => new None(),
		};
	}

	public static Numeric Parse(string s) => Parse(s, null);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Numeric result)
		=> throw new NotImplementedException();
}

internal record None() : Numeric(0, "NaN");
internal record Decimal(double Value, string ValueString) : Numeric(Value, ValueString);
internal record Digit(double Value, string ValueString) : Numeric(Value, ValueString);
