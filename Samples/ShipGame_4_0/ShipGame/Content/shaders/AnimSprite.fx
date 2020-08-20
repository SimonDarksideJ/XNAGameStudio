float4x4 ViewProj;
float4 FrameOffset;
float2 FrameSize;
float2 FrameBlend;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

void AnimSpriteVS( 
     in float4 InPosition     : POSITION0,
     in float2 InTexCoord     : TEXCOORD0,
    out float4 OutPosition    : POSITION0,
    out float2 OutTexCoord    : TEXCOORD0)
{
    OutPosition = mul(InPosition, ViewProj);
    OutTexCoord = InTexCoord;
}

float4 AnimSpritePS( in float2 TexCoord : TEXCOORD0 ) : COLOR0
{
    float2 tx1 = FrameSize * (FrameOffset.xy + TexCoord);
    float2 tx2 = FrameSize * (FrameOffset.zw + TexCoord);
    
    float4 color1 = tex2D(TextureSampler, tx1);
    float4 color2 = tex2D(TextureSampler, tx2);
    
    float4 blend_color = lerp(color1, color2, FrameBlend.x);
    blend_color.w *= FrameBlend.y;
    
    return blend_color;
}

Technique AnimSprite
{
    Pass
    {
        VertexShader = compile vs_3_0 AnimSpriteVS();
        PixelShader = compile ps_3_0 AnimSpritePS();
    }
}

