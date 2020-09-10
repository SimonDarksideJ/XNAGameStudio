//-----------------------------------------------------------------------------
// GroundEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 View;
float4x4 Projection;

// Our diffuse texture
texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};

// Our shadow render target
texture ShadowTexture;
sampler ShadowSampler = sampler_state
{
    Texture = (ShadowTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 ScreenPos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    // Transform our position
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // We output our texture coordinate directly
    output.TexCoord = input.TexCoord;

    // We also store our position in a second variable so our pixel shader can use it
    output.ScreenPos = output.Position;

    return output;
}

float4 PixelShaderFunction(in float2 texCoord : TEXCOORD0, in float4 screenPos : TEXCOORD1) : COLOR0
{
    // Sample the diffuse texture
    float4 color = tex2D(TextureSampler, texCoord);

    // Convert our screen coordinates into a usable texture coordinate for our shadow texture
    screenPos.xy /= screenPos.w;
    float2 shadowTexCoords = 0.5f * float2(screenPos.x, -screenPos.y) + 0.5f;

    // Sample the shadow texture which only contains an alpha value
    float shadow = tex2D(ShadowSampler, shadowTexCoords).a;
        
    // If there is a non-transparent value, then we are in shadows. We use the following line
    // to multiply our diffuse texture by either 1 or .5 based on whether we are in shadow. If
    // we multiply by 1, we have no change. If we multiply by .5, we darken the diffuse texture
    // which simulates a shadow.
    color.rgb *= (1 - (0.5 * ceil(shadow)));

    // Return our final color
    return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
