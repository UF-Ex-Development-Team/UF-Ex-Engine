using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.GameInterface;
using static UndyneFight_Ex.GlobalResources.Effects;
using static UndyneFight_Ex.GlobalResources.Font;
using static UndyneFight_Ex.GlobalResources.Sprites;

namespace UndyneFight_Ex;

/// <summary>
/// Resources that are used globally
/// </summary>
public static partial class GlobalResources
{
	/// <summary>
	/// Loads a file (Cross-platform, internally calls <see cref="DrawingLab.LoadContent{T}(string, ContentManager)"/>)
	/// </summary>
	/// <typeparam name="T">Content type</typeparam>
	/// <param name="path">Path to file</param>
	/// <param name="cm">Content manager to use</param>
	/// <returns>The loaded content</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T LoadContent<T>(string path, ContentManager cm = null) => DrawingLab.LoadContent<T>(path, cm);
	internal static async void Initialize(ContentManager loader)
	{
		Task task = new(()=>
		{
			//Content\FontTexture\Title.png
			string root = GameStartUp.LoadingSettings.TitleTextureRoot;
			string path = Path.Combine($"{AppContext.BaseDirectory}Content\\{root}".Split('\\'));
			if (File.Exists(path + ".xnb"))
				loadingTexture = LoadContent<Texture2D>(root, loader);

			NormalFont = new GLFont("Sprites\\font\\normal", loader);
			FightFont = new GLFont("Sprites\\font\\menu", loader);
			SansFont = new GLFont("Sprites\\font\\sans", loader);
			DamageFont = new GLFont("Sprites\\font\\DamageShow", loader);
			Japanese = new GLFont("Sprites\\font\\Japanese", loader);
			UIFont = new GLFont("Sprites\\font\\UIFont", loader);
			Chinese = new GLFont("Sprites\\font\\Chinese", loader);

			hashtex = LoadContent<Texture2D>("Global\\Shaders\\Effect Library\\hashtex", loader);
			hashtex2 = LoadContent<Texture2D>("Global\\Shaders\\Effect Library\\hashtex2", loader);
			championShip = LoadContent<Texture2D>("Global\\UI\\cup_highres", loader);
			mainGame = LoadContent<Texture2D>("Global\\UI\\maingame", loader);
			achievements = LoadContent<Texture2D>("Global\\UI\\stars", loader);
			options = LoadContent<Texture2D>("Global\\UI\\options", loader);
			cursor = LoadContent<Texture2D>("Global\\UI\\PlaceCheck", loader);
			login = LoadContent<Texture2D>("Global\\\\UI\\login", loader);
			debugArrow = LoadContent<Texture2D>("Global\\UI\\debugArrow", loader);
			record = LoadContent<Texture2D>("Global\\UI\\record", loader);
			medal = LoadContent<Texture2D>("Global\\UI\\medal1", loader);
			starMedal = LoadContent<Texture2D>("Global\\UI\\medal2", loader);
			brimMedal = LoadContent<Texture2D>("Global\\UI\\medal0", loader);
			loadingText = LoadContent<Texture2D>("Global\\Loading\\Loading", loader);
			progressArrow = LoadContent<Texture2D>("Global\\Loading\\ProgressArrow", loader);

			backGroundShader = new Shader(LoadContent<Effect>("Global\\Shaders\\BackGroundShader", loader));
			reduceBlueShader = new Shader(LoadContent<Effect>("Global\\Shaders\\reduceBlue", loader))
			{
				StableEvents = (s) => s.Parameters["reduceBlueAmount"].SetValue(Settings.SettingsManager.DataLibrary.reduceBlueAmount / 200f)
			};

			ReadCustomShaders(loader);
		});
		task.RunSynchronously();
		await task;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static async void ReadCustomShaders(ContentManager loader)
	{
		Task task = new(() =>
		{
			FightResources.Shaders.Aurora = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Aurora", loader));
			FightResources.Shaders.Sinwave = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Sinwave", loader));
			FightResources.Shaders.ColorBlend = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\ColorBlend", loader));
			FightResources.Shaders.NeonLine = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\NeonLine", loader));
			FightResources.Shaders.Camera = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\CameraSurface", loader));
			FightResources.Shaders.Cos1Ball = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Cos1Ball", loader));
			FightResources.Shaders.StepSample = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\StepSample", loader));
			FightResources.Shaders.Scale = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Scale", loader));
			FightResources.Shaders.Swirl = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Swirl", loader));
			FightResources.Shaders.Blur = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Blur", loader));
			FightResources.Shaders.BlurKawase = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\BlurKawase", loader));
			FightResources.Shaders.Polar = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Polar", loader));
			FightResources.Shaders.Gray = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Gray", loader));
			FightResources.Shaders.Seismic = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Seismic", loader));
			FightResources.Shaders.Scatter = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Scatter", loader));
			FightResources.Shaders.Mosaic = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Mosaic", loader));
			FightResources.Shaders.LightSweep = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\LightSweep", loader));
			FightResources.Shaders.Wave = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Wave", loader));

			FightResources.Shaders.Tyndall = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Tyndall", loader));
			FightResources.Shaders.Spiral = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Sprial3D", loader));
			FightResources.Shaders.Wrong = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\Wrong", loader));
			FightResources.Shaders.Fire = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\NoiseFire", loader));
			FightResources.Shaders.RadialWave = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\RadialWave", loader));

			FightResources.Shaders.DislocationX = new(LoadContent<Effect>("Global\\Shaders\\Effect Library\\DislocationX", loader));
			LoadInternals(loader);
		});
		task.RunSynchronously();
		await task;
	}

	public static partial class Effects
	{
		internal static Shader backGroundShader, reduceBlueShader;
	}
	/// <summary>
	/// A list of built-in fonts
	/// </summary>
	public static class Font
	{
		/// <summary>
		/// Mars Needs Cunnilingus
		/// </summary>
		public static GLFont FightFont { get; internal set; }
		/// <summary>
		/// Determination Mono
		/// </summary>
		public static GLFont NormalFont { get; internal set; }
		/// <summary>
		/// Sans Undertale
		/// </summary>
		public static GLFont SansFont { get; internal set; }
		/// <summary>
		/// Hachicro
		/// </summary>
		public static GLFont DamageFont { get; internal set; }
		/// <summary>
		/// ta_pop_M
		/// </summary>
		public static GLFont Japanese { get; internal set; }
		/// <summary>
		/// Crypt of Tomorrow
		/// </summary>
		public static GLFont UIFont { get; internal set; }
		/// <summary>
		/// FZXS 12
		/// </summary>
		public static GLFont Chinese { get; internal set; }
	}
	/// <summary>
	/// A list of built-in sprites
	/// </summary>
	public static class Sprites
	{
		/// <summary>
		/// Cursor sprite
		/// </summary>
		public static Texture2D cursor { get; internal set; }
		/// <summary>
		/// Legacy LOGIN sprite
		/// </summary>
		public static Texture2D login { get; internal set; }
		/// <summary>
		/// Legacy CHAMPIONSHIP sprite
		/// </summary>
		public static Texture2D championShip { get; internal set; }
		/// <summary>
		/// Hash texture
		/// </summary>
		public static Texture2D hashtex { get; internal set; }
		/// <summary>
		/// Hash texture 2
		/// </summary>
		public static Texture2D hashtex2 { get; internal set; }
		/// <summary>
		/// Legacy MAIN GAME sprite
		/// </summary>
		public static Texture2D mainGame { get; internal set; }
		/// <summary>
		/// Legacy OPTIONS sprite
		/// </summary>
		public static Texture2D options { get; internal set; }
		/// <summary>
		/// Legacy ACHIVEMENTS sprite
		/// </summary>
		public static Texture2D achievements { get; internal set; }
		/// <summary>
		/// Legacy RECORD sprite
		/// </summary>
		public static Texture2D record { get; internal set; }
		/// <summary>
		/// Debug vector arrow sprite
		/// </summary>
		public static Texture2D debugArrow { get; internal set; }
		/// <summary>
		/// Loading text
		/// </summary>
		public static Texture2D loadingText { get; internal set; }
		/// <summary>
		/// Loading arrow
		/// </summary>
		public static Texture2D progressArrow { get; internal set; }
		/// <summary>
		/// Blue star medal
		/// </summary>
		public static Texture2D medal { get; internal set; }
		/// <summary>
		/// Purple star medal
		/// </summary>
		public static Texture2D starMedal { get; internal set; }
		/// <summary>
		/// Empty medal
		/// </summary>
		public static Texture2D brimMedal { get; internal set; }
		/// <summary>
		/// Root texture
		/// </summary>
		public static Texture2D loadingTexture { get; internal set; }
	}
}
/// <summary>
/// Resources that are used in fight/charts
/// </summary>
public static class FightResources
{
	/// <summary>
	/// Loads a file (Cross-platform, internally calls <see cref="DrawingLab.LoadContent{T}(string, ContentManager)"/>)
	/// </summary>
	/// <typeparam name="T">Content type</typeparam>
	/// <param name="path">Path to file</param>
	/// <param name="cm">Content manager to use</param>
	/// <returns>The loaded content</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T LoadContent<T>(string path, ContentManager cm = null) => DrawingLab.LoadContent<T>(path, cm);
	private static readonly string[] shardCol = ["white", "yellow", "green", "purple"];
	private static readonly string[] arrType = ["", "circle_", "rot_", "tran_"];
	private static readonly string[] arrColType = ["blue", "red", "green", "purple"];
	/// <summary>
	/// The entire spritesheet for UF-Ex
	/// </summary>
	private static Texture2DAtlas _spritesheet;
	internal static async void Initialize(ContentManager loader)
	{
		Task task = new(()=>
		{
			_spritesheet = loader.Load<Texture2DAtlas>("UndyneFight_Ex Spritesheet");
			loader.RootDirectory = "Content\\Sprites";
			#region Sprites
			Sprites.pixUnit =      _spritesheet.LoadSprite("others/pixiv");
			Sprites.player =        _spritesheet.LoadSprite("SOUL/original");
			Sprites.brokenHeart =   _spritesheet.LoadSprite("SOUL\\break");
			Sprites.leftHeart =     _spritesheet.LoadSprite("SOUL\\leftSoul");
			Sprites.rightHeart =    _spritesheet.LoadSprite("SOUL\\rightSoul");
			Sprites.soulCollide =   _spritesheet.LoadSprite("SOUL\\collide");
			for (int i = 0; i < 6; i++)
			{
				//There is no need to optimize this for loop, or else the Math operation will take more time
				for (int k = 1; k <= 2; k++)
					for (int j = 0; j < 4; j++)
						Sprites.arrowShards[k - 1, j, i] = _spritesheet.LoadSprite($"bullet\\Shards\\{shardCol[j]}\\0{k}-{i + 1}");

				FightSprites.slides[i] = _spritesheet.LoadSprite("FightSprites\\frames\\frame_" + i);
				if (i < 5)
				{
					Sprites.heartPieces[i] = _spritesheet.LoadSprite("SOUL\\shard" + i);
					Sprites.GBStart[i] = _spritesheet.LoadSprite("GB\\s\\frame_" + i);
					if (i < 4)
					{
						Sprites.explodes[i] =   _spritesheet.LoadSprite("Explodes\\smallExplode" + (i + 1));
						Sprites.arrow_base[i] = _spritesheet.LoadSprite("bullet\\arrow_base" + (i + 1));
						Sprites.arrow_fore[i] = _spritesheet.LoadSprite("bullet\\arrow_fore" + (i + 1));

						for (int k = 0; k < 4; k++)
							for (int j = 0; j < 4; j++)
								Sprites.arrow[j, k, i] = _spritesheet.LoadSprite($"bullet\\{arrType[k]}{arrColType[j]}{i}");
						if (i < 2)
						{
							Sprites.GBShooting[i] = _spritesheet.LoadSprite("GB\\p\\frame_" + i);
							FightSprites.fight[i] = _spritesheet.LoadSprite("FightSprites\\atk_" + i);
							FightSprites.act[i] = _spritesheet.LoadSprite("FightSprites\\act_" + i);
							FightSprites.item[i] = _spritesheet.LoadSprite("FightSprites\\itm_" + i);
							FightSprites.mercy[i] = _spritesheet.LoadSprite("FightSprites\\mry_" + i);
						}
					}
				}
			}
			for (int i = 0; i < 18; i++)
			{
				Sprites.ParasolMett[i] = _spritesheet.LoadSprite($"OtherBarrages\\Mettaton\\spr_parasolmett_{i}");
				if (i < 7)
				{
					Sprites.MettBombCoreBlast[i] = _spritesheet.LoadSprite($"OtherBarrages\\Mettaton\\spr_plusbomb_coreblast_{i}");
					Sprites.MettBombBlast[i] = _spritesheet.LoadSprite($"OtherBarrages\\Mettaton\\spr_plusbomb_blast_{i}");
					if (i < 2)
						Sprites.MettBomb[i] = _spritesheet.LoadSprite($"OtherBarrages\\Mettaton\\spr_plusbomb_{i}");
				}
			}

			Sprites.fireball = _spritesheet.LoadSprite("OtherBarrages\\fireball");
			Sprites.spear = _spritesheet.LoadSprite("bullet\\spear");
			Sprites.spike = _spritesheet.LoadSprite("Bone\\bone_spike");
			Sprites.spider = _spritesheet.LoadSprite("OtherBarrages\\spider");
			Sprites.Croissant = _spritesheet.LoadSprite("OtherBarrages\\clo");
			Sprites.fireParticle = _spritesheet.LoadSprite("others\\fireParticle");
			Sprites.lightBall = _spritesheet.LoadSprite("others\\lightBall");
			Sprites.lightLine = _spritesheet.LoadSprite("others\\lightLine");
			Sprites.square = _spritesheet.LoadSprite("others\\square");
			Sprites.boxPiece = _spritesheet.LoadSprite("others\\boxPiece");

			Sprites.stuck1 = _spritesheet.LoadSprite("others\\GBStuck1");
			Sprites.stuck2 = _spritesheet.LoadSprite("others\\GBStuck2");

			Sprites.voidarrow[0] = _spritesheet.LoadSprite("bullet\\voidarrow\\blue0");
			Sprites.voidarrow[1] = _spritesheet.LoadSprite("bullet\\voidarrow\\red0");
			Sprites.voidarrow[2] = _spritesheet.LoadSprite("bullet\\voidarrow\\green0");
			Sprites.voidarrow[3] = _spritesheet.LoadSprite("bullet\\voidarrow\\purple0");

			Sprites.target = _spritesheet.LoadSprite("bullet\\target");
			Sprites.bullet = _spritesheet.LoadSprite("bullet\\gunBullet");
			Sprites.goldenBrim = _spritesheet.LoadSprite("bullet\\golden_tip");

			Sprites.shield = _spritesheet.LoadSprite("SOUL\\shield");
			Sprites.shinyShield = _spritesheet.LoadSprite("SOUL\\shield_shiny");
			Sprites.ShieldCircle = _spritesheet.LoadSprite("SOUL\\circle");

			Sprites.hpText = _spritesheet.LoadSprite("hp_show\\hp");
			Sprites.krText = _spritesheet.LoadSprite("hp_show\\kr");

			Sprites.boneBody = _spritesheet.LoadSprite("Bone\\bone_body");
			Sprites.boneHead = _spritesheet.LoadSprite("Bone\\bone_up");
			Sprites.boneSlab = _spritesheet.LoadSprite("Bone\\bone_slab");
			Sprites.warningLine = _spritesheet.LoadSprite("Bone\\warning_line");

			Sprites.GBLaser = _spritesheet.LoadSprite("GB\\laser");

			Sprites.explodeTrigger = _spritesheet.LoadSprite("Explodes\\explodeTrigger");
			Sprites.allPerfectText = _spritesheet.LoadSprite("others\\allPerfect");
			Sprites.accuracyBar = _spritesheet.LoadSprite("Pointer\\accuracyBar");
			Sprites.accuracyPointers[0] = _spritesheet.LoadSprite("Pointer\\accuracyPointerL");
			Sprites.accuracyPointers[1] = _spritesheet.LoadSprite("Pointer\\accuracyPointerM");
			Sprites.accuracyPointers[2] = _spritesheet.LoadSprite("Pointer\\accuracyPointerR");

			Sprites.platform[0] = _spritesheet.LoadSprite("Platform\\platform_body");
			Sprites.platform[1] = _spritesheet.LoadSprite("Platform\\platform_body2");
			Sprites.platformSide[0] = _spritesheet.LoadSprite("Platform\\platform_side");
			Sprites.platformSide[1] = _spritesheet.LoadSprite("Platform\\platform_side2");
			Sprites.SoulShoot = _spritesheet.LoadSprite("SOUL\\soulBullet");
			Sprites.MettBlockA = _spritesheet.LoadSprite("OtherBarrages\\Mettaton\\blockA");
			Sprites.MettBlockB = _spritesheet.LoadSprite("OtherBarrages\\Mettaton\\blockB");
			Sprites.MettBullet = _spritesheet.LoadSprite("OtherBarrages\\Mettaton\\bullet");
			#endregion
			#region Sounds
			loader.RootDirectory = "Content\\Sounds";
			Sounds.playerSlice = LoadContent<SoundEffect>("slice", loader);
			Sounds.printWord = LoadContent<SoundEffect>("word_sound", loader);
			Sounds.sansWord = LoadContent<SoundEffect>("sans_sound", loader);
			Sounds.Ding = LoadContent<SoundEffect>("hit", loader);
			Sounds.playerHurt = LoadContent<SoundEffect>("hurt", loader);
			Sounds.spearAppear = LoadContent<SoundEffect>("spawn", loader);
			Sounds.spearShoot = LoadContent<SoundEffect>("toss", loader);
			Sounds.pierce = LoadContent<SoundEffect>("pierce", loader);
			Sounds.select = LoadContent<SoundEffect>("choose_2", loader);
			Sounds.changeSelection = LoadContent<SoundEffect>("choose_1", loader);
			Sounds.change = LoadContent<SoundEffect>("change", loader);
			Sounds.damaged = LoadContent<SoundEffect>("damaged", loader);
			Sounds.die1 = LoadContent<SoundEffect>("die_1", loader);
			Sounds.die2 = LoadContent<SoundEffect>("die_2", loader);
			Sounds.GBSpawn = LoadContent<SoundEffect>("L_GB_summon", loader);
			Sounds.GBShoot = LoadContent<SoundEffect>("S_GB_shot", loader);
			Sounds.heal = LoadContent<SoundEffect>("heal", loader);
			Sounds.explode = LoadContent<SoundEffect>("exploding1", loader);
			Sounds.destroy = LoadContent<SoundEffect>("exploding2", loader);
			Sounds.gunTargeting = LoadContent<SoundEffect>("targeting", loader);
			Sounds.gunShot = LoadContent<SoundEffect>("gunShot", loader);
			Sounds.boneSpawnLarge = LoadContent<SoundEffect>("spawn2", loader);
			Sounds.slam = LoadContent<SoundEffect>("slam", loader);
			Sounds.largeKnife = LoadContent<SoundEffect>("knife", loader);
			Sounds.boneSlabSpawn = LoadContent<SoundEffect>("boneslab_spawn", loader);
			Sounds.switchScene = LoadContent<SoundEffect>("switch", loader);
			Sounds.Warning = LoadContent<SoundEffect>("warning", loader);
			Sounds.giga = LoadContent<SoundEffect>("giga", loader);
			Sounds.ArrowStuck = LoadContent<SoundEffect>("arrowStuck", loader);
			Sounds.sparkles = LoadContent<SoundEffect>("sparkles", loader);
			Sounds.star0 = LoadContent<SoundEffect>("star0", loader);
			Sounds.star1 = LoadContent<SoundEffect>("star1", loader);
			Sounds.YellowShoot = LoadContent<SoundEffect>("shoot", loader);
			Sounds.TargetBurst = LoadContent<SoundEffect>("objBurst", loader);
			Sounds.Bomb = LoadContent<SoundEffect>("bomb", loader);
			#endregion
			#region Other
			Sprites.star = _spritesheet.LoadSprite("OtherBarrages\\star");
			Sprites.knife = _spritesheet.LoadSprite("OtherBarrages\\Knife\\Knife");
			Sprites.KnifeWarn = _spritesheet.LoadSprite("OtherBarrages\\Knife\\Warn");

			FightSprites.aimer = _spritesheet.LoadSprite("FightSprites\\aimer");
			FightSprites.dialogBox = _spritesheet.LoadSprite("FightSprites\\dialogBox");
			FightSprites.stopBar = _spritesheet.LoadSprite("FightSprites\\stop_bar");
			FightSprites.movingBar = _spritesheet.LoadSprite("FightSprites\\moving_bar");
			loader.RootDirectory = "Content";
			#endregion
		});
		task.RunSynchronously();
		await task;
	}
	/// <summary>
	/// A list of built-in fonts
	/// </summary>
	public static class Font
	{
		/// <summary>
		/// Mars Needs Cunnilingus
		/// </summary>
		public static GLFont FightFont => GlobalResources.Font.FightFont;
		/// <summary>
		/// Sans Undertale
		/// </summary>
		public static GLFont SansFont => GlobalResources.Font.SansFont;
		/// <summary>
		/// Hachicro
		/// </summary>
		public static GLFont DamageFont => GlobalResources.Font.DamageFont;
		/// <summary>
		/// Determination Mono
		/// </summary>
		public static GLFont NormalFont => GlobalResources.Font.NormalFont;
		/// <summary>
		/// ta_pop_M
		/// </summary>
		public static GLFont Japanese => GlobalResources.Font.Japanese;
		/// <summary>
		/// FZXS 12
		/// </summary>
		public static GLFont Chinese => GlobalResources.Font.Chinese;
	}
	/// <summary>
	/// A list of built-in sprites
	/// </summary>
	public static class Sprites
	{
		/// <summary>
		/// The arrow sprite, the first dimension indicates the arrow color, the second dimension of the array indicates the  mode of the arrow (Normal, Yellow, Green, Purple), the third dimension indicates the damage level
		/// </summary>
		public static Texture2D[,,] arrow { get; internal set; } = new Texture2D[4, 4, 4];
		/// <summary>
		/// The sprite of the base of an arrow
		/// </summary>
		public static Texture2D[] arrow_base { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// The sprite of the foreground of an arrow
		/// </summary>
		public static Texture2D[] arrow_fore { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// The arrow sprite, the first dimension of the array indicates the color (Blue, Red, Green, Purple), the second dimension indicates the mode of the arrow (Normal, Yellow, Green, Purple), the third dimension indicates the shard type
		/// </summary>
		public static Texture2D[,,] arrowShards { get; } = new Texture2D[2, 4, 6];
		/// <summary>
		/// Sprites of the void arrows
		/// </summary>
		public static Texture2D[] voidarrow { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// Sprite of the Soul
		/// </summary>
		public static Texture2D player { get; internal set; }
		/// <summary>
		/// Sprite of the Graze of the soul
		/// </summary>
		public static Texture2D soulCollide { get; internal set; }
		/// <summary>
		/// Sprite of the heart broken in game over
		/// </summary>
		public static Texture2D brokenHeart { get; internal set; }
		/// <summary>
		/// Sprite of a star
		/// </summary>
		public static Texture2D star { get; internal set; }
		/// <summary>
		/// Sprite of a slash beam
		/// </summary>
		public static Texture2D knife { get; internal set; }
		/// <summary>
		/// Sprite of a knife beam warning
		/// </summary>
		public static Texture2D KnifeWarn { get; internal set; }
		/// <summary>
		/// Sprite of a fireball
		/// </summary>
		public static Texture2D fireball { get; internal set; }
		/// <summary>
		/// Left half of the heart
		/// </summary>
		public static Texture2D leftHeart { get; internal set; }
		/// <summary>
		/// Right half of the heart
		/// </summary>
		public static Texture2D rightHeart { get; internal set; }
		/// <summary>
		/// Sprite of a warning line
		/// </summary>
		public static Texture2D warningLine { get; internal set; }
		/// <summary>
		/// Sprite of a bone slab
		/// </summary>
		public static Texture2D boneSlab { get; internal set; }
		/// <summary>
		/// Sprites of heart pieces in game over
		/// </summary>
		public static Texture2D[] heartPieces { get; internal set; } = new Texture2D[5];

		/// <summary>
		/// One pixel
		/// </summary>
		public static Texture2D pixUnit { get; internal set; }
		/// <summary>
		/// Trail sprite for arrow speed up
		/// </summary>
		public static Texture2D fireParticle { get; internal set; }
		/// <summary>
		/// Gun bullet sprite
		/// </summary>
		public static Texture2D bullet { get; internal set; }
		/// <summary>
		/// Gun aiming target sprite
		/// </summary>
		public static Texture2D target { get; internal set; }
		/// <summary>
		/// Circle sprite
		/// </summary>
		public static Texture2D lightBall { get; internal set; }
		/// <summary>
		/// Sprite of pixel with bloom effect
		/// </summary>
		public static Texture2D lightLine { get; internal set; }
		/// <summary>
		/// Square sprite
		/// </summary>
		public static Texture2D square { get; internal set; }
		/// <summary>
		/// The sprite of the player's shield
		/// </summary>
		public static Texture2D shield { get; internal set; }
		/// <summary>
		/// The sprite to display on the shield when arrow collides
		/// </summary>
		public static Texture2D shinyShield { get; internal set; }
		/// <summary>
		/// Sprite background for the shield
		/// </summary>
		public static Texture2D ShieldCircle { get; internal set; }
		/// <summary>
		/// Sprite of a spear
		/// </summary>
		public static Texture2D spear { get; internal set; }
		/// <summary>
		/// Sprite of a bone spike
		/// </summary>
		public static Texture2D spike { get; internal set; }
		/// <summary>
		/// Sprite of a spider
		/// </summary>
		public static Texture2D spider { get; internal set; }
		/// <summary>
		/// Sprite of a broken box side (Unused)
		/// </summary>
		public static Texture2D boxPiece { get; internal set; }
		/// <summary>
		/// Sprite of croissant (Spider Dance)
		/// </summary>
		public static Texture2D Croissant { get; internal set; }
		/// <summary>
		/// Sprite of Green Soul Blaster hitting the shield
		/// </summary>
		public static Texture2D stuck1 { get; internal set; }
		/// <summary>
		/// Sprite of Green Soul Blaster hitting the shield
		/// </summary>
		public static Texture2D stuck2 { get; internal set; }
		/// <summary>
		/// Sprite of HP of UI
		/// </summary>
		public static Texture2D hpText { get; internal set; }
		/// <summary>
		/// Sprite of KR of UI
		/// </summary>
		public static Texture2D krText { get; internal set; }
		/// <summary>
		/// Sprites of bone end
		/// </summary>
		public static Texture2D boneHead { get; internal set; }
		/// <summary>
		/// Sprite of bone body
		/// </summary>
		public static Texture2D boneBody { get; internal set; }
		/// <summary>
		/// Sprites for platform
		/// </summary>
		public static Texture2D[] platform { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprites for platform sides
		/// </summary>
		public static Texture2D[] platformSide { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprites of GB beginning to fire
		/// </summary>
		public static Texture2D[] GBStart { get; internal set; } = new Texture2D[5];
		/// <summary>
		/// Sprites of GB during fire
		/// </summary>
		public static Texture2D[] GBShooting { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprite of GB beam
		/// </summary>
		public static Texture2D GBLaser { get; internal set; }
		/// <summary>
		/// Sprite of explosion (Eternal Spring Dream)
		/// </summary>
		public static Texture2D[] explodes { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// Sprite of exploding card (Eternal Spring Dream)
		/// </summary>
		public static Texture2D explodeTrigger { get; internal set; }
		/// <summary>
		/// Sprite of golden outline of arrow
		/// </summary>
		public static Texture2D goldenBrim { get; internal set; }
		/// <summary>
		/// Sprite of accuracy bar on the bottom
		/// </summary>
		internal static Texture2D accuracyBar { get; set; }
		/// <summary>
		/// Sprite of ALL PERFECT displayed in result
		/// </summary>
		internal static Texture2D allPerfectText { get; set; }
		/// <summary>
		/// Sprites of accuracy bars on the bottom
		/// </summary>
		internal static Texture2D[] accuracyPointers { get; set; } = new Texture2D[3];
		/// <summary>
		/// Sprite of yellow soul bullet
		/// </summary>
		public static Texture2D SoulShoot { get; set; }
		/// <summary>
		/// Sprite of a breakable Mettaton block
		/// </summary>
		public static Texture2D MettBlockA { get; set; }
		/// <summary>
		/// Sprite of a non-breakable Mettaton block
		/// </summary>
		public static Texture2D MettBlockB { get; set; }
		/// <summary>
		/// Sprite of a Mettaton with parasol barrage
		/// </summary>
		public static Texture2D[] ParasolMett { get; private set; } = new Texture2D[18];
		/// <summary>
		/// Sprite of a Mettaton '+' bomb
		/// </summary>
		public static Texture2D[] MettBomb { get; private set; } = new Texture2D[2];
		/// <summary>
		/// Sprite of the center part of the Mettaton bomb blast
		/// </summary>
		public static Texture2D[] MettBombCoreBlast { get; private set; } = new Texture2D[7];
		/// <summary>
		/// Sprite of the horizontal Mettaton blast
		/// </summary>
		public static Texture2D[] MettBombBlast { get; private set; } = new Texture2D[7];
		/// <summary>
		/// Sprite of Mettaton heart barrage
		/// </summary>
		public static Texture2D MettBullet { get; internal set; }
	}
	/// <summary>
	/// A list of built-in audio
	/// </summary>
	public static class Sounds
	{
		/// <summary>
		/// Used for large scale scene transition
		/// </summary>

		public static SoundEffect switchScene { get; internal set; }
		/// <summary>
		/// Used for creating large bones
		/// </summary>
		public static SoundEffect boneSpawnLarge { get; internal set; }
		/// <summary>
		/// Slamming SFX
		/// </summary>
		public static SoundEffect slam { get; internal set; }
		/// <summary>
		/// SFX of player attack
		/// </summary>
		public static SoundEffect playerSlice { get; internal set; }
		/// <summary>
		/// SFX of text typing
		/// </summary>
		public static SoundEffect printWord { get; internal set; }
		/// <summary>
		/// SFX of Sans talking
		/// </summary>
		public static SoundEffect sansWord { get; internal set; }
		/// <summary>
		/// SFX of warning
		/// </summary>
		public static SoundEffect Warning { get; internal set; }
		/// <summary>
		/// Ding~
		/// </summary>
		public static SoundEffect Ding { get; internal set; }
		/// <summary>
		/// SFX of new arrow collision
		/// </summary>
		public static SoundEffect ArrowStuck { get; internal set; }
		/// <summary>
		/// SFX of healing
		/// </summary>
		public static SoundEffect heal { get; internal set; }
		/// <summary>
		/// SFX of player taking damage
		/// </summary>
		public static SoundEffect playerHurt { get; internal set; }
		/// <summary>
		/// SFX of a spear appearing
		/// </summary>
		public static SoundEffect spearAppear { get; internal set; }
		/// <summary>
		/// SFX of a spear being shot
		/// </summary>
		public static SoundEffect spearShoot { get; internal set; }
		/// <summary>
		/// SFX of piercing, often used for creating bones
		/// </summary>
		public static SoundEffect pierce { get; internal set; }
		/// <summary>
		/// SFX of selecting menu choice
		/// </summary>
		public static SoundEffect select { get; internal set; }
		/// <summary>
		/// SFX of changing menu choice
		/// </summary>
		public static SoundEffect changeSelection { get; internal set; }
		/// <summary>
		/// SFX of Sans flickering the screen
		/// </summary>
		public static SoundEffect change { get; internal set; }
		/// <summary>
		/// SFX of an enemy being damaged
		/// </summary>
		public static SoundEffect damaged { get; internal set; }
		/// <summary>
		/// SFX of soul split in half
		/// </summary>
		public static SoundEffect die1 { get; internal set; }
		/// <summary>
		/// SFX of soul shattering
		/// </summary>
		public static SoundEffect die2 { get; internal set; }
		/// <summary>
		/// SFX of Gaster Blaster spawning
		/// </summary>
		public static SoundEffect GBSpawn { get; internal set; }
		/// <summary>
		/// SFX of Gaster Blaster firing
		/// </summary>
		public static SoundEffect GBShoot { get; internal set; }
		/// <summary>
		/// SFX of an explosion
		/// </summary>
		public static SoundEffect explode { get; internal set; }
		/// <summary>
		/// SFX of an item being destroyed
		/// </summary>
		public static SoundEffect destroy { get; internal set; }
		/// <summary>
		/// SFX of a gun targeting
		/// </summary>
		public static SoundEffect gunTargeting { get; internal set; }
		/// <summary>
		/// SFX of a gun being fired
		/// </summary>
		public static SoundEffect gunShot { get; internal set; }
		/// <summary>
		/// SFX of DT2 knife
		/// </summary>
		public static SoundEffect largeKnife { get; internal set; }
		/// <summary>
		/// SFX of a bone slab spawning/enemy encounter
		/// </summary>
		public static SoundEffect boneSlabSpawn { get; internal set; }
		/// <summary>
		/// SFX of DT2 yelling
		/// </summary>
		public static SoundEffect giga { get; internal set; }
		/// <summary>
		/// SFX of star appearing
		/// </summary>
		public static SoundEffect star0 { get; internal set; }
		/// <summary>
		/// SFX of star firing
		/// </summary>
		public static SoundEffect star1 { get; internal set; }
		/// <summary>
		/// SFX of a sparkle
		/// </summary>
		public static SoundEffect sparkles { get; internal set; }
		/// <summary>
		/// Yellow soul bullet shooting SFX
		/// </summary>
		public static SoundEffect YellowShoot { get; set; }
		/// <summary>
		/// SFX of a block destroyed by yellow bullet
		/// </summary>
		public static SoundEffect TargetBurst { get; set; }
		/// <summary>
		/// SFX of a yellow soul bomb exploding
		/// </summary>
		public static SoundEffect Bomb { get; internal set; }
	}
	/// <summary>
	/// A list of sprites used in fighting (Currently unused)
	/// </summary>
	internal static class FightSprites
	{
		public static Texture2D[] fight = new Texture2D[2];
		public static Texture2D[] act = new Texture2D[2];
		public static Texture2D[] item = new Texture2D[2];
		public static Texture2D[] mercy = new Texture2D[2];
		public static Texture2D aimer;
		public static Texture2D stopBar;
		public static Texture2D movingBar;

		public static Texture2D[] slides = new Texture2D[6];
		public static Texture2D dialogBox;

	}
	/// <summary>
	/// A list of built-in shaders
	/// </summary>
	public static class Shaders
	{
		/// <summary>
		/// Distorts the screen like a sine wave
		/// </summary>
		public static Shader Sinwave { get; internal set; }
		/// <summary>
		/// An shader for creating an aurora
		/// </summary>
		public static AuroraShader Aurora { get; internal set; }
		/// <summary>
		/// Creates arcade machine like lines on the screen
		/// </summary>
		public static NeonLineShader NeonLine { get; internal set; }
		/// <summary>
		/// A shader that multiplies the blending
		/// </summary>
		public static ColorBlendShader ColorBlend { get; internal set; }
		/// <summary>
		/// Creates an arcade machine like screen
		/// </summary>
		public static BallShapingShader Cos1Ball { get; internal set; }
		/// <summary>
		/// Radical blur
		/// </summary>
		public static StepSampleShader StepSample { get; internal set; }
		/// <summary>
		/// Scales the screen inwards
		/// </summary>
		public static ScaleShader Scale { get; internal set; }
		/// <summary>
		/// Creates a color scattering effect (RGB splitting)
		/// </summary>
		public static ScatterShader Scatter { get; internal set; }
		/// <summary>
		/// 3D camera effect
		/// </summary>
		public static CameraShader Camera { get; internal set; }
		/// <summary>
		/// A swirl effect (Does not distort the screen), also used for creating noise
		/// </summary>
		public static SwirlShader Swirl { get; internal set; }
		/// <summary>
		/// Gaussian Blur shader
		/// </summary>
		public static BlurShader Blur { get; internal set; }
		/// <summary>
		/// Kawase blur sahder, more efficient
		/// </summary>
		public static BlurKawaseShader BlurKawase { get; internal set; }
		/// <summary>
		/// Distorts the screen (It is difficult to explain)
		/// </summary>
		public static PolarShader Polar { get; internal set; }
		/// <summary>
		/// Gray scales the screen
		/// </summary>
		public static GrayShader Gray { get; internal set; }
		/// <summary>
		/// Creates a ripple effect with minor scaling, do not confuse with <see cref="RadialWave"/>
		/// </summary>
		public static SeismicShader Seismic { get; internal set; }
		/// <summary>
		/// Pixelates the screen
		/// </summary>
		public static MosaicShader Mosaic { get; internal set; }
		/// <summary>
		/// Scattering light shader
		/// </summary>
		public static TyndallShader Tyndall { get; internal set; }
		/// <summary>
		/// That one shader in TAS right before the glowing line
		/// </summary>
		public static SpiralShader Spiral { get; internal set; }
		/// <summary>
		/// Glitch distortion shader (Sinusoidal intensity)
		/// </summary>
		public static WrongShader Wrong { get; internal set; }
		/// <summary>
		/// Creates a fire effect on the bottom of the screen
		/// </summary>
		public static FireShader Fire { get; internal set; }
		/// <summary>
		/// Huge light beam
		/// </summary>
		public static LightSweepShader LightSweep { get; internal set; }
		/// <summary>
		/// This shader is broken
		/// </summary>
		public static DislocationShaderX DislocationX { get; internal set; }
		/// <summary>
		/// Dislocates the screen by creating displacements and wave effect
		/// </summary>
		public static WaveShader Wave { get; internal set; }
		/// <summary>
		/// Creates a ripple effect, do not confuse with <see cref="Seismic"/>
		/// </summary>
		public static RadialWaveShader RadialWave { get; internal set; }
	}
}
/// <summary>
/// Utilities for resource loading
/// </summary>
public static class Resource
{
	private static readonly Dictionary<string, Texture2D> _texData = [];
	private static Texture2D ToTexture2D(Texture2DRegion textureRegion)
	{
		if (!_texData.TryGetValue(textureRegion.Name, out Texture2D value))
		{
			GraphicsDevice graphicsDevice = RenderProduction.WindowDevice;
			//Get the source texture and its bounds from the Texture2DRegion
			Texture2D sourceTexture = textureRegion.Texture;
			Rectangle sourceRectangle = textureRegion.Bounds;
			//Create a new Texture2D with the dimensions of the region
			Texture2D newTexture = new(graphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
			// Create a color array to hold the pixel data of the region
			Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];
			//Get the pixel data from the source texture within the specified region
			sourceTexture.GetData(0, sourceRectangle, data, 0, data.Length);
			//Set the pixel data to the new texture
			newTexture.SetData(data);
			_texData.Add(textureRegion.Name, newTexture);
			return newTexture;
		}
		else
			return value;
	}
	/// <summary>
	/// Converts a <see cref="Sprite"/> into <see cref="Texture2D"/> for common support
	/// </summary>
	/// <param name="sprite">The sprite to convert</param>
	/// <returns>The texture after conversion</returns>
	public static Texture2D ToTexture2D(this Sprite sprite) => ToTexture2D(sprite.TextureRegion);
	/// <summary>
	/// Loads a sprite from a spritesheet
	/// </summary>
	/// <param name="sheet">The sheet to load from</param>
	/// <param name="key">The key in the json file</param>
	/// <returns>The texture</returns>
	public static Texture2D LoadSprite(this Texture2DAtlas sheet, string key) => sheet.CreateSprite(key.Replace('\\', '/')).ToTexture2D();
}