using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex;

public static partial class GlobalResources
{
	public static partial class Effects
	{
		/// <summary>
		/// Shader class for <see cref="FightResources.Shaders.Cos1Ball"/>
		/// </summary>
		public class BallShapingShader : Shader
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Cos1Ball"/>
			/// </summary>
			public BallShapingShader(Effect eff) : base(eff) =>
				StableEvents = (X) =>
				{
					Parameters["fSizeMult"].SetValue(Intensity);
					Parameters["scale2"].SetValue(ScreenScale);
				};
#if NET10_0_OR_GREATER
			/// <summary>
			/// The intensity of the effect
			/// </summary>
			public float Intensity { private get; set => field = 1 / value; }
#else
			/// <summary>
			/// The intensity of the effect
			/// </summary>
			public float Intensity { private get => _intensity; set => _intensity = 1 / value; }
			private float _intensity;
#endif
			/// <summary>
			/// The screen scale
			/// </summary>
			public float ScreenScale { get; set; } = 1;
		}
	}
}