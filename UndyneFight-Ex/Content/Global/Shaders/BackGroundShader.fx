#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
//x=down, y=left, z=up, w=right
float4 boundDistance;
float4 mixColor;
Texture2D SpriteTexture;
static const float2 SIZE = float2(640., 480.);

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
	float2 pos = input.TextureCoordinates * SIZE;
	float4 bounds = 1. + float4(pos.y - 480, -pos.x, -pos.y, pos.x - 640) / boundDistance;
	return tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color + mixColor * max(0, max(max(bounds.x, bounds.y), max(bounds.z, bounds.w)));
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};