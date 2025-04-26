using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public static partial class GlobalResources
    {
        public static partial class Effects
        {
            internal static Shader[] Lights { get; set; } = new Shader[1];

            internal static void LoadInternals(ContentManager loader) => Lights[0] = new Shader(LoadContent<Effect>("Global\\Shaders\\Internal Effect\\Light0", loader));
        }
    }
}