using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public static partial class GlobalResources
	{
		public static partial class Effects
		{
			/// <summary>
			/// Shader class for <see cref="FightResources.Shaders.Fire"/>
			/// </summary>
			public class FireShader : Shader
			{
				/// <summary>
				/// Shader class for <see cref="FightResources.Shaders.Fire"/>
				/// </summary>
				public FireShader(Effect eff) : base(eff) =>
					StableEvents = (x) =>
					{
						Time += Speed;
						RegisterTexture(Sprites.hashtex2, 1);

						/*
                        uniform float iDistort;//扭曲程度
                        uniform float iTime;//时间
                        uniform float iHeight;//火焰高度（240）
                        uniform float iPieceRate;//残渣量（0.1）
                        uniform float3 iBlend;//内火焰颜色RGB
                        uniform float3 iBlendEdge;//外火焰颜色RGB
                        */
						Parameters["iDistort"].SetValue(Distort);
						Parameters["iTime"].SetValue(Time);
						Parameters["iHeight"].SetValue(480 - Height);
						Parameters["iPieceRate"].SetValue(PieceRate);
						Parameters["iBlend"].SetValue(Blend.ToVector3());
						Parameters["iBlendEdge"].SetValue(BlendEdge.ToVector3());
					};

				public float Time { get; set; } = 0.0f;
				/// <summary>
				/// The distortion intensity of the fire
				/// </summary>
				public float Distort { get; set; } = 0.0f;
				/// <summary>
				/// The height of the fire
				/// </summary>
				public float Height { get; set; } = 240f;
				/// <summary>
				/// The amount of the ashes
				/// </summary>
				public float PieceRate { get; set; } = 0.1f;
				/// <summary>
				/// The inner color of the fire
				/// </summary>
				public Color Blend { get; set; }
				/// <summary>
				/// The speed of the fire
				/// </summary>
				public float Speed { get; set; }
				/// <summary>
				/// The outer color of the fire
				/// </summary>
				public Color BlendEdge { get; set; }
			}
		}
	}
}