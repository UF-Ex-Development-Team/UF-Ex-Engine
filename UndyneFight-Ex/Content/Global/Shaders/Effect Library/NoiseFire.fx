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

uniform float iDistort;//扭曲程度
uniform float iTime;//时间
uniform float iHeight;//火焰高度（240）
uniform float iPieceRate;//残渣量（0.1）
uniform float3 iBlend;//内火焰颜色RGB
uniform float3 iBlendEdge;//外火焰颜色RGB
static const float2 SCREEN = float2(WIDTH, HEIGHT);
 
sampler2D Sampler0 : register(s0);
sampler2D Sampler1 : register(s1);

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 localToColor(sampler2D textureSampler, float2 location)//获取表面上一点的颜色值
{
    location = fmod(location, SCREEN);
    return tex2D(textureSampler, location / SCREEN);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 v_vPosition = input.TextureCoordinates * SCREEN;
	
    float distort = localToColor(Sampler0, v_vPosition * 0.3149657 + iTime * 0.1) * iDistort;
    float color = localToColor(Sampler1, float2(v_vPosition.x - iTime * 0.21335, v_vPosition.y + iTime) + distort);

    float grand = smoothstep(iHeight, HEIGHT, v_vPosition.y);
    color += grand * 0.5;
	
    float color_value = color * min(1.0, color * grand);
    float color_edge_value = 1.0 - (min(color + iPieceRate, 1.0) - color) / iPieceRate;
	
    return float4(input.Color.rgb * (color_value * iBlend + color_edge_value * iBlendEdge), input.Color.a);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


