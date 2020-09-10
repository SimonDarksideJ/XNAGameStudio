//-----------------------------------------------------------------------------
// ShatterEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


float4x4 WorldViewProjection;
float4x4 World : World;
float RotationAmount;
float TranslationAmount;
texture modelTexture;
float time;

// Lighting Data
float3 eyePosition;
float3 lightPosition;
float4 ambientColor;
float4 diffuseColor;
float4 specularColor;
float specularPower;


sampler TextureSampler = sampler_state
{
    Texture = <modelTexture>;
    MinFilter = Anisotropic;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    MaxAnisotropy = 8;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

struct VertexShaderOutput
{
     float4 Position : POSITION;
     float3 WorldNormal : TEXCOORD0;
     float3 WorldPosition : TEXCOORD1;
     float2 texCoords : TEXCOORD2;
     float4 Color : COLOR0;
};

struct PixelShaderInput
{
     float3 WorldNormal : TEXCOORD0;
     float3 WorldPosition : TEXCOORD1;
     float2 TexCoords : TEXCOORD2;
     float4 Color: COLOR0;
};

struct InputVS
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoords : TEXCOORD0;
    float3 TriangleCenter : TEXCOORD1;
    float3 RotationalVelocity : TEXCOORD2;        
};

// Helper function to create a YawPitchRoll Matrix. 
// Used to rotate the verticies by the random rotational values generated in the 
// processor.
float4x4 CreateYawPitchRollMatrix(float x, float y, float z)
{
    float4x4 result;
        
    result[0][0] = cos(z)*cos(y) + sin(z)*sin(x)*sin(y);
    result[0][1] = -sin(z)*cos(y) + cos(z)*sin(x)*sin(y);
    result[0][2] = cos(x)*sin(y);
    result[0][3] = 0;
    
    result[1][0] = sin(z)*cos(x);
    result[1][1] = cos(z)*cos(x);
    result[1][2] = -sin(x);
    result[1][3] = 0;
    
    result[2][0] = cos(z)*-sin(y) + sin(z)*sin(x)*cos(y);
    result[2][1] = sin(z)*sin(y) + cos(z)*sin(x)*cos(y);
    result[2][2] = cos(x)*cos(y);
    result[2][3] = 0;
    
    result[3][0] = 0;
    result[3][1] = 0;
    result[3][2] = 0;
    result[3][3] = 1;    

    return result;
}

VertexShaderOutput ShatterVS(InputVS input) 
{
    VertexShaderOutput output = (VertexShaderOutput)0;    
    
    // Shattering Calculations
    //
    // The shatter effect is pretty simple. First we create a YawPitchRoll Matrix based
    // on the random values we generated in the processor. 
    // Second, we transform the vertex position by rotating it around it's center using
    // the rotation Matrix. Third, we translate the vertex along it's normal to have it
    // move outwards. Last, we drop the vertex along it's Y axis as a function of
    // time^2 to give a falling effect.

    
    // Create a rotation matrix
    input.RotationalVelocity *= RotationAmount;
    float4x4 rotMatrix = CreateYawPitchRollMatrix(input.RotationalVelocity.x, 
                                                  input.RotationalVelocity.y, 
                                                  input.RotationalVelocity.z);
    
    // Rotate the vertex around its triangle's center
    float3 position = input.TriangleCenter + mul(input.Position - input.TriangleCenter, 
                                            rotMatrix);
    
    // Displace the vertex along its normal
    position += input.Normal * TranslationAmount;    
    
    // Move the vertex downward as a function of time^2 to give a nice curvy falling
    // effect.
    position.y -= time*time * 200;                 
    
    // Proceed as usual
    
    output.Position = mul(float4(position,1.0),WorldViewProjection);
    
    // We must rotate the Normal as well for accurate lighting calculations.
    output.WorldNormal = mul(mul(input.Normal, rotMatrix), World); 
    float4 worldPosition = mul(float4(position,1.0),World);
    output.WorldPosition = worldPosition / worldPosition.w;
    output.texCoords = input.TexCoords;
    
    //calculate diffuse component
    float3 directionToLight = normalize(lightPosition - output.WorldPosition);
    float diffuseIntensity = saturate( dot(directionToLight, output.WorldNormal));
    float4 diffuse = diffuseColor * diffuseIntensity;
    
    output.Color = diffuse + ambientColor;
    
    return output;
}

float4 PhongPS(PixelShaderInput input) : COLOR
{
     float3 directionToLight = normalize(lightPosition - input.WorldPosition);
     float3 reflectionVector = normalize(reflect(-directionToLight, input.WorldNormal));
     float3 directionToCamera = normalize(eyePosition - input.WorldPosition);
     
     //calculate specular component
     float4 specular = specularColor *  
                       pow( saturate(dot(reflectionVector, directionToCamera)), 
                       specularPower);
     
     float4 TextureColor   = tex2D(TextureSampler, input.TexCoords);
     
     float4 color = TextureColor * input.Color + specular;
     color.a = 1.0;                   
          
     return color;
}

technique
{
    pass
    {        
        VertexShader = compile vs_2_0 ShatterVS();
        PixelShader = compile ps_2_0 PhongPS();
    }
}