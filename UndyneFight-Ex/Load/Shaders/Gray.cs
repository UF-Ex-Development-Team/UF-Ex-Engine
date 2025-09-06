using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Gray"/>
			/// </summary>
			public class GrayShader : Shader
			{
				/// <summary>
				/// The intensity of the gray scale, [0, 1]
				/// </summary>
				public float Intensity { get; set; } = 0;
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Gray"/>
				/// </summary>
				public GrayShader(Effect eff) : base(eff) =>
					StableEvents = (x) => x.Parameters["intensity"].SetValue(Intensity);
			}
		}
	}
}