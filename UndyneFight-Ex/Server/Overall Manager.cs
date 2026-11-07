using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sounds;

namespace UndyneFight_Ex.Server;

public static class OverallNetManager
{
	/// <summary>
	/// Whether the player is connected as the server owner
	/// </summary>
	public static bool IsConnectedAsServer { get; set; } = false;
	/// <summary>
	/// Whether the player is connected as a server client
	/// </summary>
	public static bool IsConnectedAsClient { get; set; } = false;
	/// <summary>
	/// Whether the player is successfully connected to a server or not
	/// </summary>
	public static bool IsSuccessfullyConnected { get; internal set; } = false;
	/// <summary>
	/// The list of connected users
	/// </summary>
	public static List<ConnectedUserData> ConnectedUsers { get; set; } = [];
	/// <summary>
	/// The owner of the server, which is the first user to connect. This user has special permissions and is responsible for managing the server.
	/// </summary>
	public static ConnectedUserData ServerOwner => ConnectedUserCount > 0 ? ConnectedUsers[0] : null;
	/// <summary>
	/// The amount of connected users
	/// </summary>
	public static int ConnectedUserCount => ConnectedUsers.Count;
	/// <summary>
	/// Function for compiling a room code (IP, Port, Password, Output string)
	/// </summary>
	public static Func<IPAddress, ushort, string, string> EncodeRoomCodeFunc { get; set; }
	/// <summary>
	/// Function for decompiling a room code (Code, Password, (Successful, IP, Port))
	/// </summary>
	public static Func<string, string, (bool, IPAddress, ushort)> DecodeRoomCodeFunc { get; set; }
	/// <summary>
	/// The netplay player data of the player, will automatically send the new information to server once modified
	/// </summary>
	public static ConnectedUserData PlayerData
	{
#if NET10_0_OR_GREATER
		get;
#else
		get => _playerData;
#endif
		set
		{
#if NET10_0_OR_GREATER
			field = value;
#else
			_playerData = value;
#endif
			if (!IsSuccessfullyConnected)
				return;
			_playerDataWasModified = true;
		}
	}
#if !NET10_0_OR_GREATER
	private static ConnectedUserData _playerData;
#endif
	internal static bool _playerDataWasModified = false;
	/// <summary>
	/// Essential player data used in netplay
	/// </summary>
	/// <param name="name">The name of the player</param>
	/// <param name="skill">The rating of the player</param>
	/// <param name="UUID">The unique user ID of the playert</param>
	public class ConnectedUserData(string name, float skill, long UUID)
	{
		/// <summary>
		/// The username of the player
		/// </summary>
		[JsonProperty("name")]
		public string PlayerName = name;
		/// <summary>
		/// The rating of the player
		/// </summary>
		[JsonProperty("rt")]
		public float Skill = skill;
		/// <summary>
		/// Whether the player will have their rating hidden in netplay UI
		/// </summary>
		[JsonProperty("hSkill")]
		public bool HideSkill { get; set; } = false;
		/// <summary>
		/// The unique user ID of the player
		/// </summary>
		public long UUID { get; init; } = UUID;
		/// <summary>
		/// Whether the user is ready to play the chart
		/// </summary>
		[JsonProperty("ready")]
		public bool IsReady { get; set; } = false;
		/// <summary>
		/// Whether the player is playing a chart
		/// </summary>
		[JsonProperty("playing")]
		public bool IsPlayingAChart { get; set; } = false;
		#region Chart playing data
		/// <summary>
		/// Fight Name of the selected chart
		/// </summary>
		[JsonProperty("selCh")]
		public string SelectedChart { get; set; } = null;
		/// <summary>
		/// 3 -> Strict; 2 -> Balanced; 1 -> Lenient
		/// </summary>
		[JsonProperty("selJd")]
		public byte SelectedJudgement { get; set; } = 1;
		[JsonProperty("selDif")]
		public byte SelectedDifficulty { get; set; } = 0;
		[JsonProperty("selGM")]
		public byte SelectedGameMode { get; set; } = 0;
		public int Seed { get; set; } = 0;
		[JsonProperty("cSco")]
		public int CurrentScore { get; set; } = 0;
		[JsonProperty("cBc")]
		public int CurrentBarrageCount { get; set; } = 0;
		[JsonProperty("cAcc")]
		public float CurrentAccuracy { get; set; } = 0;
		[JsonProperty("cHP")]
		public float CurrentHP { get; set; } = 1;
		[JsonProperty("cKR")]
		public float CurrentKR { get; set; } = 0;
		[JsonProperty("cMHP")]
		public float CurrentMaxHP { get; set; } = 1;
		[JsonProperty("MC")]
		public int MissCount { get; set; } = 0;
		[JsonProperty("OC")]
		public int OkayCount { get; set; } = 0;
		[JsonProperty("NC")]
		public int NiceCount { get; set; } = 0;
		[JsonProperty("PLC")]
		public int PerfectLCount { get; set; } = 0;
		[JsonProperty("PEC")]
		public int PerfectECount { get; set; } = 0;
		[JsonProperty("PC")]
		public int PerfectCount { get; set; } = 0;
		[JsonProperty("live")]
		public bool IsAlive { get; set; } = true;
		[JsonProperty("sTime")]
		public float SurviveTime { get; set; } = 0;
		[JsonProperty("cEnd")]
		public bool ChartEnded { get; set; } = false;
		/// <summary>
		/// Decodes into Vector2[], [SoulIndex].Position
		/// </summary>
		[JsonProperty("sPos")]
		public byte[] SoulPosition { get; set; } = [];
		/// <summary>
		/// Decodes into float[], [SoulIndex].Rotation
		/// </summary>
		[JsonProperty("sRot")]
		public byte[] SoulRotation { get; set; } = [];
		/// <summary>
		/// Decodes into bool[] with 17 bytes, recording all 4 sets of 4 input directions and Confirm
		/// </summary>
		[JsonProperty("kH")]
		public byte[] KeysHeld { get; set; } = [];
		#endregion
	}
	public readonly struct ChartData(ConnectedUserData userData)
	{
		/// <summary>
		/// Fight Name of the selected chart
		/// </summary>
		public readonly string SelectedChart = userData.SelectedChart;
		/// <summary>
		/// 3 -> Strict; 2 -> Balanced; 1 -> Lenient
		/// </summary>
		public readonly JudgementState SelectedJudgement = (JudgementState)userData.SelectedJudgement;
		public readonly Difficulty SelectedDifficulty = (Difficulty)userData.SelectedDifficulty;
		public readonly GameMode SelectedGameMode = (GameMode)userData.SelectedGameMode;
		public readonly int CurrentScore = userData.CurrentScore;
		public readonly int CurrentBarrageCount = userData.CurrentBarrageCount;
		public readonly float CurrentAccuracy = userData.CurrentAccuracy;
		public readonly float CurrentHP = userData.CurrentHP;
		public readonly float CurrentKR = userData.CurrentKR;
		public readonly float CurrentMaxHP = userData.CurrentMaxHP;
		public readonly int MissCount = userData.MissCount;
		public readonly int OkayCount = userData.OkayCount;
		public readonly int NiceCount = userData.NiceCount;
		public readonly int PerfectLCount = userData.PerfectLCount;
		public readonly int PerfectECount = userData.PerfectECount;
		public readonly int PerfectCount = userData.PerfectCount;
		public readonly bool IsAlive = userData.IsAlive;
		public readonly float SurviveTime = userData.SurviveTime;
		public readonly bool ChartEnded = userData.ChartEnded;
		/// <summary>
		/// Decodes into Vector2[], [SoulIndex].Position
		/// </summary>
		public readonly Vector2[] SoulPosition = DeserializeVector2Array(userData.SoulPosition);
		/// <summary>
		/// Decodes into float[], [SoulIndex].Rotation
		/// </summary>
		public readonly float[] SoulRotation = DeserializeFloatArray(userData.SoulRotation);
		/// <summary>
		/// Decodes into bool[] with 17 bytes, recording all 4 sets of 4 input directions and Confirm
		/// </summary>
		public readonly bool[] KeysHeld = DeserializeBoolArray(userData.KeysHeld);
	}
	public static readonly SortedList<float, ChartData> TimedChartData = [];
	public static ChartData LatestChartData => TimedChartData.Last().Value;
	/// <summary>
	/// Information to send to the server
	/// </summary>
	/// <param name="type">The type of information to send</param>
	public class ServerInformation(ServerPacketInformationType type)
	{
		/// <summary>
		/// The type of server information to send/receive
		/// </summary>
		public ServerPacketInformationType ServerPacketInformationType = type;
		/// <summary>
		/// The player data to send/receive
		/// </summary>
		public ConnectedUserData[] ServerUserData;
		/// <summary>
		/// A normal string message
		/// </summary>
		public string StringMessage;
	}
	/// <summary>
	/// Types of information the server can send/receive
	/// </summary>
	public enum ServerPacketInformationType : byte
	{
		/// <summary>
		/// A normal string message
		/// </summary>
		StringMessage,
		/// <summary>
		/// Fetch the list of connected users (Client to server)
		/// </summary>
		ReceiveConnectedUsers,
		/// <summary>
		/// Request server to add a new user (On client connection)
		/// </summary>
		AddConnectedUser,
		/// <summary>
		/// Send to everyone in the server to modify user data (Client/Server to all)
		/// </summary>
		ModifyConnectedUser,
		/// <summary>
		/// Tell all clients the chart is about to begin
		/// </summary>
		StartChart,
	}
	/// <summary>
	/// Gets the default IP address of the user for connecting to the TCP client (Prioritizes VPN IP over actual machine IP so people won't dox themselves anyways)
	/// </summary>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static IPAddress GetLocalIPAddress()
	{
		IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
			if (ip.AddressFamily == AddressFamily.InterNetwork)
				return ip;
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}
	private static bool IsValidJson(string jsonString)
	{
		if (string.IsNullOrWhiteSpace(jsonString))
			return false;
		if ((jsonString.StartsWith('{') && jsonString.EndsWith('}')) || (jsonString.StartsWith('[') && jsonString.EndsWith(']')))
		{
			try
			{
				_ = JToken.Parse(jsonString);
				return true;
			}
			catch (JsonReaderException)
			{
				return false;
			}
		}
		return false;
	}
	/// <summary>
	/// Reads the received network packet and parse the information
	/// </summary>
	/// <param name="buffer">The packet received</param>
	/// <param name="bytesRead">The amount of bytes read</param>
	private static void ProcessInformation(byte[] buffer, TcpClient sourceClient = null)
	{
		//Parse all messages
		foreach (string message in Encoding.UTF8.GetString(buffer).Trim().Trim('\n').Split('\n'))
		{
			//Check if message received is information
			if (!IsValidJson(message))
				continue;
			ServerInformation information = JsonConvert.DeserializeObject<ServerInformation>(message);
			switch (information.ServerPacketInformationType)
			{
				//Client request user list
				case ServerPacketInformationType.ReceiveConnectedUsers:
					ConnectedUsers = [.. information.ServerUserData];
					DebugState.Log("Client receiving user list", true);
					break;
				//Receive string message from another user
				case ServerPacketInformationType.StringMessage:
					DebugState.Log(information.StringMessage);
					break;
				//Add connected user when a new player joins
				case ServerPacketInformationType.AddConnectedUser:
					ConnectedUsers.AddRange(information.ServerUserData);
					//If on server, register client ID
					if (sourceClient != null && information.ServerUserData?.Length > 0)
						ServerLogic.ClientToUUID[sourceClient] = information.ServerUserData[0].UUID;
					//Update client side of user list
					ServerLogic.SendMessageToAllClients(new ServerInformation(ServerPacketInformationType.ReceiveConnectedUsers)
					{
						ServerUserData = [.. ConnectedUsers.Cast<ConnectedUserData>()]
					});
					DebugState.Log("Added new user to user list", true);
					break;
				//When a user has modified its user data, resend new data to all clients
				case ServerPacketInformationType.ModifyConnectedUser:
					ConnectedUserData newData = information.ServerUserData[0];
					//Find the user to replace
					for (int i = 0; i < ConnectedUserCount; i++)
						if (ConnectedUsers[i].UUID == newData.UUID)
						{
							ConnectedUsers[i] = newData;
							if (newData.IsPlayingAChart && newData.IsAlive)
								_ = TimedChartData.TryAdd(newData.SurviveTime, new ChartData(newData));
							break;
						}
					//Update client side of user list if sender is server
					if (IsConnectedAsServer)
						ServerLogic.SendMessageToAllClients(new ServerInformation(ServerPacketInformationType.ReceiveConnectedUsers)
						{
							ServerUserData = [.. ConnectedUsers.Cast<ConnectedUserData>()]
						});
					else //If server has modified global data (i.e. chart data)
					{
						if (ServerOwner != null)
						{
							ConnectedUserData curDat = PlayerData;
							curDat.SelectedChart = ServerOwner.SelectedChart;
							curDat.SelectedDifficulty = ServerOwner.SelectedDifficulty;
							curDat.SelectedJudgement = ServerOwner.SelectedJudgement;
							curDat.SelectedGameMode = ServerOwner.SelectedGameMode;
							PlayerData = curDat;
						}
					}
					//DebugState.Log("A user has modified data, now sending new user data.", true);
					break;
				//When server starts a chart
				case ServerPacketInformationType.StartChart:
					if (IsConnectedAsClient)
						BeginChart();
					DebugState.Log("Host has started a chart", true);
					break;
			}
		}
	}
	/// <summary>
	/// Reads information with the given stream
	/// </summary>
	/// <param name="stream">The stream to read from</param>
	/// <param name="sourceClient">Optional source client (server side)</param>
	/// <returns>Whether the information was read successfully</returns>
	internal static async Task<bool> ReadInformation(NetworkStream stream, TcpClient sourceClient = null)
	{
		//4kb buffer
		byte[] buffer = new byte[4096];
		int bytesRead = await stream.ReadAsync(buffer);
		if (bytesRead == 0)
			return false;
		for (int k = 0; k < bytesRead; k++)
		{
			byte currentByte = buffer[k];
			_accumulator.Add(currentByte);
			//Binary magic at the start overrides newline split
			if (_accumulator.Count >= 1 && _accumulator[0] == BinaryMagic)
			{
				//Format: 1(magic) + 1(type) + 2(length)
				if (_accumulator.Count >= 4)
				{
					ushort len = BitConverter.ToUInt16(_accumulator.GetRange(2, 2).ToArray());
					if (_accumulator.Count >= 4 + len)
					{
						byte type = _accumulator[1];
						byte[] payload = [.. _accumulator.GetRange(4, len)];
						//Remove consumed bytes
						_accumulator.RemoveRange(0, 4 + len);
						ProcessBinaryInformation(type, payload, sourceClient);
					}
				}
			}
			else if (currentByte == Delimiter && k != bytesRead - 1)
			{
				//Extract the full json string (excluding the \n)
				Array.Clear(_jsonBytes);
				_jsonBytes = [.. _accumulator];
				//Clear the buffer for potential newer message
				_accumulator.Clear();
			}
		}
		//Process the latest json object
		if (_accumulator.Count > 0)
		{
			//Try split by newline
			int newlineIdx = _accumulator.LastIndexOf(Delimiter);
			if (newlineIdx >= 0)
			{
				byte[] jsonSlice = [.. _accumulator.GetRange(0, newlineIdx)];
				ProcessInformation(jsonSlice, sourceClient);
				_accumulator.RemoveRange(0, newlineIdx + 1);
			}
		}
		return true;
	}
	private const byte Delimiter = (byte)'\n';
	private const byte BinaryMagic = 0x1B;
	private static readonly List<byte> _accumulator = [];
	private static byte[] _jsonBytes = new byte[4096];
	/// <summary>
	/// Starts a chart across all connected users, only executed by host
	/// </summary>
	public static void BeginChart()
	{
		InitializePlayerDataForChart();
		//Apply a 3 second warning
		for (int i = 0; i < 3; i++)
			DelayEventProcessor.AddInstantEvent(i * 60, () => PlaySound(Warning));
		IWaveSet chart = FightSystem.AllWaves[PlayerData.SelectedChart].Wave;
		string dir = GlobalData.GetTruePath($"Content\\Musics\\{chart.Music}");
		if (Directory.Exists(dir))
			dir = Path.Combine(dir, "song");
		DelayEventProcessor.AddInstantEvent(180, () =>
		{
			PlayerData.Seed = ServerOwner.Seed;
			GameStates.StartSong(chart, GlobalData.WavePaint.TryGetValue(chart.FightName, out Texture2D paint) ? paint : null, dir, PlayerData.SelectedDifficulty, (JudgementState)PlayerData.SelectedJudgement, (GameMode)PlayerData.SelectedGameMode);
		});
	}
	#region Serializing
	public static byte[] SerializeVector2Array(Vector2[] arr)
	{
		byte[] buffer = new byte[arr.Length * 8];
		for (int i = 0; i < arr.Length; i++)
		{
			Vector2 cur = arr[i];
			int curI = i * 8;
			byte[] xBytes = BitConverter.GetBytes(cur.X);
			byte[] yBytes = BitConverter.GetBytes(cur.Y);
			Buffer.BlockCopy(xBytes, 0, buffer, curI, 4);
			Buffer.BlockCopy(yBytes, 0, buffer, curI + 4, 4);
		}
		return buffer;
	}
	public static Vector2[] DeserializeVector2Array(byte[] data)
	{
		if (data.Length == 0)
			return null;
		int vecLen = data.Length / 8;
		Vector2[] vectors = new Vector2[vecLen];
		for (int i = 0; i < vecLen; i++)
		{
			int curI = i * 8;
			float x = BitConverter.ToSingle(data, curI);
			float y = BitConverter.ToSingle(data, curI + 4);
			vectors[i] = new Vector2(x, y);
		}
		return vectors;
	}
	public static byte[] SerializeIntDoubleArray(int[][] arr)
	{
		using MemoryStream ms = new();
		using BinaryWriter writer = new(ms);
		writer.Write(arr.Length);
		for (int i = 0; i < arr.Length; i++)
		{
			writer.Write(arr[i].Length);
			for (int j = 0; j < arr[i].Length; j++)
				writer.Write(arr[i][j]);
		}
		return ms.ToArray();
	}
	public static int[][] DeserializeIntDoubleArray(byte[] data)
	{
		if (data.Length == 0)
			return null;
		using MemoryStream ms = new(data);
		using BinaryReader reader = new(ms);
		int arrayCount = reader.ReadInt32();
		int[][] arr = new int[arrayCount][];
		for (int i = 0; i < arrayCount; i++)
		{
			int innerLength = reader.ReadInt32();
			arr[i] = new int[innerLength];
			for (int j = 0; j < innerLength; j++)
				arr[i][j] = reader.ReadInt32();
		}
		return arr;
	}
	public static byte[] SerializeFloatArray(float[] arr) => [.. MemoryMarshal.Cast<float, byte>(arr)];
	public static float[] DeserializeFloatArray(byte[] data) => [.. MemoryMarshal.Cast<byte, float>(data)];
	public static byte[] SerializeBoolArray(bool[] arr) => [.. MemoryMarshal.Cast<bool, byte>(arr)];
	public static bool[] DeserializeBoolArray(byte[] data) => [.. MemoryMarshal.Cast<byte, bool>(data)];
	/// <summary>
	/// Process compact binary packets received from clients (framed by ReadInformation)
	/// </summary>
	/// <param name="type">The type of information sent</param>
	/// <param name="payload">The information payload</param>
	/// <param name="sourceClient">The client that sent the informatin</param>
	private static void ProcessBinaryInformation(byte type, byte[] payload, TcpClient sourceClient)
	{
		if (type != 1 || payload == null || payload.Length == 0)
			return;
		InputUpdateFlags flags = (InputUpdateFlags)payload[0];
		int idx = 1;
		bool[] keys = null;
		Vector2[] positions = null;
		float[] rotations = null;
		//Read keys
		if ((flags & InputUpdateFlags.Keys) != 0)
		{
			int keyBytesLen = (SyncKeysCount + 7) / 8;
			if (idx + keyBytesLen <= payload.Length)
			{
				byte[] keyBytes = payload[idx..(idx + keyBytesLen)];
				keys = UnpackBoolArray(keyBytes, SyncKeysCount);
				idx += keyBytesLen;
			}
		}
		//Read soul positions
		if ((flags & InputUpdateFlags.SoulPositions) != 0)
		{
			if (idx < payload.Length)
			{
				byte count = payload[idx++];
				int bytesNeeded = count * 8;
				if (idx + bytesNeeded <= payload.Length)
				{
					byte[] posBytes = payload[idx..(idx + bytesNeeded)];
					positions = DeserializeVector2Array(posBytes);
					idx += bytesNeeded;
				}
			}
		}
		//Read soul rotations
		if ((flags & InputUpdateFlags.SoulRotations) != 0)
		{
			if (idx < payload.Length)
			{
				byte count = payload[idx++];
				int bytesNeeded = count * 4;
				if (idx + bytesNeeded <= payload.Length)
				{
					byte[] rotBytes = payload[idx..(idx + bytesNeeded)];
					rotations = DeserializeFloatArray(rotBytes);
					idx += bytesNeeded;
				}
			}
		}

		//Update server-side ConnectedUsers entry mapped to this TcpClient
		if (sourceClient == null || ServerLogic.ClientToUUID.TryGetValue(sourceClient, out long uuid))
			return;
		for (int i = 0; i < ConnectedUsers.Count; i++)
		{
			if (ConnectedUsers[i].UUID != uuid)
				continue;
			if (keys != null)
				ConnectedUsers[i].KeysHeld = SerializeBoolArray(keys);
			if (positions != null)
				ConnectedUsers[i].SoulPosition = SerializeVector2Array(positions);
			if (rotations != null)
				ConnectedUsers[i].SoulRotation = SerializeFloatArray(rotations);
			break;
		}
	}
	internal enum InputUpdateFlags : byte
	{
		Keys = 0x1,
		SoulPositions = 0x2,
		SoulRotations = 0x4
	}
	private static bool[] UnpackBoolArray(byte[] data, int expectedCount)
	{
		bool[] res = new bool[expectedCount];
		for (int i = 0; i < expectedCount; i++)
		{
			int byteIndex = i / 8;
			int bitIndex = i % 8;
			res[i] = byteIndex < data.Length && (data[byteIndex] & (1 << bitIndex)) != 0;
		}
		return res;
	}
	internal const int SyncKeysCount = 17;
	public static byte[] SerializeBoolTripleArray(bool[][][] arr)
	{
		List<bool> boolList = [];
		for (int i = 0; i < arr.Length; i++)
			for (int j = 0; j < arr[i].Length; j++)
				for (int k = 0; k < arr[i][j].Length; k++)
					boolList.Add(arr[i][j][k]);
		BitArray bitArray = new([.. boolList]);
		byte[] bytes = new byte[(bitArray.Length + 7) / 8];
		bitArray.CopyTo(bytes, 0);
		return bytes;
	}
	public static bool[][][] DeserializeBoolTripleArray(byte[] bytes, int dim1, int dim2, int dim3)
	{
		BitArray bitArray = new(bytes);
		bool[][][] array3D = new bool[dim1][][];
		int flatIndex = 0;
		for (int i = 0; i < dim1; i++)
		{
			array3D[i] = new bool[dim2][];
			for (int j = 0; j < dim2; j++)
			{
				array3D[i][j] = new bool[dim3];
				for (int k = 0; k < dim3; k++)
					array3D[i][j][k] = bitArray[flatIndex++];
			}
		}
		return array3D;
	}
	#endregion
	public static void InitializePlayerDataForChart()
	{
		ConnectedUserData curDat = PlayerData;
		curDat.IsAlive = true;
		curDat.CurrentScore = 0;
		curDat.CurrentAccuracy = 0;
		curDat.CurrentMaxHP = 1;
		curDat.CurrentHP = 1;
		curDat.CurrentKR = 0;
		curDat.SurviveTime = 0;
		curDat.ChartEnded = false;
		curDat.IsPlayingAChart = true;
		curDat.MissCount = curDat.OkayCount = curDat.NiceCount = curDat.PerfectECount = curDat.PerfectLCount = curDat.PerfectCount = 0;
		PlayerData = curDat;
	}
	/// <summary>
	/// Resets all netplay data
	/// </summary>
	public static void Reset()
	{
		IsSuccessfullyConnected = false;
		IsConnectedAsClient = false;
		IsConnectedAsServer = false;
		PlayerData = null;
		_playerDataWasModified = false;
		ConnectedUsers.Clear();
	}
}