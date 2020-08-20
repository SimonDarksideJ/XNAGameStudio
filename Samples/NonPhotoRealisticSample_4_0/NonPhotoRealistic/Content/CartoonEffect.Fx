//-----------------------------------------------------------------------------
// CartoonEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

// The light direction is shared between the Lambert and Toon lighting techniques.
float3 LightDirection = normalize(float3(1, 1, 1));

// Settings controlling the Lambert lighting technique.
float3 DiffuseLight = 0.5;
float3 AmbientLight = 0.5;

// Settings controlling the Toon lighting technique.
float ToonThresholds[2] = { 0.8, 0.4 };
float ToonBrightnessLevels[3] = { 1.3, 0.9, 0.5 };

// Is texturing enabled?
bool TextureEnabled;


// The main texture applied to the object, and a sampler for reading it.
texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Wrap;
    AddressV = Wrap;
};


// Vertex shader input structure.
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};


// Output structure for the vertex shader that applies lighting.
struct LightingVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float LightAmount : TEXCOORD1;
};


// Input structure for the Lambert and Toon pixel shaders.
struct LightingPixelShaderInput
{
    float2 TextureCoordinate : TEXCOORD0;
    float LightAmount : TEXCOORD1;
};


// Vertex shader shared between the Lambert and Toon lighting techniques.
LightingVertexShaderOutput LightingVertexShader(VertexShaderInput input)
{
    LightingVertexShaderOutput output;

    // Apply camera matrices to the input position.
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    
    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;

    // Compute the overall lighting brightness.
    float3 worldNormal = mul(input.Normal, World);
    
    output.LightAmount = dot(worldNormal, LightDirection);
    
    return output;
}


// Pixel shader applies a simple Lambert shading algorithm.
float4 LambertPixelShader(LightingPixelShaderInput input) : COLOR0
{
    float4 color = TextureEnabled ? tex2D(Sampler, input.TextureCoordinate) : 0;
    
    color.rgb *= saturate(input.LightAmount) * DiffuseLight + AmbientLight;
    
    return color;
}


// Pixel shader applies a cartoon shading algorithm.
float4 ToonPixelShader(LightingPixelShaderInput input) : COLOR0
{
    float4 color = TextureEnabled ? tex2D(Sampler, input.TextureCoordinate) : 0;
    
    float light;

    if (input.LightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (input.LightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    color.rgb *= light;
    
    return color;
}


// Output structure for the vertex shader that renders normal and depth information.
struct NormalDepthVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};


// Alternative vertex shader outputs normal and depth values, which are then
// used as an input for the edge detection filter in PostprocessEffect.fx.
NormalDepthVertexShaderOutput NormalDepthVertexShader(VertexShaderInput input)
{
    NormalDepthVertexShaderOutput output;

    // Apply camera matrices to the input position.
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    
    float3 worldNormal = mul(input.Normal, World);

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;    
}


// Simple pixel shader for rendering the normal and depth information.
float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}


// Technique draws the object using smooth Lambert shading.
technique Lambert
{
    pass P0
    {
        VertexShader = compile vs_2_0 LightingVertexShader();
        PixelShader = compile ps_2_0 LambertPixelShader();
    }
}


// Technique draws the object using banded cartoon shading.
technique Toon
{
    pass P0
    {
        VertexShader = compile vs_2_0 LightingVertexShader();
        PixelShader = compile ps_2_0 ToonPixelShader();
    }
}


// Technique draws the object as normal and depth values.
technique NormalDepth
{
    pass P0
    {
        VertexShader = compile vs_2_0 NormalDepthVertexShader();
        PixelShader = compile ps_2_0 NormalDepthPixelShader();
    }
}
