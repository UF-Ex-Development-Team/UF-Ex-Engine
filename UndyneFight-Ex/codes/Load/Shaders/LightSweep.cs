using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class of <see cref="FightResources.Shaders.LightSweep"/>
            /// </summary>
            public class LightSweepShader : Shader
            {
                /// <summary>
                /// The intensity of the light beam
                /// </summary>
                public float Intensity { get; set; } = 1;
                /// <summary>
                /// The width of the light beam
                /// </summary>
                public float Width { get; set; } = 10;
                /// <summary>
                /// The angle of the beam
                /// </summary>
                public float Direction { get; set; } = 0;
                /// <summary>
                /// The ventre of the beam
                /// </summary>
                public Vector2 Centre { get; set; } = new Vector2(320, 240);

                /// <summary>
                /// Shader class of <see cref="FightResources.Shaders.LightSweep"/>
                /// </summary>
                public LightSweepShader(Effect eff) : base(eff) =>
                    StableEvents = (x) =>
                    {
                        /*
                            uniform float2 iCenter;中心
                            uniform float iDirection; 
                            uniform float iWidth;光柱宽度
                            uniform float iSweepIntensity;调整强度
                        */
                        x.Parameters["iSweepIntensity"].SetValue(Intensity);
                        x.Parameters["iDirection"].SetValue(MathUtil.GetRadian(Direction));
                        x.Parameters["iWidth"].SetValue(Width);
                        x.Parameters["iCenter"].SetValue(Centre);
                    };
            }
        }
    }
}