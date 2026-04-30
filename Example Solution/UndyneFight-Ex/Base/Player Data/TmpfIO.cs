using System.Text;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.IO;

/// <summary>
/// Storage rules of <see cref="SaveInfo"/>:
/// <para>[] are constants，{} are an array of <see cref="values"/>, ->{} are an array of <see cref="Nexts"/>, ',' is the separator of data</para>
/// <para>Examples are as follows:</para>
/// <para>(ChampionShips)->{div=[string],score=[int],position=[int]}</para>
/// <para>PlayerName:[string],VIP:[bool]</para>
/// <para>NormalFight->{(songName):noob=[int],easy=[int],...extreme=[int]}</para>
/// <para>Achievements->{(achievementName):type=[bool],progress=[int]}</para>
/// </summary>
public class SaveInfo
{
	/// <summary>
	/// Gets the value in the save info with given string as key
	/// </summary>
	/// <param name="index">The key of the value</param>
	/// <returns>The saved value</returns>
	public string this[string index]
	{
		get => values[keysForIndexes[index]];
		set => values[keysForIndexes[index]] = value;
	}
	/// <summary>
	/// Gets the value in the save info with the given index
	/// </summary>
	/// <param name="index">The index of the save info to search for</param>
	/// <returns>The saved value</returns>
	public string this[int index]
	{
		get => values[index];
		set => values[index] = value;
	}
	/// <summary>
	/// Converts the save info to a list of strings
	/// </summary>
	/// <param name="info">The save info to convert</param>
	public static implicit operator List<string>(SaveInfo info) => IOEvent.InfoToString(info);
	/// <summary>
	/// Converts a list of strings to save info (If the format is correct)
	/// </summary>
	/// <param name="info"></param>
	public static implicit operator SaveInfo(List<string> info) => IOEvent.ToInfo(info);
	/// <summary>
	/// Converts the save info to a list of bytes
	/// </summary>
	/// <param name="info">The save info to convert</param>
	public static implicit operator List<byte>(SaveInfo info) => IOEvent.StringToByte(info);
	/// <summary>
	/// Whether the save info has nested information
	/// </summary>
	public bool HasDeepInfo;
	/// <summary>
	/// The title of the save info
	/// </summary>
	public readonly string Title;
	/// <summary>
	/// The full value of the save info, i.e. Info:A=1,B=2 will return "A=1,B=2"
	/// </summary>
	public string fullValue;
	/// <summary>
	/// The list of values in the save info
	/// </summary>
	public List<string> values;
	/// <summary>
	/// The index of the key, i.e. in A=1,B=2 -> "A" will return 0 and "B" will return 1
	/// </summary>
	public Dictionary<string, int> keysForIndexes;
	/// <summary>
	/// The key of the index, i.e. in A=1,B=2 -> "0" will return A and "1" will return B
	/// </summary>
	public Dictionary<int, string> indexForKeys;
	/// <summary>
	/// The nested save info
	/// </summary>
	public Dictionary<string, SaveInfo> Nexts;
	/// <summary>
	/// Sets the next save info
	/// </summary>
	/// <param name="mission"></param>
	/// <param name="info"></param>
	public void SetNext(string mission, string info)
	{
		if (!Nexts.TryAdd(mission, new SaveInfo(info)))
			Nexts[mission] = new SaveInfo(info);
	}
	/// <summary>
	/// The value as <see cref="float"/>
	/// </summary>
	public float FloatValue => FloatFromString(values[0]);
	/// <summary>
	/// The value as <see cref="int"/>
	/// </summary>
	public int IntValue => Convert.ToInt32(values[0]);
	/// <summary>
	/// The value as <see cref="Vector2"/>, where both values are floats
	/// </summary>
	public Vector2 VectorValue => new(FloatFromString(values[0]), FloatFromString(values[1]));
	/// <summary>
	/// The value as a <see cref="bool"/>
	/// </summary>
	public bool BoolValue => values[0] is "true" or "True";
	/// <summary>
	/// The value as <see cref="string"/>
	/// </summary>
	public string StringValue => values[0];
	/// <summary>
	/// Creates a save info with the given key
	/// </summary>
	/// <param name="val">The key and or value of the save info</param>
	public SaveInfo(string val)
	{
		string[] u1 = val.Split(':');
		Title = u1[0];
		//If there are values behind the key, i.e. key:value -> value
		if (u1.Length > 1)
		{
			fullValue = u1[1];
			values = [];
			keysForIndexes = [];
			indexForKeys = [];
			string[] units = u1[1].Split(',');
			string[] parts;
			Array.ForEach(units, s =>
			{
				parts = s.Split('=');
				if (parts.Length == 1)
					values.Add(parts[0]);
				else
				{
					keysForIndexes.Add(parts[0], values.Count);
					indexForKeys.Add(values.Count, parts[0]);
					values.Add(parts[1]);
				}
			});
		}
		else if (u1.Length == 1 && u1[0][^1] == '{')
		{
			Title = Title[..^1];
			HasDeepInfo = true;
			Nexts = [];
		}
	}
	/// <summary>
	/// Adds nested SaveInfo in <see cref="Nexts"/>
	/// </summary>
	/// <param name="info"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushNext(SaveInfo info)
	{
		Nexts ??= [];
		HasDeepInfo = true;
		if (!Nexts.TryAdd(info.Title, info))
			Nexts[info.Title] = info;
	}
	/// <summary>
	/// Gets the save info of the given key, which is a nested save info
	/// </summary>
	/// <param name="path">The key of the save info</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo GetDirectory(string path)
	{
		SaveInfo current = this;
		foreach (string item in path.Split("\\"))
			current = current.Nexts[item];
		return current;
	}
}
/// <summary>
/// Reading, writing, encoding, and decoding custom files
/// </summary>
public static class IOEvent
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static List<byte> Decoder(byte[] bytes)
	{
		for (int i = 0; i < bytes.Length; i++)
			bytes[i] = (byte)-(bytes[i] + i); //(256 - (bytes[i] + i)) % 256 is the same as -(bytes[i] + i) in bytes due to underflow
		return [.. bytes];
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte[] Encoder(List<byte> bytes)
	{
		byte[] b = [.. bytes];
		for (int i = 0; i < b.Length; i++)
			b[i] = (byte)-(bytes[i] + i); //((256 * 8192 - (bytes[i] + i)) % 256) is the same as -(bytes[i] + i) in bytes due to underflow
		return b;
	}
	/// <summary>
	/// Creates an custom encoded file
	/// </summary>
	/// <param name="Location">File path</param>
	/// <param name="bytes">File content (as <see cref="List{Byte}"/>)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteCustomFile(string Location, List<byte> bytes)
	{
		if ((bytes.Count & 1) == 0)
			bytes.Add(0);

		FileStream stream = new(Location, FileMode.OpenOrCreate);

		stream.Write(Encoder(bytes), 0, bytes.Count);

		stream.Flush();
		stream.Dispose();
	}
	/// <summary>
	/// <see cref="WriteCustomFile(string, List{byte})"/> but with a file format of '.tmpf', they function the exact same
	/// </summary>
	/// <param name="Location">File path</param>
	/// <param name="bytes">File content</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteTmpFile(string Location, List<byte> bytes) => WriteCustomFile(Location + ".Tmpf", bytes);
	/// <summary>
	/// Reads the list of bytes of the custom image file
	/// </summary>
	/// <param name="Path">The path to the file</param>
	/// <returns>The list of bytes on the image file</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<byte> ReadCustomFile(string Path) => Decoder(File.ReadAllBytes(Path));
	/// <summary>
	/// Reads the list of bytes in the tmpf file
	/// </summary>
	/// <returns>The byte list in the tmpf file</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<byte> ReadTmpfFile(string Path) => ReadCustomFile(Path + ".Tmpf");
	/// <summary>
	/// Convert a string into bytes
	/// </summary>
	/// <param name="strings">The string to convert</param>
	/// <returns>The bytes of the string</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<byte> StringToByte(string strings) => StringToByte([strings]);
	/// <summary>
	/// Converts a list of strings into bytes
	/// </summary>
	/// <param name="strings">The list of strings</param>
	/// <returns>The list of strings in bytes</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<byte> StringToByte(List<string> strings)
	{
		List<byte> bytes = [];
		System.Collections.ObjectModel.ReadOnlyCollection<string> span = strings.AsReadOnly();
		foreach (string element in span)
		{
			ReadOnlySpan<char> subspan = element.AsSpan();
			foreach (char item in subspan)
				bytes.Add((byte)item);
			bytes.Add(1);
		}
		return bytes;
	}
	/// <summary>
	/// Converts a list of bytes into string
	/// </summary>
	/// <param name="bytes">The list of bytes</param>
	/// <returns>The list of bytes in strings</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<string> ByteToString(List<byte> bytes)
	{
		List<string> strs = [];
		string temp = string.Empty;
		System.Collections.ObjectModel.ReadOnlyCollection<byte> span = bytes.AsReadOnly();
		foreach (byte curByte in span)
		{
			char item = (char)curByte;
			if (item is not ((char)1 or (char)0))
				temp += item;
			else
			{
				strs.Add(temp);
				temp = string.Empty;
			}
		}
		return strs;
	}
	/// <summary>
	/// Converts a list of string into save info
	/// </summary>
	/// <param name="strs">The list of string</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SaveInfo ToInfo(List<string> strs)
	{
		SaveInfo Last;
		Stack<SaveInfo> buffer = new();
		if (strs[0] is "StartInfo->" or "StartInfo->{")
		{
			strs.RemoveAt(0);
			strs.RemoveAt(strs.Count - 1);
		}
		buffer.Push(new SaveInfo("StartInfo->"));
		Last = buffer.Peek();
		for (int i = 0; i < strs.Count; i++)
		{
			if (string.IsNullOrWhiteSpace(strs[i]))
				continue;
			if (strs[i] == "{")
			{
				buffer.Push(Last);
				continue;
			}
			if (strs[i] is "End}" or "}")
			{
				if (buffer.Count > 1)
					_ = buffer.Pop();
				continue;
			}
			SaveInfo info = new(strs[i]);
			Last = info;
			buffer.Peek().PushNext(info);
			if (info.HasDeepInfo)
				buffer.Push(info);
		}
		return buffer.Peek();
	}
	/// <summary>
	/// Converts a save info to a list of string
	/// </summary>
	/// <param name="info">The save info to convert</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<string> InfoToString(SaveInfo info)
	{
		StringBuilder sb = new();
		List<string> res = [];
		if (info.values != null)
		{
			_ = sb.Append(info.Title + ":");
			for (int i = 0; i < info.values.Count; i++)
			{
				if (info.keysForIndexes.ContainsValue(i))
					_ = sb.Append(info.indexForKeys[i] + "=");
				_ = sb.Append(info.values[i]);
				if (i + 1 != info.values.Count)
					_ = sb.Append(',');
			}
			res.Add(sb.ToString());
		}
		else
		{
			res.Add(info.Title + "{");
			foreach (SaveInfo next in info.Nexts.Values)
				res.AddRange(InfoToString(next));
			res.Add("}");
		}
		return res;
	}
	/// <summary>
	/// Converts a save info into a list of bytes
	/// </summary>
	/// <param name="info">The save info to convert</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<byte> InfoToByte(SaveInfo info) => StringToByte(info);
}
/// <summary>
/// For reading player file data
/// </summary>
public static class FileIO
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static List<byte> Decoder(byte[] bytes)
	{
		for (int i = 0; i < bytes.Length; i++)
			bytes[i] = (byte)-(bytes[i] + i);
		return [.. bytes];
	}
	/// <summary>
	/// Reads the list of bytes in the tmpf file and converts it to <see cref="SaveInfo"/>
	/// </summary>
	/// <returns>The save info of the tmpf file</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SaveInfo ReadFile(string Path)
	{
		FileStream stream = new(Path + ".Tmpf", FileMode.OpenOrCreate);

		byte[] res1 = new byte[stream.Length];
		_ = stream.Read(res1, 0, res1.Length);

		List<string> res = IOEvent.ByteToString(Decoder(res1));

		if (res?.Count == 0)
			return null;
#if DEBUG
		string tmp = string.Empty;
		lock (res)
		{
			int tabCount = 0;
			foreach (string item in res)
			{
				tmp += item + "\n";
				if (item.EndsWith('{'))
				{
					tmp = tmp[..^2] + "\n";
					for (int i = 0; i < tabCount; i++)
						tmp += "\t";
					tmp += "{\n";
					tabCount++;
				}
				else if (item.EndsWith('}'))
				{
					tmp = tmp[..^3] + "}\n";
					tabCount--;
				}
				for (int i = 0; i < tabCount; i++)
					tmp += "\t";
			}
			if (File.Exists(Path + " Data.txt"))
				File.Delete(Path + " Data.txt");
			FileStream stream2 = new(Path + " Data.txt", FileMode.OpenOrCreate);
			StreamWriter textWriter = new(stream2);
			textWriter.Write(tmp);
			textWriter.Flush();
			stream2.Dispose();
		}
#endif
		stream.Flush();
		stream.Dispose();

		return res;
	}
	/// <summary>
	/// Creates a new player file
	/// </summary>
	/// <param name="playerName">The name of the player</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void CreatePlayerFile(string playerName)
	{
		List<string> formals = ["PlayerName:" + playerName, "VIP:false", "NormalFight{", "}"];
		IOEvent.WriteTmpFile(Path.Combine($"{GameStates.SavePath}\\Datas\\Users\\{playerName}".Split('\\')), IOEvent.StringToByte(formals));
	}
	/// <summary>
	/// Creates a player file using existing save info data
	/// </summary>
	/// <param name="info">The save info of the player</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CreatePlayerFile(SaveInfo info) => IOEvent.WriteTmpFile(Path.Combine($"{GameStates.SavePath}\\Datas\\Users\\{info.Nexts["PlayerName"].StringValue}".Split('\\')), IOEvent.StringToByte(info));
}