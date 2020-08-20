//-----------------------------------------------------------------------------
// BoosterEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

sampler samplerState : register(s0); 

float texWidth = 0;
float texHeight = 0;
float xCenter = 0.5;
float yCenter = 0.5;
float mag = 0.5;
float width = 0.5;
float radialBlurScaleFactor = -0.0125;

#define SAMPLE_COUNT 8 

//    Works only on ps_2_0 and above
float4 PS_RadialBlur(float2 texCoord : TEXCOORD0) : COLOR0 
{     
    float2 center = float2(xCenter, yCenter);
    float2 texelSize = 1.0 / float2(texWidth, texHeight);
        
    // This is our original texture color, reuse existing locations
    float4 blendColor = tex2D(samplerState, texCoord + texelSize*0.5);
    
    // For all radial blur steps scale the finalSceneMap
    float2 texCentered = (texCoord-center)*2.0;
    
    // Now apply formula to nicely increase blur factor to the borders
    for (int i=1; i<SAMPLE_COUNT; i++)
    {
        texCentered = texCentered+
            radialBlurScaleFactor*(0.5+i*0.15)*texCentered*abs(texCentered);
            
        blendColor += tex2D(samplerState, 
            (texCentered+center*2)/2.0f + texelSize*0.5);
    }        
                        
    return blendColor / SAMPLE_COUNT;
}

float4 PS_BoosterWave(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 col = 0.0;
    float2 tex = texCoord;
    float xdif = texCoord.x - xCenter;
    float ydif = texCoord.y - yCenter;
    float d = sqrt(xdif * xdif + ydif * ydif) - width;
    float t = abs(d);
    
    if (d < 0.1 && d > -0.2) 
    {
        if (d < 0.0) 
        {
            t = (0.2 - t) / 2.0;
            tex.x = tex.x - (xdif * t * mag);
            tex.y = tex.y - (ydif * t * mag);
            col = tex2D(samplerState, tex);
        } else {
            t = (0.1 - t);
            tex.x = tex.x - (xdif * t * mag);
            tex.y = tex.y - (ydif * t * mag);
            col = tex2D(samplerState, tex);            
        }
        
        col.a = t * 12.0;
    } 
    else 
    {
        col.a = 0.0;
    }
        
    return col;
}

technique BoosterEffect 
{ 
    pass P0
    { 
        PixelShader = compile ps_2_0 PS_RadialBlur();         
    } 
    
    pass P1
    { 
        PixelShader = compile ps_2_0 PS_BoosterWave();         
    } 
}
