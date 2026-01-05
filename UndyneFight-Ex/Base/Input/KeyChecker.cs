using Microsoft.Xna.Framework.Input;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.GameStates.KeyChecker;

namespace UndyneFight_Ex;

public static partial class GameStates
{
	/// <summary>
	/// The default key bindings
	/// </summary>
	public static Dictionary<InputIdentity, List<Keys>> DefaultKeys => KeyChecker.DefaultKeys;
	#region keys
	internal class KeyChecker
	{
		public KeyChecker()
		{
			allCheckers.Add(this);
			foreach (KeyValuePair<InputIdentity, List<Keys>> kvp in InputKeys)
			{
				IdentityChecker singleChecker = new();
				singleChecker.ResetKeyList(kvp.Value);
				_identityCheckers.Add(kvp.Key, singleChecker);
			}
		}
		public void Update(KeyboardState keyboardState)
		{
			foreach (IdentityChecker item in _identityCheckers.Values)
				item.Update(keyboardState);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsKeyDown(InputIdentity identity) => identity != InputIdentity.None && _identityCheckers[identity].IsKeyDown();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsKeyPressed(InputIdentity identity) => identity != InputIdentity.None && _identityCheckers[identity].IsKeyPressed();
		private readonly Dictionary<InputIdentity, IdentityChecker> _identityCheckers = [];
		public static readonly Dictionary<InputIdentity, List<Keys>> DefaultKeys = [];

		static KeyChecker()
		{
			InputKeys.Add(InputIdentity.Confirm, [Keys.Enter, Keys.Z]);
			InputKeys.Add(InputIdentity.Cancel, [Keys.LeftShift, Keys.X]);
			InputKeys.Add(InputIdentity.Alternate, [Keys.Space]);
			InputKeys.Add(InputIdentity.Special, [Keys.C]);
			InputKeys.Add(InputIdentity.MainRight, [Keys.Right]);
			InputKeys.Add(InputIdentity.MainDown, [Keys.Down]);
			InputKeys.Add(InputIdentity.MainLeft, [Keys.Left]);
			InputKeys.Add(InputIdentity.MainUp, [Keys.Up]);
			InputKeys.Add(InputIdentity.SecondRight, [Keys.D]);
			InputKeys.Add(InputIdentity.SecondDown, [Keys.S]);
			InputKeys.Add(InputIdentity.SecondLeft, [Keys.A]);
			InputKeys.Add(InputIdentity.SecondUp, [Keys.W]);
			InputKeys.Add(InputIdentity.ThirdRight, [Keys.B]);
			InputKeys.Add(InputIdentity.ThirdDown, [Keys.V]);
			InputKeys.Add(InputIdentity.ThirdLeft, [Keys.C]);
			InputKeys.Add(InputIdentity.ThirdUp, [Keys.F]);
			InputKeys.Add(InputIdentity.FourthRight, [Keys.OemComma]);
			InputKeys.Add(InputIdentity.FourthDown, [Keys.M]);
			InputKeys.Add(InputIdentity.FourthLeft, [Keys.N]);
			InputKeys.Add(InputIdentity.FourthUp, [Keys.J]);
			InputKeys.Add(InputIdentity.FullScreen, [Keys.F4]);
			InputKeys.Add(InputIdentity.ScreenShot, [Keys.F12]);
			InputKeys.Add(InputIdentity.Number1, [Keys.D1]);
			InputKeys.Add(InputIdentity.Number2, [Keys.D2]);
			InputKeys.Add(InputIdentity.Number3, [Keys.D3]);
			InputKeys.Add(InputIdentity.Number4, [Keys.D4]);
			InputKeys.Add(InputIdentity.Number5, [Keys.D5]);
			InputKeys.Add(InputIdentity.Number6, [Keys.D6]);
			InputKeys.Add(InputIdentity.Number7, [Keys.D7]);
			InputKeys.Add(InputIdentity.Number8, [Keys.D8]);
			InputKeys.Add(InputIdentity.Number9, [Keys.D9]);
			InputKeys.Add(InputIdentity.Number0, [Keys.D0]);
			InputKeys.Add(InputIdentity.Backspace, [Keys.Back]);
			InputKeys.Add(InputIdentity.Reset, [Keys.R]);
			InputKeys.Add(InputIdentity.Heal, [Keys.H]);
			InputKeys.Add(InputIdentity.Tab, [Keys.Tab]);
			InputKeys.Add(InputIdentity.QuickRestart, [Keys.F2]);
			DefaultKeys = new(InputKeys);
		}
		private static readonly List<KeyChecker> allCheckers = [];

		public static Dictionary<InputIdentity, List<Keys>> InputKeys { get; set; } = [];
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void SetIdentityKey(InputIdentity identity, List<Keys> mission)
		{
			InputKeys[identity] = mission;
			allCheckers.ForEach(s => s._identityCheckers[identity].ResetKeyList(mission));
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

	private static readonly KeyChecker checker120f = new(), checker = new();

	/// <summary>
	/// 一个键盘操作录制器(播放)或者是一个键盘操作控制器(回放)
	/// </summary>
	private static Entity keyEventBuffer;

	internal static char KeysUpdate()
	{
		KeyboardState currentKeyState = Keyboard.GetState();
		bool shift_pressed = currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift);
		for (int i = 0; i < 256; i++)
		{
			if (IsKeyPressed120f((Keys)i))
			{
				WordsChanged = true;
				switch (i)
				{
					case > 47 and < 58:
						return (char)i;
					case > 64 and < 91: //Letters
						return (char)(i + (shift_pressed ? 0 : 32));
					case 188:
						return shift_pressed ? '<' : ',';
					case 189:
						return shift_pressed ? '_' : '-';
					case 190:
						return shift_pressed ? '>' : '.';
					case 187:
						return shift_pressed ? '+' : '=';
					case 191:
						return shift_pressed ? '?' : '/';
					case 186:
						return shift_pressed ? ':' : ';';
					case 0x20:
						return (char)0x20;
					case 13:
						return (char)13;
				}
			}
		}
		WordsChanged = false;
		return (char)1;
	}
	internal static void KeysUpdate2()
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
			if (IsKeyPressed120f(InputIdentity.QuickRestart))
				EndFight();
			keyEventBuffer?.Update();
		}

		if (GameMain.Update120F)
			checker.Update(currentKeyState2);
		checker120f.Update(currentKeyState2);
	}
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is false)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed(InputIdentity key) => checker.IsKeyPressed(key);
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed120f(InputIdentity key) => checker120f.IsKeyPressed(key);
	/// <summary>
	/// Check if a key is being held
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is held</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyDown(InputIdentity key) => checker120f.IsKeyDown(key);
	/// <summary>
	/// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is pressed</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyPressed120f(Keys key) => currentKeyState2.IsKeyDown(key) && lastKeyState2.IsKeyUp(key);
	/// <summary>
	/// Check if a key is being held
	/// </summary>
	/// <param name="key">The key to check</param>
	/// <returns>Whether the key is held</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKeyDown(Keys key) => currentKeyState2.IsKeyDown(key);
	#endregion
}
public class KeybindData : ISaveLoad
{
	public static Dictionary<InputIdentity, List<Keys>> UserKeys { get; set; } = new(DefaultKeys);
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <inheritdoc/>
	public void Load(SaveInfo info)
	{
		UserKeys.Clear();
		foreach (InputIdentity Identity in DefaultKeys.Keys)
		{
			List<Keys> finKey = [];
			if (!info.Nexts.TryGetValue(Identity.ToString(), out SaveInfo value))
				finKey = DefaultKeys[Identity];
			else
			{
				foreach (string keyString in value.fullValue.Split(','))
					finKey.Add(MiscUtil.StringToKey(keyString));
			}
			_ = UserKeys.TryAdd(Identity, finKey);
		}
		InputKeys = new(UserKeys);
	}
	/// <inheritdoc/>
	public SaveInfo Save()
	{
		UserKeys ??= new(DefaultKeys);
		SaveInfo info = new("Keybinds{");
		foreach (InputIdentity Identity in DefaultKeys.Keys)
		{
			string finText = string.Empty;
			foreach (Keys finKey in UserKeys[Identity])
				finText += MiscUtil.KeyToString(finKey) + ",";
			info.PushNext(new SaveInfo($"{Identity}:{finText[..^1]}"));
		}
		return info;
	}
}