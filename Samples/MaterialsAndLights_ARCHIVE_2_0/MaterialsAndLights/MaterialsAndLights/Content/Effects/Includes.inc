//////////////////////////////////////////////////////////////
// Example 4.1                                              //
//                                                          //
// An HLSL include file is similar to a C++ include file,   //
// and may contain helper functions, shader functions,      //
// parameters, structure definitions, or any other          //
// constructs you plan to share between multiple files.     //
//                                                          //
// An include file can be any text file regardless of its   //
// extension.                                               //
//////////////////////////////////////////////////////////////



//////////////////////////////////////////////////////////////
// Example 4.2                                              //
//                                                          //
// The light structure is the shader-code implementation of //
// the paramters for an individual light.  While this isn't //
// much different from any other type of shader parameters, //
// the way in which the C# code interacts with a structure  //
// is somewhat different.                                   //
//////////////////////////////////////////////////////////////
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


//////////////////////////////////////////////////////////////
// Example 4.3                                              //
//                                                          //
// Even though the shared parameters here are listed in an  //
// include file, they are still shared across the pools for //
// the effect that references the include.                  //
//                                                          //
// Shared parameters are primarily useful for clarity, but  //
// rarely exhibit optimal performance when switching        //
// between effects in the same effect pool.  Effects will   //
// set the underlying shader constant registers on each     //
// Begin call even if shared data hasn't changed, resulting //
// in redundant state setting.                              //
//////////////////////////////////////////////////////////////
shared float4x4 view;
shared float4x4 projection;
shared float3 cameraPosition;
shared float4 ambientLightColor;
shared float numLights = 1;

//////////////////////////////////////////////////////////////
// Example 4.4                                              //
//                                                          //
// When multiple sampler or variables are defined           //
// in an effect, they correspond to a state on the graphics //
// device.  The order in which they are defined in the      //
// effect corresponds to the sampler number.                //
//                                                          //
// In this case, diffuseSampler would correspond to         //
// graphics.GraphicsDevice.Sampler[0] since it is the first //
// sampler defined in the effect.                           //
//////////////////////////////////////////////////////////////
sampler diffuseSampler;
sampler specularSampler;

//texture parameters can be used to set states in the 
//effect state pass code
texture2D diffuseTexture;
texture2D specularTexture; 

//the world paramter is not shared because it will
//change on every Draw() call
float4x4 world;


//////////////////////////////////////////////////////////////
// Example 4.5                                              //
//                                                          //
// The material paramters are listed here, and they         //
// correspond to the instance of the effect being applied   //
// in a given draw call.                                    //
//////////////////////////////////////////////////////////////
float4 materialColor;
float specularPower;
float specularIntensity;
bool diffuseTexEnabled = false;
bool specularTexEnabled = false;
float textureUReps = 1.0;
float textureVReps = 1.0;



//////////////////////////////////////////////////////////////
// Example 4.6                                              //
//                                                          //
// An include file may include entire shader functions.     //
// In this case, the simple vertex shader in this sample    //
// can be compiled in shader model 2 or 3, so it has been   //
// included here.                                           //
//////////////////////////////////////////////////////////////
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

//////////////////////////////////////////////////////////////
// Example 4.7                                              //
//                                                          //
// This function calculates the effect of a single light    //
// on a pixel.  It takes the sepcular and diffuse values    //
// into consideration, as well as various other material    //
// properties.  This function calculates Lambertian diffuse //
// lighting and Phong specular highlights and returns their //
// sum.                                                     //
//////////////////////////////////////////////////////////////
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