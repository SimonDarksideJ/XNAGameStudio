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

texture NightTexture;
sampler Night = sampler_state
{
   Texture = (NightTexture);
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

texture NormalMapTexture;
sampler NormalMap = sampler_state
{
   Texture = (NormalMapTexture);
   MAGFILTER = Linear;
   MINFILTER = Linear;
   MIPFILTER = Linear;
};

float3 DirectionalLight;
float4 DirectionalLightColor : COLOR;
float4 hazeColor : COLOR;

float specularPower;
float specularIntensity;

float3 cameraPosition;
float3 cameraLookAt;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Tex : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
    float3x3 tangentToWorld : TEXCOORD3;	
};

float DiffuseLight(float3 Normal, float3 PointDirection)
{
	return saturate(dot(Normal, PointDirection));
}

VertexShaderOutput AllTextureVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.WorldPosition = worldPosition;
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.WorldNormal = normalize(mul(input.Normal, World));
	output.Tex = input.TexCoord;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.Tangent, World);
    output.tangentToWorld[1] = mul(input.Binormal, World);
    output.tangentToWorld[2] = mul(input.Normal, World);
    
    return output;
}

float4 AllTexturePixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Direction of light
	float3 lightDir;
	lightDir = normalize(DirectionalLight);
	
	//Diffuse light with bump
	float3 localNormal = tex2D(NormalMap, input.Tex) * 2.0 - 1.0f;
	
	float3 bumpNormal;
	bumpNormal.y = -dot(input.tangentToWorld[0], lightDir);
	bumpNormal.x = dot(input.tangentToWorld[1], lightDir);
	bumpNormal.z = dot(input.tangentToWorld[2], lightDir);
	float DiffuseDot = DiffuseLight(localNormal, bumpNormal);
	float4 DiffuseColor = DirectionalLightColor * DiffuseDot;
	
	//Specular component
    float3 reflectionVector = normalize(reflect(-lightDir, input.WorldNormal));
    float3 directionToCamera = normalize(cameraPosition - input.WorldPosition);
    float4 specular = DirectionalLightColor * specularIntensity * 
                      pow( saturate(dot(reflectionVector, directionToCamera)), 
                      specularPower);    
                      
    //Haze Component
    float hazeComponent = dot(directionToCamera, input.WorldNormal);
    float haze = max(0, pow(1.0f - hazeComponent, 2.0f));
    
    //Combine colors
    float nightMod = pow((1.0 - DiffuseDot), 10.0);
    float4 output = tex2D(Diffuse, input.Tex) * DiffuseColor;
    output += haze * hazeColor * DiffuseColor;
    output += specular * tex2D(Specular, input.Tex);
    output += tex2D(Night, input.Tex) * nightMod;
    
    output.a = 1;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 AllTextureVertexShaderFunction();
        PixelShader = compile ps_2_0 AllTexturePixelShaderFunction();
    }
}
