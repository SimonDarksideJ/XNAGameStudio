//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;


// This sample uses a simple Lambert lighting model.
float3 LightDirection = normalize(float3(-1, -1, -1));
float3 DiffuseLight = 1.25;
float3 AmbientLight = 0.25;


texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};


struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // Compute lighting, using a simple Lambert model.
    float3 worldNormal = mul(input.Normal, instanceTransform);
    
    float diffuseAmount = max(-dot(worldNormal, LightDirection), 0);
    
    float3 lightingResult = saturate(diffuseAmount * DiffuseLight + AmbientLight);
    
    output.Color = float4(lightingResult, 1);

    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT)
{
    return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}


// When instancing is disabled we take the world transform from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
    return VertexShaderCommon(input, World);
}


// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}


// Hardware instancing technique.
technique HardwareInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


// For rendering without instancing.
technique NoInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 NoInstancingVertexShader();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
