using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            /// <summary>
            /// Shader class for <see cref="FightResources.Shaders.Mosaic"/>
            /// </summary>
            public class MosaicShader : Shader
            {
                /// <summary>
                /// The size of the pixels
                /// </summary>
                public Vector2 MosiacSize { get; set; } = new Vector2(4);
                /// <summary>
                /// Shader class for <see cref="FightResources.Shaders.Mosaic"/>
                /// </summary>
                public MosaicShader(Effect eff) : base(eff) => StableEvents = (x) => x.Parameters["iBlockSize"].SetValue(MosiacSize);
            }
        }
    }
}