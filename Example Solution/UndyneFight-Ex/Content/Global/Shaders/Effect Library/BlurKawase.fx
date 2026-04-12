#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif 

Texture2D SpriteTexture;

//uniform float iRotation;
uniform float2 iDelta; 

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
    float2 pos = input.TextureCoordinates;
    float4 res = float4(0, 0, 0, 0);
    res += tex2D(SpriteTextureSampler, pos + iDelta) * 0.2;
    res += tex2D(SpriteTextureSampler, pos - iDelta) * 0.2;
    float2 del2 = iDelta * float2(-1, 1);
    res += tex2D(SpriteTextureSampler, pos + del2) * 0.2;
    res += tex2D(SpriteTextureSampler, pos - del2) * 0.2;
    res += tex2D(SpriteTextureSampler, pos) * 0.2;
    return input.Color * res;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};