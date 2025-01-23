using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class of <see cref="FightResources.Shaders.RadialWave"/>
            /// </summary>
            public class RadialWaveShader : Shader
            {
                /// <summary>
                /// Shader class of <see cref="FightResources.Shaders.RadialWave"/>
                /// </summary>
                public RadialWaveShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["iCenter"].SetValue(Centre);
                        x.Parameters["iRadius"].SetValue(Radius);
                        x.Parameters["iProgress"].SetValue(Progress);
                    };
                /// <summary>
                /// The progress of the wave, [0, 1]
                /// </summary>
                public float Progress { private get; set; } = 0.0f;
                /// <summary>
                /// The radius of the wave
                /// </summary>
                public float Radius { private get; set; } = 490.0f;
                /// <summary>
                /// The centre of the wave
                /// </summary>
                public Vector2 Centre { private get; set; } = new Vector2(320, 240);
            }
        }
    }
}