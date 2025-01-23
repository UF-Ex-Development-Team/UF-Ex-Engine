using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.GameInterface;
using static UndyneFight_Ex.GlobalResources.Effects;
using static UndyneFight_Ex.GlobalResources.Font;
using static UndyneFight_Ex.GlobalResources.Sprites;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        /// <summary>
        /// Loads a file (Cross-platform)
        /// </summary>
        /// <typeparam name="T">Content type</typeparam>
        /// <param name="path">Path to file</param>
        /// <param name="cm">Content manager to use</param>
        /// <returns>The loaded content</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadContent<T>(string path, ContentManager cm = null) => (cm ??= Scene.Loader).Load<T>(Path.Combine($"{AppContext.BaseDirectory}{cm.RootDirectory}\\{path}".Split('\\')));
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
                    //Font.Chinese = new GLFont("Sprites\\font\\Chinese", loader);

                    root = AppContext.BaseDirectory + "Content\\Global\\";

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
                        StableEvents = (s) =>
                            s.Parameters["reduceBlueAmount"].SetValue(Settings.SettingsManager.DataLibrary.reduceBlueAmount / 200f)
                    };

                    UsingShader.BackGround = backGroundShader;

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
        internal static class Font
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
            //public static GLFont Chinese { get; internal set; }
        }
        /// <summary>
        /// A list of built-in sprites
        /// </summary>
        public static class Sprites
        {
            /// <summary>
            /// Cursor sprite
            /// </summary>
            public static Texture2D cursor;
            /// <summary>
            /// Legacy LOGIN sprite
            /// </summary>
            public static Texture2D login;
            /// <summary>
            /// Legacy CHAMPIONSHIP sprite
            /// </summary>
            public static Texture2D championShip;
            /// <summary>
            /// Hash texture
            /// </summary>
            public static Texture2D hashtex;
            /// <summary>
            /// Hash texture 2
            /// </summary>
            public static Texture2D hashtex2;
            /// <summary>
            /// Legacy MAIN GAME sprite
            /// </summary>
            public static Texture2D mainGame;
            /// <summary>
            /// Legacy OPTIONS sprite
            /// </summary>
            public static Texture2D options;
            /// <summary>
            /// Legacy ACHIVEMENTS sprite
            /// </summary>
            public static Texture2D achievements;
            /// <summary>
            /// Legacy RECORD sprite
            /// </summary>
            public static Texture2D record;
            /// <summary>
            /// Debug vector arrow sprite
            /// </summary>
            public static Texture2D debugArrow;
            /// <summary>
            /// Loading text
            /// </summary>
            public static Texture2D loadingText;
            /// <summary>
            /// Loading arrow
            /// </summary>
            public static Texture2D progressArrow;
            /// <summary>
            /// Blue star medal
            /// </summary>
            public static Texture2D medal;
            /// <summary>
            /// Purple star medal
            /// </summary>
            public static Texture2D starMedal;
            /// <summary>
            /// Empty medal
            /// </summary>
            public static Texture2D brimMedal;
            /// <summary>
            /// Root texture
            /// </summary>
            public static Texture2D loadingTexture;
        }
    }
    public static class FightResources
    {
        /// <summary>
        /// Loads a file (Cross-platform)
        /// </summary>
        /// <typeparam name="T">Content type</typeparam>
        /// <param name="path">Path to file</param>
        /// <param name="cm">Content manager to use</param>
        /// <returns>The loaded content</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadContent<T>(string path, ContentManager cm = null) => (cm ??= Scene.Loader).Load<T>(Path.Combine($"{AppContext.BaseDirectory}{cm.RootDirectory}\\{path}".Split('\\')));
        private static readonly string[] shardCol = ["white", "yellow", "green", "purple"];
        private static readonly string[] arrType = ["", "circle_", "rot_", "tran_"];
        private static readonly string[] arrColType = ["blue", "red", "green", "purple"];
        public static async void Initialize(ContentManager loader)
        {
            Task task = new(()=>
            {
                loader.RootDirectory = "Content\\Sprites";

                Font.FightFont = FightFont;
                Font.SansFont = SansFont;
                Font.DamageFont = DamageFont;
                Font.Japanese = Japanese;
                Font.NormalFont = NormalFont;
                //Font.Chinese = GlobalResources.Font.Chinese;
                #region Sprites
                Sprites.player = LoadContent<Texture2D>("SOUL\\original", loader);
                Sprites.brokenHeart = LoadContent<Texture2D>("SOUL\\break", loader);
                Sprites.leftHeart = LoadContent<Texture2D>("SOUL\\leftSoul", loader);
                Sprites.rightHeart = LoadContent<Texture2D>("SOUL\\rightSoul", loader);
                Sprites.soulCollide = LoadContent<Texture2D>("SOUL\\collide", loader);
                string root = "bullet\\Shards\\";
                for (int i = 0; i < 6; i++)
                {
                    //There is no need to optimize this for loop, or else the Math operation will take more time
                    for (int k = 1; k <= 2; k++)
                        for (int j = 0; j < 4; j++)
                            Sprites.arrowShards[k - 1, j, i] = LoadContent<Texture2D>($"{root}{shardCol[j]}\\0{k}-{i + 1}", loader);

                    FightSprites.slides[i] = LoadContent<Texture2D>("FightSprites\\frames\\frame_" + i, loader);
                    if (i < 5)
                    {
                        Sprites.heartPieces[i] = LoadContent<Texture2D>("SOUL\\shard" + i, loader);
                        Sprites.GBStart[i] = LoadContent<Texture2D>("GB\\s\\frame_" + i, loader);
                        if (i < 4)
                        {
                            Sprites.explodes[i] = LoadContent<Texture2D>("Explodes\\smallExplode" + (i + 1), loader);

                            for (int k = 0; k < 4; k++)
                                for (int j = 0; j < 4; j++)
                                    Sprites.arrow[j, k, i] = LoadContent<Texture2D>($"bullet\\{arrType[k]}{arrColType[j]}{i}", loader);
                            if (i < 2)
                            {
                                Sprites.GBShooting[i] = LoadContent<Texture2D>("GB\\p\\frame_" + i, loader);
                                FightSprites.fight[i] = LoadContent<Texture2D>("FightSprites\\atk_" + i, loader);
                                FightSprites.act[i] = LoadContent<Texture2D>("FightSprites\\act_" + i, loader);
                                FightSprites.item[i] = LoadContent<Texture2D>("FightSprites\\itm_" + i, loader);
                                FightSprites.mercy[i] = LoadContent<Texture2D>("FightSprites\\mry_" + i, loader);
                            }
                        }
                    }
                }

                Sprites.fireball = LoadContent<Texture2D>("OtherBarrages\\fireball", loader);
                Sprites.spear = LoadContent<Texture2D>("bullet\\spear", loader);
                Sprites.spike = LoadContent<Texture2D>("Bone\\bone_spike", loader);
                Sprites.spider = LoadContent<Texture2D>("OtherBarrages\\spider", loader);
                Sprites.Croissant = LoadContent<Texture2D>("OtherBarrages\\clo", loader);
                Sprites.pixUnit = LoadContent<Texture2D>("others\\pixiv", loader);
                Sprites.firePartical = LoadContent<Texture2D>("others\\firePartical", loader);
                Sprites.lightBall = LoadContent<Texture2D>("others\\lightBall", loader);
                Sprites.lightLine = LoadContent<Texture2D>("others\\lightLine", loader);
                Sprites.square = LoadContent<Texture2D>("others\\square", loader);
                Sprites.boxPiece = LoadContent<Texture2D>("others\\boxPiece", loader);

                Sprites.stuck1 = LoadContent<Texture2D>("others\\GBStuck1", loader);
                Sprites.stuck2 = LoadContent<Texture2D>("others\\GBStuck2", loader);

                Sprites.voidarrow[0] = LoadContent<Texture2D>("bullet\\voidarrow\\blue0", loader);
                Sprites.voidarrow[1] = LoadContent<Texture2D>("bullet\\voidarrow\\red0", loader);
                Sprites.voidarrow[2] = LoadContent<Texture2D>("bullet\\voidarrow\\green0", loader);
                Sprites.voidarrow[3] = LoadContent<Texture2D>("bullet\\voidarrow\\purple0", loader);

                Sprites.target = LoadContent<Texture2D>("bullet\\target", loader);
                Sprites.bullet = LoadContent<Texture2D>("bullet\\gunBullet", loader);
                Sprites.goldenBrim = LoadContent<Texture2D>("bullet\\golden_tip", loader);

                Sprites.shield = LoadContent<Texture2D>("SOUL\\shield", loader);
                Sprites.shinyShield = LoadContent<Texture2D>("SOUL\\shield_shiny", loader);
                Sprites.ShieldCircle = LoadContent<Texture2D>("SOUL\\circle", loader);

                Sprites.hpText = LoadContent<Texture2D>("hp_show\\hp", loader);
                Sprites.krText = LoadContent<Texture2D>("hp_show\\kr", loader);

                Sprites.boneBody = LoadContent<Texture2D>("Bone\\bone_body", loader);
                Sprites.boneHead = LoadContent<Texture2D>("Bone\\bone_up", loader);
                Sprites.boneSlab = LoadContent<Texture2D>("Bone\\bone_slab", loader);
                Sprites.warningLine = LoadContent<Texture2D>("Bone\\warning_line", loader);

                Sprites.GBLaser = LoadContent<Texture2D>("GB\\laser", loader);

                Sprites.explodeTrigger = LoadContent<Texture2D>("Explodes\\explodeTrigger", loader);
                Sprites.allPerfectText = LoadContent<Texture2D>("others\\allPerfect", loader);
                Sprites.accuracyBar = LoadContent<Texture2D>("Pointer\\accuracyBar", loader);
                Sprites.accuracyPointers[0] = LoadContent<Texture2D>("Pointer\\accuracyPointerL", loader);
                Sprites.accuracyPointers[1] = LoadContent<Texture2D>("Pointer\\accuracyPointerM", loader);
                Sprites.accuracyPointers[2] = LoadContent<Texture2D>("Pointer\\accuracyPointerR", loader);

                Sprites.platform[0] = LoadContent<Texture2D>("Platform\\platform_body", loader);
                Sprites.platform[1] = LoadContent<Texture2D>("Platform\\platform_body2", loader);
                Sprites.platformSide[0] = LoadContent<Texture2D>("Platform\\platform_side", loader);
                Sprites.platformSide[1] = LoadContent<Texture2D>("Platform\\platform_side2", loader);
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
                #endregion
                #region Other
                loader.RootDirectory = "Content\\Sprites\\OtherBarrages";
                Sprites.star = LoadContent<Texture2D>("star", loader);
                Sprites.knife = LoadContent<Texture2D>("Knife\\Knife", loader);
                Sprites.KnifeWarn = LoadContent<Texture2D>("Knife\\Warn", loader);

                loader.RootDirectory = "Content\\Sprites\\FightSprites";
                FightSprites.aimer = LoadContent<Texture2D>("aimer", loader);
                FightSprites.dialogBox = LoadContent<Texture2D>("dialogBox", loader);
                FightSprites.stopBar = LoadContent<Texture2D>("stop_bar", loader);
                FightSprites.movingBar = LoadContent<Texture2D>("moving_bar", loader);
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
            public static GLFont FightFont { get; internal set; }
            /// <summary>
            /// Sans Undertale
            /// </summary>
            public static GLFont SansFont { get; internal set; }
            /// <summary>
            /// Hachicro
            /// </summary>
            public static GLFont DamageFont { get; internal set; }
            /// <summary>
            /// Determination Mono
            /// </summary>
            public static GLFont NormalFont { get; internal set; }
            /// <summary>
            /// ta_pop_M
            /// </summary>
            public static GLFont Japanese { get; internal set; }
            //public static GLFont Chinese { get; internal set; }
        }
        /// <summary>
        /// A list of built-in sprites
        /// </summary>
        public static class Sprites
        {
            /// <summary>
            /// The arrow sprite, the first dimension of the array indicates the shard type, the second dimension indicates the mode of the arrow (Normal, Yellow, Green, Purple), the third dimension indicates the damage level
            /// </summary>
            public static Texture2D[,,] arrow = new Texture2D[4, 4, 4];
            /// <summary>
            /// The arrow sprite, the first dimension of the array indicates the color (Blue, Red, Green, Purple), the second dimension indicates the mode of the arrow (Normal, Yellow, Green, Purple), the third dimension indicates the shard type
            /// </summary>
            public static Texture2D[,,] arrowShards { get; } = new Texture2D[2, 4, 6];
            /// <summary>
            /// Sprites of the void arrows
            /// </summary>
            public static Texture2D[] voidarrow = new Texture2D[4];
            /// <summary>
            /// Sprite of the Soul
            /// </summary>
            public static Texture2D player;
            /// <summary>
            /// Sprite of the Graze of the soul
            /// </summary>
            public static Texture2D soulCollide;
            /// <summary>
            /// Sprite of the heart broken in game over
            /// </summary>
            public static Texture2D brokenHeart;
            /// <summary>
            /// Sprite of a star
            /// </summary>
            public static Texture2D star;

            public static Texture2D knife;
            public static Texture2D KnifeWarn;
            /// <summary>
            /// Sprite of a fireball
            /// </summary>
            public static Texture2D fireball;
            /// <summary>
            /// Left and right halves of the heart
            /// </summary>
            public static Texture2D leftHeart, rightHeart;
            public static Texture2D warningLine, boneSlab;
            /// <summary>
            /// Sprites of heart pieces in game over
            /// </summary>
            public static Texture2D[] heartPieces = new Texture2D[5];

            /// <summary>
            /// One pixel
            /// </summary>
            public static Texture2D pixUnit;
            /// <summary>
            /// Trail sprite for arrow speed up
            /// </summary>
            public static Texture2D firePartical;
            /// <summary>
            /// Gun bullet sprite
            /// </summary>
            public static Texture2D bullet;
            /// <summary>
            /// Gun aiming target sprite
            /// </summary>
            public static Texture2D target;
            /// <summary>
            /// Circle sprite
            /// </summary>
            public static Texture2D lightBall;
            /// <summary>
            /// Sprite of pixel with bloom effect
            /// </summary>
            public static Texture2D lightLine;
            /// <summary>
            /// Square sprite
            /// </summary>
            public static Texture2D square;

            /// <summary>
            /// The sprite of the player's shield
            /// </summary>
            public static Texture2D shield;
            /// <summary>
            /// The sprite to display on the shield when arrow collides
            /// </summary>
            public static Texture2D shinyShield;
            /// <summary>
            /// Sprite background for the shield
            /// </summary>
            public static Texture2D ShieldCircle { get; internal set; }

            /// <summary>
            /// Sprite of a spear
            /// </summary>
            public static Texture2D spear;
            /// <summary>
            /// Sprite of a bone spike
            /// </summary>
            public static Texture2D spike;
            /// <summary>
            /// Sprite of a spider
            /// </summary>
            public static Texture2D spider;
            /// <summary>
            /// Sprite of a broken box side (Unused)
            /// </summary>
            public static Texture2D boxPiece;
            /// <summary>
            /// Sprite of croissant (Spider Dance)
            /// </summary>
            public static Texture2D Croissant;
            /// <summary>
            /// Sprites of Green Soul Blaster hitting the shield
            /// </summary>
            public static Texture2D stuck1, stuck2;
            /// <summary>
            /// Sprite of HP of UI
            /// </summary>
            public static Texture2D hpText;
            /// <summary>
            /// Sprite of KR of UI
            /// </summary>
            public static Texture2D krText;
            /// <summary>
            /// Sprites of bone parts
            /// </summary>
            public static Texture2D boneHead, boneBody;
            /// <summary>
            /// Sprites for platform
            /// </summary>
            public static Texture2D[] platform = new Texture2D[2], platformSide = new Texture2D[2];
            /// <summary>
            /// Sprites of GB beginning to fire
            /// </summary>
            public static Texture2D[] GBStart = new Texture2D[5];
            /// <summary>
            /// Sprites of GB during fire
            /// </summary>
            public static Texture2D[] GBShooting = new Texture2D[2];
            /// <summary>
            /// Sprite of GB beam
            /// </summary>
            public static Texture2D GBLaser;
            /// <summary>
            /// Sprite of explosion (Eternal Spring Dream)
            /// </summary>
            public static Texture2D[] explodes = new Texture2D[4];
            /// <summary>
            /// SPrite of exploding card (Eternal Spring Dream)
            /// </summary>
            public static Texture2D explodeTrigger;
            /// <summary>
            /// Sprite of golden outline of arrow
            /// </summary>
            public static Texture2D goldenBrim;
            /// <summary>
            /// Sprite of accuraccy bar on the bottom
            /// </summary>
            internal static Texture2D accuracyBar;
            /// <summary>
            /// Sprite of ALL PERFECT displayed in result
            /// </summary>
            internal static Texture2D allPerfectText;
            /// <summary>
            /// Sprites of accuracy bars on the bottom
            /// </summary>
            internal static Texture2D[] accuracyPointers = new Texture2D[3];
        }
        /// <summary>
        /// A list of built-in audio
        /// </summary>
        public static class Sounds
        {
            /// <summary>
            /// Used for large scale scene transition
            /// </summary>

            public static SoundEffect switchScene;
            /// <summary>
            /// Used for creating large bones
            /// </summary>
            public static SoundEffect boneSpawnLarge;
            /// <summary>
            /// Slamming SFX
            /// </summary>
            public static SoundEffect slam;
            /// <summary>
            /// SFX of player attack
            /// </summary>
            public static SoundEffect playerSlice;
            /// <summary>
            /// SFX of text typing
            /// </summary>
            public static SoundEffect printWord;
            /// <summary>
            /// SFX of Sans talking
            /// </summary>
            public static SoundEffect sansWord;
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
            public static SoundEffect heal;
            /// <summary>
            /// SFX of player taking damage
            /// </summary>
            public static SoundEffect playerHurt;
            /// <summary>
            /// SFX of a spear appearing
            /// </summary>
            public static SoundEffect spearAppear;
            /// <summary>
            /// SFX of a spear being shot
            /// </summary>
            public static SoundEffect spearShoot;
            /// <summary>
            /// SFX of piercing, often used for creating bones
            /// </summary>
            public static SoundEffect pierce;
            /// <summary>
            /// SFX of selecting menu choice
            /// </summary>
            public static SoundEffect select;
            /// <summary>
            /// SFX of changing menu choice
            /// </summary>
            public static SoundEffect changeSelection;
            /// <summary>
            /// SFX of Sans flickering the screen
            /// </summary>
            public static SoundEffect change;
            /// <summary>
            /// SFX of an enemy being damaged
            /// </summary>
            public static SoundEffect damaged;
            /// <summary>
            /// SFX of soul split in half
            /// </summary>
            public static SoundEffect die1;
            /// <summary>
            /// SFX of soul shattering
            /// </summary>
            public static SoundEffect die2;
            /// <summary>
            /// SFX of Gaster Blaster spawning
            /// </summary>
            public static SoundEffect GBSpawn;
            /// <summary>
            /// SFX of Gaster Blaster firing
            /// </summary>
            public static SoundEffect GBShoot;
            /// <summary>
            /// SFX of an explosion
            /// </summary>
            public static SoundEffect explode;
            /// <summary>
            /// SFX of an item being destroyed
            /// </summary>
            public static SoundEffect destroy;
            /// <summary>
            /// SFX of a gun targetting
            /// </summary>
            public static SoundEffect gunTargeting;
            /// <summary>
            /// SFX of a gun being fired
            /// </summary>
            public static SoundEffect gunShot;
            /// <summary>
            /// SFX of DT2 knife
            /// </summary>
            public static SoundEffect largeKnife;
            /// <summary>
            /// SFX of a bone slab spawning/enemy encounter
            /// </summary>
            public static SoundEffect boneSlabSpawn;
            /// <summary>
            /// SFX of DT2 yelling
            /// </summary>
            public static SoundEffect giga;
            /// <summary>
            /// SFX of star appearing
            /// </summary>
            public static SoundEffect star0;
            /// <summary>
            /// SFX of star firing
            /// </summary>
            public static SoundEffect star1;
            /// <summary>
            /// SFX of a sparkle
            /// </summary>
            public static SoundEffect sparkles;
        }
        /// <summary>
        /// A list of sprites used in fighting
        /// </summary>
        public static class FightSprites
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
}