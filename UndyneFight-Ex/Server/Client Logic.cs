using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.Server.OverallNetManager;

namespace UndyneFight_Ex.Server;

public static class ClientLogic
{
#nullable enable
	internal static TcpClient? client;
	private static Thread? receiveThread;
#nullable disable
	/// <summary>
	/// Action to execute when connection going to be established
	/// </summary>
	public static Action PendingConnection { get; set; }
	/// <summary>
	/// Action to execute when connection is successfully established
	/// </summary>
	public static Action OnSuccessfulConnection { get; set; }
	/// <summary>
	/// Action to execute when the connection attempt failed
	/// </summary>
	public static Action OnConnectionFailure { get; set; }
	/// <summary>
	/// Action to execute when the client is disconnected
	/// </summary>
	public static Action OnDisconnect { get; set; }
	/// <summary>
	/// Grace period for disconnecting (10 seconds)
	/// </summary>
	private const int DisconnectGracePeriod = 125 * 10;
	/// <summary>
	/// The amount of frames the user is disconnected for
	/// </summary>
	private static int DisconnectGraceCounter = 0;
	/// <summary>
	/// Connects to a server with the given IP and port
	/// </summary>
	/// <param name="ip">The IP of the host</param>
	/// <param name="port">The port used by the host</param>
	/// <exception cref="InvalidOperationException"></exception>
	public static async void ConnectToServer(string ip, ushort port)
	{
		if (client != null)
			throw new InvalidOperationException("Already connected to a server.");
		//Connection time out in 10 seconds
		using CancellationTokenSource cts = new(10 * 1000);
		client = new TcpClient();
		IPAddress IP = IPAddress.Parse(ip);
		//Attempt connection
		try
		{
			PendingConnection?.Invoke();
#if DEBUG
			DebugState.Log($"Attempting to connect to server at {ip}:{port}", true);
#else
			DebugState.Log($"Attempting to connect to server.", true);
#endif
			await client.ConnectAsync(IP, port, cts.Token);
			//Send user data to server
			SendMessageToServer(new ServerInformation(ServerPacketInformationType.AddConnectedUser)
			{
				ServerUserData = [PlayerData = new ConnectedUserData(PlayerManager.CurrentUser.PlayerName, PlayerManager.PlayerSkill, PlayerManager.CurrentUser._uuid)]
			});
			IsSuccessfullyConnected = true;
			OnSuccessfulConnection?.Invoke();
#if DEBUG
			DebugState.Log($"Connected to server at {ip}:{port}", true);
#else
			DebugState.Log($"Connected to server.", true);
#endif
		}
		//Time out
		catch (OperationCanceledException)
		{
			OnConnectionFailure?.Invoke();
			client?.Close();
			client = null;
			DebugState.Log("Connection failed: Timed out.", true);
			DisconnectFromServer();
		}
		//Begin thread for receiving data from host
		receiveThread = new Thread(ReceiveData);
		receiveThread.Start();
	}
	/// <summary>
	/// Disconnects the user from the server
	/// </summary>
	public static void DisconnectFromServer()
	{
		if (client == null)
		{
			DebugState.Log("Not connected to any server.", true);
			return;
		}
		//Cleans up threads and data
		OnDisconnect?.Invoke();
		if (client.Connected)
		{
			client.GetStream().Close();
			client.Close();
		}
		receiveThread?.Join();
		client = null;
		Reset();
		//Fallback to menu
		GameStates.ResetScene(new GameMenuScene());
		DebugState.Log("Disconnected from server.", true);
	}
	/// <summary>
	/// Receiving data from host
	/// </summary>
	private static async void ReceiveData()
	{
		while (true)
		{
			if (!IsConnectedToServer())
			{
				DisconnectGraceCounter++;
				if (DisconnectGraceCounter >= DisconnectGracePeriod)
				{
					DisconnectFromServer();
					break;
				}
			}
			NetworkStream stream = client.GetStream();
			try
			{
				if (!await ReadInformation(stream))
				{
					DisconnectGraceCounter++;
					if (DisconnectGraceCounter >= DisconnectGracePeriod)
						DisconnectFromServer();
					break;
				}
			}
			catch (Exception ex)
			{
				DebugState.Log($"Error receiving data: {ex.Message}", true);
				if (ex is SocketException or OperationCanceledException)
				{
					DebugState.Log("Server unavailable.", true);
					DisconnectFromServer();
				}
				break;
			}
		}
	}
	/// <summary>
	/// Sends a message to the server
	/// </summary>
	/// <param name="message">The message to send to the server</param>
	public static async void SendMessageToServer(ServerInformation message)
	{
		if (!IsConnectedToServer())
			return;
		try
		{
			NetworkStream stream = client.GetStream();
			string serializedMessage = JsonConvert.SerializeObject(message);
			byte[] data = Encoding.UTF8.GetBytes(serializedMessage + "\n");
			await stream.WriteAsync(data);
			await stream.FlushAsync();
		}
		catch (Exception ex)
		{
			DebugState.Log($"Error sending message to server: {ex.Message}", true);
		}
	}
	public static async void SendRawInputPacket(bool[] keysHeld, vec2[] soulPositions, float[] soulRotations)
	{
		if (!IsConnectedToServer())
			return;
		try
		{
			List<byte> payload = [];
			InputUpdateFlags flags = 0;
			//Pack keys
			if (keysHeld != null)
			{
				flags |= InputUpdateFlags.Keys;
				byte[] keyBytes = PackBoolArray(keysHeld, SyncKeysCount);
				payload.AddRange(keyBytes);
			}
			//Pack positions
			if (soulPositions != null)
			{
				flags |= InputUpdateFlags.SoulPositions;
				byte count = (byte)MathHelper.Clamp(soulPositions.Length, 0, 255);
				payload.Add(count);
				payload.AddRange(SerializeVector2Array(soulPositions));
			}
			//Pack rotations
			if (soulRotations != null)
			{
				flags |= InputUpdateFlags.SoulRotations;
				byte count = (byte)MathHelper.Clamp(soulRotations.Length, 0, 255);
				payload.Add(count);
				payload.AddRange(SerializeFloatArray(soulRotations));
			}
			//Binary format: Magic, Type, Length(ushort), Payload
			const byte MagicByte = 0x1B;
			const byte InfoType = 1;
			ReadOnlySpan<byte> payloadArr = [.. payload];
			ushort len = (ushort)payloadArr.Length;
			using MemoryStream ms = new();
			ms.WriteByte(MagicByte);
			ms.WriteByte(InfoType);
			ms.Write(BitConverter.GetBytes(len), 0, 2);
			ms.Write(payloadArr);
			byte[] framed = ms.ToArray();

			NetworkStream stream = client.GetStream();
			await stream.WriteAsync(framed);
			await stream.FlushAsync();
		}
		catch (Exception ex)
		{
			DebugState.Log($"Error sending raw input packet: {ex.Message}", true);
		}
	}
	private static byte[] PackBoolArray(bool[] arr, int expectedCount)
	{
		byte[] outb = new byte[(expectedCount + 7) / 8];
		for (int i = 0; i < expectedCount && i < arr.Length; i++)
			if (arr[i])
				outb[i / 8] |= (byte)(1 << (i % 8));
		return outb;
	}
	/// <summary>
	/// Whether the user is connected to the host server
	/// </summary>
	/// <returns></returns>
	public static bool IsConnectedToServer() => client?.Connected ?? false;
}