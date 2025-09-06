using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.Fight.Functions;

namespace UndyneFight_Ex.SongSystem
{
	/// <summary>
	/// The interface of the chart
	/// </summary>
	public interface IWaveSet
	{
		/// <summary>
		/// The action to invoke when the chart begins, do NOT add the 'override' keyword or else it will override the function
		/// </summary>
		void Start();
		/// <summary>
		/// Barrage for Noob difficulty
		/// </summary>
		void Noob() { }
		/// <summary>
		/// Barrage for Easy difficulty
		/// </summary>
		void Easy() { }
		/// <summary>
		/// Barrage for Normal difficulty
		/// </summary>
		void Normal() { }
		/// <summary>
		/// Barrage for Hard difficulty
		/// </summary>
		void Hard() { }
		/// <summary>
		/// Barrage for Extreme difficulty
		/// </summary>
		void Extreme() { }
		/// <summary>
		/// Barrage for Extreme+ difficulty
		/// </summary>
		void ExtremePlus() { }
		/// <summary>
		/// File name of the chart song
		/// </summary>
		string Music { get; }
		/// <summary>
		/// Display name of the chart (Affects save file)
		/// </summary>
		string FightName { get; }
		/// <summary>
		/// The <see cref="SongInformation"/> attributes of the chart
		/// </summary>
		SongInformation Attributes { get; }
	}
	/// <summary>
	/// Simplified version of <see cref="IWaveSet"/>, note that all functions in <see cref="IWaveSet"/> will be overwritten by <see cref="Chart"/>
	/// </summary>
	public interface IWaveSetS : IWaveSet
	{
		/// <summary>
		/// The chart function, contains all difficulties
		/// </summary>
		void Chart();
	}
	/// <summary>
	/// If the chart is a championship chart
	/// </summary>
	public interface IChampionShip
	{
		/// <summary>
		/// The <see cref="IWaveSet"/> content, which is the chart itself
		/// </summary>
		IWaveSet GameContent { get; }
		/// <summary>
		/// The [Name, Difficulty] of the championship chart
		/// </summary>
		Dictionary<string, Difficulty> DifficultyPanel { get; }
	}

	/// <summary>
	/// Wave data
	/// </summary>
	public class WaveConstructor : GameObject
	{
		/// <summary>
		/// The list of loaded contents to dispose
		/// </summary>
		private readonly List<string> loadedContents = [];
		/// <summary>
		/// The list of loaded images to dispose
		/// </summary>
		private readonly List<Texture2D> loadedImages = [];
		/// <summary>
		/// Whether the chart has multiple BPMs
		/// </summary>
		internal static bool _isMultiBPM = false;
		/// <summary>
		/// The list of bpms and their durations
		/// </summary>
		private static (float beatCount, float bpm)[] _MultiBPM;
		private static float[] _MultiBPMChange;
		private int _curBPMStage = 0;
		/// <summary>
		/// Initializes the wave data
		/// </summary>
		/// <param name="beatTime">Duration of 1 beat or the BPM</param>
		/// <param name="isBPM">Whether <paramref name="beatTime"/> is the BPM itself (Default false)</param>
		public WaveConstructor(float beatTime, bool isBPM = false)
		{
			SingleBeat = isBPM ? (62.5f / (beatTime / 60f)) : beatTime;
			DelayEnabled = true;
		}
		/// <summary>
		/// Initializes the wave data
		/// </summary>
		/// <param name="beats">Duration of each beat, [Beat Count, BPM]</param>
		public WaveConstructor(params (float beatCount, float bpm)[] beats)
		{
			_isMultiBPM = true;
			//Convert BPM into frames
			_MultiBPMChange = new float[beats.Length];
			for (int i = 0; i < beats.Length; ++i)
			{
				beats[i].bpm = 62.5f / (beats[i].bpm / 60f);
				_MultiBPMChange[i] = beats[i].beatCount + (i > 0 ? _MultiBPMChange[i - 1] : 0);
			}
			//Set initial BPM
			SingleBeat = (_MultiBPM = beats)[0].bpm;
			DelayEnabled = true;
		}
		/// <summary>
		/// Duration of 1 beat
		/// </summary>
		public static float SingleBeat { get; private set; }
		/// <summary>
		/// Duration of the given beat time in frames
		/// </summary>
		/// <param name="x">Amount of beats</param>
		/// <returns>Amount of frames for <paramref name="x"/> beats</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float BeatTime(float x)
		{
			if (_isMultiBPM)
			{
				float AccumulateFrames = 0;
				foreach ((float beatCount, float bpm) in _MultiBPM)
				{
					if (x <= beatCount)
						return AccumulateFrames + x * bpm;
					else
					{
						AccumulateFrames += beatCount * bpm;
						x -= beatCount;
					}
				}
			}
			return x * SingleBeat;
		}
		/// <summary>
		/// Whether the chart is at the given beat
		/// </summary>
		/// <param name="beat">The beat to check</param>
		/// <returns>Whether the chart is currently at the given beat</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InBeat(float beat) => GametimeF >= BeatTime(beat) && GametimeF < BeatTime(beat) + 0.5f;
		/// <summary>
		/// Whether the chart is currently in the range of the given beats
		/// </summary>
		/// <param name="leftBeat">Starting beat</param>
		/// <param name="rightBeat">Ending beat</param>
		/// <returns>Whether the chart is currently between the given beat</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InBeat(float leftBeat, float rightBeat) => GametimeF >= BeatTime(leftBeat) && GametimeF <= BeatTime(rightBeat) + 0.5f;
		/// <summary>
		/// Check whether the chart is currently at a multiple of the given beat
		/// </summary>
		/// <param name="beatCount">The beat to check</param>
		/// <returns>Whether the chart is at a multiple of the Xth beat</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool At0thBeat(float beatCount) => (int)(GametimeF % BeatTime(beatCount) * 2) == 0;
		/// <summary>
		/// Check whether the chart is currently at a multiple of the given beat plus the frames given
		/// </summary>
		/// <param name="beatCount">The beat to check</param>
		/// <param name="K">The frame remainder to check</param>
		/// <returns>Whether the chart is at a multiple of the Xth beat plus the frames given</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AtKthBeat(float beatCount, float K) => (int)(GametimeF % BeatTime(beatCount) * 2) == (int)K * 2;
		/// <summary>
		/// Invokes an action after the given beats
		/// </summary>
		/// <param name="delayBeat">The amount of    to delay</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DelayBeat(float delayBeat, Action action) => AddInstance(new InstantEvent(delayBeat * SingleBeat, action));
		/// <summary>
		/// Invokes an action after the given frames
		/// </summary>
		/// <param name="delay">The amount of frames to delay</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Delay(float delay, Action action) => AddInstance(new InstantEvent(delay, action));
		/// <summary>
		/// Invokes an action for the next given beats (Using int calculation, recommended not to use)
		/// </summary>
		/// <param name="durationBeat">The duration of the action</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForBeat(float durationBeat, Action action) => AddInstance(new TimeRangedEvent(durationBeat * SingleBeat, action));
		/// <summary>
		/// Invokes an action for the next given beats (Using float calculation, recommended to use)
		/// </summary>
		/// <param name="durationBeat">The duration of the action</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForBeat120(float durationBeat, Action action) => AddInstance(new TimeRangedEvent(durationBeat * SingleBeat, action) { UpdateIn120 = true });
		/// <summary>
		/// Invokes an action for the next given beats after the given beats (Using int calculation, recommended not to use)
		/// </summary>
		/// <param name="delayBeat">The amount of beats to delay before invoking the action</param>
		/// <param name="durationBeat">The duration of the action</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForBeat(float delayBeat, float durationBeat, Action action) => AddInstance(new TimeRangedEvent(delayBeat * SingleBeat, durationBeat * SingleBeat, action));
		/// <summary>
		/// Invokes an action for the next given beats after the given beats (Using float calculation, recommended to use)
		/// </summary>
		/// <param name="delayBeat">The amount of beats to delay before invoking the action</param>
		/// <param name="durationBeat">The duration of the action</param>
		/// <param name="action">The action to invoke</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForBeat120(float delayBeat, float durationBeat, Action action) => AddInstance(new TimeRangedEvent(delayBeat * SingleBeat, durationBeat * SingleBeat, action) { UpdateIn120 = true });
		/// <summary>
		/// The process for all arrows that will be executed in <see cref="CreateChart(float, float, float, string[])"/>
		/// </summary>
		public Action<Arrow> ArrowProcesser { private get; set; } = null;

		private class BracketTreeNode
		{
			public BracketTreeNode(string s)
			{
				string cur = string.Empty, self = string.Empty;
				BracketTreeNode curNode = null;
				int cnt = 0;
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '(')
					{
						//Increment count after checking
						if (cnt++ > 0)
							cur += s[i];
					}
					else if (s[i] == ')')
					{
						if (--cnt == 0)
						{
							sons.Add(curNode = new(cur));
							cur = string.Empty;
						}
						else
							cur += s[i];
					}
					else if (cnt == 0 && s[i] == '[')
					{
						i++;
						string mul = string.Empty;
						for (; s[i] != ']'; i++)
							mul += s[i];
						CalculateTimes(curNode, mul);
					}
					else
					{
						if (cnt == 0)
							self += s[i];
						else
							cur += s[i];
					}
				}
				info = self;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void CalculateTimes(BracketTreeNode curNode, string mul)
			{
				if (mul.Contains(':'))
				{
					string[] splits = mul.Split(':');
					curNode.enumer = splits[0];
					mul = splits[1];
				}
				if (mul.Contains(".."))
				{
					string[] splits = mul.Split("..");
					curNode.boundL = TryPraseInt(curNode, splits[0]);
					mul = splits[1];
					curNode.boundR = 1;
				}
				int mulInt = TryPraseInt(curNode, mul);
				curNode.boundR += mulInt - 1;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int TryPraseInt(BracketTreeNode curNode, string mul)
			{
				if (int.TryParse(mul, out int mulInt))
				{
					if (curNode == null)
						throw new ArgumentException(string.Format("[] must be placed after )"));
				}
				else
					throw new ArgumentException(string.Format("{0} isn't a number in the []", mul));

				return mulInt;
			}

			private readonly string info;
			private int boundL = 0, boundR = 0;
			private string enumer = "";
			private readonly List<BracketTreeNode> sons = [];
			public List<string> GetAll(Dictionary<string, int> enums)
			{
				List<string> res = [];
				// in the foreach, we recursively get the items in the subtrees.
				foreach (BracketTreeNode son in sons)
				{
					bool existEnumer;
					if (existEnumer = !string.IsNullOrEmpty(son.enumer))
						enums.Add(son.enumer, 0);
					for (int i = son.boundL; i <= son.boundR; i++)
					{
						if (existEnumer)
							enums[son.enumer] = i;
						res.AddRange(son.GetAll(enums));
					}
					if (existEnumer)
						_ = enums.Remove(son.enumer);
				}

				//if info exists, then we push the info in current node into the results list.
				if (!string.IsNullOrEmpty(info))
				{
					string cur = info;
					foreach (KeyValuePair<string, int> pair in enums)
						cur = cur.Replace("*" + pair.Key, pair.Value.ToString());
					res.Add(cur);
				}
				return res;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] SplitBracket(string origin) => origin.Count(s => s == '(') != origin.Count(s => s == ')')
				? throw new ArgumentException($"{origin} isn't a legal bracket sequence")
				: [.. new BracketTreeNode(origin).GetAll([])];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string[] ProduceTag(ref string origin)
		{
			if (origin[^1] != '}')
				return null;
			int tag = origin.LastIndexOf('{');
			if (tag == -1)
				throw new ArgumentException($"{nameof(origin)} has no character '{{'", origin);

			string result = origin[(tag + 1)..^1];
			origin = origin[..tag];

			return result.Split(',');
		}
		/// <summary>
		/// This is the most unmaintainable code as rated in the MSVS Code Metrics, only having 29/100
		/// </summary>
		/// <param name="shootShieldTime"></param>
		/// <param name="origin"></param>
		/// <param name="speed"></param>
		/// <param name="arrowAttribute"></param>
		/// <param name="normalized"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IEnumerable<GameObject> MakeChartObject(float shootShieldTime, string origin, float speed, ArrowAttribute arrowAttribute, bool normalized)
		{
			//Empty entry
			if (string.IsNullOrWhiteSpace(origin) || origin == "/")
				return null;
			string originCopy = origin;
			string[] entityTags = ProduceTag(ref origin);
			bool isFunction = false;
			GameObject[] results = TryGetObjects(origin, shootShieldTime, ref isFunction);
			if (isFunction)
				return results;

			int speedPos = -1, tagPos = -1;
			bool multiTag = false;
			for (int i = origin.Length - 1; i >= 0; i--)
			{
				if (origin[i] == ',')
					multiTag = true;
				//Change speed
				else if (origin[i] == '\'')
					speedPos = i;
				//Apply tag
				else if (origin[i] == '@')
					tagPos = i;
			}
			string tag = null;
			float speedMul = 1f;
			bool isvoid = false;
			//Apply speed multiplication
			if (speedPos != -1)
				speedMul = tagPos > speedPos
					? MathUtil.FloatFromString(origin[(speedPos + 1)..tagPos])
					: MathUtil.FloatFromString(origin[(speedPos + 1)..]);
			//Set tag
			if (tagPos != -1)
				tag = speedPos > tagPos ? origin[(tagPos + 1)..speedPos] : origin[(tagPos + 1)..];

			int cut1 = speedPos;
			if (cut1 == -1)
				cut1 = tagPos;
			else if (tagPos != -1)
				cut1 = Math.Min(cut1, tagPos);
			if (cut1 != -1)
				origin = origin[..cut1];
			//Arrow attributes
			int curSpecialI = 0;
			if (origin[curSpecialI] == '~')
			{
				arrowAttribute |= ArrowAttribute.Void;
				curSpecialI++;
				isvoid = true;
			}
			if (origin[curSpecialI] == '*')
			{
				arrowAttribute |= ArrowAttribute.Tap;
				curSpecialI++;
				if (Settings.GreenTap)
					arrowAttribute |= ArrowAttribute.ForceGreen;
			}
			else if (origin[curSpecialI] == '_')
			{
				arrowAttribute |= ArrowAttribute.Hold;
				curSpecialI++;
			}
			if (origin[curSpecialI] == '<')
			{
				arrowAttribute |= ArrowAttribute.RotateL;
				curSpecialI++;
			}
			else if (origin[curSpecialI] == '>')
			{
				arrowAttribute |= ArrowAttribute.RotateR;
				curSpecialI++;
			}
			if (origin[curSpecialI] == '^')
			{
				arrowAttribute |= ArrowAttribute.SpeedUp;
				curSpecialI++;
			}
			if (origin[curSpecialI] == '!')
			{
				arrowAttribute |= ArrowAttribute.NoScore;
				curSpecialI++;
			}
			origin = origin[curSpecialI..];
			bool GB = origin[0] is '#' or '%', hasArrowInGB = origin[0] == '#';
			string cut = string.Empty;
			if (GB)
			{
				int pos;
				char GBTxt = origin[0];
				cut = origin[(origin.IndexOf(GBTxt) + 1)..(pos = origin.LastIndexOf(GBTxt))];
				origin = origin[(pos + 1)..];
			}
			if ((origin.Length == 1 || origin[1] != ' ') && (origin[0] is 'R' or 'D' or 'd'))
				origin = $"{origin[0]} {origin[1..]}";
			if (origin.Length == 2)
				origin += "00";
			else if (origin.Length == 3)
				origin += "0";
			Arrow arr = null;
			List<Entity> result = [];
			if (GB)
			{
				string way = origin[..2];
				if (normalized && hasArrowInGB)
					result.Add(arr = MakeArrow(shootShieldTime, way, speed * speedMul, origin[2] - '0', 0, arrowAttribute));
				result.Add(new GreenSoulGB(shootShieldTime, hasArrowInGB ? "+0" : way, origin[2] - '0', BeatTime(MathUtil.FloatFromString(cut))) { AppearVolume = Settings.GBAppearVolume, ShootVolume = Settings.GBShootVolume, Follow = Settings.GBFollowing });
			}
			else
				result.Add(arr = MakeArrow(shootShieldTime, origin, speed * speedMul, origin[2] - '0', origin[3] - '0', arrowAttribute));

			if (!string.IsNullOrEmpty(tag))
				result.ForEach(s => s.Tags = multiTag ? tag.Split(',') : [tag]);
			if (arr != null)
			{
				if (entityTags != null)
					arr.Tags = entityTags;
				if (isvoid)
					arr.VolumeFactor *= Settings.VoidArrowVolume;
				LastArrow = arr;

				ArrowProcesser?.Invoke(arr);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private GameObject[] TryGetObjects(string origin, float delay, ref bool isFunction)
		{
			string args = string.Empty;
			bool delayMode = DelayEnabled;
			if (origin[0] == '<')
			{
				if (origin.Contains('>'))
				{
					int i = 1;
					if (origin[1] == '!')
					{
						i = 3;
						delayMode = false;
					}
					if (origin[2] == '>' && origin[1] == '!')
						i = 2;
					else
						for (; origin[i] != '>'; i++)
							args += origin[i];
					origin = origin[(i + 1)..];
				}
				else
				{
					isFunction = false;
					return null;
				}
			}
			if (chartingActions.TryGetValue(origin, out Action value))
			{
				isFunction = true;
				if (!string.IsNullOrEmpty(args))
				{
					string[] argStrings = args.Split(',');
					float[] argsFloat = new float[argStrings.Length];
					for (int i = 0; i < argsFloat.Length; i++)
						argsFloat[i] = MathUtil.FloatFromString(argStrings[i]);

					if (delayMode)
					{
						Action action = value;
						GameObject[] list = [new InstantEvent(delay, () => {
							Arguments = argsFloat;
							action();
						})];
						return list;
					}
					else
					{
						Arguments = argsFloat;
						value();
						return null;
					}
				}
				if (delayMode)
				{
					GameObject[] list = [new InstantEvent(delay, value)];
					return list;
				}
				else
				{
					value();
					return null;
				}
			}
			return null;
		}
		/// <summary>
		/// The settings of the charts
		/// </summary>
		public class ChartSettings
		{
			/// <summary>
			/// The appearing volume of <see cref="GreenSoulGB"/> (Default 0.5f)
			/// </summary>
			public float GBAppearVolume = 0.5f;
			/// <summary>
			/// The shooting volume of <see cref="GreenSoulGB"/> (Default 0.5f)
			/// </summary>
			public float GBShootVolume = 0.75f;
			/// <summary>
			/// The volume of collision of <see cref="Arrow"/> that has <see cref="Arrow.VoidMode"/> set to true (Default 0.5f)
			/// </summary>
			public float VoidArrowVolume = 0.5f;
			/// <summary>
			/// Whether all Tap arrows are displayed as green outlined arrows (Default false)
			/// </summary>
			public bool GreenTap = false;
			/// <summary>
			/// Whether the <see cref="GreenSoulGB"/> follows the player rotation or not (Default false)
			/// </summary>
			public bool GBFollowing { get; set; } = false;
		}
		/// <summary>
		/// The <see cref="ChartSettings"/> of the chart (Note that the settings persist)
		/// </summary>
		protected ChartSettings Settings { get; private set; } = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public GameObject[] MakeArrows(float shootShieldTime, float speed, string allArrowTag, bool normalized = false)
		{
			string[] arrowTags = SplitBracket(allArrowTag);
			List<GameObject> arrows = [];

			for (int i = 0; i < arrowTags.Length; i++)
			{
				IEnumerable<GameObject> t = MakeChartObject(shootShieldTime, arrowTags[i], speed, ArrowAttribute.None, normalized);
				if (t != null)
					arrows.AddRange(t);
			}
			return [.. arrows];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public GameObject[] MakeArrows(float shootShieldTime, float speed, string allArrowTag, ArrowAttribute arrowattribute, bool normalized = false)
		{
			string[] arrowTags = SplitBracket(allArrowTag);
			List<GameObject> arrows = [];

			for (int i = 0; i < arrowTags.Length; i++)
			{
				IEnumerable<GameObject> t = MakeChartObject(shootShieldTime, arrowTags[i], speed, arrowattribute, normalized);
				if (t != null)
					arrows.AddRange(t);
			}
			return [.. arrows];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public void CreateArrows(float shootShieldTime, float speed, string allArrowTag)
		{
			GameObject[] arrows = MakeArrows(shootShieldTime, speed, allArrowTag);
			for (int i = 0; i < arrows.Length; i++)
			{
				if (arrows[i] != null)
					AddInstance(arrows[i]);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public void CreateArrows(float shootShieldTime, float speed, string allArrowTag, ArrowAttribute arrowAttribute)
		{
			GameObject[] arrows = MakeArrows(shootShieldTime, speed, allArrowTag, arrowAttribute);
			for (int i = 0; i < arrows.Length; i++)
			{
				if (arrows[i] != null)
					AddInstance(arrows[i]);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public void NormalizedChart(float shootShieldTime, float speed, string allArrowTag)
		{
			GameObject[] arrows = MakeArrows(shootShieldTime, speed, allArrowTag, true);
			for (int i = 0; i < arrows.Length; i++)
			{
				if (arrows[i] != null)
					AddInstance(arrows[i]);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use CreateChart() instead")]
		public GameObject[] NormalizedObjects(float shootShieldTime, float speed, string allArrowTag) => MakeArrows(shootShieldTime, speed, allArrowTag, true);

		private readonly Dictionary<string, Action> chartingActions = [];
		/// <summary>
		/// Registers a function for <see cref="CreateChart(float, float, float, string[])"/> to execute
		/// </summary>
		/// <param name="name">The name of the function</param>
		/// <param name="action">The action to invoke when executed</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterFunction(string name, Action action)
		{
			if (!chartingActions.TryAdd(name, action))
				chartingActions[name] = action;
		}
		private readonly List<string> removingActions = [];
		/// <summary>
		/// Registers a one time function for <see cref="CreateChart(float, float, float, string[])"/> to execute, function will be unregistered in the next frame
		/// </summary>
		/// <param name="name">The name of the function</param>
		/// <param name="action">The action to invoke when executed</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterFunctionOnce(string name, Action action)
		{
			if (chartingActions.TryAdd(name, action))
				removingActions.Add(name);
		}
		/// <summary>
		/// Allocates a direction for arrows
		/// </summary>
		/// <param name="slot">The slot to allocate in (Range is [0, 9])</param>
		/// <param name="direction">The direction to allocate</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrowAllocate(int slot, int direction) => DirectionAllocate[slot] = direction;
		/// <summary>
		/// The last arrow created from <see cref="CreateChart(float, float, float, string[])"/>
		/// </summary>
		protected static Arrow LastArrow { get; private set; }
		/// <summary>
		/// The current time calculated in <see cref="CreateChart(float, float, float, string[])"/>
		/// </summary>
		public static float CurrentTime { get; private set; } = 0;
		public static bool DelayEnabled { private get; set; } = true;
		/// <summary>
		/// Temporary variable slot you can use, has a size of 100
		/// </summary>
		public static object[] Temps { get; private set; } = new object[100];
		/// <summary>
		/// Arguments supplied to the function in the strings in <seealso cref="CreateChart(float, float, float, string[])"/>
		/// </summary>
		public static float[] Arguments { get; private set; }
		/// <summary>
		/// String based chart creator, use an empty string for an empty beat.<br/>
		/// Optional Args: "!": No Score, "^": Accelerate, "&lt;": RotateL, "&gt;": RotateR, "*": Tap, "~": Void Sprite, "_": Hold<br/>
		/// Order of parsing: ~*_&lt;&gt;^!<br/>
		/// Direction Args: "R": Random, "D": Different, "+/-x" Add/Sub x to the last dir. , "$x": Fixed on x direction, "Nx": Not x, "Ax": The xth allocated direction<br/>
		/// Optional Color Args: 0-> Blue, 1-> Red, 2-> Green, 3-> Purple<br/>
		/// Optional Rotation Args: 0-> None, 1-> Reverse, 2-> Diagonal<br/>
		/// GB：#xx#yz, Where "xx" means the duration beat, "y" beats direction, "z" means color, replace '#' with '%' if you don't want arrows<br/>
		/// Combinations: "(R)(+0)", NOT "R(+0)"<br/>
		/// Misc: use ' to multiply the speed of the arrow, &lt;&lt; or &gt;&gt; to adjust the current beat (>>0.5 will skip 0.5 beats)<br/>
		/// Use <see cref="RegisterFunction(string, Action)"/> or <see cref="RegisterFunctionOnce(string, Action)"/> to declare functions to execute them inside here<br/>
		/// For example RegisterFunctionOnce("func", ()=> {});<br/>
		/// "(func)(R)", will invoke the action in "func" and creates an arrow<br/>
		/// "!!X/Y", Each item will last 'Beat / 2X' beats (i.e. X = 2 then 1 beat is 4 items instead of the usual 8) for the next Y beats (If Y is undefined then it will last for the rest of the function)<br/>
		/// You can add arguments in the form of "&lt;Arg1,Arg2...&gt;Action"<br/>
		/// You may use <see cref="Arguments"/> inside the declared action in RegisterFunction(Once) to access them.<br/>
		/// Adding '@' at the end would apply a tag to the arrow, i.e. "$0@E" would apply the tag "E" to the arrow
		/// </summary>
		/// <param name="Delay">The delay for the events to be executed, generally used for preventing spawning immediately within view</param>
		/// <param name="Beat">Duration of 8 beats, generally used with <see cref="BeatTime(float)"/></param>
		/// <param name="arrowspeed">The speed of the arrows</param>
		/// <param name="Barrage">The array of strings that contains the barrage</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void CreateChart(float Delay, float Beat, float arrowspeed, string[] Barrage)
		{
			float t = Delay, effectLast = 0, currentCount = 4;
			Arguments = [];
			for (int i = 0; i < Barrage.Length; i++)
			{
				ReadOnlySpan<char> cur = Barrage[i];
				if (cur.Length > 2)
				{
					//改变间隔
					if (cur.StartsWith("!!"))
					{
						string str = Barrage[i][2..];
						int pos = str.LastIndexOf('/');
						if (pos == -1)
							effectLast = currentCount = MathUtil.FloatFromString(str);
						else
						{
							currentCount = MathUtil.FloatFromString(str[..pos]);
							effectLast = Convert.ToInt32(str[(pos + 1)..]);
						}
						continue;
					}
					else if (cur.StartsWith("\'\'"))
					{
						arrowspeed = MathUtil.FloatFromString(cur[2..].ToString());
						continue;
					}
					else if (cur.StartsWith("<<"))
					{
						t -= BeatTime(MathUtil.FloatFromString(cur[2..].ToString()));
						continue;
					}
					else if (cur.StartsWith(">>"))
					{
						t += BeatTime(MathUtil.FloatFromString(cur[2..].ToString()));
						continue;
					}
				}
				CurrentTime = t;
				if (!string.IsNullOrWhiteSpace(cur.ToString()))
					NormalizedChart(t, arrowspeed, cur.ToString());
				t += Beat / (currentCount * 2f);
				if (effectLast > 0)
					effectLast--;
				if (effectLast <= 0)
					currentCount = 4;
			}
		}
		public sealed override void Update()
		{
			//Remove charting actions
			if (removingActions.Count > 0)
			{
				removingActions.ForEach(s => chartingActions.Remove(s));
				removingActions.Clear();
			}
			//Set multiple BPM
			if (_isMultiBPM && InBeat(_MultiBPMChange[_curBPMStage]))
				SingleBeat = _MultiBPM[++_curBPMStage].bpm;
		}
		/// <summary>
		/// Loads a file (Cross-platform, will auto dispose)
		/// </summary>
		/// <typeparam name="T">Content type</typeparam>
		/// <param name="path">Path to file</param>
		/// <param name="cm">Content manager to use</param>
		/// <returns>The loaded content</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T LoadContent<T>(string path, ContentManager cm = null)
		{
			loadedContents.Add(path);
			return DrawingLab.LoadContent<T>(path, cm);
		}
		/// <summary>
		/// Loads an image (.bmp /.gif /.jpg /.png, will auto dispose)
		/// </summary>
		/// <param name="path">Path of the image</param>
		/// <returns>The loaded texture</returns>
		public Texture2D LoadImage(string path)
		{
			Texture2D image = Texture2D.FromFile(GameStates.SpriteBatch.GraphicsDevice, path);
			loadedImages.Add(image);
			return image;
		}
		public override void Dispose()
		{
			loadedContents.ForEach(Scene.Loader.UnloadAsset);
			loadedImages.ForEach(s => s.Dispose());
			loadedContents.Clear();
			loadedImages.Clear();
		}
	}
	/// <summary>
	/// The list of information of the chart
	/// </summary>
	public abstract class SongInformation
	{
		/// <summary>
		/// Whether the music is an .ogg file
		/// </summary>
		public bool MusicOptimized { get; protected set; } = true;
		/// <summary>
		/// The display name of the chart (Does not affect save data)
		/// </summary>
		public virtual string DisplayName => "";
		/// <summary>
		/// The composer of the song
		/// </summary>
		public virtual string SongAuthor => "Unknown";
		/// <summary>
		/// The charter
		/// </summary>
		public virtual string BarrageAuthor => "Unknown";
		/// <summary>
		/// The person who made the effects of the chart
		/// </summary>
		public virtual string AttributeAuthor => "Unknown";
		/// <summary>
		/// The artist of the chart cover
		/// </summary>
		public virtual string PaintAuthor => "Unknown";
		/// <summary>
		/// Extra text displayed on the loading screen
		/// </summary>
		public virtual string Extra => "";
		/// <summary>
		/// The position of the <see cref="Extra"/> text
		/// </summary>
		public virtual Vector2 ExtraPosition => new(20, 50);
		/// <summary>
		/// The <see cref="Color"/> of the <see cref="Extra"/> text
		/// </summary>
		public virtual Color ExtraColor => Color.White;
		/// <summary>
		/// Whether the chart is hidden or not, you can use a get; set; for setting this
		/// </summary>
		public virtual bool Hidden => false;
		/// <summary>
		/// The beginning and end of the music preview (In seconds)
		/// </summary>
		public virtual float[] MusicPreview => [0, 15];
		/// <summary>
		/// The tags of the chart (Used in chart grouping)
		/// </summary>
		public virtual string[] Tags => ["Ungrouped"];
		/// <summary>
		/// The difficulty constants for completing the chart
		/// </summary>
		public virtual Dictionary<Difficulty, float> CompleteDifficulty => [];
		/// <summary>
		/// The difficulty constant for achieving accuracy in the chart
		/// </summary>
		public virtual Dictionary<Difficulty, float> ComplexDifficulty => [];
		/// <summary>
		/// The difficulty constants for All Perfecting the chart
		/// </summary>
		public virtual Dictionary<Difficulty, float> APDifficulty => [];
		/// <summary>
		/// The difficulties that are unlocked, if they are locked, the player cannot play them
		/// </summary>
		public virtual HashSet<Difficulty> UnlockedDifficulties => [Difficulty.Noob, Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Extreme, Difficulty.ExtremePlus];
	}
}