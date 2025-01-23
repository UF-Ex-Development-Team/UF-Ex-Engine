using Microsoft.Xna.Framework.Audio;

namespace UndyneFight_Ex.Fight
{
    /// <summary>
    /// The base class for text attributes, contains
    /// </summary>
    public abstract class TextAttribute
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void Reset(PrintingSettings textPrinter);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void ResetEnd(PrintingSettings textPrinter) { }
    }
    /// <summary>
    /// Invokes an action when the text reaches this point
    /// </summary>
    /// <param name="act">The action to invoke</param>
    public class TextAction(Action act) : TextAttribute
    {
        readonly Action action = act;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => action.Invoke();
    }
    /// <summary>
    /// Moves the text(s) using provided function
    /// </summary>
    /// <param name="act">The easing function</param>
    public class TextMoveAttribute(Func<float, Vector2> act) : TextAttribute
    {
        readonly Func<float, Vector2> action = act;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => textPrinter.charPositionDelta = action.Invoke(textPrinter.CurrentData.restTime);
    }
    /// <summary>
    /// Scales the text(s) by the given scale
    /// </summary>
    /// <param name="scale">The scale of the text</param>
    public class TextSizeAttribute(float scale) : TextAttribute
    {
        readonly float size = scale;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => textPrinter.TextSize = size;
    }
    /// <summary>
    /// Changes the speed of the typing
    /// </summary>
    /// <param name="speed">The speed to change to (Default 20)</param>
    public class TextSpeedAttribute(float speed) : TextAttribute
    {
        private readonly float speed = speed;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => textPrinter.PrintSpeed = speed;
    }
    /// <summary>
    /// Pauses the typer for the given duration of time
    /// </summary>
    /// <param name="time">The time to pause</param>
    public class TextTimeThreshold(float time) : TextAttribute
    {
        private readonly float time = time;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter)
        {
            if (textPrinter.CurrentData.totalTime < time)
                textPrinter.PrintSpeed = 0;
            else
                textPrinter.CurrentData.restTime = textPrinter.CurrentData.totalTime - time;
        }
    }
    public class TextCharMovingAttribute(Func<float, Vector2> movingFunc) : TextAttribute
    {
        private readonly Func<float, Vector2> movingFunc = movingFunc;
        private Vector2 del;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter)
        {
            del = movingFunc.Invoke(textPrinter.CurrentData.restTime);
            textPrinter.charPositionDelta += del;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ResetEnd(PrintingSettings textPrinter) => textPrinter.charPositionDelta -= del;
    }
    /// <summary>
    /// Toggles the blom effect of the text
    /// </summary>
    /// <param name="enabled">Whether to enable bloom or not</param>
    public class TextGleamAttribute(bool enabled) : TextAttribute
    {
        private readonly bool enabled = enabled;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => textPrinter.LightEnabled = enabled;
    }
    /// <summary>
    /// Sets the color of the text to the given color
    /// </summary>
    /// <param name="color">The color to set to</param>
    public class TextColorAttribute(Color color) : TextAttribute
    {
        private Color color = color;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter) => textPrinter.textColor = color;
    }
    /// <summary>
    /// Fades the text after a time delay in the given duration
    /// </summary>
    /// <param name="delay">The delay before the text fades</param>
    /// <param name="duration">The duration of the fading</param>
    public class TextFadeoutAttribute(float delay, float duration) : TextAttribute
    {
        readonly float delay = delay, duration = duration;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter)
        {
            float t = textPrinter.CurrentData.restTime;
            if (t <= delay)
                return;
            float val = MathF.Max(0, (delay + duration - t) / duration);
            if (val == 0)
                textPrinter.ShouldDispose = true;
            textPrinter.TextColorAlpha = val;
        }
    }
    /// <summary>
    /// A <see cref="TextAttribute"/> that combines several <see cref="TextAttribute"/>s, you can use this to avoid using multiple $
    /// </summary>
    /// <param name="textAttributes">The <see cref="TextAttribute"/>s to combine</param>
    public class AttributeSet(params TextAttribute[] textAttributes) : TextAttribute
    {
        private readonly TextAttribute[] textAttributes = textAttributes;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Reset(PrintingSettings textPrinter)
        {
            for (int i = 0; i < textAttributes.Length; i++)
                textAttributes[i].Reset(textPrinter);
        }
    }

    public class PrintingSettings
    {
        public bool LightEnabled { get; set; } = false;
        public float PrintSpeed { get; set; } = 20;
        public float TextSize { get; set; } = 1.0f;
        public bool ShouldDispose { get; set; } = false;
        public float TextColorAlpha { get; set; } = 1.0f;
        public Color textColor = Color.White;
        public Vector2 charPositionDelta = Vector2.Zero;
        /// <summary>
        /// The font for the text (Default <see cref="FightResources.Font.NormalFont"/>)
        /// </summary>
        public GLFont renderFont = FightResources.Font.NormalFont;
        /// <summary>
        /// The printing sound of the typer (Default <see cref="FightResources.Sounds.printWord"/>)
        /// </summary>
        public SoundEffect printSound = FightResources.Sounds.printWord;

        public class CurrentPrintingData
        {
            public float restTime;
            public float totalTime;
        }
        public CurrentPrintingData CurrentData { get; set; } = new();
    }
    /// <summary>
    /// The text typing entity
    /// </summary>
    public class TextPrinter : Entity
    {
        /// <summary>
        /// Instantly ends the text typer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InstantEnd()
        {
            if (appearTime < ForceTime)
                return;
            appearTime = 0x3f3f3f3f;
            maxPosition = text.Length - 1;
            Dispose();
        }
        /// <summary>
        /// Whether the text typer has displayed all it's text
        /// </summary>
        public bool AllShowed => maxPosition >= text.Length - 1 && (ForceTime == -1 || appearTime >= ForceTime);

        private readonly string text;
        private float appearTime;
        /// <summary>
        /// Whether the text typer plays the typing text
        /// </summary>
        public bool PlaySound { get; set; } = true;
        private int maxPosition = 0;
        /// <summary>
        /// The distance between the lines for each line break (Default 40 pixels)
        /// </summary>
        public float LinesDistance { private get; set; } = 40;
        /// <summary>
        /// Whether the texts are alligned to the centre (WIP)
        /// </summary>
        public bool CentreDraw { get; set; } = false;
        public float ForceTime { private set; get; } = -1;
        /// <summary>
        /// The position of the text typer
        /// </summary>
        public Vector2 Position { get; set; }
        private readonly TextAttribute[] textAttributes;
        /// <summary>
        /// Creates a text printer with a given duration
        /// </summary>
        /// <param name="forceTime">The time before the text typer disposes</param>
        /// <param name="text">The text to draw</param>
        /// <param name="position">The position of the text typer</param>
        /// <param name="textAttributes">The attributes of the text</param>
        public TextPrinter(float forceTime, string text, Vector2 position, params TextAttribute[] textAttributes)
        {
            UpdateIn120 = true;
            Depth = 0.5f;
            ForceTime = forceTime;
            this.text = text;
            Position = position;
            this.textAttributes = textAttributes;
        }
        /// <summary>
        /// Creates a text printer with a given duration
        /// </summary>
        /// <param name="forceTime">The time before the text typer disposes</param>
        /// <param name="text">The text to draw</param>
        /// <param name="textAttributes">The attributes of the text</param>
        public TextPrinter(float forceTime, string text, params TextAttribute[] textAttributes)
        {
            UpdateIn120 = true;
            Depth = 0.5f;
            ForceTime = forceTime;
            this.text = text;
            this.textAttributes = textAttributes;
        }
        /// <summary>
        /// Creates a text typer
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="position">The position of the text typer</param>
        /// <param name="textAttributes">The attributes of the text</param>
        public TextPrinter(string text, Vector2 position, params TextAttribute[] textAttributes)
        {
            UpdateIn120 = true;
            Depth = 0.5f;
            this.text = text;
            Position = position;
            this.textAttributes = textAttributes;
        }
        /// <summary>
        /// Creates a text typer
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="textAttributes">The attributes of the text</param>
        public TextPrinter(string text, params TextAttribute[] textAttributes)
        {
            UpdateIn120 = true;
            Depth = 0.5f;
            this.text = text;
            this.textAttributes = textAttributes;
        }
        public override void Draw()
        {
            PrintingSettings printingSettings = new();
            printingSettings.CurrentData.totalTime = appearTime;
            float rest = appearTime;
            int attributeCount = 0;
            bool soundPlayed = false;
            Vector2 currentPosition = Position;
            float maxSize = 0.0f;
            for (int i = 0; i < text.Length; i++)
            {
                if (rest <= 0)
                    break;
                printingSettings.CurrentData.restTime = rest;
                if (text[i] == '$')
                {
                    textAttributes[attributeCount].Reset(printingSettings);
                    rest = printingSettings.CurrentData.restTime;
                    if (printingSettings.PrintSpeed == 0)
                        break;
                    attributeCount++;
                    if (printingSettings.ShouldDispose)
                    {
                        Dispose();
                        return;
                    }
                    continue;
                }
                if (text[i] == '#')
                {
                    string s = "";
                    i++;
                    while (text[i] != '#')
                    {
                        s += text[i];
                        i++;
                    }
                    switch (s)
                    {
                        case "sans":
                            printingSettings.TextSize = 0.8f;
                            printingSettings.renderFont = FightResources.Font.SansFont;
                            printingSettings.textColor = Color.White;
                            printingSettings.printSound = FightResources.Sounds.sansWord;
                            break;
                        case "Japanese":
                            printingSettings.TextSize = 0.8f;
                            printingSettings.renderFont = FightResources.Font.Japanese;
                            printingSettings.textColor = Color.White;
                            break;

                        case "enemy":
                            printingSettings.TextSize = 0.65f;
                            printingSettings.textColor = Color.Black;
                            break;
                    }
                    continue;
                }
                if (text[i] is '\r' or '\n')
                {
                    currentPosition = new Vector2(Position.X, currentPosition.Y + LinesDistance * maxSize);
                    maxSize = 0.0f;
                    continue;
                }
                if (i > maxPosition)
                {
                    if (text[i] != ' ' && (!soundPlayed))
                    {
                        soundPlayed = true;
                        if (PlaySound)
                            Functions.PlaySound(printingSettings.printSound);
                    }
                    maxPosition = i;
                }
                maxSize = MathF.Max(maxSize, printingSettings.TextSize);

                Color col = printingSettings.textColor * printingSettings.TextColorAlpha;
                if (!CentreDraw)
                    printingSettings.renderFont.Draw(text[i] + "", currentPosition + printingSettings.charPositionDelta, col, printingSettings.TextSize, Depth);
                else
                    printingSettings.renderFont.CentreDraw(text[i] + "", currentPosition + printingSettings.charPositionDelta - printingSettings.renderFont.SFX.MeasureString(text) / 2, col, printingSettings.TextSize, Depth);
                if (printingSettings.LightEnabled)
                {
                    Vector2 textdel = printingSettings.renderFont.SFX.MeasureString(text[i].ToString());
                    textdel = MathUtil.Rotate(textdel, Rotation);

                    for (int x = 0; x <= 4; x++)
                    {
                        if (!CentreDraw)
                            printingSettings.renderFont.Draw(text[i] + "", currentPosition + printingSettings.charPositionDelta - textdel * (x + 1) * 0.07f, col * (0.5f - x * 0.1f), printingSettings.TextSize * (1.14f + x * 0.14f), Depth);
                        else
                            printingSettings.renderFont.CentreDraw(text[i] + "", currentPosition + printingSettings.charPositionDelta - printingSettings.renderFont.SFX.MeasureString(text) / 2 - textdel * (x + 1) * 0.07f, col * (0.5f - x * 0.1f), printingSettings.TextSize * (1.14f + x * 0.14f), Depth);
                    }
                }

                Vector2 size = printingSettings.renderFont.SFX.MeasureString(text[i] + "");
                currentPosition += MathUtil.Rotate(new Vector2(size.X * printingSettings.TextSize, 0), Rotation);

                rest -= 60 / printingSettings.PrintSpeed;
                if (text[i] == '$')
                    textAttributes[attributeCount].ResetEnd(printingSettings);
            }
        }

        public override void Update()
        {
            appearTime += 0.5f;
            if (ForceTime != -1 && appearTime >= ForceTime)
                InstantEnd();
        }
    }

    public class DialogBox : Entity
    {
        public Action AfterDispose { private get; set; }

        private readonly TextPrinter[] textPrinters;
        private Vector2 location;

        public DialogBox(Vector2 position, TextPrinter[] textPrinters)
        {
            Image = FightResources.FightSprites.dialogBox;
            this.textPrinters = textPrinters;
            foreach (TextPrinter v in this.textPrinters)
            {
                v.LinesDistance = 38;
                v.Position = position + new Vector2(36, 10);
            }
            location = position;
        }
        public DialogBox(Vector2 location, string text, params TextAttribute[] textAttributes) : this(location, [new TextPrinter(text, textAttributes)]) { }

        private int currentProgress = 0;

        public override void Draw()
        {
            if (currentProgress >= textPrinters.Length)
                return;
            textPrinters[currentProgress].Draw();
            FormalDraw(Image, location, Color.White, 0, Vector2.Zero);
        }

        public override void Update()
        {
            while (!Settings.SettingsManager.DataLibrary.dialogAvailable && textPrinters[currentProgress].ForceTime != -1)
            {
                textPrinters[currentProgress].InstantEnd();
                currentProgress++;
                return;
            }
            if (currentProgress < textPrinters.Length)
                textPrinters[currentProgress].Update();
            if (GameStates.IsKeyPressed(InputIdentity.Cancel))
                textPrinters[currentProgress].InstantEnd();
            if (textPrinters[currentProgress].AllShowed && GameStates.IsKeyPressed(InputIdentity.Confirm))
                currentProgress++;
            else if (textPrinters[currentProgress].AllShowed && textPrinters[currentProgress].ForceTime != -1)
                currentProgress++;
            if (currentProgress >= textPrinters.Length)
                Dispose();
        }

        public override void Dispose()
        {
            AfterDispose?.Invoke();
            base.Dispose();
        }
    }
    public class BoxMessage : Entity
    {
        public Action AfterDispose { private get; set; }

        private readonly TextPrinter[] textPrinters;
        public BoxMessage(TextPrinter[] textPrinters)
        {
            this.textPrinters = textPrinters;
            foreach (TextPrinter v in this.textPrinters)
                if (v.Position == Vector2.Zero)
                    v.Position = new Vector2(40, 256);
        }
        public BoxMessage(string text, params TextAttribute[] textAttributes) : this([new TextPrinter(text, new Vector2(40, 256), textAttributes)]) { }

        private int currentProgress = 0;

        public override void Draw()
        {
            if (currentProgress < textPrinters.Length)
                textPrinters[currentProgress].Draw();
        }

        public override void Update()
        {
            textPrinters[currentProgress].Update();
            if (GameStates.IsKeyPressed(InputIdentity.Cancel))
                textPrinters[currentProgress].InstantEnd();
            if (textPrinters[currentProgress].AllShowed && GameStates.IsKeyPressed(InputIdentity.Confirm))
                currentProgress++;
            if (currentProgress == textPrinters.Length)
                Dispose();
        }

        public override void Dispose()
        {
            AfterDispose?.Invoke();
            base.Dispose();
        }
    }
}