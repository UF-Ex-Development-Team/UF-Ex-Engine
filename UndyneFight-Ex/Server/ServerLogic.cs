using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using static UndyneFight_Ex.Server.OverallNetManager;

namespace UndyneFight_Ex.Server;

public static class ServerLogic
{
#nullable enable
	internal static TcpListener? server;
	private static Thread? listenThread;
	private static Thread? processThread;
#nullable disable
	/// <summary>
	/// Whether the server is being closed by host
	/// </summary>
	private static bool ServerIsBeingClosed = false;
	/// <summary>
	/// The list of clients connected
	/// </summary>
	public static ConcurrentBag<TcpClient> ConnectedClients { get; set; } = [];
	/// <summary>
	/// Per-client send queues for non-blocking async sends
	/// </summary>
	public static readonly ConcurrentDictionary<TcpClient, ConcurrentQueue<byte[]>> ClientSendQueues = new();
	/// <summary>
	/// Mapping from TcpClient to player's UUID (set when AddConnectedUser arrives)
	/// </summary>
	public static readonly ConcurrentDictionary<TcpClient, long> ClientToUUID = new();
	/// <summary>
	/// The invite code to the host
	/// </summary>
	public static string InviteCode { get; set; }
	/// <summary>
	/// Starts a server
	/// </summary>
	/// <param name="port">The port used to host</param>
	/// <param name="password">The password to the server</param>
	/// <exception cref="InvalidOperationException">Crashes when user already started a server</exception>
	public static void StartServer(int port, string password)
	{
		if (server != null)
			throw new InvalidOperationException("Server is already running.");
		//Initialize server
		server = new TcpListener(GetLocalIPAddress(), port);
		server.Start();
		IsSuccessfullyConnected = true;
		ServerIsBeingClosed = false;
		//Add self as a connected user
		ConnectedUsers.Add(PlayerData = new(PlayerManager.CurrentUser.PlayerName, PlayerManager.PlayerSkill, PlayerManager.CurrentUser._uuid));
		listenThread = new Thread(ListenForClients);
		listenThread.Start();
		processThread = new Thread(ReceiveData);
		processThread.Start();
		//Generate invite code
		InviteCode = EncodeRoomCodeFunc(GetLocalIPAddress(), (ushort)port, password);
		DebugState.Log($"Server started by user.", true);
	}
	/// <summary>
	/// Stops the server
	/// </summary>
	/// <exception cref="InvalidOperationException">Crashes when the server is not running</exception>
	public static void StopServer()
	{
		if (server == null)
			throw new InvalidOperationException("Server is not running.");
		//Inform the server us being closed
		ServerIsBeingClosed = true;
		//Clean up threads and data
		listenThread?.Join();
		processThread?.Join();
		server.Stop();
		server = null;
		ConnectedClients.Clear();
		ConnectedUsers.Clear();
		Reset();
		DebugState.Log("Server stopped", true);
	}
	/// <summary>
	/// The thread for listening for new joining clients
	/// </summary>
	private static async void ListenForClients()
	{
		//Will not listen if server is not started or server is being shut down
		while (server != null && !ServerIsBeingClosed)
		{
			try
			{
				TcpClient client = await server.AcceptTcpClientAsync();
				//Register client
				ConnectedClients.Add(client);
				RegisterClient(client);
				DebugState.Log($"Client connected: {client.Client.RemoteEndPoint}", true);
			}
			catch (SocketException)
			{
				//Server was stopped
				break;
			}
		}
	}
	public static void RegisterClient(TcpClient client)
	{
		ClientSendQueues[client] = new();
		//Start sender task for this client
		_ = Task.Run(() => ProcessSendQueue(client));
	}
	public static void UnregisterClient(TcpClient client)
	{
		ClientSendQueues.TryRemove(client, out _);
		ClientToUUID.TryRemove(client, out _);
	}
	private static async Task ProcessSendQueue(TcpClient client)
	{
		try
		{
			NetworkStream stream = client.GetStream();
			while (client?.Connected ?? false)
			{
				if (ClientSendQueues.TryGetValue(client, out ConcurrentQueue<byte[]> q) && q.TryDequeue(out byte[] data))
				{
					try
					{
						await stream.WriteAsync(data);
						await stream.FlushAsync();
					}
					catch
					{
						//Write failed, implement disconnect
						break;
					}
				}
				else
				{
					await Task.Delay(5);
				}
			}
		}
		catch { }
		finally
		{
			try
			{ client?.Close(); }
			catch { }
			UnregisterClient(client);
		}
	}
	/// <summary>
	/// Sends a message to an individual client
	/// </summary>
	/// <param name="client">The client to send to</param>
	/// <param name="message">The information to send</param>
	public static void SendMessageToClient(TcpClient client, ServerInformation message)
	{
		NetworkStream stream = client.GetStream();
		string serializedMessage = JsonConvert.SerializeObject(message);
		byte[] data = Encoding.UTF8.GetBytes(serializedMessage + "\n");
		stream.Write(data);
		stream.Flush();
#if DEBUG
		//DebugState.Log($"Sent to client {client.Client.RemoteEndPoint}: {serializedMessage}", true);
#endif
	}
	/// <summary>
	/// Sends a message to all clients
	/// </summary>
	/// <param name="info">The information to send</param>
	public static void SendMessageToAllClients(ServerInformation info)
	{
		if (server == null)
			DebugState.Log("Server is not running.", true);
		else
			foreach (TcpClient client in ConnectedClients)
			{
				if (client.Connected)
					SendMessageToClient(client, info);
			}
	}
	/// <summary>
	/// Thread to receive client data
	/// </summary>
	private static async void ReceiveData()
	{
		//Will not receive if server is not running or is shutting down
		while (server != null && !ServerIsBeingClosed)
		{
			int i = 0;
			foreach (TcpClient client in ConnectedClients)
			{
				while (client?.Connected ?? false)
				{
					NetworkStream stream = client.GetStream();
					try
					{
						_ = await ReadInformation(stream, client);
					}
					//Funny hard code of user alt+f4
					catch (IOException ex) when (ex.InnerException is SocketException sex && sex.NativeErrorCode == 10054)
					{
						if (ConnectedUsers.Count > i + 1)
							ConnectedUsers.RemoveAt(i + 1);
						DebugState.Log($"Error receiving data: {ex.Message}", true);
						continue;
					}
				}
				++i;
			}
		}
	}
}