using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Tyndall"/>
			/// </summary>
			public class TyndallShader : Shader
			{
				/// <summary>
				/// The position of the light
				/// </summary>
				public Vector2 LightPos { get; set; } = Vector2.Zero;
				public float Distance { get; set; } = 5;
				public float Sampling { get; set; } = 1;
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Tyndall"/>
				/// </summary>
				public TyndallShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						x.Parameters["iLightPos"].SetValue(LightPos);
						x.Parameters["iDistance"].SetValue(Distance);
						x.Parameters["iSampling"].SetValue(Sampling);
					};
			}
		}
	}
}