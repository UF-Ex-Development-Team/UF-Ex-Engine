#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
#define fSize float2(640.0, 480.0)
float fSizeMult;
float scale2;
#define fBallArgument 1.57079

float2 Turnpolar( float2 pos )
{
	return float2(atan2(pos.y, pos.x), length(pos));
}

float2 Turnposition( float2 polar )
{
	return float2(cos(polar.x), sin(polar.x)) * polar.y;
}

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 center = fSize * 0.5;
	float2 position = fSize * input.TextureCoordinates;
	float2 using_polar = Turnpolar(position - center);
	float mult_len = length(center) * fSizeMult;
	float2 using_position = Turnposition(float2(using_polar.x, asin(using_polar.y / mult_len) / fBallArgument * mult_len)) * fBallArgument / scale2 + center;
	float2 using_TextureCoord = using_position / fSize;
	return tex2D(SpriteTextureSampler, using_TextureCoord) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

