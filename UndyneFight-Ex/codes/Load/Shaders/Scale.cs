using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class of <see cref="FightResources.Shaders.Scale"/>
            /// </summary>
            public class ScaleShader : Shader
            {
                /// <summary>
                /// Shader class of <see cref="FightResources.Shaders.Scale"/>
                /// </summary>
                public ScaleShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["iPos"].SetValue(Centre);
                        x.Parameters["iValue"].SetValue(Intensity);
                    };
                /// <summary>
                /// The scale intensity
                /// </summary>
                public float Intensity { private get; set; } = 1.0f;
                /// <summary>
                /// The centre of the effect
                /// </summary>
                public Vector2 Centre { get; set; } = new Vector2(320, 240);
            }
        }
    }
}