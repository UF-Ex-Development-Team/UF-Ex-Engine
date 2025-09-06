using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Spiral"/>
			/// </summary>
			public class SpiralShader : Shader
			{
				/// <summary>
				/// The time of the shader
				/// </summary>
				public float Time { get; set; } = 0;
				/// <summary>
				/// The speed of the spiralling
				/// </summary>
				public float Speed { get; set; } = 1;
				/// <summary>
				/// The intensity of the spiralling
				/// </summary>
				public float Intensity { get; set; } = 0;
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Spiral"/>
				/// </summary>
				public SpiralShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						Time += 0.5f * Speed;
						x.Parameters["iTime"].SetValue(Time);
						x.Parameters["iUnit"].SetValue(Intensity);
					};
			}
		}
	}
}