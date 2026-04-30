#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

//#define CAMERAHIGH 400.0
#define PI 3.1415926

uniform float iTime;
uniform float iValue;
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

float2 GetRandomF2(float2 Area, float iTime)
{
	float cosT = cos(iTime);
	float sinT = sin(iTime);
	float cosYT = cos(Area.y * iTime);
	return float2(sqrt(abs(cos(sin(Area.x * 9.0 / cosT) / cosYT * iTime))), sqrt(abs(sin(cos(Area.x * 16.0 * sinT) * cosYT * iTime))));
}

float4 GetRandomF4(float2 Position, float iTime, float2 Block, float2 Offset)
{
	float2 pos = floor(Position / Block);
	pos += Offset;
	float sqrtTime = sqrt(iTime);
	float logTime = log2(iTime);
	float commonT = (2.0 * iTime) / logTime;
	float cosT2 = cos(commonT);
	float xy = pos.x * pos.y;
	return float4(sin(sqrtTime / 3.0) * cosT2 * sin(xy * (fmod(iTime, 20.0) / 20.0)),
				sin(sqrtTime / 7.0) * cosT2 * cos(xy * (fmod(iTime, 5.0) / 20.0)),
				cos(sqrtTime / 12.0) * cos(4.0 * commonT) * sin(xy * (fmod(iTime, 10.0) / 20.0)),
				0.2) * 5;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 v_vPosition = input.TextureCoordinates * SIZESURFACE;
	float2 Offect = SIZESURFACE;
	float4 clr = abs(GetRandomF4(v_vPosition, iTime, 1.0, Offect));

	float2 BlockArea = floor(v_vPosition / (4.0 + GetRandomF2(float2(35.0, 23.0), iTime) * 12.0));
	float2 BlockInGetRandomF2 = GetRandomF2(BlockArea, iTime) * 100.0;

	float4 moveHigh = GetRandomF4(v_vPosition, iTime, max(1., BlockInGetRandomF2), Offect);
	float2 move = (float)(clr.x > 0.9) * float2(moveHigh.x + moveHigh.z, moveHigh.y + moveHigh.x) * 0.01;
	clr.xyz += (float)(clr.x < 0.9);

	float4 effectColor = clr * tex2D(SpriteTextureSampler, input.TextureCoordinates + move);

	return input.Color * lerp(tex2D(SpriteTextureSampler, input.TextureCoordinates), effectColor, iValue);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};