using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
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
				/// <summary>
				/// The intensity of the effect
				/// </summary>
				public float Intensity { private get => intensity; set => intensity = 1 / value; }
				/// <summary>
				/// The screen scale
				/// </summary>
				public float ScreenScale { get; set; } = 1;

				private float intensity;
			}
		}
	}
}