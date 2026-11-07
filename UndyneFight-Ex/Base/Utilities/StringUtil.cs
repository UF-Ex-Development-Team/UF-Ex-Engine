using System.Text;

namespace UndyneFight_Ex;
/// <summary>
/// Utilities for mathematical operations
/// </summary>
public static class StringUtil
{
#if NET10_0_OR_GREATER
	extension(string text)
	{
		/// <summary>
		/// Gets the amount of substring inside of a string
		/// </summary>
		/// <param name="substring">The substring to check</param>
		/// <returns>The amount of <param name="substring"/> in <param name="text"/></returns>
		public int SubstringCount(string substring)
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
		public string DefaultIfNullOrEmpty(string defaultText) => string.IsNullOrEmpty(text) ? defaultText : text;
		/// <summary>
		/// Returns a new string that is a truncated version of the given string, with "..." added to the start or end based on current display position
		/// </summary>
		/// <param name="maxLength">The maximum amount of characters that can be displayed (excluding "...")</param>
		/// <param name="position">The position of the text to display ([-0.5f, 0] -> ...abc; (0, 1) -> cde; [1, 1.5f] -> ...efg)</param>
		/// <returns></returns>
		public string ShiftingEllipsis(int maxLength, float position)
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
		/// <summary>
		/// Encodes a string using the Vigenere cipher
		/// </summary>
		/// <param name="key">The key to encrypt</param>
		/// <returns>The encrypted code</returns>
		public string VigenereEncrypt(string key)
		{
			StringBuilder ciphertext = new();
			int keyIndex = 0;
			foreach (char c in text)
			{
				//Only encrypt letters
				if (char.IsLetter(c))
				{
					bool isUpper = char.IsUpper(c);
					char offset = isUpper ? 'A' : 'a';
					//Get corresponding shift value from the key
					char currentKeyChar = key[keyIndex % key.Length];
					int shift = char.ToUpper(currentKeyChar) - 'A';
					//Apply Caesar and wrap around
					char encryptedChar = (char)((c - offset + shift) % 26 + offset);
					_ = ciphertext.Append(encryptedChar);
					keyIndex++;
				}
				else
					_ = ciphertext.Append(c);
			}
			return ciphertext.ToString();
		}
		/// <summary>
		/// Decodes a string using the Vigenere cipher
		/// </summary>
		/// <param name="key">The key to decrypt</param>
		/// <returns>The decrypted text</returns>
		public string VigenereDecrypt(string key)
		{
			StringBuilder plaintext = new();
			int keyIndex = 0;

			foreach (char c in text)
			{
				if (char.IsLetter(c))
				{
					bool isUpper = char.IsUpper(c);
					char offset = isUpper ? 'A' : 'a';
					//Get corresponding shift value from the key
					char currentKeyChar = key[keyIndex % key.Length];
					int shift = char.ToUpper(currentKeyChar) - 'A';
					//Reverse the shift, handling negative modulo bounds in C#
					int decryptedValue = MathUtil.Posmod(c - offset - shift, 26);
					char decryptedChar = (char)(decryptedValue + offset);
					_ = plaintext.Append(decryptedChar);
					keyIndex++;
				}
				else
					_ = plaintext.Append(c);
			}
			return plaintext.ToString();
		}
		/// <summary>
		/// Converts the string into a float
		/// </summary>
		/// <returns>The float value</returns>
		public float ToFloat()
		{
			//Uses , as separator
			if (text.Contains(','))
				text = text.Trim(',');
			string[] strs = text.Split('.');
			string BeforeDecimal = strs.First();
			string AfterDecimal = strs.Last();
			//Has multiple '.'
			if (strs.Length > 2)
				for (int i = 1; i < strs.Length - 2; i++)
					BeforeDecimal += strs[i];
			try
			{
				float fin = (float)Convert.ToDouble(BeforeDecimal) + (float)Convert.ToDouble(AfterDecimal) / MathF.Pow(10, AfterDecimal.Length);
				return text.Contains('.') ? fin : (float)Convert.ToDouble(BeforeDecimal);
			}
			catch
			{
				return 0;
			}
		}
	}
#else

	/// <summary>
	/// Gets the amount of substring inside of a string
	/// </summary>
	/// <param name="substring">The substring to check</param>
	/// <returns>The amount of <param name="substring"/> in <param name="text"/></returns>
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
	/// <summary>
	/// Encodes a string using the Vigenere cipher
	/// </summary>
	/// <param name="key">The key to encrypt</param>
	/// <returns>The encrypted code</returns>
	public static string VigenereEncrypt(this string text, string key)
	{
		StringBuilder ciphertext = new();
		int keyIndex = 0;
		foreach (char c in text)
		{
			//Only encrypt letters
			if (char.IsLetter(c))
			{
				bool isUpper = char.IsUpper(c);
				char offset = isUpper ? 'A' : 'a';
				//Get corresponding shift value from the key
				char currentKeyChar = key[keyIndex % key.Length];
				int shift = char.ToUpper(currentKeyChar) - 'A';
				//Apply Caesar and wrap around
				char encryptedChar = (char)((c - offset + shift) % 26 + offset);
				_ = ciphertext.Append(encryptedChar);
				keyIndex++;
			}
			else
				_ = ciphertext.Append(c);
		}
		return ciphertext.ToString();
	}
	/// <summary>
	/// Decodes a string using the Vigenere cipher
	/// </summary>
	/// <param name="key">The key to decrypt</param>
	/// <returns>The decrypted text</returns>
	public static string VigenereDecrypt(this string text, string key)
	{
		StringBuilder plaintext = new();
		int keyIndex = 0;

		foreach (char c in text)
		{
			if (char.IsLetter(c))
			{
				bool isUpper = char.IsUpper(c);
				char offset = isUpper ? 'A' : 'a';
				//Get corresponding shift value from the key
				char currentKeyChar = key[keyIndex % key.Length];
				int shift = char.ToUpper(currentKeyChar) - 'A';
				//Reverse the shift, handling negative modulo bounds in C#
				int decryptedValue = MathUtil.Posmod(c - offset - shift, 26);
				char decryptedChar = (char)(decryptedValue + offset);
				_ = plaintext.Append(decryptedChar);
				keyIndex++;
			}
			else
				_ = plaintext.Append(c);
		}
		return plaintext.ToString();
	}
	/// <summary>
	/// Converts the string into a float
	/// </summary>
	/// <returns>The float value</returns>
	public static float ToFloat(this string text)
	{
		//Uses , as separator
		if (text.Contains(','))
			text = text.Trim(',');
		string[] strs = text.Split('.');
		string BeforeDecimal = strs.First();
		string AfterDecimal = strs.Last();
		//Has multiple '.'
		if (strs.Length > 2)
			for (int i = 1; i < strs.Length - 2; i++)
				BeforeDecimal += strs[i];
		try
		{
			float fin = (float)Convert.ToDouble(BeforeDecimal) + (float)Convert.ToDouble(AfterDecimal) / MathF.Pow(10, AfterDecimal.Length);
			return text.Contains('.') ? fin : (float)Convert.ToDouble(BeforeDecimal);
		}
		catch
		{
			return 0;
		}
	}
#endif
}