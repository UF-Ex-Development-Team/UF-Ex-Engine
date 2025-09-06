using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Aurora"/>
			/// </summary>
			public class AuroraShader : Shader
			{
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Aurora"/>
				/// </summary>
				/// <param name="eff">The shader itself</param>
				public AuroraShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						Time += 0.01f;
						RegisterTexture(Sprites.hashtex, 1);

						x.Parameters["iTime"].SetValue(Time);
						x.Parameters["iRGB1"].SetValue(ThemeColorA.ToVector3());
						x.Parameters["iRGB2"].SetValue(ThemeColorB.ToVector3());
						x.Parameters["iSlope"].SetValue(Slope);

						float u = YCentre / 640f;
						// y * slope = addition
						// fx : float y1 = 1.0 - abs(uv.y * iSlope - iAddition);
						x.Parameters["iAddition"].SetValue(u * Slope);
					};
				/// <summary>
				/// The time of the shader
				/// </summary>
				public float Time { get; set; } = 0.0f;
				/// <summary>
				/// NOT the y coordinate of the effect
				/// </summary>
				public float YCentre { get; set; } = 320.0f;
				/// <summary>
				/// The slope for the color blending, the higher the value the more dominant the <see cref="ThemeColorB"/> is
				/// </summary>
				public float Slope { get; set; } = 2.0f;
				/// <summary>
				/// The color on the left side
				/// </summary>
				public Color ThemeColorA { get; set; } = Color.White;
				/// <summary>
				/// The color on the right side
				/// </summary>
				public Color ThemeColorB { get; set; } = Color.White;
			}
		}
	}
}