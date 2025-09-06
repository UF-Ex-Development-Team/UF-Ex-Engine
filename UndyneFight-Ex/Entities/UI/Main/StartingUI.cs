using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.GameInterface;
using static UndyneFight_Ex.Entities.StartingShower;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources.Sprites;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// The items to display on startup
	/// </summary>
	public static class StartingShower
	{
		/// <summary>
		/// The default UI to show
		/// </summary>
		public static Type defaultUI = typeof(IntroUI);
		/// <summary>
		/// The title display on the loading screen
		/// </summary>
		public static Type TitleSetUp;
		/// <summary>
		/// The title display on the menu
		/// </summary>
		public static Type TitleShower;
	}
	internal class IntroUI : Selector
	{
		private static bool hasCreated = false;
		private const int selectionCount = 6;

		private readonly Texture2D[] selectionTextures = new Texture2D[selectionCount];
		private readonly string[] selectionIntroduction = new string[selectionCount];
		private readonly Color[] selectionBack = new Color[selectionCount];

		private Entity UpdatingEntity;

		private float alpha = 0;
		private int appearTime = 110;

		public static RatingShowing RatingShowing { get; private set; }

		public override void Start()
		{
			InstanceCreate(RatingShowing = new RatingShowing());
			base.Start();
		}

		public void CreateBackground()
		{
			if (hasCreated)
			{
				if (TitleShower != null)
					UpdatingEntity = Activator.CreateInstance(TitleShower) as Entity;
				appearTime = 1;
			}
			else
			{
				if (TitleSetUp != null)
					UpdatingEntity = Activator.CreateInstance(TitleSetUp) as Entity;
				hasCreated = true;
			}
			if (UpdatingEntity != null)
				AddChild(UpdatingEntity);
		}
		public IntroUI()
		{
			CreateBackground();
			AutoDispose = false;
			IsCancelAvailable = false;

			ImageCentre = new Vector2(72);
			selectionTextures = [login, mainGame, championShip, options, achievements];
			selectionBack = [Color.Lime, col.Red, col.Gold, col.Silver, col.Pink];
			selectionIntroduction = ["Login", "Start game", "Championships", "Options", "My achievements"];

			if (PlayerManager.CurrentUser != null)
				currentSelect = 1;

			SelectChanger += () =>
			{
				if (IsKeyPressed120f(InputIdentity.MainUp) || IsKeyPressed120f(InputIdentity.MainLeft))
				{
					currentSelect--;
				}
				else if (IsKeyPressed120f(InputIdentity.MainDown) || IsKeyPressed120f(InputIdentity.MainRight))
				{
					currentSelect++;
				}
				currentSelect = MathUtil.Posmod(currentSelect, SelectionCount);
			};
			SelectChanged += () => PlaySound(changeSelection, 0.9f);
			PushSelection(new SelectionEntities.User(this));
			PushSelection(new SelectionEntities.MainGame(this));

			PushSelection(new SelectionEntities.Championship(this));
			PushSelection(new SelectionEntities.SettingsIntro(this));

			if (ClassicalGUI.MainMenuSettings.AchievementsEnabled)
				PushSelection(new SelectionEntities.AchievementsIntro(this));
		}

		private static class SelectionEntities
		{
			public class User(IntroUI ui) : Model(ui, 0, (ui) => InstanceCreate(string.IsNullOrEmpty(PlayerManager.currentPlayer) ? (PlayerManager.playerInfos.Count != 0 ? new LoginUI() : new RegisterUI()) : new AccountManager())) { }
			public class MainGame(IntroUI ui) : Model(ui, 1, (ui) =>
				{
					FightSystem.SelectMainSet();
					InstanceCreate(new ModeSelector());
				})
			{ }
			public class Championship(IntroUI ui) : Model(ui, 2, (ui) => InstanceCreate(new ChampionShipSelector())) { }
			public class SettingsIntro(IntroUI ui) : Model(ui, 3, (ui) => InstanceCreate(new Settings.SettingsShower())) { }
			public class AchievementsIntro(IntroUI ui) : Model(ui, 4, (ui) => InstanceCreate(GameStartUp.AchievementUI ?? new Achievements.AchievementUI())) { }
			public class Model : Entity, ISelectAble
			{
				private readonly Selector father;
				protected Model(IntroUI sel, int id, Action<Selector> select)
				{
					Image = sel.selectionTextures[id];
					father = sel;
					Centre = new Vector2(320, 304);
					introduction = sel.selectionIntroduction[id];
					back = sel.selectionBack[id];
					this.select = select;
					UpdateIn120 = true;
				}

				private Color back;
				private readonly Action<Selector> select;
				private readonly string introduction;
				private bool enabled = false;
				private float showingScale = 0;
				public void DeSelected()
				{
					enabled = false;
					Update();
					Depth += 0.02f;
				}

				public override void Draw()
				{
					if (showingScale <= 0.01f)
						return;
					CollideRect area = new(0, 0, Image.Width, Image.Height * showingScale);
					FormalDraw(Image, Centre, area.ToRectangle(), Color.White, 0, ImageCentre);
					area.SetCentre(Centre - new Vector2(0, Image.Height * (1 - showingScale) * 0.5f));
					Depth -= 0.005f;
					Color c = back * 0.2f;
					c.A = 135;
					FormalDraw(FightResources.Sprites.pixUnit, area.ToRectangle(), c);
					Depth += 0.005f;

					if (enabled)
					{
						FightResources.Font.NormalFont.CentreDraw(introduction, new(320, 410), Color.Lerp(Color.White, back, 0.2f));
					}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public void Selected()
				{
					enabled = true;
					showingScale = 1.0f;
					Update();
					Depth -= 0.02f;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public void SelectionEvent()
				{
					select(father);
					father.Dispose();
				}

				public override void Update()
				{
					Depth = 0.6f - showingScale * 0.3f - (enabled ? 0.1f : 0);
					if (!enabled)
						showingScale *= 0.85f;
				}
			}
		}

		public override void Draw()
		{
			DrawingLab.DrawRectangle(new CollideRect(new Vector2(320 - 145 / 2f, 304 - 145 / 2f), new(145, 145)), Color.White, 3, 0.9f);
			base.Draw();
		}

		public override void Update()
		{
			if (appearTime >= 100f && alpha < 1)
				alpha += 0.025f;

			base.Update();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Reverse()
		{
			CreateBackground();
			base.Reverse();
		}
	}
}