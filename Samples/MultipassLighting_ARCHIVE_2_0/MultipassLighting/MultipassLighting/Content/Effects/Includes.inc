//structs shared between all effects
struct Light 
{
    float4 color;
    float4 position;
    float falloff;
    float range;
};
struct VertexShaderOutput
{
     float4 Position : POSITION;
     float2 TexCoords : TEXCOORD0;
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};
struct PixelShaderInput
{
     float2 TexCoords : TEXCOORD0;
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};


//shared scene parameters
shared float4x4 view;
shared float4x4 projection;
shared float3 cameraPosition;
shared float4 ambientLightColor;
shared int numLightsPerPass = 1;

sampler diffuseSampler;
sampler specularSampler;

//texture parameters can be used to set states in the 
//effect state pass code
texture2D diffuseTexture;
texture2D specularTexture; 

//the world paramter is not shared because it will
//change on every Draw() call
float4x4 world;


//these material paramters are set per effect instance
float4 materialColor;
float specularPower;
float specularIntensity;
bool diffuseTexEnabled = false;
bool specularTexEnabled = false;
float textureUReps = 1.0;
float textureVReps = 1.0;



//This function transforms the model to projection space and set up
//interpolators used by the pixel shader
VertexShaderOutput BasicVS(
     float3 position : POSITION,
     float3 normal : NORMAL,
     float2 texCoord : TEXCOORD0 )
{
     VertexShaderOutput output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);

     output.WorldNormal =  mul(normal, world);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     output.WorldPosition = worldPosition / worldPosition.w;
     
     //copy the tex coords to the interpolator
     output.TexCoords.x = texCoord.x * textureUReps;
     output.TexCoords.y = texCoord.y * textureVReps;

     //return the output structure
     return output;
}

//The Ambient pixel shader simply adds an ambient color to the
//back buffer while outputting depth information.
float4 AmbientPS(PixelShaderInput input) : COLOR
{
	float4 diffuseColor = materialColor;
	
	if(diffuseTexEnabled)
    {
        diffuseColor *= tex2D(diffuseSampler, input.TexCoords);
    }
	
	float4 color = ambientLightColor * diffuseColor;
	
	return color;
}


//This function calculates the diffuse and specular effect of a single light
//on a pixel given the world position, normal, and material properties
float4 CalculateSingleLight(Light light, float3 worldPosition, float3 worldNormal, 
                            float4 diffuseColor, float4 specularColor )
{
     float3 lightVector = light.position - worldPosition;
     float lightDist = length(lightVector);
     float3 directionToLight = normalize(lightVector);
     
     //calculate the intensity of the light with exponential falloff
     float baseIntensity = pow(saturate((light.range - lightDist) / light.range),
                                 light.falloff);
     
     
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseIntensity * light.color * diffuseColor;

     //calculate Phong components per-pixel
     float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
     float3 directionToCamera = normalize(cameraPosition - worldPosition);
     
     //calculate specular component
     float4 specular = saturate(light.color * specularColor * specularIntensity * 
                       pow(saturate(dot(reflectionVector, directionToCamera)), 
                           specularPower));
                           
     return  baseIntensity * (diffuse + specular);
}