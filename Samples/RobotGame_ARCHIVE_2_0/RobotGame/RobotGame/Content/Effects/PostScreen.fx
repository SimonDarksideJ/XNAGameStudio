//-----------------------------------------------------------------------------
// PostScreen.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

sampler BloomSampler : register(s0); 
sampler BaseSampler : register(s1); 

float BloomThreshold = 0.25;
float BloomIntensity = 1.25;
float BaseIntensity = 1.0;
float BloomSaturation = 1.0;
float BaseSaturation = 1.0;


// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}

float4 PS_BloomExtract(float2 texCoord : TEXCOORD0) : COLOR0 
{    
    // Look up the original image color.
    float4 c = tex2D(BloomSampler, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}

float4 PS_BloomCombine(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 bloomColor = tex2D(BloomSampler, texCoord);
    float4 baseColor = tex2D(BaseSampler, texCoord);
        
    // Adjust color saturation and intensity.
    bloomColor = AdjustSaturation(bloomColor, BloomSaturation) * BloomIntensity;
    baseColor = AdjustSaturation(baseColor, BaseSaturation) * BaseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    baseColor *= (1 - saturate(bloomColor));
    
    // Combine the two images.
    return baseColor + bloomColor;
}

technique PostScreen
{
    pass P0
    {
        PixelShader = compile ps_2_0 PS_BloomExtract();    
    }
    
    pass P1
    {
        PixelShader = compile ps_2_0 PS_BloomCombine();
    }
}
