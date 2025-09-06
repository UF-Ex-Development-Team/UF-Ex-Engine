using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class of <see cref="FightResources.Shaders.Scatter"/>
			/// </summary>
			public class ScatterShader : Shader
			{
				/// <summary>
				/// Shader class of <see cref="FightResources.Shaders.Scatter"/>
				/// </summary>
				public ScatterShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						Time += 0.5f;
						x.Parameters["intensity"].SetValue(Intensity);
						x.Parameters["time"].SetValue(Time);
						x.Parameters["ratio"].SetValue(Ratio);
					};
				/// <summary>
				/// The intensity of the scattering
				/// </summary>
				public float Intensity { private get; set; } = 3.0f;
				/// <summary>
				/// The time of the shader
				/// </summary>
				public float Time { get; set; } = 0;
				/// <summary>
				/// The ratio of the scattering
				/// </summary>
				public float Ratio { private get; set; } = 0.14f;
			}
		}
	}
}