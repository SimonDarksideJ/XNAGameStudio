//-----------------------------------------------------------------------------
// Lighting.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// 
// Based on the Ashikhmin/Shirley lighting technique, first published in their 
// paper “An Anisotropic Phong Light Reflection Model”.
//
// For more information, please see "Pickture.html", distributed with this game.
//
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 WorldView;
float4x4 WorldViewProjection;

float3 LightPos;
float3 CameraPos;

const float Pi = 3.14159f;

const float DiffuseReflectance = 0.95f;
const float SpecularReflectance = 0.03f;
const float PhongExponentU = 700.0f;
const float PhongExponentV = 700.0f;

// An ambient light value that brightens the entire image
// -- a "moodier" option would be to set this to zero.
const float ambientTerm = 0.3;

// Use: ((28.0f * DiffuseReflectance) / (23.0f * Pi)) * (1.0f - SpecularReflectance)
const float ashDiffuseConstantTerm = 0.3570886;

// Use: (sqrt((1.0f + PhongExponentU) * (1.0f + PhongExponentV))) / (8.0f * Pi)
const float ashSpecularConstantTerm = 27.89193;

float GlowScale;
float AlphaOverride;

float2 TexCoordScale;
float2 TexCoordTranslation;

float4 ColorOverride;



texture DiffuseTexture;
sampler DiffuseSampler = sampler_state
{
    Texture = <DiffuseTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};



struct Ash_VS_Output
{
    float4 Position : POSITION;
    float2 Texture0 : TEXCOORD0;
    float3 vertexToLight : TEXCOORD1;
    float3 vertexToViewer : TEXCOORD2;
    float3 Normal : TEXCOORD3;
    float3 Tangent : TEXCOORD4;
    float3 Binormal : TEXCOORD5;
};

Ash_VS_Output Ash_VS(float4 vPos : POSITION,
                     float3 vNormal : NORMAL,
                     float2 vTexCoord0 : TEXCOORD0,
                     float3 vTangent : TANGENT)
{
    Ash_VS_Output Output;
    
    Output.Position = mul(vPos, WorldViewProjection);
    Output.Texture0 = vTexCoord0;
    
    Output.Tangent = vTangent;
    Output.Normal = vNormal;
    Output.Binormal = normalize(cross(vTangent, vNormal));
    Output.Tangent = normalize(cross(vNormal, Output.Binormal));
    
    Output.Tangent = mul(Output.Tangent, World);
    Output.Normal = mul(Output.Normal, World);
    Output.Binormal = mul(Output.Binormal, World);
    
    float3 worldPosition = mul(vPos, World);
    Output.vertexToLight = normalize(LightPos - worldPosition);
    Output.vertexToViewer = normalize(CameraPos - worldPosition);

    return Output;
}


float4 Ash_Diffuse(float3 vectorToLight, float3 vectorToViewer, float3 normal)
{
    return saturate(dot(vectorToLight, normal) * dot(vectorToViewer, normal) *
        DiffuseReflectance * (1 - SpecularReflectance));
}

float4 Ash_Specular(float3 vectorToLight, float3 vectorToViewer, float3 normal,
                    float3 tangent, float3 binormal, float4 specularColor)
{
    float3 halfVector = (vectorToLight + vectorToViewer) / 2.0f;

    float hDotN = dot(halfVector, normal);
    float hDotT = dot(halfVector, tangent);
    float hDotB = dot(halfVector, binormal);

    float numeratorPower = (PhongExponentU * hDotT * hDotT) +
        (PhongExponentV * hDotB * hDotB);
    numeratorPower /= (1 - (hDotN * hDotN));
    
    float numerator = pow(hDotN, numeratorPower);
    float denominator = dot(halfVector, vectorToLight) * max(dot(normal, vectorToLight),
        dot(normal, vectorToViewer));
    float fresnelTerm = SpecularReflectance + ((1 - SpecularReflectance) *
        pow((1 - dot(vectorToLight, halfVector)), 5));

    return (ashSpecularConstantTerm * numerator * specularColor * fresnelTerm) /
        denominator;
}

float4 Ash_General_PS(Ash_VS_Output In, float4 diffuseColor)
{
    float3 tangent = normalize(In.Tangent);
    float3 normal = normalize(In.Normal);
    float3 binormal = normalize(In.Binormal);
    
    float4 diffuseTerm = Ash_Diffuse(In.vertexToLight, In.vertexToViewer, normal);
    float4 color = saturate(diffuseColor * (ambientTerm + diffuseTerm));
    color += Ash_Specular(In.vertexToLight, In.vertexToViewer, normal, tangent,
                          binormal, diffuseColor);
    color += (GlowScale * ColorOverride * diffuseColor);
    color *= ColorOverride;
    
    return color;
}

float4 Ash_NoTexture_PS(Ash_VS_Output In) : COLOR
{
    float4 diffuseColor = 1.0;
    
    return Ash_General_PS(In, diffuseColor);
}

float4 Ash_SingleTexture_PS(Ash_VS_Output In) : COLOR
{
    // Texture
    float2 textureCoord = (In.Texture0 * TexCoordScale) + TexCoordTranslation;
    float4 diffuseColor = tex2D(DiffuseSampler, textureCoord);
    
    return Ash_General_PS(In, diffuseColor);
}


technique NoTexture
{
    pass P0
    {
        VertexShader = compile vs_2_0 Ash_VS();
        PixelShader = compile ps_2_0 Ash_NoTexture_PS();
    }
}

technique SingleTexture
{
    pass P0
    {
        VertexShader = compile vs_2_0 Ash_VS();
        PixelShader = compile ps_2_0 Ash_SingleTexture_PS();
    }
}