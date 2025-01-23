using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class for <see cref="FightResources.Shaders.Wrong"/>
            /// </summary>
            public class WrongShader : Shader
            {
                /// <summary>
                /// The time of the shader
                /// </summary>
                public float Time { get; set; } = 0;
                /// <summary>
                /// The intensity of the effect
                /// </summary>
                public float Intensity { get; set; } = 0;
                /// <summary>
                /// Shader class for <see cref="FightResources.Shaders.Wrong"/>
                /// </summary>
                public WrongShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["iTime"].SetValue(Time += 0.01f);
                        x.Parameters["iValue"].SetValue(Intensity);
                    };
            }
        }
    }
}