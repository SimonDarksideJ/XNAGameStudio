//-----------------------------------------------------------------------------
// Distort.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

sampler SceneTexture : register(s0);
sampler DistortionMap : register(s1);

#define SAMPLE_COUNT 15
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

// The Distortion map represents zero displacement as 0.5, but in an 8 bit color
// channel there is no exact value for 0.5. ZeroOffset adjusts for this error.
const float ZeroOffset = 0.5f / 255.0f;

float4 Distort_PixelShader(float2 TexCoord : TEXCOORD0, 
    uniform bool distortionBlur) : COLOR0
{
    // Look up the displacement
    float2 displacement = tex2D(DistortionMap, TexCoord).rg;
    
    float4 finalColor = 0;
    // We need to constrain the area potentially subjected to the gaussian blur to the
    // distorted parts of the scene texture.  Therefore, we can sample for the color
    // we used to clear the distortion map (black).  We used 0 to avoid any potential
    // rounding errors.
    if ((displacement.x == 0) && (displacement.y == 0))
    {
        finalColor = tex2D(SceneTexture, TexCoord);
    }
    else
    {
        // Convert from [0,1] to [-.5, .5) 
        // .5 is excluded by adjustment for zero
        displacement -= .5 + ZeroOffset;

        if (distortionBlur)
        {
            // Combine a number of weighted displaced-image filter taps
            for (int i = 0; i < SAMPLE_COUNT; i++)
            {
                finalColor += tex2D(SceneTexture, TexCoord.xy + displacement + 
                    SampleOffsets[i]) * SampleWeights[i];
            }
        }
        else
        {
            // Look up the displaced color, without multisampling
            finalColor = tex2D(SceneTexture, TexCoord.xy + displacement);  
        }
    }

    return finalColor;
}

technique Distort
{
    pass
    {
        PixelShader = compile ps_2_0 Distort_PixelShader(false);
    }
}

technique DistortBlur
{
    pass
    {
        PixelShader = compile ps_2_0 Distort_PixelShader(true);
    }
}