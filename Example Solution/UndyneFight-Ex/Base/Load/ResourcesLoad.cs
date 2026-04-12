using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.GameInterface;
using System.Reflection;
using static UndyneFight_Ex.GlobalResources.Effects;
using static UndyneFight_Ex.GlobalResources.Sprites;

namespace UndyneFight_Ex;

/// <summary>
/// The attribute to load custom resources
/// </summary>
/// <param name="path">The path of the resource relative to ./Content/</param>
/// <param name="type">The type of the resource (i.e. Texture2D, Effect)</param>
/// <param name="dim">The dimensions of each resource if it is a multi-dimensional array</param>
/// <param name="replacement">The string to replace for the index if it is not, each separated by "|"</param>
public class UFEXResourceAttribute(string path, Type type, int[] dim = null, string[] replacement = null) : Attribute
{
	/// <summary>
	/// The path to the resource
	/// </summary>
	public string Path { get; init; } = path;
	/// <summary>
	/// The type of the resource
	/// </summary>
	public Type Type { get; init; } = type;
	/// <summary>
	/// Whether the resource is loaded
	/// </summary>
	public bool Loaded { get; set; } = false;
	/// <summary>
	/// The amount of sprites in each dimension
	/// </summary>
	public int[] Dimensions = dim ?? [1];
	/// <summary>
	/// The string to replace the index in the path, i.e. ["a|b", "A|B"] will replace &lt;1&gt; with a and b, and replace &lt;2&gt; with A and B
	/// </summary>
	public string[] IndexReplacement = replacement ?? [""];
}
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
			bool hasTitle;
			if (hasTitle = File.Exists(path + ".xnb"))
				loadingTexture = LoadContent<Texture2D>(root, loader);
			PropertyInfo loadingTexProp = typeof(Sprites).GetProperty("loadingTexture");
			(Attribute.GetCustomAttribute(loadingTexProp, typeof(UFEXResourceAttribute)) as UFEXResourceAttribute).Loaded = true;
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Fonts;
		task = new(() =>
		{
			//Load Fonts
			int ResourcesCount = typeof(Font).GetProperties().Length;
			for (int i = 0; i < ResourcesCount; i++)
			{
				PropertyInfo property = typeof(Font).GetProperties()[i];
				if (Attribute.GetCustomAttribute(property, typeof(UFEXResourceAttribute)) is UFEXResourceAttribute attribute && !attribute.Loaded)
				{
					property.SetValue(typeof(Font), new GLFont(attribute.Path, loader));
					attribute.Loaded = true;
				}
			}
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Global_Sprites;
		task = new(() =>
		{
			//Load Sprites
			int ResourcesCount = typeof(Sprites).GetProperties().Length;
			for (int i = 0; i < ResourcesCount; i++)
			{
				PropertyInfo property = typeof(Sprites).GetProperties()[i];
				if (Attribute.GetCustomAttribute(property, typeof(UFEXResourceAttribute)) is UFEXResourceAttribute attribute && !attribute.Loaded && File.Exists(Path.Combine($"{AppContext.BaseDirectory}Content\\{attribute.Path}.xnb".Split('\\'))))
				{
					property.SetValue(typeof(Sprites), LoadContent<Texture2D>(attribute.Path, loader));
					attribute.Loaded = true;
				}
			}
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Global_Shaders;
		task = new(() =>
		{
			//Load shaders
			backGroundShader = new Shader(LoadContent<Effect>("Global\\Shaders\\BackGroundShader", loader));
			reduceBlueShader = new Shader(LoadContent<Effect>("Global\\Shaders\\reduceBlue", loader))
			{
				StableEvents = (s) => s.Parameters["reduceBlueAmount"].SetValue(Settings.SettingsManager.DataLibrary.reduceBlueAmount / 200f)
			};
			//Load Effects
			int ResourcesCount = typeof(FightResources.Shaders).GetProperties().Length;
			for (int i = 0; i < ResourcesCount; i++)
			{
				PropertyInfo property = typeof(FightResources.Shaders).GetProperties()[i];
				if (Attribute.GetCustomAttribute(property, typeof(UFEXResourceAttribute)) is UFEXResourceAttribute attribute && !attribute.Loaded)
				{
					Effect shd = LoadContent<Effect>(attribute.Path, loader);
					object instance = Activator.CreateInstance(property.PropertyType, shd);
					property.SetValue(typeof(FightResources.Shaders), instance);
					attribute.Loaded = true;
				}
			}
			LoadInternals(loader);
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Fight_Sprites;
	}

	public static partial class Effects
	{
		[UFEXResource("Global\\Shaders\\BackGroundShader", typeof(Effect))]
		internal static Shader backGroundShader;
		[UFEXResource("Global\\Shaders\\reduceBlue", typeof(Effect))]
		internal static Shader reduceBlueShader;
	}
	/// <summary>
	/// A list of built-in fonts
	/// </summary>
	public static class Font
	{
		/// <summary>
		/// Mars Needs Cunnilingus
		/// </summary>
		[UFEXResource("Sprites\\font\\menu", typeof(GLFont))]
		public static GLFont FightFont { get; internal set; }
		/// <summary>
		/// Determination Mono
		/// </summary>
		[UFEXResource("Sprites\\font\\normal", typeof(GLFont))]
		public static GLFont NormalFont { get; internal set; }
		/// <summary>
		/// Sans Undertale
		/// </summary>
		[UFEXResource("Sprites\\font\\sans", typeof(GLFont))]
		public static GLFont SansFont { get; internal set; }
		/// <summary>
		/// Hachicro
		/// </summary>
		[UFEXResource("Sprites\\font\\DamageShow", typeof(GLFont))]
		public static GLFont DamageFont { get; internal set; }
		/// <summary>
		/// ta_pop_M
		/// </summary>
		[UFEXResource("Sprites\\font\\Japanese", typeof(GLFont))]
		public static GLFont Japanese { get; internal set; }
		/// <summary>
		/// Crypt of Tomorrow
		/// </summary>
		[UFEXResource("Sprites\\font\\UIFont", typeof(GLFont))]
		public static GLFont UIFont { get; internal set; }
		/// <summary>
		/// FZXS 12
		/// </summary>
		[UFEXResource("Sprites\\font\\Chinese", typeof(GLFont))]
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
		[UFEXResource("Global\\UI\\PlaceCheck", typeof(Texture2D))]
		public static Texture2D cursor { get; internal set; }
		/// <summary>
		/// Legacy LOGIN sprite
		/// </summary>
		[UFEXResource("Global\\\\UI\\login", typeof(Texture2D))]
		public static Texture2D login { get; internal set; }
		/// <summary>
		/// Legacy CHAMPIONSHIP sprite
		/// </summary>
		[UFEXResource("Global\\UI\\cup_highres", typeof(Texture2D))]
		public static Texture2D championShip { get; internal set; }
		/// <summary>
		/// Hash texture
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\hashtex", typeof(Texture2D))]
		public static Texture2D hashtex { get; internal set; }
		/// <summary>
		/// Hash texture 2
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\hashtex2", typeof(Texture2D))]
		public static Texture2D hashtex2 { get; internal set; }
		/// <summary>
		/// Legacy MAIN GAME sprite
		/// </summary>
		[UFEXResource("Global\\UI\\maingame", typeof(Texture2D))]
		public static Texture2D mainGame { get; internal set; }
		/// <summary>
		/// Legacy OPTIONS sprite
		/// </summary>
		[UFEXResource("Global\\UI\\options", typeof(Texture2D))]
		public static Texture2D options { get; internal set; }
		/// <summary>
		/// Legacy ACHIVEMENTS sprite
		/// </summary>
		[UFEXResource("Global\\UI\\stars", typeof(Texture2D))]
		public static Texture2D achievements { get; internal set; }
		/// <summary>
		/// Legacy RECORD sprite
		/// </summary>
		[UFEXResource("Global\\UI\\record", typeof(Texture2D))]
		public static Texture2D record { get; internal set; }
		/// <summary>
		/// Debug vector arrow sprite
		/// </summary>
		[UFEXResource("Global\\UI\\debugArrow", typeof(Texture2D))]
		public static Texture2D debugArrow { get; internal set; }
		/// <summary>
		/// Loading text
		/// </summary>
		[UFEXResource("Global\\Loading\\Loading", typeof(Texture2D))]
		public static Texture2D loadingText { get; internal set; }
		/// <summary>
		/// Loading arrow
		/// </summary>
		[UFEXResource("Global\\Loading\\ProgressArrow", typeof(Texture2D))]
		public static Texture2D progressArrow { get; internal set; }
		/// <summary>
		/// Blue star medal
		/// </summary>
		[UFEXResource("Global\\UI\\medal1", typeof(Texture2D))]
		public static Texture2D medal { get; internal set; }
		/// <summary>
		/// Purple star medal
		/// </summary>
		[UFEXResource("Global\\UI\\medal2", typeof(Texture2D))]
		public static Texture2D starMedal { get; internal set; }
		/// <summary>
		/// Empty medal
		/// </summary>
		[UFEXResource("Global\\UI\\medal0", typeof(Texture2D))]
		public static Texture2D brimMedal { get; internal set; }
		/// <summary>
		/// Root texture
		/// </summary>
		[UFEXResource("Global\\UI\\root", typeof(Texture2D))] //Proxy
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
		//Load sprites
		Task task = new(()=>
		{
			_spritesheet = loader.Load<Texture2DAtlas>("UndyneFight_Ex Spritesheet");
			//Early load
			Sprites.pixUnit = _spritesheet.LoadSprite("others/pixiv");
			//Load Sprites
			int ResourcesCount = typeof(Sprites).GetProperties().Length;
			for (int i = 0; i < ResourcesCount; i++)
			{
				PropertyInfo property = typeof(Sprites).GetProperties()[i];
				if (Attribute.GetCustomAttribute(property, typeof(UFEXResourceAttribute)) is UFEXResourceAttribute attribute && !attribute.Loaded)
				{
					int SpriteArrayRank = attribute.Dimensions.Length;
					int[] SpriteSubIndex = new int[SpriteArrayRank]; //Max 3 layers
					for (int j = 0; j < SpriteArrayRank; j++)
						SpriteSubIndex[j] = 0;
					string curPath = attribute.Path;
					string[][] replacementStrings = new string[SpriteArrayRank][];
					for (int j = 0; j < SpriteArrayRank; j++)
						replacementStrings[j] = attribute.IndexReplacement[j].Split('|');
					for (int j = 0; j < SpriteArrayRank; j++)
						if (replacementStrings[j][0] != "")
							curPath = curPath.Replace(attribute.IndexReplacement[j] != "" ? replacementStrings[j][0] : $"<{j + 1}>", SpriteSubIndex[j].ToString());
					//Get the total count of sprites to load for this resource
					int totalLoopCount = 1;
					for (int j = 0; j < SpriteArrayRank; j++)
						totalLoopCount *= attribute.Dimensions[j];
					for (int j = 0; j < totalLoopCount; j++)
					{
						curPath = attribute.Path;
						for (int k = 0; k < SpriteArrayRank; k++)
							curPath = curPath.Replace($"<{k + 1}>", attribute.IndexReplacement[k] != "" ? replacementStrings[k][SpriteSubIndex[k]] : SpriteSubIndex[k].ToString());
						if (property.GetValue(typeof(Sprite)) is Array arrayProperty)
						{
							arrayProperty.SetValue(_spritesheet.LoadSprite(curPath), SpriteSubIndex);
							//A function to update the index of the array to process, looping the current index to 0 and increasing the index of the higher dimension if needed
							static void UpdateIndex(ref int[] indexes, ref int[] limit, int index)
							{
								if (indexes[index] >= limit[index])
								{
									indexes[index] = 0;
									if (index > 0)
									{
										indexes[index - 1]++;
										UpdateIndex(ref indexes, ref limit, index - 1);
									}
								}
							}
							//Update index values
							SpriteSubIndex[^1]++;
							//Check for loop back
							UpdateIndex(ref SpriteSubIndex, ref attribute.Dimensions, SpriteArrayRank - 1);
						}
						else
							property.SetValue(typeof(Sprite), _spritesheet.LoadSprite(curPath));
					}
					attribute.Loaded = true;
				}
			}
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Fight_Audio;
		//Load audio
		task = new(() =>
		{
			int ResourcesCount = typeof(Sounds).GetProperties().Length;
			for (int i = 0; i < ResourcesCount; i++)
			{
				PropertyInfo property = typeof(Sounds).GetProperties()[i];
				if (Attribute.GetCustomAttribute(property, typeof(UFEXResourceAttribute)) is UFEXResourceAttribute attribute && !attribute.Loaded)
				{
					property.SetValue(typeof(Sounds), LoadContent<SoundEffect>(attribute.Path, loader));
					attribute.Loaded = true;
				}
			}
			#region Other
			// FightSprites.aimer = _spritesheet.LoadSprite("FightSprites\\aimer");
			//FightSprites.dialogBox = _spritesheet.LoadSprite("FightSprites\\dialogBox");
			//FightSprites.stopBar = _spritesheet.LoadSprite("FightSprites\\stop_bar");
			//FightSprites.movingBar = _spritesheet.LoadSprite("FightSprites\\moving_bar");
			#endregion
		});
		task.RunSynchronously();
		await task;
		ResourcesLoadingScene.LoadState = ResourcesLoadingScene.ResourcesLoadState.Finished;
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
		[UFEXResource($"bullet\\<2><1><3>", typeof(Texture2D), [4, 4, 4], ["blue|red|green|purple", "|circle_|rot_|tran_", ""])]
		public static Texture2D[,,] arrow { get; internal set; } = new Texture2D[4, 4, 4];
		/// <summary>
		/// The sprite of the base of an arrow
		/// </summary>
		[UFEXResource($"bullet\\arrow_base<1>", typeof(Texture2D), [4])]
		public static Texture2D[] arrow_base { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// The sprite of the foreground of an arrow
		/// </summary>
		[UFEXResource($"bullet\\arrow_fore<1>", typeof(Texture2D), [4])]
		public static Texture2D[] arrow_fore { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// The arrow sprite, the first dimension of the array indicates the color (Blue, Red, Green, Purple), the second dimension indicates the mode of the arrow (Normal, Yellow, Green, Purple), the third dimension indicates the shard type
		/// </summary>
		[UFEXResource($"bullet\\Shards\\<2>\\0<1>-<3>", typeof(Texture2D), [2, 4, 6], ["", "white|yellow|green|purple", ""])]
		public static Texture2D[,,] arrowShards { get; } = new Texture2D[2, 4, 6];
		/// <summary>
		/// Sprites of the void arrows
		/// </summary>
		[UFEXResource($"bullet\\voidarrow\\<1>0", typeof(Texture2D), [4], ["blue|red|green|purple"])]
		public static Texture2D[] voidarrow { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// Sprite of the Soul
		/// </summary>
		[UFEXResource("SOUL\\original", typeof(Texture2D))]
		public static Texture2D player { get; internal set; }
		/// <summary>
		/// Sprite of the Graze of the soul
		/// </summary>
		[UFEXResource("SOUL\\collide", typeof(Texture2D))]
		public static Texture2D soulCollide { get; internal set; }
		/// <summary>
		/// Sprite of the heart broken in game over
		/// </summary>
		[UFEXResource("SOUL\\break", typeof(Texture2D))]
		public static Texture2D brokenHeart { get; internal set; }
		/// <summary>
		/// Sprite of a star
		/// </summary>
		[UFEXResource("OtherBarrages\\star", typeof(Texture2D))]
		public static Texture2D star { get; internal set; }
		/// <summary>
		/// Sprite of a slash beam
		/// </summary>
		[UFEXResource("OtherBarrages\\Knife\\Knife", typeof(Texture2D))]
		public static Texture2D knife { get; internal set; }
		/// <summary>
		/// Sprite of a knife beam warning
		/// </summary>
		[UFEXResource("OtherBarrages\\Knife\\Warn", typeof(Texture2D))]
		public static Texture2D KnifeWarn { get; internal set; }
		/// <summary>
		/// Sprite of a fireball
		/// </summary>
		[UFEXResource("OtherBarrages\\fireball", typeof(Texture2D))]
		public static Texture2D fireball { get; internal set; }
		/// <summary>
		/// Left half of the heart
		/// </summary>
		[UFEXResource("SOUL\\leftSoul", typeof(Texture2D))]
		public static Texture2D leftHeart { get; internal set; }
		/// <summary>
		/// Right half of the heart
		/// </summary>
		[UFEXResource("SOUL\\rightSoul", typeof(Texture2D))]
		public static Texture2D rightHeart { get; internal set; }
		/// <summary>
		/// Sprite of a warning line
		/// </summary>
		[UFEXResource("Bone\\warning_line", typeof(Texture2D))]
		public static Texture2D warningLine { get; internal set; }
		/// <summary>
		/// Sprite of a bone slab
		/// </summary>
		[UFEXResource("Bone\\bone_slab", typeof(Texture2D))]
		public static Texture2D boneSlab { get; internal set; }
		/// <summary>
		/// Sprites of heart pieces in game over
		/// </summary>
		[UFEXResource("SOUL\\shard<1>", typeof(Texture2D), [5])]
		public static Texture2D[] heartPieces { get; internal set; } = new Texture2D[5];
		/// <summary>
		/// One pixel
		/// </summary>
		[UFEXResource("others\\pixiv", typeof(Texture2D))]
		public static Texture2D pixUnit { get; internal set; }
		/// <summary>
		/// Trail sprite for arrow speed up
		/// </summary>
		[UFEXResource("others\\fireParticle", typeof(Texture2D))]
		public static Texture2D fireParticle { get; internal set; }
		/// <summary>
		/// Gun bullet sprite
		/// </summary>
		[UFEXResource("bullet\\gunBullet", typeof(Texture2D))]
		public static Texture2D bullet { get; internal set; }
		/// <summary>
		/// Gun aiming target sprite
		/// </summary>
		[UFEXResource("bullet\\target", typeof(Texture2D))]
		public static Texture2D target { get; internal set; }
		/// <summary>
		/// Circle sprite
		/// </summary>
		[UFEXResource("others\\lightBall", typeof(Texture2D))]
		public static Texture2D lightBall { get; internal set; }
		/// <summary>
		/// Sprite of pixel with bloom effect
		/// </summary>
		[UFEXResource("others\\lightLine", typeof(Texture2D))]
		public static Texture2D lightLine { get; internal set; }
		/// <summary>
		/// Square sprite
		/// </summary>
		[UFEXResource("others\\square", typeof(Texture2D))]
		public static Texture2D square { get; internal set; }
		/// <summary>
		/// The sprite of the player's shield
		/// </summary>
		[UFEXResource("SOUL\\shield", typeof(Texture2D))]
		public static Texture2D shield { get; internal set; }
		/// <summary>
		/// The sprite to display on the shield when arrow collides
		/// </summary>
		[UFEXResource("SOUL\\shield_shiny", typeof(Texture2D))]
		public static Texture2D shinyShield { get; internal set; }
		/// <summary>
		/// Sprite background for the shield
		/// </summary>
		[UFEXResource("SOUL\\circle", typeof(Texture2D))]
		public static Texture2D ShieldCircle { get; internal set; }
		/// <summary>
		/// Sprite of a spear
		/// </summary>
		[UFEXResource("bullet\\spear", typeof(Texture2D))]
		public static Texture2D spear { get; internal set; }
		/// <summary>
		/// Sprite of a bone spike
		/// </summary>
		[UFEXResource("Bone\\bone_spike", typeof(Texture2D))]
		public static Texture2D spike { get; internal set; }
		/// <summary>
		/// Sprite of a spider
		/// </summary>
		[UFEXResource("OtherBarrages\\spider", typeof(Texture2D))]
		public static Texture2D spider { get; internal set; }
		/// <summary>
		/// Sprite of a broken box side (Unused)
		/// </summary>
		[UFEXResource("others\\boxPiece", typeof(Texture2D))]
		public static Texture2D boxPiece { get; internal set; }
		/// <summary>
		/// Sprite of croissant (Spider Dance)
		/// </summary>
		[UFEXResource("OtherBarrages\\clo", typeof(Texture2D))]
		public static Texture2D Croissant { get; internal set; }
		/// <summary>
		/// Sprite of Green Soul Blaster hitting the shield
		/// </summary>
		[UFEXResource("others\\GBStuck1", typeof(Texture2D))]
		public static Texture2D stuck1 { get; internal set; }
		/// <summary>
		/// Sprite of Green Soul Blaster hitting the shield
		/// </summary>
		[UFEXResource("others\\GBStuck2", typeof(Texture2D))]
		public static Texture2D stuck2 { get; internal set; }
		/// <summary>
		/// Sprite of HP of UI
		/// </summary>
		[UFEXResource("hp_show\\hp", typeof(Texture2D))]
		public static Texture2D hpText { get; internal set; }
		/// <summary>
		/// Sprite of KR of UI
		/// </summary>
		[UFEXResource("hp_show\\kr", typeof(Texture2D))]
		public static Texture2D krText { get; internal set; }
		/// <summary>
		/// Sprites of bone end
		/// </summary>
		[UFEXResource("Bone\\bone_up", typeof(Texture2D))]
		public static Texture2D boneHead { get; internal set; }
		/// <summary>
		/// Sprite of bone body
		/// </summary>
		[UFEXResource("Bone\\bone_body", typeof(Texture2D))]
		public static Texture2D boneBody { get; internal set; }
		/// <summary>
		/// Sprites for platform
		/// </summary>
		[UFEXResource("Platform\\platform_body<1>", typeof(Texture2D), [2])]
		public static Texture2D[] platform { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprites for platform sides
		/// </summary>
		[UFEXResource("Platform\\platform_side<1>", typeof(Texture2D), [2])]
		public static Texture2D[] platformSide { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprites of GB beginning to fire
		/// </summary>
		[UFEXResource("GB\\s\\frame_<1>", typeof(Texture2D), [5])]
		public static Texture2D[] GBStart { get; internal set; } = new Texture2D[5];
		/// <summary>
		/// Sprites of GB during fire
		/// </summary>
		[UFEXResource("GB\\p\\frame_<1>", typeof(Texture2D), [2])]
		public static Texture2D[] GBShooting { get; internal set; } = new Texture2D[2];
		/// <summary>
		/// Sprite of GB beam
		/// </summary>
		[UFEXResource("GB\\laser", typeof(Texture2D))]
		public static Texture2D GBLaser { get; internal set; }
		/// <summary>
		/// Sprite of explosion (Eternal Spring Dream)
		/// </summary>
		[UFEXResource("Explodes\\smallExplode<1>", typeof(Texture2D), [4])]
		public static Texture2D[] explodes { get; internal set; } = new Texture2D[4];
		/// <summary>
		/// Sprite of exploding card (Eternal Spring Dream)
		/// </summary>
		[UFEXResource("Explodes\\explodeTrigger", typeof(Texture2D))]
		public static Texture2D explodeTrigger { get; internal set; }
		/// <summary>
		/// Sprite of golden outline of arrow
		/// </summary>
		[UFEXResource("bullet\\golden_tip", typeof(Texture2D))]
		public static Texture2D goldenBrim { get; internal set; }
		/// <summary>
		/// Sprite of accuracy bar on the bottom
		/// </summary>
		[UFEXResource("Pointer/accuracyBar", typeof(Texture2D))]
		public static Texture2D accuracyBar { internal get; set; }
		/// <summary>
		/// Sprite of ALL PERFECT displayed in result
		/// </summary>
		[UFEXResource("others\\allPerfect", typeof(Texture2D))]
		public static Texture2D allPerfectText { internal get; set; }
		/// <summary>
		/// Sprites of accuracy bars on the bottom
		/// </summary>
		[UFEXResource("Pointer/accuracyPointer<1>", typeof(Texture2D), [3], ["L|M|R"])]
		public static Texture2D[] accuracyPointers { internal get; set; } = new Texture2D[3];
		/// <summary>
		/// Sprite of yellow soul bullet
		/// </summary>
		[UFEXResource("SOUL\\soulBullet", typeof(Texture2D))]
		public static Texture2D SoulShoot { get; set; }
		/// <summary>
		/// Sprite of a breakable Mettaton block
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\blockA", typeof(Texture2D))]
		public static Texture2D MettBlockA { get; set; }
		/// <summary>
		/// Sprite of a non-breakable Mettaton block
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\blockB", typeof(Texture2D))]
		public static Texture2D MettBlockB { get; set; }
		/// <summary>
		/// Sprite of a Mettaton with parasol barrage
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\spr_parasolmett_<1>", typeof(Texture2D), [18])]
		public static Texture2D[] ParasolMett { get; private set; } = new Texture2D[18];
		/// <summary>
		/// Sprite of a Mettaton '+' bomb
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\spr_plusbomb_<1>", typeof(Texture2D), [2])]
		public static Texture2D[] MettBomb { get; private set; } = new Texture2D[2];
		/// <summary>
		/// Sprite of the center part of the Mettaton bomb blast
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\spr_plusbomb_coreblast_<1>", typeof(Texture2D), [7])]
		public static Texture2D[] MettBombCoreBlast { get; private set; } = new Texture2D[7];
		/// <summary>
		/// Sprite of the horizontal Mettaton blast
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\spr_plusbomb_blast_<1>", typeof(Texture2D), [7])]
		public static Texture2D[] MettBombBlast { get; private set; } = new Texture2D[7];
		/// <summary>
		/// Sprite of Mettaton heart barrage
		/// </summary>
		[UFEXResource("OtherBarrages\\Mettaton\\bullet", typeof(Texture2D))]
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
		[UFEXResource("Sounds\\switch", typeof(SoundEffect))]
		public static SoundEffect switchScene { get; internal set; }
		/// <summary>
		/// Used for creating large bones
		/// </summary>
		[UFEXResource("Sounds\\spawn2", typeof(SoundEffect))]
		public static SoundEffect boneSpawnLarge { get; internal set; }
		/// <summary>
		/// Slamming SFX
		/// </summary>
		[UFEXResource("Sounds\\slam", typeof(SoundEffect))]
		public static SoundEffect slam { get; internal set; }
		/// <summary>
		/// SFX of player attack
		/// </summary>
		[UFEXResource("Sounds\\slice", typeof(SoundEffect))]
		public static SoundEffect playerSlice { get; internal set; }
		/// <summary>
		/// SFX of text typing
		/// </summary>
		[UFEXResource("Sounds\\word_sound", typeof(SoundEffect))]
		public static SoundEffect printWord { get; internal set; }
		/// <summary>
		/// SFX of Sans talking
		/// </summary>
		[UFEXResource("Sounds\\sans_sound", typeof(SoundEffect))]
		public static SoundEffect sansWord { get; internal set; }
		/// <summary>
		/// SFX of warning
		/// </summary>
		[UFEXResource("Sounds\\warning", typeof(SoundEffect))]
		public static SoundEffect Warning { get; internal set; }
		/// <summary>
		/// Ding~
		/// </summary>
		[UFEXResource("Sounds\\hit", typeof(SoundEffect))]
		public static SoundEffect Ding { get; internal set; }
		/// <summary>
		/// SFX of new arrow collision
		/// </summary>
		[UFEXResource("Sounds\\arrowStuck", typeof(SoundEffect))]
		public static SoundEffect ArrowStuck { get; internal set; }
		/// <summary>
		/// SFX of healing
		/// </summary>
		[UFEXResource("Sounds\\heal", typeof(SoundEffect))]
		public static SoundEffect heal { get; internal set; }
		/// <summary>
		/// SFX of player taking damage
		/// </summary>
		[UFEXResource("Sounds\\hurt", typeof(SoundEffect))]
		public static SoundEffect playerHurt { get; internal set; }
		/// <summary>
		/// SFX of a spear appearing
		/// </summary>
		[UFEXResource("Sounds\\spawn", typeof(SoundEffect))]
		public static SoundEffect spearAppear { get; internal set; }
		/// <summary>
		/// SFX of a spear being shot
		/// </summary>
		[UFEXResource("Sounds\\toss", typeof(SoundEffect))]
		public static SoundEffect spearShoot { get; internal set; }
		/// <summary>
		/// SFX of piercing, often used for creating bones
		/// </summary>
		[UFEXResource("Sounds\\pierce", typeof(SoundEffect))]
		public static SoundEffect pierce { get; internal set; }
		/// <summary>
		/// SFX of selecting menu choice
		/// </summary>
		[UFEXResource("Sounds\\choose_2", typeof(SoundEffect))]
		public static SoundEffect select { get; internal set; }
		/// <summary>
		/// SFX of changing menu choice
		/// </summary>
		[UFEXResource("Sounds\\choose_1", typeof(SoundEffect))]
		public static SoundEffect changeSelection { get; internal set; }
		/// <summary>
		/// SFX of Sans flickering the screen
		/// </summary>
		[UFEXResource("Sounds\\change", typeof(SoundEffect))]
		public static SoundEffect change { get; internal set; }
		/// <summary>
		/// SFX of an enemy being damaged
		/// </summary>
		[UFEXResource("Sounds\\damaged", typeof(SoundEffect))]
		public static SoundEffect damaged { get; internal set; }
		/// <summary>
		/// SFX of soul split in half
		/// </summary>
		[UFEXResource("Sounds\\die_1", typeof(SoundEffect))]
		public static SoundEffect die1 { get; internal set; }
		/// <summary>
		/// SFX of soul shattering
		/// </summary>
		[UFEXResource("Sounds\\die_2", typeof(SoundEffect))]
		public static SoundEffect die2 { get; internal set; }
		/// <summary>
		/// SFX of Gaster Blaster spawning
		/// </summary>
		[UFEXResource("Sounds\\L_GB_summon", typeof(SoundEffect))]
		public static SoundEffect GBSpawn { get; internal set; }
		/// <summary>
		/// SFX of Gaster Blaster firing
		/// </summary>
		[UFEXResource("Sounds\\S_GB_shot", typeof(SoundEffect))]
		public static SoundEffect GBShoot { get; internal set; }
		/// <summary>
		/// SFX of an explosion
		/// </summary>
		[UFEXResource("Sounds\\exploding1", typeof(SoundEffect))]
		public static SoundEffect explode { get; internal set; }
		/// <summary>
		/// SFX of an item being destroyed
		/// </summary>
		[UFEXResource("Sounds\\exploding2", typeof(SoundEffect))]
		public static SoundEffect destroy { get; internal set; }
		/// <summary>
		/// SFX of a gun targeting
		/// </summary>
		[UFEXResource("Sounds\\targeting", typeof(SoundEffect))]
		public static SoundEffect gunTargeting { get; internal set; }
		/// <summary>
		/// SFX of a gun being fired
		/// </summary>
		[UFEXResource("Sounds\\gunShot", typeof(SoundEffect))]
		public static SoundEffect gunShot { get; internal set; }
		/// <summary>
		/// SFX of DT2 knife
		/// </summary>
		[UFEXResource("Sounds\\knife", typeof(SoundEffect))]
		public static SoundEffect largeKnife { get; internal set; }
		/// <summary>
		/// SFX of a bone slab spawning/enemy encounter
		/// </summary>
		[UFEXResource("Sounds\\boneslab_spawn", typeof(SoundEffect))]
		public static SoundEffect boneSlabSpawn { get; internal set; }
		/// <summary>
		/// SFX of DT2 yelling
		/// </summary>
		[UFEXResource("Sounds\\giga", typeof(SoundEffect))]
		public static SoundEffect giga { get; internal set; }
		/// <summary>
		/// SFX of star appearing
		/// </summary>
		[UFEXResource("Sounds\\star0", typeof(SoundEffect))]
		public static SoundEffect star0 { get; internal set; }
		/// <summary>
		/// SFX of star firing
		/// </summary>
		[UFEXResource("Sounds\\star1", typeof(SoundEffect))]
		public static SoundEffect star1 { get; internal set; }
		/// <summary>
		/// SFX of a sparkle
		/// </summary>
		[UFEXResource("Sounds\\sparkles", typeof(SoundEffect))]
		public static SoundEffect sparkles { get; internal set; }
		/// <summary>
		/// Yellow soul bullet shooting SFX
		/// </summary>
		[UFEXResource("Sounds\\shoot", typeof(SoundEffect))]
		public static SoundEffect YellowShoot { get; set; }
		/// <summary>
		/// SFX of a block destroyed by yellow bullet
		/// </summary>
		[UFEXResource("Sounds\\objBurst", typeof(SoundEffect))]
		public static SoundEffect TargetBurst { get; set; }
		/// <summary>
		/// SFX of a yellow soul bomb exploding
		/// </summary>
		[UFEXResource("Sounds\\bomb", typeof(SoundEffect))]
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
		[UFEXResource("Global\\Shaders\\Effect Library\\Sinwave", typeof(Effect))]
		public static Shader Sinwave { get; internal set; }
		/// <summary>
		/// An shader for creating an aurora
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Aurora", typeof(Effect))]
		public static AuroraShader Aurora { get; internal set; }
		/// <summary>
		/// Creates arcade machine like lines on the screen
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\NeonLine", typeof(Effect))]
		public static NeonLineShader NeonLine { get; internal set; }
		/// <summary>
		/// A shader that multiplies the blending
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\ColorBlend", typeof(Effect))]
		public static ColorBlendShader ColorBlend { get; internal set; }
		/// <summary>
		/// Creates an arcade machine like screen
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Cos1Ball", typeof(Effect))]
		public static BallShapingShader Cos1Ball { get; internal set; }
		/// <summary>
		/// Radical blur
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\StepSample", typeof(Effect))]
		public static StepSampleShader StepSample { get; internal set; }
		/// <summary>
		/// Scales the screen inwards
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Scale", typeof(Effect))]
		public static ScaleShader Scale { get; internal set; }
		/// <summary>
		/// Creates a color scattering effect (RGB splitting)
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Scatter", typeof(Effect))]
		public static ScatterShader Scatter { get; internal set; }
		/// <summary>
		/// 3D camera effect
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\CameraSurface", typeof(Effect))]
		public static CameraShader Camera { get; internal set; }
		/// <summary>
		/// A swirl effect (Does not distort the screen), also used for creating noise
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Swirl", typeof(Effect))]
		public static SwirlShader Swirl { get; internal set; }
		/// <summary>
		/// Gaussian Blur shader
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Blur", typeof(Effect))]
		public static BlurShader Blur { get; internal set; }
		/// <summary>
		/// Kawase blur sahder, more efficient
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\BlurKawase", typeof(Effect))]
		public static BlurKawaseShader BlurKawase { get; internal set; }
		/// <summary>
		/// Distorts the screen (It is difficult to explain)
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Polar", typeof(Effect))]
		public static PolarShader Polar { get; internal set; }
		/// <summary>
		/// Gray scales the screen
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Gray", typeof(Effect))]
		public static GrayShader Gray { get; internal set; }
		/// <summary>
		/// Creates a ripple effect with minor scaling, do not confuse with <see cref="RadialWave"/>
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Seismic", typeof(Effect))]
		public static SeismicShader Seismic { get; internal set; }
		/// <summary>
		/// Pixelates the screen
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Mosaic", typeof(Effect))]
		public static MosaicShader Mosaic { get; internal set; }
		/// <summary>
		/// Scattering light shader
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Tyndall", typeof(Effect))]
		public static TyndallShader Tyndall { get; internal set; }
		/// <summary>
		/// That one shader in TAS right before the glowing line
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Sprial3D", typeof(Effect))]
		public static SpiralShader Spiral { get; internal set; }
		/// <summary>
		/// Glitch distortion shader (Sinusoidal intensity)
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Wrong", typeof(Effect))]
		public static WrongShader Wrong { get; internal set; }
		/// <summary>
		/// Creates a fire effect on the bottom of the screen
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\NoiseFire", typeof(Effect))]
		public static FireShader Fire { get; internal set; }
		/// <summary>
		/// Huge light beam
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\LightSweep", typeof(Effect))]
		public static LightSweepShader LightSweep { get; internal set; }
		/// <summary>
		/// This shader is broken
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\DislocationX", typeof(Effect))]
		public static DislocationShaderX DislocationX { get; internal set; }
		/// <summary>
		/// Dislocates the screen by creating displacements and wave effect
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\Wave", typeof(Effect))]
		public static WaveShader Wave { get; internal set; }
		/// <summary>
		/// Creates a ripple effect, do not confuse with <see cref="Seismic"/>
		/// </summary>
		[UFEXResource("Global\\Shaders\\Effect Library\\RadialWave", typeof(Effect))]
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
			//Create a color array to hold the pixel data of the region
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