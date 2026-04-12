#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//#define CAMERAHIGH 400.0
#define PI 3.1415926

uniform float2 iPos;
uniform float iValue;//程度
Texture2D SpriteTexture;

#define SIZESURFACE float2(640.0, 480.0)//
#define SIZEPIXEL 1.0 / SIZESURFACE

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

float4 localToColor(sampler2D samplerTexture, float2 Position)
{
	return tex2D(samplerTexture, SIZEPIXEL * Position);
}

float vectorToAngle(float2 vec)
{
	return (-atan2(vec.y, vec.x) + 2.0 * PI);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 v_vPosition = input.TextureCoordinates * SIZESURFACE;
	float2 Vector = v_vPosition - iPos;
    Vector *= (1.0 + smoothstep(0.0, 0.3, max(0.0, -length(Vector) / 320.0 * 0.3 + 0.3)) * iValue);
	return input.Color * localToColor(SpriteTextureSampler, Vector + iPos);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};