//-----------------------------------------------------------------------------
// ReplaceColor.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 WorldViewProjection;
float4x4 World;

float3 Ambient = 0.2f;
float3 LightColor = 0.8f;
float3 LightDirection = normalize(float3(-1, -1, -1));


// This color is blended with the diffuse texture.
// The amount of blend is defined by the diffuse texture's alpha channel.
float3 TargetColor;


// DiffuseTexture defines a base color before color replacement in the rgb channels.
// The alpha channel defines how much to blend in the replacement color.
// Fully transparent indicates using the diffuse texture's original color.
// Fully opaque indicates using the target color.
// Intermediate levels of opacity will blend the two colors.
texture2D DiffuseTexture;
sampler2D DiffuseSampler = sampler_state
{
    Texture = <DiffuseTexture>;   
};


struct PositionNormalTextured
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD;
};

struct OutVertex
{
    float4 Position     : POSITION;
    float3 LightDiffuse : COLOR0;
    float2 TexCoord     : TEXCOORD0;
};


OutVertex VertexShaderFunction(PositionNormalTextured vertex)
{
    OutVertex outVertex;
    
    // Copy the texture coordinate
    outVertex.TexCoord = vertex.TexCoord;
    
    // Transform and project position
    outVertex.Position = mul(vertex.Position, WorldViewProjection);
    
    // Transform normal for lighting
    float3 normal = normalize(mul(vertex.Normal, World));
    
    // Calculate lighting
    outVertex.LightDiffuse =
        Ambient + LightColor * max(0, dot(-LightDirection, normal));
    
    return outVertex;
}


float4 PixelShaderFunction(float3 lightDiffuse : COLOR0, float4 texCoord : TEXCOORD0) : COLOR
{
    // Look up the base color and blend amount
    float4 diffuse = tex2D(DiffuseSampler, texCoord);
    
    // Blend the base color with the target color by the amount of the diffuse alpha
    float3 color = lerp(diffuse.rgb, TargetColor, diffuse.a);
    
    // Apply lighting
    color *= lightDiffuse;
    
    // Always return a full opaque color
    return float4(color, 1);
}


technique
{
    pass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader  = compile ps_2_0 PixelShaderFunction();
    }
}
