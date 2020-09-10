//-----------------------------------------------------------------------------
// Billboard.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera parameters.
float4x4 View;
float4x4 Projection;


// Lighting parameters.
float3 LightDirection;
float3 LightColor = 0.8;
float3 AmbientColor = 0.4;


// Parameters controlling the wind effect.
float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount;
float WindTime;


// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.95;


// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

texture Texture;


struct VS_INPUT
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float Random : TEXCOORD1;
};


struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};


VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;

    // Apply a scaling factor to make some of the billboards
    // shorter and fatter while others are taller and thinner.
    float squishFactor = 0.75 + abs(input.Random) / 2;

    float width = BillboardWidth * squishFactor;
    float height = BillboardHeight / squishFactor;

    // Flip half of the billboards from left to right. This gives visual variety
    // even though we are actually just repeating the same texture over and over.
    if (input.Random < 0)
        width = -width;

    // Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;

    float3 rightVector = normalize(cross(viewDirection, input.Normal));

    // Calculate the position of this billboard vertex.
    float3 position = input.Position;

    // Offset to the left or right.
    position += rightVector * (input.TexCoord.x - 0.5) * width;
    
    // Offset upward if we are one of the top two vertices.
    position += input.Normal * (1 - input.TexCoord.y) * height;

    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(position, WindDirection) * WindWaveSize;
    
    waveOffset += input.Random * WindRandomness;
    
    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;
    
    // But it should only affect the top two vertices of the billboard!
    wind *= (1 - input.TexCoord.y);
    
    position += WindDirection * wind;

    // Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), View);

    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    
    // Compute lighting.
    float diffuseLight = max(-dot(input.Normal, LightDirection), 0);
    
    output.Color.rgb = diffuseLight * LightColor + AmbientColor;
    output.Color.a = 1;
    
    return output;
}


sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    color *= tex2D(TextureSampler, texCoord);

    // Apply the alpha test.
    clip((color.a - AlphaTestThreshold) * AlphaTestDirection);

    return color;
}


technique Billboards
{
    pass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
