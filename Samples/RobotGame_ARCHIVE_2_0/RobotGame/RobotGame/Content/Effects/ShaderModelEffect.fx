//-----------------------------------------------------------------------------
// ShaderModelEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 View;
float4x4 Projection;

// ***** Light properties *****
float3 LightPosition;
float4 LightColor;
float4 AmbientLightColor;
// ****************************

// ***** material properties *****
// output from phong specular will be scaled by this amount
float Shininess;

// specular exponent from phong lighting model.  controls the "tightness" of
// specular highlights.
float SpecularPower;
// *******************************

//    NormalMap texture sampler
texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

//    SpecularMap texture sampler
texture SpecularMap;
sampler2D SpecularMapSampler = sampler_state
{
    Texture = <SpecularMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

//    Diffuse texture sampler
texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

//    Environment-Cube Map
texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
    Texture = <EnvironmentMap>;
};

struct VS_INPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 normal            : NORMAL0;    
    float3 binormal            : BINORMAL0;
    float3 tangent            : TANGENT0;
};

// output from the vertex shader, and input to the pixel shader.
// lightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked 
// with TEXCOORD3, it will actually take TEXCOORD3, 4, and 5.
struct VS_OUTPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 lightDirection    : TEXCOORD1;
    float3 viewDirection    : TEXCOORD2;
    float3x3 tangentToWorld    : TEXCOORD3;
};

VS_OUTPUT VertexShader( VS_INPUT input )
{
    VS_OUTPUT output;
    
    // transform the position into projection space
    float4 worldSpacePos = mul(input.position, World);
    output.position = mul(worldSpacePos, View);
    output.position = mul(output.position, Projection);
    
    // calculate the light direction ( from the surface to the light ), which is not
    // normalized and is in world space
    output.lightDirection = LightPosition - worldSpacePos;
        
    // similarly, calculate the view direction, from the eye to the surface.  not
    // normalized, in world space.
    float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));    
    output.viewDirection = worldSpacePos - eyePosition;    
    
    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.tangent,    World);
    output.tangentToWorld[1] = mul(input.binormal,    World);
    output.tangentToWorld[2] = mul(input.normal,    World);
    
    // pass the texture coordinate through without additional processing
    output.texCoord = input.texCoord;
    
    return output;
}

float4 PixelShader( VS_OUTPUT input ) : COLOR0
{    
    // look up the normal from the normal map, and transform from tangent space
    // into world space using the matrix created above.  normalize the result
    // in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord);
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    input.viewDirection = normalize(input.viewDirection);
    input.lightDirection = normalize(input.lightDirection);    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float dotLight = max(dot(normalFromMap, input.lightDirection), 0);
    float4 diffuse = LightColor * dotLight;
    
    float3 reflectCoord = reflect( normalFromMap, input.viewDirection);
    float4 environmentColor = texCUBE( EnvironmentMapSampler, reflectCoord);    
   
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.        
    float3 reflectedLight = reflect(input.lightDirection, normalFromMap);
    float dotView = max(dot(reflectedLight, input.viewDirection), 0);        
    
    float4 specularColor = tex2D( SpecularMapSampler, input.texCoord) * 
                            float4(2, 2, 2, 255);
                            
    float3 specular = specularColor.rgb * pow(dotView, specularColor.a);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);

    AmbientLightColor.rgb = AmbientLightColor.rgb * environmentColor.rgb;
    
    // return the combined result.
    return float4( (diffuse.rgb + AmbientLightColor.rgb) * diffuseTexture.rgb + 
                specular.rgb, diffuseTexture.a);
}

Technique NormalMapping
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_a PixelShader();
    }
}