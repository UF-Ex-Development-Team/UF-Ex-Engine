using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class of <see cref="FightResources.Shaders.StepSample"/>
            /// </summary>
            public class StepSampleShader : Shader
            {
                /// <summary>
                /// Shader class of <see cref="FightResources.Shaders.StepSample"/>
                /// </summary>
                public StepSampleShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        x.Parameters["iLightPos"].SetValue(new Vector2(CentreX, CentreY));
                        x.Parameters["iDistance"].SetValue(4 * Intensity);
                        x.Parameters["iSampling"].SetValue(1f);
                    };
                /// <summary>
                /// The intensity of the sampling
                /// </summary>
                public float Intensity { private get; set; } = 1.0f;
                /// <summary>
                /// The x coordinate of the centre
                /// </summary>
                public float CentreX { private get; set; } = 320f;
                /// <summary>
                /// The y coordinate of the centre
                /// </summary>
                public float CentreY { private get; set; } = 240f;
            }
        }
    }
}