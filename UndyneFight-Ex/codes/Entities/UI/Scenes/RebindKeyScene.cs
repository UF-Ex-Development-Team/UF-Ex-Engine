using Microsoft.Xna.Framework.Input;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// v0.3.0+ Rebinding scene
    /// </summary>
    public class RebindKeyScene : Scene
    {
        private Dictionary<InputIdentity, List<Keys>> InputKeys;
        private Dictionary<InputIdentity, string> KeyNames;
        private List<Keys> UsedKeys, KeysHeldBeforeBinding;
        //I know, it's a holy table, but it has to be this way
        private static readonly HashSet<Keys> ForbiddenKeys = [Keys.Escape, Keys.CapsLock, Keys.LeftWindows, Keys.RightWindows, Keys.Apps, Keys.Sleep, Keys.Separator, Keys.BrowserBack, Keys.BrowserFavorites, Keys.BrowserForward, Keys.BrowserHome, Keys.BrowserRefresh, Keys.BrowserSearch, Keys.BrowserStop, Keys.VolumeMute, Keys.VolumeDown, Keys.VolumeUp, Keys.MediaNextTrack, Keys.MediaNextTrack, Keys.MediaPlayPause, Keys.MediaStop, Keys.MediaStop, Keys.SelectMedia, Keys.LaunchMail, Keys.LaunchApplication1, Keys.LaunchApplication2, Keys.Pause, Keys.Scroll, Keys.PrintScreen];
        private static readonly HashSet<InputIdentity> debugKeys = [InputIdentity.Number0, InputIdentity.Number1, InputIdentity.Number2, InputIdentity.Number3, InputIdentity.Number4, InputIdentity.Number5, InputIdentity.Number6, InputIdentity.Number7, InputIdentity.Number8, InputIdentity.Number9, InputIdentity.Heal, InputIdentity.Tab, InputIdentity.Special, InputIdentity.Backspace];
        private int curSelection = 0, keysCount, arrowHeld = 0, state = 0, RKeyTimer = 0;
        private float scrollY = 175, scrollYTar = 175, bindAlpha = 0;
        private bool moreBindSelection;
        private string BindingDisplayText;
        private float[] textScale, textScaleTar;
        private enum BindingState
        {
            Choosing = 0,
            Binding = 1,
            ConfirmMoreBind = 2,
            BindMore = 3
        }
        public RebindKeyScene() => Initialize();
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void Initialize()
        {
            InputKeys = KeyChecker.InputKeys;
            KeyNames = [];
            UsedKeys = [];
            for (int i = 0; i < InputKeys.Keys.Count; i++)
            {
                if (debugKeys.Contains(InputKeys.Keys.ElementAt(i)))
                    continue;
                string FinTxt = string.Empty;
                foreach (Keys item in InputKeys.Values.ElementAt(i))
                {
                    FinTxt += MiscUtil.KeyToString(item) + ", ";
                    UsedKeys.Add(item);
                }
                KeyNames.Add(InputKeys.Keys.ElementAt(i), FinTxt[..^2]);
                keysCount = KeyNames.Keys.Count;
            }
            textScale = new float[keysCount];
            textScaleTar = new float[keysCount];
            for (int i = 0; i < keysCount; i++)
            {
                textScale[i] = 1;
                textScaleTar[i] = 1;
            }
        }
        public override void Update()
        {
            if (RKeyTimer > 0)
                RKeyTimer--;
            textScaleTar[curSelection] = 1.2f;
            for (int i = 0; i < textScaleTar.Length; i++)
                textScale[i] = MathHelper.Lerp(textScale[i], textScaleTar[i], 0.14f);
            if (state == (int)BindingState.Choosing)
            {
                if (IsKeyPressed120f(Keys.R))
                {
                    if (RKeyTimer == 0)
                        RKeyTimer = 30;
                    else
                    {
                        RKeyTimer = 0;
                        InputKeys = new(KeyChecker.DefaultKeys);
                        for (int i = 0; i < InputKeys.Count; i++)
                            KeyChecker.SetIdentityKey(InputKeys.Keys.ElementAt(i), KeyChecker.DefaultKeys.Values.ElementAt(i));
                        Initialize();
                        FightResources.Sounds.damaged.CreateInstance().Play();
                    }
                }
                if (IsKeyPressed(InputIdentity.Cancel))
                {
                    if (PlayerManager.CurrentUser != null)
                    {
                        KeybindData.UserKeys = new(InputKeys);
                        PlayerManager.Save();
                    }
                    ResetScene(new GameMenuScene());
                }
                if (IsKeyDown(InputIdentity.MainDown) || IsKeyDown(InputIdentity.MainUp))
                    arrowHeld++;
                if (!IsKeyDown(InputIdentity.MainDown) && !IsKeyDown(InputIdentity.MainUp))
                    arrowHeld = 0;
                if (IsKeyPressed120f(InputIdentity.MainUp) || (arrowHeld > 60 && arrowHeld % 15 == 0 && IsKeyDown(InputIdentity.MainUp)))
                {
                    FightResources.Sounds.changeSelection.CreateInstance().Play();
                    textScaleTar[curSelection] = 1;
                    curSelection--;
                    curSelection = MathUtil.Posmod(curSelection, keysCount);
                }
                else if (IsKeyPressed120f(InputIdentity.MainDown) || (arrowHeld > 60 && arrowHeld % 15 == 0 && IsKeyDown(InputIdentity.MainDown)))
                {
                    FightResources.Sounds.changeSelection.CreateInstance().Play();
                    textScaleTar[curSelection] = 1;
                    curSelection++;
                    curSelection = MathUtil.Posmod(curSelection, keysCount);
                }
                if (IsKeyPressed120f(Keys.Enter))
                {
                    state = (int)BindingState.Binding;
                    BindingDisplayText = $"Press the key you want to bind to\n{KeyNames.Keys.ElementAt(curSelection)}";
                    KeysHeldBeforeBinding = [.. Keyboard.GetState().GetPressedKeys()];
                    KeyChecker.InputKeys[KeyNames.Keys.ElementAt(curSelection)].ForEach(x => UsedKeys.Remove(x));
                    FightResources.Sounds.select.CreateInstance().Play();
                }
            }
            else if (state == (int)BindingState.Binding)
            {
                foreach (Keys item in KeysHeldBeforeBinding)
                {
                    if (!IsKeyDown(item))
                    {
                        KeysHeldBeforeBinding.Remove(item);
                        break;
                    }
                }
                if (Keyboard.GetState().GetPressedKeyCount() > 0)
                {
                    Keys[] list = Keyboard.GetState().GetPressedKeys();
                    IEnumerable<Keys> finList = list.Except(KeysHeldBeforeBinding);
                    if (!finList.Any())
                        return;
                    Keys firstKey = finList.First();
                    if (ForbiddenKeys.Contains(firstKey))
                        BindingDisplayText = $"This key is forbidden\nPlease choose another key for\n{KeyNames.Keys.ElementAt(curSelection)}";
                    else if (UsedKeys.Contains(firstKey))
                        BindingDisplayText = $"This key is already chosen\nPlease choose another key for\n{KeyNames.Keys.ElementAt(curSelection)}";
                    else
                    {
                        KeyChecker.InputKeys[KeyNames.Keys.ElementAt(curSelection)] = [firstKey];
                        UsedKeys.Add(firstKey);
                        Initialize();
                        KeyChecker.SetIdentityKey(KeyNames.Keys.ElementAt(curSelection), [firstKey]);
                        BindingDisplayText = $"Do you want to bind more keys to {KeyNames.Keys.ElementAt(curSelection)}?";
                        state = (int)BindingState.ConfirmMoreBind;
                        moreBindSelection = false;
                    }
                }
            }
            else if (state == (int)BindingState.ConfirmMoreBind)
            {
                if (IsKeyPressed120f(InputIdentity.MainLeft) || IsKeyPressed120f(InputIdentity.MainRight))
                    moreBindSelection ^= true;
                if (IsKeyPressed120f(InputIdentity.Confirm))
                {
                    state = (int)(moreBindSelection ? BindingState.BindMore : BindingState.Choosing);
                    if (moreBindSelection)
                    {
                        KeysHeldBeforeBinding = [.. Keyboard.GetState().GetPressedKeys()];
                        BindingDisplayText = $"Press the key you want to bind to\n{KeyNames.Keys.ElementAt(curSelection)}";
                    }
                }
            }
            else if (state == (int)BindingState.BindMore)
            {
                if (Keyboard.GetState().GetPressedKeyCount() > 0)
                {
                    Keys[] list = Keyboard.GetState().GetPressedKeys();
                    IEnumerable<Keys> finList = list.Except(KeysHeldBeforeBinding);
                    if (!finList.Any())
                        return;
                    Keys firstKey = finList.First();
                    if (ForbiddenKeys.Contains(firstKey))
                        BindingDisplayText = $"This key is forbidden\nPlease choose another key for\n{KeyNames.Keys.ElementAt(curSelection)}";
                    else if (KeyChecker.InputKeys[KeyChecker.InputKeys.Keys.ElementAt(curSelection)].Contains(firstKey) || UsedKeys.Contains(firstKey))
                        BindingDisplayText = $"This key is already chosen\nPlease choose another key for\n{KeyNames.Keys.ElementAt(curSelection)}";
                    else
                    {
                        KeyChecker.InputKeys[KeyNames.Keys.ElementAt(curSelection)].Add(firstKey);
                        UsedKeys.Add(firstKey);
                        KeyChecker.SetIdentityKey(KeyNames.Keys.ElementAt(curSelection), KeyChecker.InputKeys[KeyNames.Keys.ElementAt(curSelection)]);
                        Initialize();
                        BindingDisplayText = $"Do you want to bind more keys to\n{KeyNames.Keys.ElementAt(curSelection)}?";
                        state = (int)BindingState.ConfirmMoreBind;
                        moreBindSelection = false;
                    }
                }
            }
            scrollY = MathHelper.LerpPrecise(scrollY, scrollYTar, 0.18f);
            bindAlpha = MathHelper.LerpPrecise(bindAlpha, state != 0 ? 0.7f : 0, 0.06f);
            base.Update();
        }
        public override void Draw()
        {
            base.Draw();
            DrawingLab.DrawLine(new Vector2(320, 0), new Vector2(320, 95), 640, Color.Black, 0.1f);
            DrawingLab.DrawLine(new Vector2(320, 92), new Vector2(320, 95), 640, Color.White, 0.2f);
            FightResources.Font.FightFont.CentreDraw("Keyboard\nKeybinds", new Vector2(320, 50), Color.White, 1, 0.2f);
            FightResources.Font.FightFont.CentreDraw("Key name", new Vector2(110, 80), Color.White, 1, 0.2f);
            FightResources.Font.FightFont.CentreDraw("Press Enter\nto change\nkey bind", new Vector2(480, 50), Color.White, 1, 0.2f);
            FightResources.Font.FightFont.CentreDraw("Press R twice\nto reset\nkey binds", new Vector2(80, 30), Color.White, 0.8f, 0.2f);
            for (int i = 0; i < KeyNames.Keys.Count; i++)
            {
                bool selected = i == curSelection;
                col finCol = selected ? Color.Yellow : Color.White;
                if (selected)
                {
                    if (scrollY + i * 40 >= 455)
                        scrollYTar -= 40;
                    else if (scrollY + i * 40 <= 135)
                        scrollYTar += 40;
                    scrollYTar = MathHelper.Clamp(scrollYTar, -965, 135);
                }
                FightResources.Font.NormalFont.CentreDraw(KeyNames.Keys.ElementAt(i).ToString(), new Vector2(110, scrollY + i * 40), finCol, textScale[i], 0);
                FightResources.Font.NormalFont.CentreDraw(KeyNames.Values.ElementAt(i), new Vector2(470, scrollY + i * 40), finCol, textScale[i], 0);
            }
            //Binding fade
            DrawingLab.DrawLine(new Vector2(0, 240), new Vector2(640, 240), 480, Color.Black * bindAlpha, 0.3f);
            //Binding Text
            if (state != (int)BindingState.Choosing)
                FightResources.Font.FightFont.CentreDraw(BindingDisplayText, new Vector2(320, 240), Color.White, 1, 0.4f);
            if (state == (int)BindingState.ConfirmMoreBind)
            {
                FightResources.Font.FightFont.CentreDraw("No", new Vector2(200, 360), moreBindSelection ? Color.White : Color.Yellow, 1, 0.4f);
                FightResources.Font.FightFont.CentreDraw("Yes", new Vector2(440, 360), !moreBindSelection ? Color.White : Color.Yellow, 1, 0.4f);
            }
        }

    }
}