namespace UndyneFight_Ex;
/// <summary>
/// Utilities for mathematical operations
/// </summary>
public static class StringUtil
{
	/// <summary>
	/// Gets the amount of substring inside of a string
	/// </summary>
	/// <param name="text">The source text</param>
	/// <param name="substring">The substring to check</param>
	/// <returns></returns>
	public static int SubstringCount(this string text, string substring)
	{
		int count = 0, minIndex = text.IndexOf(substring);
		while (minIndex != -1)
		{
			minIndex = text.IndexOf(substring, minIndex + substring.Length);
			count++;
		}
		return count;
	}
	/// <summary>
	/// Returns the specified default string if the input string is null or empty; otherwise, returns the input string.
	/// </summary>
	/// <param name="text">The string to check for null or empty. If this value is null or empty, the method returns <paramref
	/// name="defaultText"/>.</param>
	/// <param name="defaultText">The string to return if <paramref name="text"/> is null or empty.</param>
	/// <returns>A string that is either the original <paramref name="text"/> if it is not null or empty, or <paramref
	/// name="defaultText"/> if <paramref name="text"/> is null or empty.</returns>
	public static string DefaultIfNullOrEmpty(this string text, string defaultText) => string.IsNullOrEmpty(text) ? defaultText : text;
	/// <summary>
	/// Returns a new string that is a truncated version of the given string, with "..." added to the start or end based on current display position
	/// </summary>
	/// <param name="text">The string to truncate</param>
	/// <param name="maxLength">The maximum amount of characters that can be displayed (excluding "...")</param>
	/// <param name="position">The position of the text to display ([-0.5f, 0] -> ...abc; (0, 1) -> cde; [1, 1.5f] -> ...efg)</param>
	/// <returns></returns>
	public static string ShiftingEllipsis(this string text, int maxLength, float position)
	{
		//Return text if it's short enough
		if (text.Length <= maxLength)
			return text;
		string returnText = "";
		//Ensure position is within range
		position = MathUtil.Posmod(position + 0.5f, 2) - 0.5f;
		int MinIndex = MathHelper.Clamp((int)(position / (text.Length - maxLength)), 0, text.Length - maxLength);
		//Dot padding
		int dotCount = int.Min(3, MinIndex);
		if (MinIndex > 0)
			for (int j = 0; j < dotCount; j++)
				returnText += ".";
		returnText += text[MinIndex..(maxLength + MinIndex)];
		if (returnText != text)
			for (int j = 0; j < 3 - dotCount; j++)
				returnText += ".";
		return returnText;
	}
}