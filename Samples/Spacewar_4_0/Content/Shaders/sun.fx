//-----------------------------------------------------------------------------
// Sun.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//Input variables
float4x4 world;
float4x4 worldViewProjection;
float blendFactor;

texture Sun_Tex0;
sampler Sun0 = sampler_state
{
   Texture = (Sun_Tex0);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture Sun_Tex1;
sampler Sun1 = sampler_state
{
   Texture = (Sun_Tex1);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

struct VS_OUTPUT 
{
   float4 Pos:   POSITION;
   float2 tex:   TEXCOORD0;
   float2 tex1:  TEXCOORD1;
};

VS_OUTPUT SunVS(float4 Pos: POSITION)
{
   VS_OUTPUT Out;

   //Move to screen space
   Out.Pos = mul(Pos, worldViewProjection);
    
   //Texture coords deduced from postion
   Out.tex = Pos.xy;
   Out.tex1 = Pos.xy;

   return Out;
}

float4 SunPS( float2 tex: TEXCOORD0, float2 tex1: TEXCOORD1 )   : COLOR 
{
    float4 color0 = tex2D(Sun0, tex);
    float4 color1 = tex2D(Sun1, tex1);

    float4 color = saturate( (color0 * blendFactor) + (color1 * (1.0f - blendFactor)) );
    return color;
}


//--------------------------------------------------------------//
// Technique Section for Sun Effects
//--------------------------------------------------------------//
technique Sun
{
   pass Single_Pass
   {
      CULLMODE = NONE;
      ALPHABLENDENABLE = TRUE;
      SRCBLEND = SRCALPHA;
      DESTBLEND = INVSRCALPHA;
      ZENABLE = FALSE; //Always want this on top

      VertexShader = compile vs_1_1 SunVS();
      PixelShader = compile ps_2_0 SunPS();
   }

}
