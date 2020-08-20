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

float3 cameraPosition;

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
	float3 WorldNormal : TEXCOORD1;
};

struct AtmosBumpVertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Tex : TEXCOORD0;
    float3x3 tangentToWorld : TEXCOORD3;	
};

float DiffuseLight(float3 Normal, float3 PointDirection)
{
	return saturate(dot(Normal, PointDirection));
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.WorldNormal = normalize(mul(input.Normal, World));
	
	output.Tex = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Direction of point light to this vertex
	float3 lightDir;
	lightDir = normalize(DirectionalLight);
	float DiffuseDot = DiffuseLight(input.WorldNormal, lightDir);
	float4 lightColor = DirectionalLightColor * DiffuseDot;
	
	float4 output = tex2D(Diffuse, input.Tex);
	float alpha = output.a;
	output *= lightColor;
	output.a = alpha;
	
	return output;
}

AtmosBumpVertexShaderOutput AtmosBumpVertexShaderFunction(VertexShaderInput input)
{
    AtmosBumpVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Tex = input.TexCoord;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.Tangent, World);
    output.tangentToWorld[1] = mul(input.Binormal, World);
    output.tangentToWorld[2] = mul(input.Normal, World);
    
    return output;
}

float4 AtmosBumpPixelShaderFunction(AtmosBumpVertexShaderOutput input) : COLOR0
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
	
    //Combine colors     
    float4 output = tex2D(Diffuse, input.Tex);
    float alpha = output.a;
    output *= DiffuseColor;
    output.a = alpha;
    
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
    
    pass Pass2
    {
        VertexShader = compile vs_2_0 AtmosBumpVertexShaderFunction();
        PixelShader = compile ps_2_0 AtmosBumpPixelShaderFunction();
	}    
}
