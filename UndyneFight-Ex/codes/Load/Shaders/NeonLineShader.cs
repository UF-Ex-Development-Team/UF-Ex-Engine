using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class of <see cref="FightResources.Shaders.NeonLine"/>
            /// </summary>
            public class NeonLineShader : Shader
            {
                /// <summary>
                /// Shader class of <see cref="FightResources.Shaders.NeonLine"/>
                /// </summary>
                public NeonLineShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["maintime"].SetValue(Time += Speed);
                        x.Parameters["maincolor"].SetValue(DrawingColor.ToVector4() * 0.5f);
                    };
                /// <summary>
                /// The speed of the lines
                /// </summary>
                public float Speed { private get; set; } = 1.0f;
                public float Time { get; set; } = 1;
                /// <summary>
                /// The color of the lines
                /// </summary>
                public Color DrawingColor { private get; set; } = Color.White;
            }
        }
    }
}