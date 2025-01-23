using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using static UndyneFight_Ex.Settings.SettingsManager;

namespace UndyneFight_Ex.Settings
{
    public static class SettingsResetInterface
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SaveSettings(IEnumerable<Setting> e)
        {
            foreach (Setting sel in e)
            {
                sel.Save();
            }
            PlayerManager.Save();
            ApplySettings();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplySettings()
        {
            MediaPlayer.Volume = SoundEffect.MasterVolume = MathF.Pow(DataLibrary.masterVolume / 100f, 2);

            GameMain.ResetRendering();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnterSettings() => SoundEffect.MasterVolume = 1.0f;
    }
}