using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.Entities
{
    internal class RecordSelector : Selector
    {
        private class TextSelectionEx(string texts, Vector2 Centre) : Entity, ISelectAble
        {
            private static Vector2 missionDelta;
            private static Vector2 currentDelta;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void MoveRight() => missionDelta += new Vector2(-640, 0);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void MoveLeft() => missionDelta += new Vector2(640, 0);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Reset() => missionDelta = currentDelta = Vector2.Zero;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Move() => currentDelta = currentDelta * 0.8f + missionDelta * 0.2f;

            public class ShinyTextEffect : Entity
            {
                private readonly string texts;
                private float alpha = 0.0f, size;
                private Color showingColor;

                public ShinyTextEffect(TextSelectionEx s)
                {
                    Centre = s.Centre;
                    size = s.size * s.currentSize;
                    alpha = s.alpha;
                    texts = s.texts;
                    showingColor = Color.Lerp(s.showingColor, Color.Gold, 0.5f);
                }
                public override void Draw() => FightResources.Font.NormalFont.CentreDraw(texts, Centre, showingColor * alpha, size, 0.9f);

                public override void Update()
                {
                    collidingBox.Y -= 0.1f + alpha * 0.4f;
                    alpha *= 0.9f;
                    alpha -= 0.03f;
                    size += ((2 - alpha) / 40f + 0.04f) / 1.6f;
                    if (alpha < 0)
                        Dispose();
                }
            }

            private Vector2 startCentre = Centre;
            protected string texts = texts;
            private float alpha = 0.0f;
            private float currentSize = 1.0f;
            private float maxSize = 1.35f;
            private const float sizeChangeSpeed = 0.18f;
            private bool isSelected;

            public float MaxSize
            {
                set => maxSize = value;
            }
            public float Size
            {
                set => size = value;
            }
            public Color SetColor
            {
                set => showingColor = value;
            }
            private float size = 0.8f;
            private Color showingColor = Color.White;

            private Action action;
            public Action SetSelectionAction
            {
                set => action = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DeSelected() => isSelected = false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void Draw() => FightResources.Font.NormalFont.CentreDraw(texts, Centre, showingColor * alpha, currentSize * size, 0.9f);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Selected() => isSelected = true;

            public override void Update()
            {
                if (alpha < 1f)
                    alpha += 0.025f;
                else
                    alpha = 1f;
                currentSize = isSelected
                    ? currentSize * (1 - sizeChangeSpeed) + maxSize * sizeChangeSpeed
                    : currentSize * (1 - sizeChangeSpeed) + sizeChangeSpeed;
                Centre = currentDelta + startCentre;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public virtual void SelectionEvent()
            {
                action?.Invoke();
                GameStates.InstanceCreate(new ShinyTextEffect(this));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadLevel(string path)
        {
            SaveInfo info = IOEvent.ToInfos(IOEvent.ByteToString(IOEvent.ReadCustomFile(path)));
            string fightName = info.Nexts["typeName"].StringValue;
            int difficulty = info.Nexts["difficulty"].IntValue;
            GameStates.seed = info.Nexts["seed"].IntValue;
            //int mode = info.Nexts["mode"].IntValue;
            FightSystem.SelectMainSet();
            Type type = FightSystem.CurrentSongs[fightName];

            IWaveSet tem = Activator.CreateInstance(type) as IWaveSet;
            GameStates.isRecord = false;
            GameStates.isReplay = true;
            string path2 = $"{AppContext.BaseDirectory}Content\\Musics\\";
            if (File.Exists(Path.Combine((path2 + tem.Music + ".xnb").Split('\\'))))
                GameStates.StartSong(tem, null, path2, difficulty, JudgementState.Strict, GameMode.None);
            else
                GameStates.StartSong(tem, null, Path.Combine((path2 + "\\song.xnb").Split('\\')), difficulty, JudgementState.Strict, GameMode.None);
            Replayer.Info = info;
        }

        public RecordSelector()
        {
            TextSelectionEx.Reset();
            SelectChanger += () =>
            {
                if (GameStates.IsKeyPressed120f(InputIdentity.MainUp))
                {
                    currentSelect -= 1;
                    if (currentSelect < 0)
                        currentSelect = 0;
                    if (currentSelect % 6 == 5)
                        TextSelectionEx.MoveLeft();
                }
                else if (GameStates.IsKeyPressed120f(InputIdentity.MainDown))
                {
                    currentSelect += 1;
                    if (currentSelect >= SelectionCount)
                        currentSelect = SelectionCount - 1;
                    if (currentSelect % 6 == 0 && currentSelect != 0)
                        TextSelectionEx.MoveRight();
                }
                else if (GameStates.IsKeyPressed120f(InputIdentity.MainRight))
                {
                    int next = currentSelect + 6;
                    if (next < SelectionCount)
                    {
                        currentSelect = next;
                        TextSelectionEx.MoveRight();
                    }
                }
                else if (GameStates.IsKeyPressed120f(InputIdentity.MainLeft))
                {
                    int next = currentSelect - 6;
                    if (next >= 0)
                    {
                        currentSelect = next;
                        TextSelectionEx.MoveLeft();
                    }
                }
            };

            ResetSelect();
            Fight.Functions.PlaySound(FightResources.Sounds.select, 0.9f);
            SelectChanged += () => Fight.Functions.PlaySound(FightResources.Sounds.changeSelection, 0.9f);

            string[] files = Directory.GetFiles("Datas\\Records");

            int x = 0, y = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string loc = files[i];
                string[] parts = loc.Split('\\');
                string res = parts[^1];
                string[] parts2 = res.Split('.');

                int plc = i;
                TextSelectionEx selection = new(parts2[0], new Vector2(x * 640 + 320, y * 60 + 100))
                {
                    SetSelectionAction = () => LoadLevel(files[plc])
                };
                PushSelection(selection);
                y++;
                if (y == 6)
                {
                    y = 0;
                    x++;
                }
            }
        }

        public override void Update()
        {
            TextSelectionEx.Move();
            base.Update();
        }
    }
}