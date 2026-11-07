using Microsoft.Xna.Framework.Input;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex;

/// <summary>
/// Central input manager replacing dual-checker pattern. Provides per-identity queries and events.
/// </summary>
internal class InputManager()
{
	/// <summary>
	/// The dictinary of all idendity checkers for each input
	/// </summary>
	private readonly Dictionary<InputIdentity, IdentityChecker> _identityCheckers = [];
	/// <summary>
	/// Whether the input was previously held down
	/// </summary>
	private readonly Dictionary<InputIdentity, bool> _prevDown = [];
	/// <summary>
	/// Whether the input manager is initialized
	/// </summary>
	private bool _initialized = false;
	//User events
#nullable enable
	public event Action<InputIdentity>? ActionPressed;
	public event Action<InputIdentity>? ActionReleased;
	public event Action<InputIdentity>? ActionHeld;
#nullable disable
	public void SetSyncPressed(InputIdentity id, bool value)
	{
		if (_identityCheckers.TryGetValue(id, out IdentityChecker checker))
			checker.syncPressed = value;
	}
	/// <summary>
	/// Update the key bindings for an identity at runtime.
	/// </summary>
	public void SetBinding(InputIdentity id, List<Keys> keys)
	{
		if (_identityCheckers.TryGetValue(id, out IdentityChecker checker))
			checker.ResetKeyList(keys);
		else
		{
			IdentityChecker newChecker = new();
			newChecker.ResetKeyList(keys);
			_identityCheckers[id] = newChecker;
			_prevDown[id] = false;
		}
		PlayerManager.SaveSettings();
	}
	private void InitializeFromBindings()
	{
		//Create identity checkers from current bindings
		foreach ((InputIdentity input, List<Keys> keys) in KeyChecker.InputKeys)
		{
			IdentityChecker checker = new();
			checker.ResetKeyList(keys);
			_identityCheckers[input] = checker;
			_prevDown[input] = false;
		}
		_initialized = true;
	}
	/// <summary>
	/// Update input manager state. Call once per input sampling (frame or tick).
	/// </summary>
	public void Update(KeyboardState curState)
	{
		CheckInitialized();
		//Update each identity checker
		foreach ((InputIdentity id, IdentityChecker checker) in _identityCheckers)
		{
			checker.Update(curState);
			//On key press
			if (checker.IsKeyPressed())
				ActionPressed?.Invoke(id);

			//On key hold
			if (checker.IsKeyDown())
				ActionHeld?.Invoke(id);

			//On key release
			bool curDown = checker.IsKeyDown();
			bool prev = _prevDown.GetValueOrDefault(id);
			if (prev && !curDown)
				ActionReleased?.Invoke(id);
			_prevDown[id] = curDown;
		}
	}
	public bool IsKeyPressed(InputIdentity identity) => _identityCheckers.TryGetValue(identity, out IdentityChecker checker) && checker.IsKeyPressed();
	public bool IsKeyDown(InputIdentity identity) => _identityCheckers.TryGetValue(identity, out IdentityChecker checker) && checker.IsKeyDown();
	public static bool IsKeyPressed(Keys key) => currentKeyState2.IsKeyDown(key) && lastKeyState2.IsKeyUp(key);
	public static bool IsKeyDown(Keys key) => currentKeyState2.IsKeyDown(key);
	private void CheckInitialized()
	{
		if (!_initialized)
			InitializeFromBindings();
	}
}