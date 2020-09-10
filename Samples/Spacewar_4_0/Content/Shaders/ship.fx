//-----------------------------------------------------------------------------
// Ship.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//Input variables
float4x4 world;
float4x4 inverseWorld;
float4x4 worldViewProjection;
float4 viewPosition;

float4 Ambient;
float4 DirectionalDirection;
float4 DirectionalColor;
float4 PointPosition;
float4 PointColor;
float PointFactor;

float4 Material;

texture SkinTexture;
sampler Skin = sampler_state
{
   Texture = (SkinTexture);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture NormalMapTexture;
sampler NormalMap = sampler_state
{
   Texture = (NormalMapTexture);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture ReflectionTexture;
sampler Reflection = sampler_state
{
   Texture = (ReflectionTexture);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};


struct VS_INPUT
{
    float4 ObjectPos: POSITION;
    float2 Tex : TEXCOORD0;
    float3 ObjectNormal : NORMAL;
};

struct VS_OUTPUT 
{
   float4 ScreenPos:   POSITION;
   float2 Tex:   TEXCOORD0;
   float3 WorldNormal: TEXCOORD1;
   float4 PointDirection: COLOR;
};

float4 DirectionalLight(float3 WorldNormal)
{
    return DirectionalColor * saturate(dot(WorldNormal, normalize(DirectionalDirection)));
}

float4 PointLight(float3 Normal, float4 PointDirection)
{
    return PointColor * saturate(PointDirection.w * dot(Normal, normalize(PointDirection.xyz)));
}

VS_OUTPUT ShipVS(VS_INPUT In)
{
    VS_OUTPUT Out;

    //Move to screen space
    Out.ScreenPos = mul(In.ObjectPos, worldViewProjection);
    
    float4 WorldPos = mul(In.ObjectPos, world);
    float4 WorldNormal = normalize(mul(In.ObjectNormal, world));  // First optimization, lets not normalize per pixel, eh?
    
    //Pass on texture coordinates and normal
    Out.Tex = In.Tex;
    Out.WorldNormal = WorldNormal;

    //Direction of point light to this vertex
    float4 lightDir;
    lightDir= PointPosition - WorldPos;
    float dist = length(lightDir);
    lightDir = lightDir/dist;
    
    //Store attenuation in w
    lightDir.w = saturate(1/(PointFactor * dist));

    float4 Directional = DirectionalLight(WorldNormal);
    float4 Point = PointLight(WorldNormal, lightDir);
    
    Out.PointDirection = Directional + Point + Ambient;

    return Out;
}

float4 ShipPS( float2 tex: TEXCOORD0, float3 WorldNormal : TEXCOORD1, float4 LightColor : COLOR)   : COLOR 
{
    //Specular map is in alpha of diffusemap and material
    float MattFactor = saturate(Material.a * tex2D(Skin, tex).a);

    //Add it all up
    float4 retval;

    retval= saturate( Material *
                    (((LightColor) *  float4(tex2D(Skin, tex).rgb, 1)) +
                    (1-MattFactor) * texCUBE(Reflection, WorldNormal)));
    return retval;
}



//--------------------------------------------------------------//
// Technique Section for Ship Effects
//--------------------------------------------------------------//
technique Ship
{
   pass Single_Pass
   {
        ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;        
        ALPHABLENDENABLE = FALSE;        
        
        CULLMODE = CCW;

        VertexShader = compile vs_1_1 ShipVS();
        PixelShader = compile ps_2_0 ShipPS();
   }

}
