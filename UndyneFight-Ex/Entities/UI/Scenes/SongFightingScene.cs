using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.DebugState;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// Chart scene
	/// </summary>
	public class SongFightingScene : FightScene
	{
		private class SongConditionOptimizer : Entity
		{
			private readonly SongFightingScene fatherScene;
			public SongConditionOptimizer(SongFightingScene scene)
			{
				UpdateIn120 = true;
				fatherScene = scene;
			}
			public override void Draw()
			{
#if DEBUG
				FightResources.Font.NormalFont.CentreDraw(GameMain.UpdateCost.ToString("F3"), new(100, 150), Color.White * ScreenDrawing.UIColor.A, 0.7f, 0.1f);
				FightResources.Font.NormalFont.CentreDraw(KeyCheckTime1.ToString("F3"), new(50, 200), Color.White * ScreenDrawing.UIColor.A, 0.7f, 0.1f);
				FightResources.Font.NormalFont.CentreDraw(KeyCheckTime2.ToString("F3"), new(150, 200), Color.White * ScreenDrawing.UIColor.A, 0.7f, 0.1f);
#endif
			}

			public override void Update()
			{
				//I assume the additional 15 frames are for the internal music delay
				float idealPos = GametimeF - GametimeDelta + CurrentFightingScene.PlayOffset + 10;
				float curTime = CurrentFightingScene.music.TryGetPosition(out bool result);
				if (result && MathF.Abs(curTime - idealPos) > 10 && idealPos > 0)
					CurrentFightingScene.SetSongPosition(idealPos);
			}
		}
		/// <summary>
		/// Sets the current song to the given position
		/// </summary>
		/// <param name="position">The position to set to</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetSongPosition(float position) => music.TrySetPosition(position);
		/// <summary>
		/// Sets the parameters of the chart
		/// </summary>
		/// <param name="waveset">The <see cref="IWaveSet"/> interface of the chart</param>
		/// <param name="songIllustration">The illustration of the chart</param>
		/// <param name="difficulty">The current difficulty of the chart</param>
		/// <param name="musicPath">The file path to the music of the chart</param>
		/// <param name="judgeState">The judgement state of the current chart</param>
		/// <param name="mode">The game mode of the chart (Default <see cref="GameMode.None"/></param>
		/// <param name="unload">Unload assets (Default true)</param>
		public class SceneParams(IWaveSet waveset, Texture2D songIllustration, int difficulty, string musicPath, JudgementState judgeState, GameMode mode = GameMode.None, bool unload = true)
		{
			/// <summary>
			/// The <see cref="IWaveSet"/> of the current chart
			/// </summary>
			public IWaveSet Waveset => (IWaveSet)Activator.CreateInstance(wavesetType);

			private readonly Type wavesetType = waveset.GetType();
			/// <summary>
			/// The current difficulty of the chart
			/// </summary>
			public int difficulty = difficulty;
			private readonly string musicPath = musicPath;
			/// <summary>
			/// The current <see cref="GameMode"/> of the chart
			/// </summary>
			public GameMode mode = mode;
			/// <summary>
			/// The current <see cref="JudgementState"/> of the chart
			/// </summary>
			public JudgementState JudgeState = judgeState;
			/// <summary>
			/// Is the music an ogg file
			/// </summary>
			public bool MusicOptimized { get; set; } = false;
			/// <summary>
			/// Loads the music
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void LoadMusic()
			{
				if (SongIllustration?.IsDisposed ?? false)
				{
					string name = SongIllustration.Name;
					name = name.StartsWith("Content") ? name[8..] : name;
					SongIllustration = DrawingLab.LoadContent<Texture2D>(Path.Combine(name.Split('\\')));
				}
				Music = new(MusicOptimized ? musicPath + ".ogg" : musicPath, Loader);
				MusicDuration = (float)Music.SongDuration.TotalSeconds * 62.5f;
			}
			/// <summary>
			/// The current music of the chart
			/// </summary>
			public Audio Music { get; private set; }
			/// <summary>
			/// Whether the music is loaded
			/// </summary>
			public bool MusicLoaded => Music != null;
			/// <summary>
			/// The duration of the music (In frames)
			/// </summary>
			public float MusicDuration { get; private set; }
			/// <summary>
			/// The illustration of the chart
			/// </summary>
			public Texture2D SongIllustration { get; set; } = songIllustration;
			/// <summary>
			/// Whether the assets will be unloaded
			/// </summary>
			public bool IsUnload { get; private set; } = unload;
		}

		internal IWaveSet waveset;
		private readonly SceneParams currentParam;
		private int appearTime = 0;
		/// <inheritdoc/>
		public SongFightingScene(SceneParams _params, Challenge challenge = null)
		{
			_challenge = challenge;
			currentParam = _params;
			difficulty = _params.difficulty;
		}
		private readonly Challenge _challenge = null;
		/// <summary>
		/// The accuracy bar
		/// </summary>
		public AccuracyBar Accuracy { get; set; }
		internal StateShower ScoreState { get; set; }
		internal TimeShower Time { get; set; }
		/// <summary>
		/// The current <see cref="JudgementState"/> of the chart
		/// </summary>
		public JudgementState JudgeState => currentParam.JudgeState;
		/// <summary>
		/// The current <see cref="Difficulty"/> of the chart
		/// </summary>
		public Difficulty CurrentDifficulty => (Difficulty)currentParam.difficulty;

		private GameMode mode;
		/// <summary>
		/// The current <see cref="GameMode"/> of the chart
		/// </summary>
		public override GameMode Mode => mode;

		private volatile bool songLoaded = false;
		private Audio music;
		private bool forceEnd = false;
		/// <summary>
		/// The offset of the music of the chart (In frames)
		/// </summary>
		public float PlayOffset { get; set; } = 0;
		/// <summary>
		/// Whether the chart will automatically end after the song ends
		/// </summary>
		public bool AutoEnd { get; set; } = true;
		/// <summary>
		/// Whether the HP had reached 0 in <see cref="GameMode.Practice"/>
		/// </summary>
		internal bool HPReached0 = false;
		/// <summary>
		/// Whether the soul had became green in <see cref="GameMode.NoGreenSoul"/>
		/// </summary>
		internal bool GreenSoulUsed = false;
		/// <summary>
		/// Whether an item was used (that affects result) during the chart
		/// </summary>
		internal bool ItemUsed = false;
		/// <summary>
		/// The score multiplier
		/// </summary>
		internal float ScoreMultiplier = 1;
		/// <summary>
		/// The illustration of the chart
		/// </summary>
		public Texture2D SongIllustration { get; set; } = null;
		private bool endRan = false;

		private int restartTimer = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void ForceEnd() => forceEnd = true;
		/// <summary>
		/// Whether the chart music has been played (Sanity check for negative <see cref="PlayOffset"/>)
		/// </summary>
		private bool MusicPlayed = false;

		/// <inheritdoc/>
		public override void Update()
		{
			if (waveset != null)
			{
				restartTimer = IsKeyDown(InputIdentity.Reset) ? restartTimer++ : 0;
				if (restartTimer >= 60 || (IsKeyDown(InputIdentity.Reset) && IsKeyDown(InputIdentity.Alternate)))
				{
					GameMain.instance.SetGameoverScreen();
					PlayDeath();
					return;
				}
			}

			//Play Music
			if (++appearTime >= 30)
			{
				if (!songLoaded)
				{
					lock (this)
					{
						SetSongFight();
						music = currentParam.Music;
						if (PlayOffset < 0)
							AddInstance(new InstantEvent(-PlayOffset, () =>
							{
								music.Play();
								MusicPlayed = true;
							}));
						else
						{
							MusicPlayed = true;
							music.PlayPosition = PlayOffset;
							music.Play();
						}
						InstanceCreate(new SongConditionOptimizer(this));
						isInBattle = songLoaded = true;
						ResetTime();
						//Initialize Items
						if (PlayerManager.CurrentUser != null)
							foreach (StoreItem item in StoreData.UserItems.Values)
							{
								if (item.Activated && (item.Attributes & StoreItem.ItemAttribute.Initialize) != 0)
									item.InitializeItem();
							}
					}
				}
				else if (waveset != null)
					UpdateSong();
				//Items
				void ProcessItems()
				{
					//Sanity check for the 0.5f delay
					if (CurrentScene is not SongFightingScene)
						return;
					ScoreMultiplier = 1;
					if (PlayerManager.CurrentUser != null)
						foreach (StoreItem item in StoreData.UserItems.Values)
						{
							if (item.Activated)
							{
								bool ItemVoidScore = (item.Attributes & StoreItem.ItemAttribute.VoidScore) != 0;
								if ((item.Attributes & StoreItem.ItemAttribute.Decoration) != 0)
								{
									item.Decoration();
									if (ItemVoidScore)
										ItemUsed = true;
								}
								if ((item.Attributes & StoreItem.ItemAttribute.Consumable) != 0 && item.TriggerCondition())
								{
									item.Used();
									if (ItemVoidScore)
										ItemUsed = true;
								}
								if (item.Affecting && (item.Attributes & StoreItem.ItemAttribute.ReduceScore) != 0)
								{
									ScoreMultiplier *= 1 - item.ReducePercentage;
								}
							}
						}
				}
				ProcessItems();
				AddInstance(new InstantEvent(0.5f, ProcessItems));
			}

			bool needEnd = waveset != null && MusicPlayed && appearTime > currentParam.MusicDuration * 2 && (music?.IsEnd ?? false);
			if (needEnd)
			{
				if (!endRan)
				{
					StateShower.instance.EndAction?.Invoke();
					endRan = true;
				}
			}
			if ((needEnd && AutoEnd) || forceEnd)
			{
				Surface.Normal.drawingAlpha -= 0.015f;
				ScreenDrawing.MasterAlpha -= 0.015f;
				if (Surface.Normal.drawingAlpha < 0 && ScreenDrawing.MasterAlpha < 0)
				{
					ScreenDrawing.MasterAlpha = 1;
					if (_challenge == null || !IsInChallenge)
						WinFight();
					else
						ChallengeSave();
				}
			}
			mode = currentParam.mode;
			base.Update();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void WinFight()
			{
				StateShower ss = StateShower.instance;
				ResetFightState(false);
				ResetScene(new WinScene(ss, PlayerInstance.GameAnalyzer));
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void ChallengeSave()
			{
				SongResult result;
				result = StateShower.instance.GenerateResult();
				PlayerManager.RecordMark(currentParam.Waveset.FightName, currentParam.difficulty,
					result.CurrentMark, result.Score, result.AC, result.AP, result.Accuracy);
				PlayerManager.Save();
				ResetFightState(false);
				_challenge.ResultBuffer.Add(result);
				ResetScene(ChallengeCount == ++CurChallengeNum
					? new ChallengeWinScene(_challenge)
					: new SongLoadingScene(_challenge, ChallengeCharts[1..]));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSongFight()
		{
			otherAuto = redShieldAuto = blueShieldAuto = greenShieldAuto = purpleShieldAuto = (mode & GameMode.Autoplay) != 0;

			MathUtil.rander = new Random(seed);
			InstanceCreate(Accuracy = new());
			InstanceCreate(ScoreState = new StateShower(waveset = currentParam.Waveset, currentParam.difficulty, JudgeState, currentParam.mode, currentParam.MusicDuration));
			InstanceCreate(Time = new());
			//This function MUST be called before the creation of the player for proper
			//garbage collection to prevent bugs such as:
			//Blue soul not moving after giving force
			StartBattle();
			InstanceCreate(PlayerInstance = new());
			InstanceCreate(HPBar = new());
			if (waveset is GameObject)
				InstanceCreate(waveset as GameObject);
			waveset.Start();
			SongIllustration = currentParam.SongIllustration;
		}
		private void UpdateSong()
		{
			if (waveset is IWaveSetS waveS)
				waveS.Chart();
			else
			{
				switch (currentParam.difficulty)
				{
					case 0:
						waveset.Noob();
						break;
					case 1:
						waveset.Easy();
						break;
					case 2:
						waveset.Normal();
						break;
					case 3:
						waveset.Hard();
						break;
					case 4:
						waveset.Extreme();
						break;
					case 5:
						waveset.ExtremePlus();
						break;
				}
			}
		}

		//debug
		private void TempIntro()
		{
			// debug
			_challenge.ResultBuffer.Add(new SongResult(SkillMark.Impeccable, 0, 0.99f, false, false));
			_challenge.ResultBuffer.Add(new SongResult(SkillMark.Excellent, 0, 0.98f, true, false));
			_challenge.ResultBuffer.Add(new SongResult(SkillMark.Acceptable, 0, 0.96f, true, true));
			ResetScene(new ChallengeWinScene(_challenge));
		}
		/// <inheritdoc/>
		public override void Dispose()
		{
			music?.Stop();
			base.Dispose();
			waveset = null;
		}
		/// <summary>
		/// Call player death event
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void PlayerDied() => ResetScene(new TryAgainScene(StateShower.instance));
	}
}