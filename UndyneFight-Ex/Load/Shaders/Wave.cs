using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Wave"/>
			/// </summary>
			public class WaveShader : Shader
			{
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Wave"/>
				/// </summary>
				public WaveShader(Effect eff) : base(eff) => StableEvents = (x) =>
																			  {
																				  Time += 0.5f * Speed / 480f;

																				  x.Parameters["iTime"].SetValue(Time);

																				  x.Parameters["iIntensity"].SetValue(new Vector3(Intensity[0], Intensity[1], Intensity[2]) / 640f);
																				  x.Parameters["iFrequency"].SetValue(new Vector3(Frequency[0], Frequency[1], Frequency[2]) * MathF.PI * 960f);
																			  };
				/// <summary>
				/// The time of the shader
				/// </summary>
				public float Time { private get; set; } = 0.0f;
				/// <summary>
				/// The speed of the waving
				/// </summary>
				public float Speed { private get; set; } = 1.0f;
				/// <summary>
				/// The intensity of the shader, the 3 values are just for noise, you can assign only 1
				/// </summary>
				public float[] Intensity = new float[3];
				/// <summary>
				/// The frequency of the shader, the 3 values are just for noise, you can assign only 1
				/// </summary>
				public float[] Frequency = new float[3];
			}
		}
	}
}