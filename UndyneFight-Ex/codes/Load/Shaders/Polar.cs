using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class for <see cref="FightResources.Shaders.Polar"/>
            /// </summary>
            public class PolarShader : Shader
            {
                /// <summary>
                /// Shader class for <see cref="FightResources.Shaders.Polar"/>
                /// </summary>
                public PolarShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["itype"].SetValue(IType ? 1 : 0);
                        x.Parameters["idegree"].SetValue(Intensity);
                    };
                /// <summary>
                /// The type of ditortion to apply
                /// </summary>
                public bool IType { get; set; } = false;
                /// <summary>
                /// The intensity of the distortion
                /// </summary>
                public float Intensity { get; set; } = 0;
            }
        }
    }
}