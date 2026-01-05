using Microsoft.Xna.Framework.Input;

namespace UndyneFight_Ex;
/// <summary>
/// Input verbs
/// </summary>
public enum InputIdentity
{
	/// <summary>
	/// Unassigned key
	/// </summary>
	None = 999999,
	/// <summary>
	/// Confirm key
	/// </summary>
	Confirm = 0,
	/// <summary>
	/// Cancel key
	/// </summary>
	Cancel = 1,
	/// <summary>
	/// Alternate key (Default spacebar)
	/// </summary>
	Alternate = 2,
	/// <summary>
	/// Menu key (Default C)
	/// </summary>
	Special = 3,
	/// <summary>
	/// Primary Right key
	/// </summary>
	MainRight = 4,
	/// <summary>
	/// Primary Down key
	/// </summary>
	MainDown = 5,
	/// <summary>
	/// Primary Left key
	/// </summary>
	MainLeft = 6,
	/// <summary>
	/// Primary Up key
	/// </summary>
	MainUp = 7,
	/// <summary>
	/// Secondary Right key
	/// </summary>
	SecondRight = 8,
	/// <summary>
	/// Secondary Down key
	/// </summary>
	SecondDown = 9,
	/// <summary>
	/// Secondary Left key
	/// </summary>
	SecondLeft = 10,
	/// <summary>
	/// Secondary Up key
	/// </summary>
	SecondUp = 11,
	/// <summary>
	/// Ternary Right key
	/// </summary>
	ThirdRight = 12,
	/// <summary>
	/// Ternary Down key
	/// </summary>
	ThirdDown = 13,
	/// <summary>
	/// Ternary Left key
	/// </summary>
	ThirdLeft = 14,
	/// <summary>
	/// Ternary Up key
	/// </summary>
	ThirdUp = 15,
	/// <summary>
	/// Quaternary Right key
	/// </summary>
	FourthRight = 16,
	/// <summary>
	/// Quaternary Down key
	/// </summary>
	FourthDown = 17,
	/// <summary>
	/// Quaternary Left key
	/// </summary>
	FourthLeft = 18,
	/// <summary>
	/// Quaternary Up key
	/// </summary>
	FourthUp = 19,
	/// <summary>
	/// Fullscreen key
	/// </summary>
	FullScreen = 20,
	/// <summary>
	/// Screenshot key
	/// </summary>
	ScreenShot = 21,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number1 = 22,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number2 = 23,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number3 = 24,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number4 = 25,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number5 = 26,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number6 = 27,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number7 = 28,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number8 = 29,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number9 = 30,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Number0 = 31,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Backspace = 32,
	/// <summary>
	/// Reset key
	/// </summary>
	Reset = 33,
	/// <summary>
	/// Debug healing key
	/// </summary>
	Heal = 34,
	/// <summary>
	/// Self-explanatory
	/// </summary>
	Tab = 35,
	/// <summary>
	/// Quick restart key
	/// </summary>
	QuickRestart = 36
}
/// <summary>
/// Checker for player input of <see cref="InputIdentity"/>
/// </summary>
public class IdentityChecker
{
	private List<Keys> checkList;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ResetKeyList(List<Keys> checkList) => this.checkList = checkList;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsKeyPressed() => !lastPressed && curPressed;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsKeyDown() => curPressed;
	private bool lastPressed = false, curPressed = false;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Update(KeyboardState curState)
	{
		lastPressed = curPressed;
		curPressed = false;
		foreach (Keys key in checkList)
		{
			if (curState.IsKeyDown(key))
			{
				curPressed = true;
				break; //Breaks when any of the assigned keys is pressed
			}
		}
	}
}