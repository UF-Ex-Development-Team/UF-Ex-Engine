using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class for <see cref="FightResources.Shaders.Blur"/>
            /// </summary>
            public class BlurShader : Shader
            {
                public BlurShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["iFactor"].SetValue(Factor);
                        x.Parameters["iSigma2"].SetValue(Sigma * Sigma);
                    };

                public Vector2 Factor { private get; set; } = Vector2.Zero;
                /// <summary>
                /// The intensity of the blue
                /// </summary>
                public float Sigma { get; set; } = 0;
            }
            /// <summary>
            /// Shader class for <see cref="FightResources.Shaders.BlurKawase"/>
            /// </summary>
            public class BlurKawaseShader : Shader
            {
                public BlurKawaseShader(Effect eff) : base(eff) =>
                    StableEvents = (x) => x.Parameters["iDelta"].SetValue(Factor);

                internal Vector2 Factor { private get; set; } = Vector2.Zero;
            }
        }
    }
}