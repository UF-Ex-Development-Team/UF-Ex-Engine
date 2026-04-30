using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.Fight;
using UndyneFight_Ex.GameInterface;
using UndyneFight_Ex.SongSystem;
using static GlobalData;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources;

namespace UndyneFight_Ex;

/// <summary>
/// Loading scene
/// </summary>
public class LoadingScene : Scene
{
	internal LoadingScene(Action loadingFinished, Action loadingAction, bool unLoad = true)
	{
		if (CurrentScene != null)
			CurrentScene.CurrentDrawingSettings.defaultWidth = 640f;
		if (unLoad)
			Loader.Unload();
		LoadingFinishedEvent = loadingFinished;

		loadingTask = new(() =>
		{
			loadingAction();
			finishedLoad = true;
		});
		//Load audio previews if its the initial loading screen
		if (this is ResourcesLoadingScene)
			LoadSongPreviews();
	}

	private readonly Action LoadingFinishedEvent;
	private readonly Task loadingTask;
	internal int appearTime = 0;

	internal bool finishedLoad = false;
	internal const int LeastLoadingTime = 120;

	internal bool eventInvoked = false;

	/// <inheritdoc/>
	public override void Update()
	{
		//Run the loading task after a tiny buffer
		if (++appearTime == 2)
			loadingTask.Start();
		//Loading is finished when the task is complete and all audio previews are loaded if it's the initial loading screen
		else if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked && (this is not ResourcesLoadingScene || LoadedPreviewAudioCount == AudioPreviewDatas.Count))
		{
			eventInvoked = true;
			LoadingFinishedEvent();
		}
		base.Update();
	}
}
/// <summary>
/// Loading a song
/// </summary>
public class SongLoadingScene : LoadingScene
{
	private readonly SongInformation Information;
	private static SongFightingScene.SceneParams songParams;
	/// <summary>
	/// Creates a loading scene
	/// </summary>
	/// <param name="songParams">The parameters of the chart</param>
	public SongLoadingScene(SongFightingScene.SceneParams songParams) : base(() =>
	{
		//When loading is done, move to chart scene after the fade out animation
		DelayEventProcessor.AddInstantEvent(90, () => ResetScene(new SongFightingScene(songParams)));
		Loaded = true;
	}, () =>
	{
		//Main loading action
		songParams.MusicOptimized = songParams.Waveset.Attributes?.MusicOptimized ?? false;
		SongLoadingScene.songParams.LoadMusic();
	}, songParams.IsUnload)
	{
		//Other stuff
		Loaded = false;
		SongLoadingScene.songParams = songParams;
		Information = songParams.Waveset.Attributes;
		tipID = MathUtil.GetRandom(0, Tips.Count() - 1);
	}
	/// <summary>
	/// New load challenge method
	/// </summary>
	/// <param name="challenge">The challenge to start</param>
	/// <param name="songParams"></param>
	public SongLoadingScene(Challenge challenge, params SongFightingScene.SceneParams[] songParams) : base(() =>
	{
		//When loading is done, move to chart scene after the fade out animation
		DelayEventProcessor.AddInstantEvent(30, () => ResetScene(new SongFightingScene(songParams[0], challenge)));
		Loaded = true;
	}, () =>
	{
		//Main loading action
		if (songParams[0].Waveset.Attributes?.MusicOptimized ?? false)
			songParams[0].MusicOptimized = true;
		songParams[0].LoadMusic();
	}, songParams[0].IsUnload)
	{
		//Other stuff
		IsInChallenge = true;
		ChallengeCount = songParams.Length;
		CurChallengeNum = 0;
		ChallengeCharts = songParams;
		Loaded = false;
		SongLoadingScene.songParams = songParams[0];
		difficulty = SongLoadingScene.songParams.difficulty;
		Information = songParams[0].Waveset.Attributes;
		tipID = MathUtil.GetRandom(0, Tips.Count() - 1);
	}
	private float alpha = 0, tipY = 500, infoX = -640, titleY = -80, titleAlpha = 0, paintAlpha = 1;
	private static bool Loaded = false;

	private readonly JToken Tips = Localization.GetTranslationData().SelectToken("LoadingScene.Tips");

	/// <inheritdoc/>
	public override void Draw()
	{
		(GLFont drawFont, float fontScale) = Localization.GetFontData("NormalFont");
		Depth = -0.1f;
		DrawingLab.DrawRectangle(new Rectangle(new(316, 79), new(323, 242)), Color.DeepSkyBlue * paintAlpha * 0.6f, 2.5f, 0.99f);
		Texture2D chartPaint = songParams.SongIllustration;
		//Fake blur, I need to figure out how to apply shaders on the fly
		if (chartPaint != null)
		{
			for (int i = 0; i < 8; i++)
				for (int k = 0; k < 4; k++)
					GeneralDraw(chartPaint, new Vector2(320, 240) + MathUtil.GetVector2(k * 3, i * 45), Color.White * titleAlpha * 0.025f, new Vector2(640f / chartPaint.Width, 480f / chartPaint.Height), depth: 0);
			GeneralDraw(chartPaint, new Vector2(477.5f, 200), Color.White * paintAlpha, new Vector2(320f / chartPaint.Width, 240f / chartPaint.Height), depth: 1);
		}
		else
			Localization.DrawLocalizedText("LoadingScene.NoPaint", new Vector2(477.5f, 200), font: "FightFont", color: Color.Red * paintAlpha, depth: 1, align: Localization.DrawAlign.Middle);
		//Title
		string songName = GetWavesetDisplayName(songParams.Waveset);
		drawFont.CentreDraw(songName, new Vector2(320, titleY), Color.White, new Vector2(float.Min(1, 600f / Font.NormalFont.SFX.MeasureString(songName).X), 1), 1);
		Color DiffCol = difficulty switch
		{
			0 => Color.White,
			1 => Color.LawnGreen,
			2 => Color.LightBlue,
			3 => Color.MediumPurple,
			4 => Color.Orange,
			_ => Color.Gray
		};
		drawFont.CentreDraw(ChartDifficultyNames[songParams.Waveset.FightName][(Difficulty)difficulty], new Vector2(320, titleY + 35), DiffCol, fontScale, 0.1f);
		Color lerpWhite = Color.White * alpha;
		//Chart information
		if (Information != null)
		{
			int CurPos = 150;
			if (Information.BarrageAuthor != "Unknown")
			{
				Localization.DrawLocalizedText("LoadingScene.Barrage", new Vector2(infoX, CurPos), scale: new(0.8f), color: lerpWhite, depth: 0.5f);
				CurPos += 22;
				drawFont.Draw(Information.BarrageAuthor, new(infoX + 20, CurPos), lerpWhite, float.Min(0.4f, 280f / Font.NormalFont.SFX.MeasureString(Information.BarrageAuthor).X) * fontScale, 0.5f);
				CurPos += 15;
			}
			if (Information.SongAuthor != "Unknown")
			{
				Localization.DrawLocalizedText("LoadingScene.Composer", new Vector2(infoX, CurPos), scale: new(0.8f), color: lerpWhite, depth: 0.5f);
				CurPos += 22;
				drawFont.Draw(Information.SongAuthor, new(infoX + 20, CurPos), lerpWhite, float.Min(0.4f, 280f / Font.NormalFont.SFX.MeasureString(Information.SongAuthor).X) * fontScale, 0.5f);
				CurPos += 15;
			}
			if (Information.PaintAuthor != "Unknown")
			{
				Localization.DrawLocalizedText("LoadingScene.Paint", new Vector2(infoX, CurPos), scale: new(0.8f), color: lerpWhite, depth: 0.5f);
				CurPos += 22;
				drawFont.Draw(Information.PaintAuthor, new(infoX + 20, CurPos), lerpWhite, float.Min(0.4f, 280f / Font.NormalFont.SFX.MeasureString(Information.PaintAuthor).X) * fontScale, 0.5f);
				CurPos += 15;
			}
			if (Information.AttributeAuthor != "Unknown")
			{
				Localization.DrawLocalizedText("LoadingScene.Effect", new Vector2(infoX, CurPos), scale: new(0.8f), color: lerpWhite, depth: 0.5f);
				CurPos += 22;
				drawFont.Draw(Information.AttributeAuthor, new(infoX + 20, CurPos), lerpWhite, float.Min(0.4f, 280f / Font.NormalFont.SFX.MeasureString(Information.AttributeAuthor).X) * fontScale, 0.5f);
			}

			drawFont.CentreDraw(Information.Extra, new(320, 360), Information.ExtraColor * alpha, 0.75f * fontScale, 0.5f);
		}
		//Tips
		drawFont.Draw(Tips[tipID].ToString(), new Vector2(12, tipY), lerpWhite, float.Min(0.48f, 600f / Font.NormalFont.SFX.MeasureString(Tips[tipID].ToString()).X) * fontScale, 0.5f);
		base.Draw();
		//Loading sprites
		GeneralDraw(Sprites.loadingText, new Vector2(280, 430), lerpWhite, depth: 1);
		for (int i = 0; i < 6; i++)
			GeneralDraw(Sprites.progressArrow, new(395 + i * 20, 430), lerpWhite * (Functions.Sin((appearTime - i * 6 - 20) * 3.75f) * 0.9f + 0.1f) * 0.8f);
	}
	private int tipID;
	/// <inheritdoc/>
	public override void Update()
	{
		//Change tips
		if (IsKeyPressed120f(InputIdentity.Alternate))
		{
			Functions.PlaySound(FightResources.Sounds.Ding);
			tipID = MathUtil.GetRandom(0, Tips.Count() - 1);
		}
		//Lerping stuff
		alpha = float.Lerp(alpha, Loaded ? 0 : 1, 0.16f);
		titleAlpha = float.Lerp(titleAlpha, Loaded ? 0 : 1, 0.03f);
		paintAlpha = float.Lerp(paintAlpha, Loaded ? 0 : 1, 0.08f);
		titleY = float.Lerp(titleY, Loaded ? -80 : 20, 0.08f);
		tipY = float.Lerp(tipY, Loaded ? 500 : 464, 0.08f);
		infoX = float.Lerp(infoX, Loaded ? -640 : 20, 0.08f);
		base.Update();
	}
}
/// <summary>
/// Initial loading scene for loading global resources
/// </summary>
internal class ResourcesLoadingScene : LoadingScene
{
	private float loadProgress = 0, splashAlpha = 0;
	private int splashHoldTime = -90;
	private bool splashIsFading = false;
	private static ContentManager loader;
	private SplashState SplashScreenState = SplashState.Undertale;
	public static ResourcesLoadState LoadState = ResourcesLoadState.Title_Sprite;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void MainResourcesLoad()
	{
		FightResources.Initialize(loader);
		GameStartUp.Initialize?.Invoke(loader);
	}
	public ResourcesLoadingScene(ContentManager loader) : base(() => ResetScene(new GameMenuScene()), MainResourcesLoad) => ResourcesLoadingScene.loader = loader;
	public override void Draw()
	{
		(GLFont drawFont, float fontScale) = Localization.GetFontData("NormalFont");
		if (SplashScreenState != SplashState.Ended)
		{
			//Splash screen
			GeneralDraw(FightResources.Sprites.pixUnit, new(320, 240), Color.Black * (1 - splashAlpha), new(640, 480), depth: 0.99f);
			if (SplashScreenState == SplashState.Undertale)
			{
				Localization.DrawLocalizedText("SplashScreen.First[0]", new Vector2(320, 200), align: Localization.DrawAlign.Middle);
				Localization.DrawLocalizedText("SplashScreen.First[1]", new Vector2(320, 240), scale: new(2), align: Localization.DrawAlign.Middle);
				Localization.DrawLocalizedText("SplashScreen.First[2]", new Vector2(320, 280), align: Localization.DrawAlign.Middle);
			}
			else
			{
				Localization.DrawLocalizedText("SplashScreen.Second[0]", new Vector2(320, 225), align: Localization.DrawAlign.Middle);
				Localization.DrawLocalizedText("SplashScreen.Second[1]", new Vector2(320, 255), align: Localization.DrawAlign.Middle);
			}
			return;
		}
		GeneralDraw(FightResources.Sprites.pixUnit, new(320, 240), Color.Black * (1 - splashAlpha), new(640, 480), depth: 0.99f);
		base.Draw();
		GeneralDraw(Sprites.loadingText, new Vector2(280, 430), Color.White * (appearTime / 20f));
		for (int i = 0; i < 6; i++)
			GeneralDraw(Sprites.progressArrow, new(395 + i * 20, 430), Color.White * (Functions.Sin((appearTime - i * 6 - 20) * 3.75f) * 0.9f + 0.1f) * 0.8f);
		Localization.DrawLocalizedText("SplashScreen.Boot", new Vector2(320, 120), scale: new Vector2(0.8f), depth: 0, align: Localization.DrawAlign.Middle);
		//Audio preview loading bar
		Localization.DrawLocalizedText("SplashScreen.AudioPreview", new Vector2(320, 320), [LoadedPreviewAudioCount, AudioPreviewDatas.Count], color: Color.White * MathF.Abs(Functions.Sin(appearTime)), scale: new Vector2(0.6f), depth: 0.98f, align: Localization.DrawAlign.Middle);
		GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(320, 320), Color.White, new Vector2(404, 24));
		GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(320, 320), Color.Gray, new Vector2(400, 20));
		GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(120 + loadProgress, 320), Color.LimeGreen, new Vector2(loadProgress * 2, 20));
		string str = Localization.GetText("SplashScreen.Loading", Localization.GetText($"SplashScreen.LoadState.{LoadState}"));
		for (int i = 0; i < DateTime.Now.Ticks / 5000000 % 4; i++)
			str += ".";
		drawFont.CentreDraw(str, new Vector2(320, 360), Color.White, 0.6f * fontScale, 0.5f);
		if (Sprites.loadingTexture != null)
			GeneralDraw(Sprites.loadingTexture, GameStartUp.LoadingSettings.TitleCentrePosition, Color.White * (appearTime / 20f), new Vector2(MathF.Min(640f / Sprites.loadingTexture.Width, 1)));
	}
	public override void Update()
	{
		base.Update();
		loadProgress = float.Lerp(loadProgress, 200f * LoadedPreviewAudioCount / AudioPreviewDatas.Count, 0.04f);
		if (splashHoldTime < 0)
			splashHoldTime++;
		else
			splashAlpha = float.Lerp(splashAlpha, splashIsFading ? 0 : 1, 0.08f);
		if (SplashScreenState == SplashState.Ended)
			return;
		if (splashAlpha < 0.005f && splashHoldTime >= 0 && splashIsFading)
		{
			splashAlpha = 0;
			SplashScreenState = SplashScreenState == SplashState.Undertale ? SplashState.MadeBy : SplashState.Ended;
			splashHoldTime = -90;
			splashIsFading = false;
		}
		//Ensure evaluation order
		else if ((splashAlpha > 0.995f || IsKeyPressed120f(InputIdentity.Confirm)) && ++splashHoldTime == 90)
		{
			splashIsFading = true;
			//Buffer time for fading
			splashHoldTime = -120;
		}
	}
	private enum SplashState
	{
		Undertale,
		MadeBy,
		Ended
	}
	internal enum ResourcesLoadState
	{
		Title_Sprite,
		Global_Sprites,
		Fonts,
		Global_Audio,
		Global_Shaders,
		Fight_Sprites,
		Fight_Audio,
		Finished
	}
}