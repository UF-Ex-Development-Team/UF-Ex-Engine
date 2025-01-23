using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.GameStates.KeyChecker;

namespace UndyneFight_Ex
{
    public static partial class GameStates
    {
        #region keys
        internal class KeyChecker
        {
            public KeyChecker()
            {
                allCheckers.Add(this);
                foreach (KeyValuePair<InputIdentity, List<Keys>> kvp in InputKeys)
                {
                    IdentityChecker singleChecker = new();
                    singleChecker.ResetKeyList(kvp.Value);
                    _identityCheckers.Add(kvp.Key, singleChecker);
                }
            }
            public void Update(KeyboardState keyboardState)
            {
                for (int i = 0; i < _identityCheckers.Count; i++)
                    _identityCheckers.ElementAt(i).Value.Update(keyboardState);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool IsKeyDown(InputIdentity identity) => identity != InputIdentity.None && _identityCheckers[identity].IsKeyDown();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool IsKeyPressed(InputIdentity identity) => identity != InputIdentity.None && _identityCheckers[identity].IsKeyPressed();
            readonly Dictionary<InputIdentity, IdentityChecker> _identityCheckers = [];
            public readonly static Dictionary<InputIdentity, List<Keys>> DefaultKeys = [];

            static KeyChecker()
            {
                InputKeys.Add(InputIdentity.Confirm, [Keys.Enter, Keys.Z]);
                InputKeys.Add(InputIdentity.Cancel, [Keys.LeftShift, Keys.X]);
                InputKeys.Add(InputIdentity.Alternate, [Keys.Space]);
                InputKeys.Add(InputIdentity.Special, [Keys.C]);
                InputKeys.Add(InputIdentity.MainRight, [Keys.Right, Keys.OemSemicolon]);
                InputKeys.Add(InputIdentity.MainDown, [Keys.Down, Keys.L]);
                InputKeys.Add(InputIdentity.MainLeft, [Keys.Left, Keys.K]);
                InputKeys.Add(InputIdentity.MainUp, [Keys.Up, Keys.O]);
                InputKeys.Add(InputIdentity.SecondRight, [Keys.D]);
                InputKeys.Add(InputIdentity.SecondDown, [Keys.S]);
                InputKeys.Add(InputIdentity.SecondLeft, [Keys.A]);
                InputKeys.Add(InputIdentity.SecondUp, [Keys.W]);
                InputKeys.Add(InputIdentity.ThirdRight, [Keys.B]);
                InputKeys.Add(InputIdentity.ThirdDown, [Keys.V]);
                InputKeys.Add(InputIdentity.ThirdLeft, [Keys.C]);
                InputKeys.Add(InputIdentity.ThirdUp, [Keys.F]);
                InputKeys.Add(InputIdentity.FourthRight, [Keys.OemComma]);
                InputKeys.Add(InputIdentity.FourthDown, [Keys.M]);
                InputKeys.Add(InputIdentity.FourthLeft, [Keys.N]);
                InputKeys.Add(InputIdentity.FourthUp, [Keys.J]);
                InputKeys.Add(InputIdentity.FullScreen, [Keys.F4]);
                InputKeys.Add(InputIdentity.ScreenShot, [Keys.F12]);
                InputKeys.Add(InputIdentity.Number1, [Keys.D1]);
                InputKeys.Add(InputIdentity.Number2, [Keys.D2]);
                InputKeys.Add(InputIdentity.Number3, [Keys.D3]);
                InputKeys.Add(InputIdentity.Number4, [Keys.D4]);
                InputKeys.Add(InputIdentity.Number5, [Keys.D5]);
                InputKeys.Add(InputIdentity.Number6, [Keys.D6]);
                InputKeys.Add(InputIdentity.Number7, [Keys.D7]);
                InputKeys.Add(InputIdentity.Number8, [Keys.D8]);
                InputKeys.Add(InputIdentity.Number9, [Keys.D9]);
                InputKeys.Add(InputIdentity.Number0, [Keys.D0]);
                InputKeys.Add(InputIdentity.Backspace, [Keys.Back]);
                InputKeys.Add(InputIdentity.Reset, [Keys.R]);
                InputKeys.Add(InputIdentity.Heal, [Keys.H]);
                InputKeys.Add(InputIdentity.Tab, [Keys.Tab]);
                InputKeys.Add(InputIdentity.QuickRestart, [Keys.F2]);
                DefaultKeys = new(InputKeys);

                //Check user keybinds
                if (PlayerManager.CurrentUser != null)
                    InputKeys = new(KeybindData.UserKeys);
            }
            private static readonly List<KeyChecker> allCheckers = [];

            public static Dictionary<InputIdentity, List<Keys>> InputKeys { get; set; } = [];
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetIdentityKey(InputIdentity identity, List<Keys> mission)
            {
                InputKeys[identity] = mission;
                allCheckers.ForEach(s => s._identityCheckers[identity].ResetKeyList(mission));
            }
        }
        public static bool WordsChanged { get; private set; } = false;
        public static char CharInput { get; private set; }
#if DEBUG
        public static float KeyCheckTime1 { get; private set; }
        public static float KeyCheckTime2 { get; private set; }
#endif
        internal static KeyboardState currentKeyState2;
        internal static KeyboardState lastKeyState2;

        private static readonly KeyChecker checker120f = new(), checker = new();

        /// <summary>
        /// 一个键盘操作录制器(播放)或者是一个键盘操作控制器(回放)
        /// </summary>
        private static Entity keyEventBuffer;

        internal static char KeysUpdate()
        {
            KeyboardState currentKeyState;
            currentKeyState = Keyboard.GetState();

            bool shift_pressed = currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < 256; i++)
            {
                Keys t = (Keys)i;
                if (IsKeyPressed120f(t))
                {
                    WordsChanged = true;
                    //Letter keys
                    if (i < 91 && i > 64)
                    {
                        return (char)((shift_pressed ? 0 : 32) + i);
                    }
                    else if (i > 47 && i < 58)
                        return (char)i;
                    else
                        switch (i)
                        {
                            case 188:
                                return shift_pressed ? '<' : ',';
                            case 189:
                                return shift_pressed ? '_' : '-';
                            case 190:
                                return shift_pressed ? '>' : '.';
                            case 187:
                                return shift_pressed ? '+' : '=';
                            case 191:
                                return shift_pressed ? '?' : '/';
                            case 186:
                                return shift_pressed ? ':' : ';';
                            case 0x20:
                                return (char)0x20;
                            case 13:
                                return (char)13;
                        }
                }
            }
            WordsChanged = false;
            return (char)1;
        }
        internal static void KeysUpdate2()
        {
#if DEBUG
            Stopwatch stopwatch = new();
            stopwatch.Start();
#endif
            lastKeyState2 = currentKeyState2;
            currentKeyState2 = Keyboard.GetState();

#if DEBUG
            if (IsKeyDown(Keys.LeftControl))
            {
                if (IsKeyPressed120f(Keys.D1))
                    GameMain.GameSpeed = 0.02f;
                if (IsKeyPressed120f(Keys.D2))
                    GameMain.GameSpeed = 0.05f;
                if (IsKeyPressed120f(Keys.D3))
                    GameMain.GameSpeed = 0.1f;
                if (IsKeyPressed120f(Keys.D4))
                    GameMain.GameSpeed = 0.25f;
                if (IsKeyPressed120f(Keys.D5))
                    GameMain.GameSpeed = 0.5f;
                if (IsKeyPressed120f(Keys.D6))
                    GameMain.GameSpeed = 0.7f;
                if (IsKeyPressed120f(Keys.Y))
                    GameMain.GameSpeed = 0.85f;
                if (IsKeyPressed120f(Keys.D7))
                    GameMain.GameSpeed = 1f;
                if (IsKeyPressed120f(Keys.D8))
                    GameMain.GameSpeed = 1.5f;
                if (IsKeyPressed120f(Keys.D9))
                    GameMain.GameSpeed = 2f;
            }
            if (IsKeyPressed120f(Keys.H) && CurrentScene is FightScene)
                (CurrentScene as FightScene).PlayerInstance.hpControl.Regenerate();
#endif  
            if (isInBattle)
            {
                if (IsKeyPressed120f(InputIdentity.QuickRestart))
                    EndFight();
                keyEventBuffer?.Update();
            }

            if (GameMain.Update120F)
                checker.Update(currentKeyState2);
            checker120f.Update(currentKeyState2);

#if DEBUG

            KeyCheckTime2 = (float)stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Stop();
#endif
        }
        /// <summary>
        /// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is false)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is pressed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed(InputIdentity key) => checker.IsKeyPressed(key);
        /// <summary>
        /// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is pressed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed120f(InputIdentity key) => checker120f.IsKeyPressed(key);
        /// <summary>
        /// Check if a key is being held
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is held</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyDown(InputIdentity key) => checker120f.IsKeyDown(key);
        /// <summary>
        /// Check if a key is pressed (Used when <see cref="GameObject.UpdateIn120"/> is true)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is pressed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed120f(Keys key) => currentKeyState2.IsKeyDown(key) && lastKeyState2.IsKeyUp(key);
        /// <summary>
        /// Check if a key is being held
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether the key is held</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyDown(Keys key) => currentKeyState2.IsKeyDown(key);
        #endregion
    }
    internal class KeybindData : ISaveLoad
    {
        public static Dictionary<InputIdentity, List<Keys>> UserKeys { get; set; } = new(DefaultKeys);
        public List<ISaveLoad> Children => throw new NotImplementedException();

        public void Load(SaveInfo info)
        {
            UserKeys.Clear();
            foreach (var Identity in DefaultKeys.Keys)
            {
                info.Nexts.TryGetValue(Identity.ToString(), out SaveInfo value);
                List<Keys> finKey = [];
                if (value is null)
                    finKey = DefaultKeys[Identity];
                else
                {
                    foreach (string keyString in value.StringValue.Split(','))
                        finKey.Add(MiscUtil.StringToKey(keyString));
                }
                UserKeys.TryAdd(Identity, finKey);
            }
            InputKeys = new(UserKeys);
        }
        public SaveInfo Save()
        {
            UserKeys ??= new(DefaultKeys);
            SaveInfo info = new("Keybinds{");
            foreach (var Identity in DefaultKeys.Keys)
            {
                string finText = string.Empty;
                foreach (Keys finKey in UserKeys[Identity])
                    finText += MiscUtil.KeyToString(finKey) + ",";
                finText = finText[..^1];
                info.PushNext(new SaveInfo($"{Identity}:{finText}"));
            }
            return info;
        }
    }
}