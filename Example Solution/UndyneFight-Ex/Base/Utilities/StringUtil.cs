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
	public static int CountSubstring(this string text, string substring)
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
}