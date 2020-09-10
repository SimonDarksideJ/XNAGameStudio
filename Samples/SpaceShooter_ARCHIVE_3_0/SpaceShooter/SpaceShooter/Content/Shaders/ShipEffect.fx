float4x4 World;
float4x4 View;
float4x4 Projection;

texture DiffuseTexture;
sampler Diffuse = sampler_state
{
   Texture = (DiffuseTexture);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture SpecularTexture;
sampler Specular = sampler_state
{
   Texture = (SpecularTexture);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture NormalTexture;
sampler NormalMap = sampler_state
{
   Texture = (NormalTexture);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture EmissiveTexture;
sampler EmissiveMap = sampler_state
{
   Texture = (EmissiveTexture);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
    float3x3 tangentToWorld : TEXCOORD3;
};

float3 DirectionalLight;
float4 DirectionalLightColor : COLOR;

float specularPower;
float specularIntensity;

float3 cameraPosition;

float DiffuseLight(float3 Normal, float3 PointDirection)
{
	return saturate(dot(Normal, PointDirection));
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.WorldPosition = worldPosition;
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.WorldNormal = normalize(mul(input.Normal, World));
	output.TexCoord = input.TexCoord;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.Tangent, World);
    output.tangentToWorld[1] = mul(input.Binormal, World);
    output.tangentToWorld[2] = mul(input.Normal, World);
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Direction of light
	float3 lightDir;
	lightDir = normalize(DirectionalLight);
	
	//Diffuse light with bump
	float3 localNormal = tex2D(NormalMap, input.TexCoord) * 2.0 - 1.0f;
	
	float3 bumpNormal;
	bumpNormal.y = -dot(input.tangentToWorld[0], lightDir);
	bumpNormal.x = dot(input.tangentToWorld[1], lightDir);
	bumpNormal.z = dot(input.tangentToWorld[2], lightDir);
	float DiffuseDot = DiffuseLight(localNormal, bumpNormal);
	float4 DiffuseColor = DirectionalLightColor * DiffuseDot;
	
	//Specular component
    float3 reflectionVector = normalize(reflect(lightDir, input.WorldNormal));
    float3 directionToCamera = normalize(cameraPosition - input.WorldPosition);
    float4 specular = DirectionalLightColor * specularIntensity * 
                      pow( saturate(dot(reflectionVector, directionToCamera)), 
                      specularPower);    
                      
    //Combine colors     
    float4 output = tex2D(Diffuse, input.TexCoord) * DiffuseColor;
    output += specular * tex2D(Specular, input.TexCoord);
    output += tex2D(EmissiveMap, input.TexCoord);
    output.a = 1;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
