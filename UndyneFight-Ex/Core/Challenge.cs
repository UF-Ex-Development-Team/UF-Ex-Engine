using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex
{
	/// <summary>
	/// A list of charts that makes up a challenge
	/// </summary>
	/// <param name="iconPath">The path to the icon of the challenge</param>
	/// <param name="title">The title of the challenge</param>
	/// <param name="desc">The description of the challenge</param>
	/// <param name="routes">The list of charts of the challenge, with <see cref="Tuple{Type, Difficulty}"/> of [Chart class, Difficulty]</param>
	public class Challenge(string iconPath, string title, string desc, Tuple<Type, Difficulty>[] routes)
	{
		/// <summary>
		/// The string path to the icon of the challenge
		/// </summary>
		public string IconPath = iconPath;
		/// <summary>
		/// The title of the challenge
		/// </summary>
		public string Title = title;
		/// <summary>
		/// The description of the challenge
		/// </summary>
		public string Desc = desc;
		/// <summary>
		/// The <see cref=" Tuple"/> list of charts of the challenge
		/// </summary>

		public Tuple<Type, Difficulty>[] Routes = routes;
		/// <summary>
		/// The list of <see cref="SongResult"/> of the charts
		/// </summary>
		public List<SongResult> ResultBuffer { get; init; } = [];
		/// <summary>
		/// Resets the <see cref="ResultBuffer"/>
		/// </summary>
		public void Reset() => ResultBuffer.Clear();
	}
}