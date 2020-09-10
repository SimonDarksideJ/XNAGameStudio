//-----------------------------------------------------------------------------
// Distorters.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// common parameters
float4x4 WorldViewProjection;
float4x4 WorldView;
float DistortionScale;
float Time;


//-----------------------------------------------------------------------------
//
// Displacement Mapping
//
//-----------------------------------------------------------------------------

struct PositionTextured
{
   float4 Position : POSITION;
   float2 TexCoord : TEXCOORD;
};

PositionTextured TransformAndTexture_VertexShader(PositionTextured input)
{
    PositionTextured output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    
    return output;
}

texture2D DisplacementMap;
sampler2D DisplacementMapSampler = sampler_state
{
    texture = <DisplacementMap>;
};

float4 Textured_PixelShader(float2 texCoord : TEXCOORD) : COLOR
{
    float4 color = tex2D(DisplacementMapSampler, texCoord);
    
    // Ignore the blue channel    
    return float4(color.rg, 0, color.a);
}

technique DisplacementMapped
{
    pass
    {
        VertexShader = compile vs_2_0 TransformAndTexture_VertexShader();
        PixelShader = compile ps_2_0 Textured_PixelShader();
    }
}


//-----------------------------------------------------------------------------
//
// Heat-Haze Displacement
//
//-----------------------------------------------------------------------------

struct PositionPosition
{
    float4 Position : POSITION;
        
    // the pixel shader does not have direct access to the position of the pixel
    // being shaded, so we must pass this information through from the vertex shader
    float4 PositionAsTexCoord : TEXCOORD;
};

PositionPosition TransformAndCopyPosition_VertexShader(float4 position : POSITION)
{
    PositionPosition output;
    
    output.Position = mul(position, WorldViewProjection);
    output.PositionAsTexCoord = output.Position;
    
    return output;
}

float4 HeatHaze_PixelShader(float4 position : TEXCOORD) : COLOR
{
    float2 displacement;
    displacement.x = sin(position.x / 60 + Time * 1.5) * sin(position.x / 10) * 
        cos(position.x / 50);
    displacement.y = sin(position.y / 50 - Time * 2.75);
    displacement *= DistortionScale;
    displacement = (displacement + float2(1, 1)) / 2;
    
    return float4(displacement, 0, 1);
}

technique HeatHaze
{
    pass
    {
        VertexShader = compile vs_1_1 TransformAndCopyPosition_VertexShader();
        PixelShader = compile ps_2_0 HeatHaze_PixelShader();
    }
}


//-----------------------------------------------------------------------------
//
// Pull-In Displacement
//
//-----------------------------------------------------------------------------

struct PositionNormal
{
   float4 Position : POSITION;
   float3 Normal : NORMAL;
};

struct PositionDisplacement
{
   float4 Position : POSITION;
   float2 Displacement : TEXCOORD;
};

PositionDisplacement PullIn_VertexShader(PositionNormal input)
{
   PositionDisplacement output;

   output.Position = mul(input.Position, WorldViewProjection);
   float3 normalWV = mul(input.Normal, WorldView);
   normalWV.y = -normalWV.y;
   
   float amount = dot(normalWV, float3(0,0,1)) * DistortionScale;
   output.Displacement = float2(.5,.5) + float2(amount * normalWV.xy);

   return output;   
}

float4 DisplacementPassthrough_PixelShader(float2 displacement : TEXCOORD) : COLOR
{  
   return float4(displacement, 0, 1);
}

technique PullIn
{
    pass
    {
        VertexShader = compile vs_2_0 PullIn_VertexShader();
        PixelShader = compile ps_2_0 DisplacementPassthrough_PixelShader();
    }
}


//-----------------------------------------------------------------------------
//
// Zero Displacement (provided for reference)
//
//-----------------------------------------------------------------------------


float4 TransformOnly_VertexShader(float4 position : POSITION) : POSITION
{
    return mul(position, WorldViewProjection);
}

float4 ZeroDisplacement_PixelShader() : COLOR
{
    return float4(.5, .5, 0, 0);
}

technique ZeroDisplacement
{
    pass
    {
        VertexShader = compile vs_2_0 TransformOnly_VertexShader();
        PixelShader = compile ps_2_0 ZeroDisplacement_PixelShader();
    }
}