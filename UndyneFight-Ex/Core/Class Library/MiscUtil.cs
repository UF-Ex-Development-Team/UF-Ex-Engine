using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace UndyneFight_Ex
{
	/// <summary>
	/// Miscellaneous Utilities
	/// </summary>
	public class MiscUtil
	{
		#region Shorthand names
		/// <summary>
		/// The shorthand name of the object
		/// </summary>
		/// <param name="name">The shorthand name of the object</param>
		[AttributeUsage(AttributeTargets.All)]
		public class ShorthandName(string name) : Attribute
		{
			/// <summary>
			/// The shorthand name of the value
			/// </summary>
			public string Name = name;
		}
		/// <summary>
		/// Gets the <see cref="ShorthandName"/> of the object
		/// </summary>
		/// <typeparam name="T">An object</typeparam>
		/// <param name="thing">The object to pull the shorthand name of</param>
		/// <returns>The <see cref="ShorthandName"/> of the object</returns>
		public static string GetShorthandName(object thing)
		{
			object[] attributes = thing.GetType().GetMember(thing.ToString())[0].GetCustomAttributes(typeof(ShorthandName), false);
			return (attributes.Length > 0) ? ((ShorthandName)attributes[0]).Name : "";
		}
		#endregion
		#region String Key convsersion
		private static readonly Dictionary<Keys, string> KeyStringDict = new()
		{
			{Keys.Escape, "Esc"},
			{Keys.Enter, "Enter"},
			{Keys.Back, "Backspace"},
			{Keys.Tab, "Tab"},
			{Keys.LeftShift, "L Shift"},
			{Keys.RightShift, "R Shift"},
			{Keys.LeftControl, "L Ctrl"},
			{Keys.RightControl, "R Ctrl"},
			{Keys.LeftAlt, "L Alt"},
			{Keys.RightAlt, "R Alt"},
			{Keys.Space, "Space"},
			{Keys.CapsLock, "Caps Lock"},
			{Keys.PageUp, "Pg Up"},
			{Keys.PageDown, "Pg Down"},
			{Keys.End, "End"},
			{Keys.Home, "Home"},
			{Keys.Select, "Select"},
			{Keys.Print, "Print"},
			{Keys.Execute, "Exe"},
			{Keys.PrintScreen, "PrntScrn"},
			{Keys.Insert, "Ins"},
			{Keys.Delete, "Del"},
			{Keys.Help, "Help"},
			{Keys.LeftWindows, "L Windows"},
			{Keys.RightWindows, "R Windows"},
			{Keys.NumPad0, "Num 0"},
			{Keys.NumPad1, "Num 1"},
			{Keys.NumPad2, "Num 2"},
			{Keys.NumPad3, "Num 3"},
			{Keys.NumPad4, "Num 4"},
			{Keys.NumPad5, "Num 5"},
			{Keys.NumPad6, "Num 6"},
			{Keys.NumPad7, "Num 7"},
			{Keys.NumPad8, "Num 8"},
			{Keys.NumPad9, "Num 9"},
			{Keys.F1, "F1"},
			{Keys.F2, "F2"},
			{Keys.F3, "F3"},
			{Keys.F4, "F4"},
			{Keys.F5, "F5"},
			{Keys.F6, "F6"},
			{Keys.F7, "F7"},
			{Keys.F8, "F8"},
			{Keys.F9, "F9"},
			{Keys.F10, "F10"},
			{Keys.F11, "F11"},
			{Keys.F12, "F12"},
			{Keys.Multiply, "Num Mult"},
			{Keys.Add, "Num Add"},
			{Keys.Separator, "Num /"},
			{Keys.Subtract, "Num -"},
			{Keys.Decimal, "Num ."},
			{Keys.Divide, "Num /"},
			{Keys.NumLock, "Num Lock"},
			{Keys.Scroll, "Scroll Lock"},
			{Keys.OemSemicolon, ";"},
			{Keys.OemComma, "Comma"},
			{Keys.OemPlus, "+"},
			{Keys.OemMinus, "-"},
			{Keys.OemPeriod, "."},
            //For some reason, '/' is stored as '?'
            {Keys.OemQuestion, "/"},
			{Keys.OemOpenBrackets, "["},
			{Keys.OemCloseBrackets, "]"},
			{Keys.OemPipe, "\\"},
			{Keys.OemQuotes, "'"},
			{Keys.Pause, "Pause"},
			{Keys.Kana, "Kana"},
			{Keys.Kanji, "Kanji"},
			{Keys.Right, "Right"},
			{Keys.Left, "Left"},
			{Keys.Up, "Up"},
			{Keys.Down, "Down"},
		};
		//TODO Dictionary
		/// <summary>
		/// <br>Converts a key to string</br>
		/// <br>Warning, this may not cover all the keys, report to the Discord server if you found any missing keys</br>
		/// </summary>
		/// <param name="key">The key to convert</param>
		/// <returns>The string name of the key</returns>
		public static string KeyToString(Keys key) => KeyStringDict.TryGetValue(key, out string val) ? val : key.ToString();
		/// <summary>
		/// <br>Converts a string to key</br>
		/// <br>The strings should follow the format of <see cref="KeyToString(Keys)"/></br>
		/// <br>Warning, this may not cover all the keys, report to the Discord server if you found any missing keys</br>
		/// </summary>
		/// <param name="text">The string to convert</param>
		/// <returns>The key the string represents</returns>
		public static Keys StringToKey(string text) => KeyStringDict.ContainsValue(text) ?
					//Existing Dict
					KeyStringDict.Where(pair => pair.Value == text)
					.Select(pair => pair.Key)
					.FirstOrDefault() :
					//Individual chars
					(text.Length == 1 && ((text[0] >= 'a' && text[0] <= 'z') || (text[0] >= 'A' && text[0] <= 'Z') || (text[0] >= '0' && text[0] <= '9')) ? (Keys)text[0] : Keys.None);
		/// <summary>
		/// Gets the current key binding of the input identity
		/// </summary>
		/// <param name="identity">The identity to get</param>
		/// <returns>The key the identity is binded to</returns>
		public static List<Keys> GetInputKeys(InputIdentity identity) => GameStates.KeyChecker.InputKeys[identity];
		#endregion
		/// <summary>
		/// Gets the asset from the name, i.e. FightResources.Sounds.spearAppear -> GetAsset("spearAppear", typeof(FightResources.Sounds)) would return the audio
		/// </summary>
		/// <param name="name">The name of the asset</param>
		/// <param name="type">The type the asset belongs to</param>
		/// <returns>The asset</returns>
		public static object GetAsset(string name, Type type)
		{
			foreach (System.Reflection.MemberInfo asset in type.GetMembers())
				if (asset.Name == name)
					return type.GetField(name)?.GetValue(null);
			Debug.WriteLine($"Error loading {name}, check if you made a typo");
			return null;
		}
		#region Text renderer
		/// <summary>
		/// User defined textures for sprite drawing
		/// </summary>
		private static readonly Dictionary<string, Texture2D> GlobalDefinedTextures = [];
		/// <summary>
		/// User defined fonts for sprite drawing
		/// </summary>
		private static readonly Dictionary<string, GLFont> GlobalDefinedFonts = [];
		/// <summary>
		/// <para>A text builder that allows text formatting on the fly</para>
		/// <para>Use '[[' for displaying '[' and '\b' for displaying ']'</para>
		/// </summary>
		/// <param name="text">The text to display with format commands</param>
		/// <param name="typer">The text typer class to associate (Can be null)</param>
		/// <param name="tag_start">The char for tag start, default '['</param>
		/// <param name="tag_end">The char for tag end, default ']'</param>
		public class TextBuilder(string text, TextTyper typer = null, char tag_start = '[', char tag_end = ']') : Entity
		{
			#region Variables
			/// <summary>
			/// The text typer to associate with
			/// </summary>
			private TextTyper Typer = typer;
			/// <summary>
			/// The index of the text to display
			/// </summary>
			private float TextDisplayIndex = typer is null ? -1 : 0;
			/// <summary>
			/// The open and end command tags, default '[', ']'
			/// </summary>
			private readonly char[] CommandTag = [tag_start, tag_end];
			/// <summary>
			/// The list of indexes to display the <see cref="CommandTag"/>
			/// </summary>
			private readonly List<int> CommandTagIndexes = [];
			/// <summary>
			/// The parsed text to display
			/// </summary>
			private string ParsedText = text;
			/// <summary>
			/// The list of text height separated by the lines
			/// </summary>
			private readonly List<float> TextLineHeights = [];
			/// <summary>
			/// The list of text width separated by the lines
			/// </summary>
			private readonly List<float> TextLineWidths = [];
			/// <summary>
			/// The maximum line width
			/// </summary>
			private float forceMaxLineWidth = -1;
			/// <summary>
			/// Whether the wrapping of the text will ignore spaces
			/// </summary>
			private bool wrapIgnoreSpace = false;
			/// <summary>
			/// The total height of the parsed text
			/// </summary>
			private float TotalTextHeight = 0;
			/// <summary>
			/// The list of arguments for each <see cref="Commands"/>, format: List[char index, TextArgument]
			/// </summary>
			private readonly List<TextArgument> Arguments = [];
			/// <summary>
			/// The list of commands to execute, format: List[char index, command text]
			/// </summary>
			private readonly List<string> Commands = [];
			/// <summary>
			/// The command queue to execute
			/// </summary>
			private Queue<int> CommandQueue = [];
			/// <summary>
			/// The current ID of the command to execute
			/// </summary>
			private int CurrentCommandCount = 0;
			/// <summary>
			/// Cache of char positions
			/// </summary>
			private readonly List<Vector2> drawingPositions = [];
			/// <summary>
			/// The position of the current char during parsing
			/// </summary>
			private Vector2 _parseDrawingPosition;
			/// <summary>
			/// The default text settings
			/// </summary>
			private TextBuilderData defaultTextBuilderData = new();
			/// <summary>
			/// The current text settings during drawing
			/// </summary>
			private TextBuilderData currentDrawingData = new();
			/// <summary>
			/// User defined textures for sprite drawing
			/// </summary>
			private readonly Dictionary<string, Texture2D> DefinedTextures = [];
			/// <summary>
			/// User defined fonts for sprite drawing
			/// </summary>
			private readonly Dictionary<string, GLFont> DefinedFonts = [];
			/// <summary>
			/// Whether it is currently at drawing state
			/// </summary>
			private bool isDrawing = false;
			/// <summary>
			/// The current drawing char index
			/// </summary>
			private int _currentDrawingCharIndex = 0;
			/// <summary>
			/// The current amount of sprites drawn (Index displacement due to inserting the null char)
			/// </summary>
			private int _currentDrawSpriteCount = 0;
			/// <summary>
			/// The current line of drawing
			/// </summary>
			private int _curDrawingLine = 0;
			/// <summary>
			/// The current amount of Effects stacked
			/// </summary>
			private int curStackEffCount = 0;
			/// <summary>
			/// Time elapsed
			/// </summary>
			private int Timer = 0;
			/// <summary>
			/// Whether the rendering process will be done automatically or requires <see cref="Render"/> to render
			/// </summary>
			public bool AutoRender = true;
			/// <summary>
			/// Whether to render the text
			/// </summary>
			private bool DoRender = true;
			#endregion
			/// <summary>
			/// Parses the text for display
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private void ParseText()
			{
				//Clears existing cache
				Commands.Clear();
				CommandQueue.Clear();
				Arguments.Clear();
				TextLineHeights.Clear();
				TextLineWidths.Clear();
				//Reset position
				drawingPositions.Clear();
				_parseDrawingPosition = defaultTextBuilderData.Position;
				_parseDrawingPosition = Vector2.Zero;
				//Check for close command tags, while "[[" is for indication for displaying '[' in text
				while (ParsedText.Contains(CommandTag[1]) || ParsedText.Contains($"{CommandTag[0]}{CommandTag[0]}"))
				{
					int commandTagStartPos = ParsedText.IndexOf(CommandTag[0]);
					int commandTagEndPos = ParsedText.IndexOf(CommandTag[1]) + 1;
					int doubleCommandPos = ParsedText.IndexOf($"{CommandTag[0]}{CommandTag[0]}");
					//Check for sole close tag
					//Check for double tag
					if (doubleCommandPos == commandTagStartPos && !CommandTagIndexes.Contains(commandTagStartPos))
					{
						CommandTagIndexes.Add(commandTagStartPos);
						ParsedText = ParsedText[..(commandTagStartPos + 1)] + ParsedText[(commandTagStartPos + 2)..];
						continue;
					}
					//If there exists another '[' before a ']', the first '[' is a displayed '[', i.e. [ text [command]
					int nextCommandTagPos = ParsedText[(commandTagStartPos + 1)..].IndexOf(CommandTag[0]) + 1 + commandTagStartPos;
					while (nextCommandTagPos < commandTagEndPos && nextCommandTagPos != commandTagStartPos + 1 && commandTagEndPos != -1)
					{
						commandTagStartPos = nextCommandTagPos + 1;
						nextCommandTagPos = ParsedText[commandTagStartPos..].IndexOf(CommandTag[0]) + 1 + commandTagStartPos;
						commandTagStartPos--;
					}
					string stringBeforeCommand = ParsedText[..commandTagStartPos];
					string stringAfterCommand = ParsedText[commandTagEndPos..];
					string function = ParsedText[(commandTagStartPos + 1)..(commandTagEndPos - 1)];
					//Check for arguments in functions
					if (function.Contains(','))
					{
						string[] dividedText = function.Split(',');
						Commands.Add(new(dividedText[0]));
						//The rest are arguments
						Arguments.Add(new(dividedText[0], dividedText[1..]));
					}
					else
					{
						Commands.Add(new(function));
						Arguments.Add(new(function, [""]));
					}
					//Add the tag pos into the command queue for caching
					CommandQueue.Enqueue(commandTagStartPos);
					ParsedText = stringBeforeCommand + stringAfterCommand;
				}
				//Set all text to instantly display if no typer is associated
				//Replace backspace for ']'
				ParsedText = ParsedText.Replace('\b', CommandTag[1]);
				PostParseAlignment();
				if (TextDisplayIndex == -1)
					TextDisplayIndex = ParsedText.Length;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private void ParseCommand(string command_text)
			{
				//Cycle through the command for action
				switch (command_text)
				{
					case "/":
						int curEffCount = currentDrawingData.curEffCount;
						Vector2 prevPos = currentDrawingData.Position;
						List<List<object>> effs = currentDrawingData.EffectParam;
						Effects eff = currentDrawingData.Effect;
						currentDrawingData = (TextBuilderData)defaultTextBuilderData.Clone();
						currentDrawingData.curEffCount = curEffCount + 1; //Add 1 since '/' is also an effect
						currentDrawingData.Position = prevPos;
						currentDrawingData.EffectParam = effs;
						currentDrawingData.Effect = eff;
						currentDrawingData.curEffCount--;
						break;
					case "Color":
						//Check whether it is just 1 color or 4 colors
						TextArgument ColorArg = Arguments.ElementAt(CurrentCommandCount);
						//Allocate array
						currentDrawingData.Col = new Color[4];
						if (ColorArg.Arguments.Length == 1)
							for (int i = 0; i < 4; i++)
								currentDrawingData.Col[i] = ToXNAColor(System.Drawing.Color.FromName(ColorArg.Arguments[0]));
						else if (ColorArg.Arguments.Length == 4)
							currentDrawingData.Col = [
								ToXNAColor(System.Drawing.Color.FromName(ColorArg.Arguments[0])),
								ToXNAColor(System.Drawing.Color.FromName(ColorArg.Arguments[1])),
								ToXNAColor(System.Drawing.Color.FromName(ColorArg.Arguments[2])),
								ToXNAColor(System.Drawing.Color.FromName(ColorArg.Arguments[3]))];
						break;
					case "/Color":
						currentDrawingData.Col = defaultTextBuilderData.Col;
						break;
					case "Scale":
						//Check for a scalar scale or vector scale
						string[] ScaleArg = Arguments.ElementAt(CurrentCommandCount).Arguments;
						currentDrawingData.Scale = ScaleArg.Length == 1
							? new(MathUtil.FloatFromString(ScaleArg[0]))
							: new(MathUtil.FloatFromString(ScaleArg[0]), MathUtil.FloatFromString(ScaleArg[1]));
						break;
					case "/Scale":
						currentDrawingData.Scale = defaultTextBuilderData.Scale;
						break;
					case "Font":
						string FontArg = Arguments.ElementAt(CurrentCommandCount).Arguments[0];
						//Check for defined fonts
						if (DefinedFonts.TryGetValue(FontArg, out GLFont value))
							currentDrawingData.CurrentFont = value;
						//Check for shorthand names
						FontArg = FontArg switch
						{
							"Normal" => "NormalFont",
							"Sans" => "SansFont",
							"Fight" => "FightFont",
							"Damage" => "DamageFont",
							_ => FontArg
						};
						currentDrawingData.CurrentFont = GetAsset(FontArg, typeof(FightResources.Font)) as GLFont;
						break;
					case "/Font":
						currentDrawingData.CurrentFont = defaultTextBuilderData.CurrentFont;
						break;
					case "Sprite":
						void ProcessTexture(Texture2D tex, Vector2 spritePosDelta)
						{
							if (isDrawing)
							{
								TextBuilderData curData = currentDrawingData;
								FormalDraw(tex, ParseRenderPosition(_currentDrawingCharIndex) + spritePosDelta, curData.Col[0], curData.Scale, 0, curData.VAlignment switch
								{
									TextVAlignment.Top => Vector2.Zero,
									TextVAlignment.Center => new(0, tex.Height / 2f),
									TextVAlignment.Bottom => new(0, tex.Height)
								});
								if (_currentDrawingCharIndex == MathF.Floor(TextDisplayIndex))
									Typer.Delay(Typer.TypingSpeed * 125f);
								_currentDrawSpriteCount++;
							}
							else
							{
								_parseDrawingPosition.X += tex.Width + spritePosDelta.X;
								ParsedText = ParsedText.Insert(_currentDrawingCharIndex, "\u0000");
								_currentDrawSpriteCount++;
								drawingPositions.Add(drawingPositions[_currentDrawingCharIndex - 1]);
							}
						}
						TextArgument SpriteArg = Arguments.ElementAt(CurrentCommandCount);
						string SpriteToDraw = SpriteArg.Arguments[0];
						Vector2 spritePosDelta = Vector2.Zero;
						int spriteIndex = 0;
						if (SpriteArg.Arguments.Length > 1)
						{
							//Displacement
							spritePosDelta = new(
								SpriteArg.Arguments[1] == "" ? 0 : MathUtil.FloatFromString(SpriteArg.Arguments[1]),
								SpriteArg.Arguments[2] == "" ? 0 : MathUtil.FloatFromString(SpriteArg.Arguments[2]));
							//Index
							if (SpriteArg.Arguments.Length > 3)
								spriteIndex = (int)MathUtil.FloatFromString(SpriteArg.Arguments[3]);
						}
						//Check defined textures
						if (DefinedTextures.TryGetValue(SpriteToDraw, out Texture2D tex))
							ProcessTexture(tex, spritePosDelta);
						//Check GlobalResources
						object preTex = GetAsset(SpriteToDraw, typeof(GlobalResources.Sprites));
						if (preTex is not null)
						{
							ProcessTexture(preTex is Texture2D ? preTex as Texture2D : (preTex as Texture2D[])[spriteIndex], spritePosDelta);
						}
						else //Check FightResources
						{
							if ((preTex = GetAsset(SpriteToDraw, typeof(FightResources.Sprites))) is not null)
								ProcessTexture(preTex is Texture2D ? preTex as Texture2D : (preTex as Texture2D[])[spriteIndex], spritePosDelta);
						}
						break;
					case "Wave":
						currentDrawingData.Effect ^= Effects.Wave;
						if (!isDrawing)
						{
							string[] WaveArg = Arguments.ElementAt(CurrentCommandCount).Arguments;
							int countDiff = currentDrawingData.EffectParam.Count - currentDrawingData.curEffCount;
							if (countDiff == 0)
							{
								currentDrawingData.EffectParam.Add([MathUtil.FloatFromString(WaveArg[0])]);
								curStackEffCount++;
							}
							else if (countDiff > 0)
							{
								currentDrawingData.EffectParam.Last().Add(MathUtil.FloatFromString(WaveArg[0]));
								curStackEffCount++;
							}
						}
						break;
					case "/Wave":
						if ((currentDrawingData.Effect & Effects.Wave) != 0)
						{
							currentDrawingData.Effect ^= Effects.Wave;
							curStackEffCount--;
						}
						break;
					case "Shake":
						currentDrawingData.Effect ^= Effects.Shake;
						if (!isDrawing)
						{
							string[] ShakeArg = Arguments.ElementAt(CurrentCommandCount).Arguments;
							Vector3 ShakeParam = ShakeArg.Length switch
							{
								1 => new(new Vector2(MathUtil.FloatFromString(ShakeArg[0])), 1),
								2 => new(MathUtil.FloatFromString(ShakeArg[0]), MathUtil.FloatFromString(ShakeArg[1]), 1),
								3 => new(MathUtil.FloatFromString(ShakeArg[0]), MathUtil.FloatFromString(ShakeArg[1]), MathUtil.FloatFromString(ShakeArg[2]))
							};
							int countDiff = currentDrawingData.EffectParam.Count - currentDrawingData.curEffCount;
							if (countDiff <= 0)
							{
								currentDrawingData.EffectParam.Add([ShakeParam]);
								curStackEffCount++;
							}
							else if (countDiff > 0)
							{
								currentDrawingData.EffectParam.Last().Add(ShakeParam);
								curStackEffCount++;
							}
						}
						break;
					case "/Shake":
						if ((currentDrawingData.Effect & Effects.Shake) != 0)
						{
							currentDrawingData.Effect ^= Effects.Shake;
							curStackEffCount--;
						}
						break;
#if DEBUG
					default:
						Debug.WriteLine($"Unrecognized command: {command_text}");
						break;
#endif
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private void PostParseAlignment()
			{
				//Reset current drawing data
				currentDrawingData = (TextBuilderData)defaultTextBuilderData.Clone();
				currentDrawingData.Position = Vector2.Zero;
				_currentDrawSpriteCount = 0;
				int[] TempQueue = [..CommandQueue];
				//Parse text positions
				CurrentCommandCount = 0;
				Vector2 charSize = Vector2.Zero;
				float maxCharHeight = 0;
				float lineWidth = 0;
				float mult;
				float prevXScale = currentDrawingData.Scale.X;
				bool LineBroken = false;
				for (int i = 0; i < ParsedText.Length; i++)
				{
					prevXScale = currentDrawingData.Scale.X;
					while (TempQueue.Contains(i - _currentDrawSpriteCount))
					{
						_currentDrawingCharIndex = i;
						ParseCommand(Commands[CurrentCommandCount]);
						TempQueue = TempQueue[1..];
						CurrentCommandCount++;
					}
					//Debug.WriteLine($"{ParsedText[i]}: {_parseDrawingPosition}");
					//Try parse drawable text (NULL char for sprite -> cannot parse size)
					if (ParsedText[i] != '\u0000')
					{
						charSize = currentDrawingData.CurrentFont.SFX.MeasureString(ParsedText[i].ToString());
						//I don't know why yet, but the first char is displaced by 1 char width
						if (i == 0 && currentDrawingData.HAlignment == TextHAlignment.Left)
						{
							_parseDrawingPosition.X -= charSize.X * currentDrawingData.Scale.X;
						}
						lineWidth += charSize.X * currentDrawingData.Scale.X;
						//Line wrapping
						if (forceMaxLineWidth != -1 && lineWidth > forceMaxLineWidth && LineBroken)
						{
							int lastIndex = ParsedText[..(i + 1)].LastIndexOf(' ');
							int checkIndex = wrapIgnoreSpace ? i : (lastIndex == -1 ? i : lastIndex);
							ParsedText = ParsedText.Insert(checkIndex, "\n");
							lineWidth = 0;
							drawingPositions.RemoveRange(checkIndex, i - checkIndex);
							_parseDrawingPosition = drawingPositions[^1];
							i = checkIndex;
							//Adjust command queue values due to extra '\n'
							int[] tmp = [.. CommandQueue];
							for (int j = 0; j < tmp.Length; j++)
							{
								if (tmp[j] >= i)
									tmp[j]++;
							}
							CommandQueue = new(tmp); //Do not convert
													 //Check for space
							if (!wrapIgnoreSpace)
							{
								while (ParsedText.Last() != '\n' && ParsedText[i + 1] == ' ')
								{
									ParsedText = ParsedText.Remove(i + 1, 1);
									//Adjust command queue values due to removed space
									tmp = [.. CommandQueue];
									for (int j = 0; j < tmp.Length; j++)
									{
										if (tmp[j] >= i + 1)
											tmp[j]--;
									}
									CommandQueue = new(tmp); //Do not convert
								}
							}
						}
						if (LineBroken = ParsedText[i] != '\n')
							maxCharHeight = MathF.Max(maxCharHeight, charSize.Y * currentDrawingData.Scale.Y);
					}
					else
					{
						LineBroken = false;
						continue;
					}
					//Parse line breaking
					vec2 prevCharSize = i == 0 ? Vector2.Zero : currentDrawingData.CurrentFont.SFX.MeasureString(ParsedText[i - 1].ToString());
					if (ParsedText[i] == '\n')
					{
						float charYDisplace = maxCharHeight == 0 ? prevCharSize.Y * currentDrawingData.Scale.Y : maxCharHeight;
						currentDrawingData.Position.Y += charYDisplace;
						TextLineHeights.Add(charYDisplace);
						maxCharHeight = 0;
						mult = currentDrawingData.HAlignment switch
						{
							TextHAlignment.Middle => 2f,
							TextHAlignment.Right => 1f,
							_ => 0
						};
						TextLineWidths.Add(_parseDrawingPosition.X + prevCharSize.X * currentDrawingData.Scale.X * mult);
						_parseDrawingPosition.X = currentDrawingData.HAlignment == TextHAlignment.Left ? -prevCharSize.X * prevXScale : 0;
						lineWidth = 0;
					}
					else
						_parseDrawingPosition.X += charSize.X * prevXScale;
					_parseDrawingPosition.Y = currentDrawingData.Position.Y;
					drawingPositions.Add(_parseDrawingPosition);
					_currentDrawingCharIndex = 0;
				}
				_currentDrawSpriteCount = 0;
				TextLineHeights.Add(currentDrawingData.CurrentFont.SFX.MeasureString(ParsedText[^1..]).Y * currentDrawingData.Scale.Y);
				TotalTextHeight = TextLineHeights.Sum();
				bool hasSpace = ParsedText.Contains('\n') && ParsedText[ParsedText.LastIndexOf('\n')..].Contains(' ');
				mult = currentDrawingData.HAlignment switch
				{
					TextHAlignment.Middle => hasSpace ? 1 : 2,
					TextHAlignment.Right => hasSpace ? 0 : 1,
					_ => 0
				};
				TextLineWidths.Add(_parseDrawingPosition.X + charSize.X * currentDrawingData.Scale.X * mult);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private Vector2 ParseRenderPosition(int index)
			{
				TextBuilderData curData = currentDrawingData;
				vec2 DrawingPos = drawingPositions[index];
				curStackEffCount = 0;
				if ((curData.Effect & Effects.Wave) != 0)
				{
					DrawingPos += MathUtil.GetVector2((float)curData.EffectParam[curData.curEffCount][curStackEffCount], Timer + index * (Typer?.Speed() ?? 1));
					curStackEffCount++;
				}
				if ((curData.Effect & Effects.Shake) != 0)
				{
					Vector3 shakeParams = (Vector3)curData.EffectParam[curData.curEffCount][curStackEffCount];
					if ((Timer % shakeParams.Z) == 0)
					{
						Vector2 shakeDelta = new(shakeParams.X, shakeParams.Y);
						shakeDelta.Rotate(MathUtil.GetRandom(0, MathF.PI * 2));
						DrawingPos += shakeDelta;
					}
					curStackEffCount++;
				}
				if (curData.Effect != Effects.None)
					curData.curEffCount++;
				DrawingPos.Y -= currentDrawingData.VAlignment switch
				{
					TextVAlignment.Center => TotalTextHeight / 2f,
					TextVAlignment.Bottom => TotalTextHeight,
					_ => 0
				};
				DrawingPos.X -= currentDrawingData.HAlignment switch
				{
					TextHAlignment.Middle => TextLineWidths[_curDrawingLine] / 2f,
					TextHAlignment.Right => TextLineWidths[_curDrawingLine],
					_ => 0
				};
				return DrawingPos + defaultTextBuilderData.Position;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private static Color ToXNAColor(System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);
			public override void Update()
			{
				Timer++;
				//If there exists a text typer
				if (Typer?.DelayTimer == 0)
				{
					float prevIndex = TextDisplayIndex;
					TextDisplayIndex = MathHelper.Clamp(TextDisplayIndex + Typer.TypingSpeed, 0, ParsedText.Length);
					//Play audio and execute action if a char is displayed
					if (MathF.Floor(TextDisplayIndex) != MathF.Floor(prevIndex))
					{
						if (ParsedText[Math.Min((int)TextDisplayIndex, ParsedText.Length - 1)] == '\n')
							TextDisplayIndex++;
						//Play audio
						Typer.SoundPerChar?.CreateInstance().Play();
						//Execute action
						Typer.ActionPerChar?.Invoke();
					}
				}
			}
			public override void Draw()
			{
				if (!(DoRender || AutoRender))
					return;
				isDrawing = true;
				int i = 0;
				CurrentCommandCount = 0;
				_curDrawingLine = 0;
				//Reset current drawing data
				List<List<object>> EffParam = currentDrawingData.EffectParam;
				currentDrawingData = (TextBuilderData)defaultTextBuilderData.Clone();
				currentDrawingData.Position = Vector2.Zero;
				currentDrawingData.EffectParam = EffParam;
				_currentDrawSpriteCount = 0;
				currentDrawingData.curEffCount = 0;
				Queue<int> TempQueue = new(CommandQueue); //Do not convert to collection expression
				int charsToDisplay = (int)MathF.Floor(TextDisplayIndex);
				while (i <= charsToDisplay)
				{
					while (TempQueue.Contains(i - _currentDrawSpriteCount))
					{
						_currentDrawingCharIndex = Math.Min(i, charsToDisplay - 1);
						ParseCommand(Commands[CurrentCommandCount]);
						TempQueue.Dequeue();
						CurrentCommandCount++;
					}
					//Check if the current char is not a NULL char (Reserved for sprite)
					if (i < charsToDisplay)
					{
						char curChar = ParsedText[i];
						if (curChar == '\u0000')
							continue;
						if (curChar == '\n')
						{
							_curDrawingLine++;
							i++;
							continue;
						}
						TextBuilderData curData = currentDrawingData;
						Vector2 parsedPos = ParseRenderPosition(i);
						System.Drawing.RectangleF rect = new(-100, -100, GraphicsDeviceManager.DefaultBackBufferWidth * Fight.Functions.ScreenDrawing.ScreenScale + 200, GraphicsDeviceManager.DefaultBackBufferHeight * Fight.Functions.ScreenDrawing.ScreenScale + 200);
						if (rect.Contains(new System.Drawing.PointF(parsedPos.X, parsedPos.Y)))
							SpriteBatch.DrawString(curData.CurrentFont, curChar.ToString(), parsedPos, curData.Col, 0, scaleN: curData.Scale, layerDepth: Depth);
					}
					++i;
				}
				isDrawing = false;
				DoRender = false;
			}
			#region Definitions
			private struct TextArgument(string Command, string[] Arguments)
			{
				/// <summary>
				/// The command of the argument
				/// </summary>
				public string Command = Command;
				/// <summary>
				/// The list of arguments
				/// </summary>
				public string[] Arguments = Arguments;
				public override readonly string ToString()
				{
					string tmp = string.Empty;
					foreach (string item in Arguments)
						tmp += item + ", ";
					return $"Command {Command} has argument(s) {tmp[..^2]}.";
				}
			}
			/// <summary>
			/// The horizontal text alignment
			/// </summary>
			public enum TextHAlignment
			{
				/// <summary>
				/// Aligns the text to the left side
				/// </summary>
				Left,
				/// <summary>
				/// Aligns the text horizontally at the center
				/// </summary>
				Middle,
				/// <summary>
				/// Aligns the text to the right side
				/// </summary>
				Right
			}
			/// <summary>
			/// The vertical text alignment
			/// </summary>
			public enum TextVAlignment
			{
				/// <summary>
				/// Aligns the text at the top
				/// </summary>
				Top,
				/// <summary>
				/// Aligns the text vertically at the center
				/// </summary>
				Center,
				/// <summary>
				/// Aligns the text at the bottom (It is normal to have a small displacement)
				/// </summary>
				Bottom
			}
			private struct TextBuilderData() : ICloneable
			{
				/// <summary>
				/// The position of the text builder
				/// </summary>
				public Vector2 Position = Vector2.Zero;
				/// <summary>
				/// The scale of the text
				/// </summary>
				public Vector2 Scale = Vector2.One;
				/// <summary>
				/// The color of the text
				/// </summary>
				public Color[] Col = [Color.White, Color.White, Color.White, Color.White];
				/// <summary>
				/// The horizontal alignment of the text
				/// </summary>
				public TextHAlignment HAlignment = TextHAlignment.Left;
				/// <summary>
				/// The vertical alignment of the text
				/// </summary>
				public TextVAlignment VAlignment = TextVAlignment.Top;
				/// <summary>
				/// The current font of drawing of the text
				/// </summary>
				public GLFont CurrentFont = GlobalResources.Font.NormalFont;
				/// <summary>
				/// The current text effect
				/// </summary>
				public Effects Effect = Effects.None;
				/// <summary>
				/// Current effect index
				/// </summary>
				public int curEffCount = 0;
				/// <summary>
				/// The current text effect parameters
				/// </summary>
				public List<List<object>> EffectParam = [];
				public readonly object Clone() => MemberwiseClone();
			}
			/// <summary>
			/// Available text effects to apply
			/// </summary>
			[Flags]
			public enum Effects
			{
				/// <summary>
				/// No effect
				/// </summary>
				None = 0,
				/// <summary>
				/// Shaking text
				/// </summary>
				Shake = 1,
				/// <summary>
				/// Wave-motion text
				/// </summary>
				Wave = 2
			}
			#endregion
			#region User callable functions
			/// <summary>
			/// Overwrites the text in the text builder (Automatically rebuilds, may cause lag spike)
			/// </summary>
			/// <param name="text">The new text to draw</param>
			/// <returns></returns>
			public TextBuilder Overwrite(string text)
			{
				ParsedText = text;
				TextDisplayIndex = Typer is null ? -1 : 0;
				ParseText();
				return this;
			}
			/// <summary>
			/// Builds the text for text rendering
			/// </summary>
			/// <returns></returns>
			public TextBuilder Build()
			{
				ParseText();
				GameStates.InstanceCreate(this);
				return this;
			}
			/// <summary>
			/// Gets the position of the text
			/// </summary>
			/// <returns>The position of the text</returns>
			public Vector2 Position() => defaultTextBuilderData.Position;
			/// <summary>
			/// Sets the position of the text
			/// </summary>
			/// <param name="position">The position of the text to set to</param>
			/// <returns></returns>
			public TextBuilder Position(Vector2 position)
			{
				defaultTextBuilderData.Position = position;
				return this;
			}
			/// <summary>
			/// Gets the current text alignment
			/// </summary>
			/// <returns>The (Horizontal, Vertical) alignment</returns>
			public (TextHAlignment, TextVAlignment) Align() => (defaultTextBuilderData.HAlignment, defaultTextBuilderData.VAlignment);
			/// <summary>
			/// Sets the alignment of the text
			/// </summary>
			/// <param name="halignment">The horizontal alignment to set to</param>
			/// <param name="valignment">The vertical alignment to set to</param>
			/// <returns></returns>
			public TextBuilder Align(TextHAlignment? halignment = null, TextVAlignment? valignment = null)
			{
				(defaultTextBuilderData.HAlignment, defaultTextBuilderData.VAlignment) = (halignment ?? defaultTextBuilderData.HAlignment, valignment ?? defaultTextBuilderData.VAlignment);
				return this;
			}
			/// <summary>
			/// Gets the current scale of the text
			/// </summary>
			/// <returns></returns>
			public new Vector2 Scale() => defaultTextBuilderData.Scale;
			/// <summary>
			/// Sets the scale of the text
			/// </summary>
			/// <param name="scale">The scale to set to</param>
			/// <returns></returns>
			public new TextBuilder Scale(Vector2 scale)
			{
				defaultTextBuilderData.Scale = scale;
				return this;
			}
			/// <summary>
			/// Gets the default blend of the text
			/// </summary>
			/// <returns></returns>
			public Color[] Blend() => defaultTextBuilderData.Col;
			/// <summary>
			/// Sets the default color of the text
			/// </summary>
			/// <param name="color">The color to set to</param>
			/// <returns></returns>
			public TextBuilder Blend(Color color)
			{
				defaultTextBuilderData.Col = [color, color, color, color];
				return this;
			}
			/// <summary>
			/// Sets the default color blend of the text
			/// </summary>
			/// <param name="color">The color blend to set to</param>
			/// <returns></returns>
			public TextBuilder Blend(Color[] color)
			{
				defaultTextBuilderData.Col = color;
				return this;
			}
			/// <summary>
			/// Sets the default font of the text
			/// </summary>
			/// <param name="font">The font to set to</param>
			/// <returns></returns>
			public TextBuilder Font(GLFont font)
			{
				defaultTextBuilderData.CurrentFont = font;
				return this;
			}
			/// <summary>
			/// Defines a custom texture for this <see cref="TextBuilder"/> with the given name
			/// </summary>
			/// <param name="key">The name of the texture to draw in-line</param>
			/// <param name="texture">The texture to draw</param>
			/// <returns></returns>
			public TextBuilder DefineTexture(string key, Texture2D texture)
			{
				if (!DefinedTextures.TryAdd(key, texture))
					DefinedTextures[key] = texture;
				return this;
			}
			/// <summary>
			/// Defines a custom texture with the given name
			/// </summary>
			/// <param name="key">The name of the texture to draw in-line</param>
			/// <param name="texture">The texture to draw</param>
			/// <returns></returns>
			public TextBuilder DefineGlobalTexture(string key, Texture2D texture)
			{
				if (!GlobalDefinedTextures.TryAdd(key, texture))
					GlobalDefinedTextures[key] = texture;
				return this;
			}
			/// <summary>
			/// Defines a custom font for this <see cref="TextBuilder"/> with the given name
			/// </summary>
			/// <param name="key">The name of the font to draw in-line</param>
			/// <param name="font">The font to draw</param>
			/// <returns></returns>
			public TextBuilder DefineFont(string key, GLFont font)
			{
				if (!DefinedFonts.TryAdd(key, font))
					DefinedFonts[key] = font;
				return this;
			}
			/// <summary>
			/// Defines a custom font with the given name
			/// </summary>
			/// <param name="key">The name of the font to draw in-line</param>
			/// <param name="font">The font to draw</param>
			/// <returns></returns>
			public TextBuilder DefineGlobalFont(string key, GLFont font)
			{
				if (!GlobalDefinedFonts.TryAdd(key, font))
					GlobalDefinedFonts[key] = font;
				return this;
			}
			public TextBuilder Wrap(float lineWidth, bool ignoreSpace = false)
			{
				forceMaxLineWidth = lineWidth;
				wrapIgnoreSpace = ignoreSpace;
				return this;
			}
			/// <summary>
			/// Gets the <see cref="MiscUtil.TextTyper"/> of this instance
			/// </summary>
			/// <returns></returns>
			public TextTyper TextTyper() => Typer;
			/// <summary>
			/// Sets the text typer
			/// </summary>
			/// <param name="typer">The typer to set to</param>
			/// <returns></returns>
			public TextBuilder TextTyper(TextTyper typer)
			{
				Typer = typer;
				return this;
			}
			/// <summary>
			/// Renders the text if <see cref="AutoRender"/> is false
			/// </summary>
			public void Render() => DoRender = true;
			#endregion
			public override void Dispose()
			{
				base.Dispose();
				Typer?.Dispose();
			}
		}
		/// <summary>
		/// A text typer class associated with <see cref="TextBuilder"/>
		/// </summary>
		public class TextTyper : Entity
		{
			/// <summary>
			/// A text typer class associated with <see cref="TextBuilder"/>
			/// </summary>
			public TextTyper()
			{
				UpdateIn120 = true;
				GameStates.InstanceCreate(this);
			}
			#region Variables
			/// <summary>
			/// The action to execute whenever a char is updated to be displayed
			/// </summary>
			internal Action ActionPerChar;
			/// <summary>
			/// The sound to play when a char is displayed
			/// </summary>
			internal SoundEffect SoundPerChar;
			/// <summary>
			/// The typing speed of the typer
			/// </summary>
			internal float TypingSpeed = 1;
			/// <summary>
			/// The timer left for delaying the typer
			/// </summary>
			internal float DelayTimer = 0;
			#endregion
			public override void Draw() { }
			public override void Update()
			{
				if (DelayTimer > 0)
				{
					DelayTimer = MathF.Max(0, --DelayTimer);
				}
			}
			#region User callable functions
			/// <summary>
			/// Gets the current typing speed per second
			/// </summary>
			/// <returns>The typing speed per second</returns>
			public float Speed() => TypingSpeed * 62.5f;
			/// <summary>
			/// Sets the amount of characters to reveal per second
			/// </summary>
			/// <param name="speed">The typing speed to set to</param>
			/// <returns>The typing speed per second</returns>
			public TextTyper Speed(float speed)
			{
				TypingSpeed = speed / 62.5f;
				return this;
			}
			/// <summary>
			/// Sets the action per char into the defined action
			/// </summary>
			/// <param name="action">The action to invoke</param>
			/// <returns></returns>
			public TextTyper SetActionPerChar(Action action)
			{
				ActionPerChar = action;
				return this;
			}
			/// <summary>
			/// Sets the sound per char into the defined audio asset
			/// </summary>
			/// <param name="sound">The sound to play</param>
			/// <returns></returns>
			public TextTyper SetSoundPerChar(SoundEffect sound)
			{
				SoundPerChar = sound;
				return this;
			}
			/// <summary>
			/// Delays the typer by the given frames (Incremental)
			/// </summary>
			/// <param name="time">The frames to delay</param>
			/// <returns></returns>
			public TextTyper Delay(float time)
			{
				DelayTimer += time;
				return this;
			}
			/// <summary>
			/// Delays the typer by the given frames (Set)
			/// </summary>
			/// <param name="time">The frames to delay</param>
			/// <returns></returns>
			public TextTyper SetDelay(float time)
			{
				DelayTimer = time;
				return this;
			}
			#endregion
		}
		#endregion
	}
}
