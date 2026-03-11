//Assign global usings
global using Microsoft.Xna.Framework;
global using MonoGame.Extended.Graphics;
global using System;
global using System.Runtime.CompilerServices;
global using col = Microsoft.Xna.Framework.Color;
global using vec2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.StringUtil;

/// <summary>
/// Global data, usually for caching and accessing such data
/// </summary>
public static class GlobalData
{
	/// <summary>
	/// The global loader
	/// </summary>
	public static ContentManager Loader;
	#region Waveset utilities
	/// <summary>
	/// The cache for paints of charts, note that key is the <see cref="IWaveSet.FightName"/> of the waveset, not the display name
	/// </summary>
	public static Dictionary<string, Texture2D?> WavePaint = [];
	/// <summary>
	/// The cache for charts, note that key is the <see cref="IWaveSet.FightName"/> of the waveset, not the display name
	/// </summary>
	public static Dictionary<string, IWaveSet> WaveCache = [];
	/// <summary>
	/// Gets the name of the waveset
	/// </summary>
	/// <param name="set">The waveset to get</param>
	/// <returns>The <see cref="IWaveSet.Attributes.DisplayName"/> of the waveset, if any, <see cref="IWaveSet.FightName"/> if not</returns>
	public static string GetWavesetDisplayName(IWaveSet set) => set.Attributes.DisplayName.DefaultIfNullOrEmpty(set.FightName);
	/// <summary>
	/// Gets the paint of the given waveset
	/// </summary>
	/// <param name="set">The waveset to get</param>
	/// <returns>The paint if any, null if not</returns>
	public static Texture2D GetWavePaint(IWaveSet set) => WavePaint.TryGetValue(set.FightName, out Texture2D paint) ? paint : null;
	#endregion
}