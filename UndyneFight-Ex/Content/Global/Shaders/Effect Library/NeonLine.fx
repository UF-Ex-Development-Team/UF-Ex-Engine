#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float4 maincolor;//渲染的主色调(BELEND)
uniform float1 maintime; 

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 NeonlineGetColor(float2 _xy, float1 time, float4 using_color)
{
	return using_color * saturate((sin(412.0 * _xy.y + 1.6924 * time) * sin(128.0 * _xy.y - 0.04 * time) - sin(156.0 * _xy.y + 0.3895 * time) - cos(147.0 * _xy.y - 0.1 * time)) * 0.6 + 0.1);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return tex2D(SpriteTextureSampler, input.TextureCoordinates) + NeonlineGetColor(input.TextureCoordinates, maintime, maincolor);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


