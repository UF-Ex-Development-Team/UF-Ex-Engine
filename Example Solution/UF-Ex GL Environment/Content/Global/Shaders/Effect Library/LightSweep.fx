#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define WIDTH 640.0
#define HEIGHT 480.0
#define PI 3.1415926
// #define TORAD 0.01745329251994329576923690768489

uniform float2 iCenter;
uniform float iDirection;
uniform float iWidth;
uniform float iSweepIntensity;

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

float point_distance(float2 center, float angle, float2 position)
{
    return abs(dot(float2(-sin(angle), cos(angle)), position - center));
}

//return tex2D(samplerTexture, SIZEPIXEL * Position);

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return input.Color * saturate(iSweepIntensity *
	exp(-point_distance(iCenter, iDirection, input.TextureCoordinates * float2(WIDTH, HEIGHT)) / iWidth * 8.63378723));
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};