using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class of <see cref="FightResources.Shaders.Seismic"/>
			/// </summary>
			public class SeismicShader : Shader
			{
				/// <summary>
				/// Shader class of <see cref="FightResources.Shaders.Seismic"/>
				/// </summary>
				public SeismicShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						x.Parameters["iCenter"].SetValue(Centre);
						x.Parameters["iRadius"].SetValue(Radius);
						x.Parameters["iProgress"].SetValue(Progress);
					};
				/// <summary>
				/// The progress of the effect, [0, 1]
				/// </summary>
				public float Progress { private get; set; } = 0.0f;
				/// <summary>
				/// The radius of the effect
				/// </summary>
				public float Radius { private get; set; } = 100;
				/// <summary>
				/// The centre of the effect
				/// </summary>
				public Vector2 Centre { private get; set; } = new(320, 240);
			}
		}
	}
}