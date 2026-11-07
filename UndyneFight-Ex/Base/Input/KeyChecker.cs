using Microsoft.Xna.Framework.Input;
using System.Text;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.Server;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.GameStates.KeyChecker;

namespace UndyneFight_Ex;

public static partial class GameStates
{
	#region keys
	internal class KeyChecker
	{
		/// <summary>
		/// The list of default keys for each input identity
		/// </summary>
		public static readonly Dictionary<InputIdentity, List<Keys>> DefaultKeys = new()
		{
			[InputIdentity.Confirm] = [Keys.Enter, Keys.Z],
			[InputIdentity.Cancel] = [Keys.LeftShift, Keys.X],
			[InputIdentity.Alternate] = [Keys.Space],
			[InputIdentity.Special] = [Keys.C],
			[InputIdentity.MainRight] = [Keys.Right],
			[InputIdentity.MainDown] = [Keys.Down],
			[InputIdentity.MainLeft] = [Keys.Left],
			[InputIdentity.MainUp] = [Keys.Up],
			[InputIdentity.SecondRight] = [Keys.D],
			[InputIdentity.SecondDown] = [Keys.S],
			[InputIdentity.SecondLeft] = [Keys.A],
			[InputIdentity.SecondUp] = [Keys.W],
			[InputIdentity.ThirdRight] = [Keys.N],
			[InputIdentity.ThirdDown] = [Keys.B],
			[InputIdentity.ThirdLeft] = [Keys.V],
			[InputIdentity.ThirdUp] = [Keys.G],
			[InputIdentity.FourthRight] = [Keys.L],
			[InputIdentity.FourthDown] = [Keys.K],
			[InputIdentity.FourthLeft] = [Keys.J],
			[InputIdentity.FourthUp] = [Keys.I],
			[InputIdentity.FullScreen] = [Keys.F4],
			[InputIdentity.ScreenShot] = [Keys.F12],
			[InputIdentity.Number1] = [Keys.D1],
			[InputIdentity.Number2] = [Keys.D2],
			[InputIdentity.Number3] = [Keys.D3],
			[InputIdentity.Number4] = [Keys.D4],
			[InputIdentity.Number5] = [Keys.D5],
			[InputIdentity.Number6] = [Keys.D6],
			[InputIdentity.Number7] = [Keys.D7],
			[InputIdentity.Number8] = [Keys.D8],
			[InputIdentity.Number9] = [Keys.D9],
			[InputIdentity.Number0] = [Keys.D0],
			[InputIdentity.Backspace] = [Keys.Back],
			[InputIdentity.Reset] = [Keys.R],
			[InputIdentity.Heal] = [Keys.H],
			[InputIdentity.Tab] = [Keys.Tab],
			[InputIdentity.QuickRestart] = [Keys.F2],
		};
		/// <summary>
		/// The current key bindings for each input identity
		/// </summary>
		public static Dictionary<InputIdentity, List<Keys>> InputKeys { get; set; } = new(DefaultKeys);
		/// <summary>
		/// Sets the key bindings for an input identity
		/// </summary>
		/// <param name="identity">The input identity to set to</param>
		/// <param name="mission">The keys assigned to the identity</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void SetIdentityKey(InputIdentity identity, List<Keys> mission)
		{
			InputKeys[identity] = mission;
			//Change binding of central manager
			InputMgr.SetBinding(identity, mission);
		}
	}
	/// <summary>
	/// Player has typed
	/// </summary>
	public static bool WordsChanged { get; private set; } = false;
	/// <summary>
	/// The character the player has inputted
	/// </summary>
	public static char CharInput { get; private set; }
	internal static KeyboardState currentKeyState2, lastKeyState2;

	//Input manager for both 60fps and 125fps checking
	internal static readonly InputManager InputMgr = new();

	/// <summary>
	/// Keyboard recorder/replayer
	/// </summary>
	private static Entity keyEventBuffer;
	internal static bool[] syncKeysHeld;
	/// <summary>
	/// List of keys to sync in multiplayer
	/// </summary>
	private static readonly List<InputIdentity> SyncKeys = [
		InputIdentity.Confirm,
		InputIdentity.MainUp,
		InputIdentity.MainDown,
		InputIdentity.MainLeft,
		InputIdentity.MainRight,
		InputIdentity.SecondUp,
		InputIdentity.SecondDown,
		InputIdentity.SecondLeft,
		InputIdentity.SecondRight,
		InputIdentity.ThirdUp,
		InputIdentity.ThirdDown,
		InputIdentity.ThirdLeft,
		InputIdentity.ThirdRight,
		InputIdentity.FourthUp,
		InputIdentity.FourthDown,
		InputIdentity.FourthLeft,
		InputIdentity.FourthRight,
	];
	/// <summary>
	/// Get the character the player is pressing
	/// </summary>
	/// <returns></returns>
	internal static char KeysUpdate()
	{
		KeyboardState currentKeyState = Keyboard.GetState();
		//Spectating
		if (OverallNetManager.IsSuccessfullyConnected)
		{
			if (OverallNetManager.PlayerData.IsAlive)
			{
				//Sync keys to spectating users (also send compact binary input packet)
				bool[] keysPressed = new bool[SyncKeys.Count];
				for (int i = 0; i < SyncKeys.Count; i++)
					keysPressed[i] = IsKeyDown(SyncKeys[i]);
				OverallNetManager.ConnectedUserData curDat = OverallNetManager.PlayerData;
				curDat.KeysHeld = OverallNetManager.SerializeBoolArray(keysPressed);
				OverallNetManager.PlayerData = curDat;

				//Store soul positions and rotations
				Vector2[] soulPos = null;
				float[] soulRot = null;
				try
				{
					if (Player.hearts != null && Player.hearts.Count > 0)
					{
						soulPos = new Vector2[Player.hearts.Count];
						soulRot = new float[Player.hearts.Count];
						for (int i = 0; i < Player.hearts.Count; i++)
						{
							soulPos[i] = Player.hearts[i].Centre;
							soulRot[i] = Player.hearts[i].Rotation;
						}
					}
				}
				catch
				{
					soulPos = null;
					soulRot = null;
				}
				ClientLogic.SendRawInputPacket(keysPressed, soulPos, soulRot);
			}
			else if (OverallNetManager.ConnectedUsers.Any(user => user.IsAlive))
			{
				//Check if the spectating user has the key pressed
				if (CurrentScene is SongFightingScene sfs)
				{
					List<OverallNetManager.ConnectedUserData> aliveUsers = [.. OverallNetManager.ConnectedUsers.Where(u => u.IsAlive)];
					if (aliveUsers.Count == 0)
						syncKeysHeld = [];
					else
					{
						int idx = MathUtil.Posmod(sfs.SpectatingUser, aliveUsers.Count);
						bool[] des = null;
						try
						{
							des = OverallNetManager.DeserializeBoolArray(aliveUsers[idx]?.KeysHeld ?? []);
						}
						catch
						{
							des = null;
						}
						if (des == null)
							syncKeysHeld = new bool[SyncKeys.Count];
						else if (des.Length != SyncKeys.Count)
						{
							syncKeysHeld = new bool[SyncKeys.Count];
							for (int j = 0; j < Math.Min(des.Length, syncKeysHeld.Length); j++)
								syncKeysHeld[j] = des[j];
						}
						else
							syncKeysHeld = des;
					}
				}
				else
					syncKeysHeld = [];
				for (int i = 0; i < SyncKeys.Count; i++)
				{
					bool val = i < syncKeysHeld.Length && (syncKeysHeld?[i] ?? false);
					//Update sync state on the central InputManager
					InputMgr.SetSyncPressed(SyncKeys[i], val);
				}
			}
		}
		//Handle text input via the platform TextInput event
		if (TextInputAvailable(out char c))
		{
			WordsChanged = true;
			return c;
		}
		//Fallback for control keys not delivered by TextInput, no else if required due to early return
		if (IsKeyPressed120f(Keys.Enter))
		{
			WordsChanged = true;
			return (char)13;
		}
		if (IsKeyPressed120f(Keys.Back))
		{
			WordsChanged = true;
			return (char)8;
		}

		WordsChanged = false;
		return (char)1;
	}
	internal static void UpdateDebugKeys()
	{
		lastKeyState2 = currentKeyState2;
		currentKeyState2 = Keyboard.GetState();
#if DEBUG
		if (IsKeyDown(Keys.LeftControl))
		{
			if (IsKeyPressed120f(Keys.D1))
				GameMain.GameSpeed = 0.02f;
			if (IsKeyPressed120f(Keys.D2))
				GameMain.GameSpeed = 0.05f;
			if (IsKeyPressed120f(Keys.D3))
				GameMain.GameSpeed = 0.1f;
			if (IsKeyPressed120f(Keys.D4))
				GameMain.GameSpeed = 0.25f;
			if (IsKeyPressed120f(Keys.D5))
				GameMain.GameSpeed = 0.5f;
			if (IsKeyPressed120f(Keys.D6))
				GameMain.GameSpeed = 0.7f;
			if (IsKeyPressed120f(Keys.Y))
				GameMain.GameSpeed = 0.85f;
			if (IsKeyPressed120f(Keys.D7))
				GameMain.GameSpeed = 1f;
			if (IsKeyPressed120f(Keys.D8))
				GameMain.GameSpeed = 1.5f;
			if (IsKeyPressed120f(Keys.D9))
				GameMain.GameSpeed = 2f;
		}
		if (IsKeyPressed120f(Keys.H) && CurrentScene is FightScene FScene)
			FScene.PlayerInstance.hpControl.Regenerate();
#endif
		if (isInBattle)
		{
			if (IsKeyPressed120f(InputIdentity.QuickRestart) && !OverallNetManager.IsSuccessfullyConnected)
				EndFight();
			keyEventBuffer?.Update();
		}
		InputMgr.Update(currentKeyState2);
	}

	/// <summary>
	/// Text input queue for <see cref="TextInputer"/>
	/// </summary>
	private static readonly Queue<char> _textInputQueue = new();
	internal static void EnqueueTextChar(char c) => _textInputQueue.Enqueue(c);
	/// <summary>
	/// Check if the input queue has any valid characters
	/// </summary>
	/// <param name="c">The character to return</param>
	/// <returns>Whether a character is in the queue</returns>
	private static bool TextInputAvailable(out char c)
	{
		bool isAvail = _textInputQueue.Count > 0;
		c = isAvail ? _textInputQueue.Dequeue() : (char)0;
		return isAvail;
	}
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is false)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed(InputIdentity key) => InputMgr.IsKeyPressed(key);
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed120f(InputIdentity key) => InputMgr.IsKeyPressed(key);
	/// <summary>
	/// Check if a key is being held
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is held</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyDown(InputIdentity key) => InputMgr.IsKeyDown(key);
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed120f(Keys key) => InputManager.IsKeyPressed(key);
	/// <summary>
	/// Check if a key is being held
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is held</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyDown(Keys key) => InputManager.IsKeyDown(key);
	#endregion
}
/// <summary>
/// The save data of keybinds
/// </summary>
public class KeybindData : ISaveLoad
{
	/// <summary>
	/// The key bindings the user is using
	/// </summary>
	public static Dictionary<InputIdentity, List<Keys>> UserKeys { get; set; } = new(DefaultKeys);
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <inheritdoc/>
	public void Load(SaveInfo info)
	{
		UserKeys.Clear();
		List<string> addedStrings = [];
		foreach (InputIdentity Identity in DefaultKeys.Keys)
		{
			List<Keys> finKey = [];
			if (!info.Nexts.TryGetValue(Identity.ToString(), out SaveInfo value))
				finKey = DefaultKeys[Identity];
			else
			{
				foreach (string keyString in value.fullValue.Split(','))
				{
					if (addedStrings.Contains(keyString))
						continue;
					finKey.Add(MiscUtil.StringToKey(keyString));
					addedStrings.Add(keyString);
				}
			}
			UserKeys.Add(Identity, finKey);
			MiscUtil.SetInputKeys(Identity, finKey);
			addedStrings.Clear();
		}
	}
	/// <inheritdoc/>
	public SaveInfo Save()
	{
		UserKeys ??= new(DefaultKeys);
		SaveInfo info = new("Keybinds{");
		StringBuilder sb = new();
		foreach (InputIdentity Identity in DefaultKeys.Keys)
		{
			if (!UserKeys.ContainsKey(Identity))
				continue;
			foreach (Keys finKey in UserKeys[Identity])
			{
				string curStr = MiscUtil.KeyToString(finKey) + ",";
				_ = sb.Append(curStr);
			}
			if (sb.Length == 0)
				_ = sb.Append(',');
			info.PushNext(new SaveInfo($"{Identity}:{sb.ToString()[..^1]}"));
			sb = sb.Clear();
		}
		return info;
	}
}