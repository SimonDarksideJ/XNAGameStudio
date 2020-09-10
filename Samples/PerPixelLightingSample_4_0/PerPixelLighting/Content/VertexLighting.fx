float4x4 world;
float4x4 view;
float4x4 projection;

//light properties
float3 lightPosition;
float4 ambientLightColor;
float4 diffuseLightColor;
float4 specularLightColor;

//////////////////////////////////////////////////////////////
// Example 1.1                                              //
//                                                          //
// The specular power and intensity values are used to      //
// customize the "shininess" properties of a material.      //
//////////////////////////////////////////////////////////////
float specularPower;
float specularIntensity;

//////////////////////////////////////////////////////////////
// Example 1.2                                              //
//                                                          //
// The eye position (or camera position) is necessary to    //
// calculate the specular component.                        //
//////////////////////////////////////////////////////////////
float3 cameraPosition;

struct VertexShaderOutput 
{
     float4 Position : POSITION;
     float4 Color : COLOR0;
};

struct PixelShaderInput
{
     float4 Color: COLOR0;
};



VertexShaderOutput VertexDiffuseAndPhong(
     float3 position : POSITION,
     float3 normal : NORMAL )
{
     VertexShaderOutput output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);
     
    
     //////////////////////////////////////////////////////////////
     // Example 1.3                                              //
     //                                                          //
     // Here we calculate the world-space components for use     //
     // in the lighting calculations, since our eye position     //
     // and light position are both represented in world space   //
     //////////////////////////////////////////////////////////////
     float3 worldNormal =  mul(normal, world);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     
     //////////////////////////////////////////////////////////////
     // Example 1.4                                              //
     //                                                          //
     // to ensure the postion coordinates are homogenous, we     //
     // divideby the W component so that W == 1                  //
     //////////////////////////////////////////////////////////////
     worldPosition = worldPosition / worldPosition.w;
    
    
     //////////////////////////////////////////////////////////////
     // Example 1.5                                              //
     //                                                          //
     // This section simply calculates the diffuse color at      //
     // the vertex similar to Shader Series 1: Vertex Lighting.  //
     //////////////////////////////////////////////////////////////
     float3 directionToLight = normalize(lightPosition - worldPosition.xyz);
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseLightColor * diffuseIntensity;
     
     
     
     //////////////////////////////////////////////////////////////
     // Example 1.6                                              //
     //                                                          //
     // For the first part of specular lighting, we calculate a  //
     // direct reflection vector for the light.                  //
     //                                                          //
     // The equation for a reflection vector can be found in     //
     // the sample documentation.  Fortunately, HLSL provides a  //
     // handy reflect() function.                                //
     //////////////////////////////////////////////////////////////
     float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
        
     
     //////////////////////////////////////////////////////////////
     // Example 1.7                                              //
     //                                                          //
     // The next part of the specular light is to approximate    //
     // specular highlight by taking the dot product of the      //
     // reflection vector and the view direction to the          //
     // specular power and multiplying by specular intensity.    //
     //////////////////////////////////////////////////////////////
     float3 directionToCamera = normalize(cameraPosition - worldPosition.xyz);
     float4 specular = specularLightColor * specularIntensity * 
                       pow(saturate(dot(reflectionVector, directionToCamera)),
                           specularPower);
     
     
     //////////////////////////////////////////////////////////////
     // Example 1.8                                              //
     //                                                          //
     // This section simply calculates the diffuse color at      //
     // the vertex similar to Shader Series 1: Vertex Lighting.  //
     //////////////////////////////////////////////////////////////
     output.Color = specular + diffuse + ambientLightColor;
     output.Color.a = 1.0;


     //return the output structure
     return output;
}

VertexShaderOutput VertexDiffuse(
     float3 position : POSITION,
     float3 normal : NORMAL )
{
     VertexShaderOutput output;
     
     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);
     
     //get world space vectors
     float3 worldNormal =  mul(normal, world);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     worldPosition = worldPosition / worldPosition.w;
     
     //calculate diffuse light
     float3 directionToLight = normalize(lightPosition - worldPosition.xyz);
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseLightColor * diffuseIntensity;
     
     //sum components for final color
     output.Color = diffuse + ambientLightColor;
     output.Color.a = 1.0;


     //return the output structure
     return output;
}



float4 SimplePixelShader(PixelShaderInput input) : COLOR
{
     return input.Color;
}


technique PerVertexDiffuse
{
    pass P0
    {
          //set the VertexShader state to the vertex shader function
          VertexShader = compile vs_2_0 VertexDiffuse();
          
          //set the PixelShader state to the pixel shader function          
          PixelShader = compile ps_2_0 SimplePixelShader();
    }
}

technique PerVertexDiffuseAndPhong
{
    pass P0
    {
          //set the VertexShader state to the vertex shader function
          VertexShader = compile vs_2_0 VertexDiffuseAndPhong();
          
          //set the PixelShader state to the pixel shader function          
          PixelShader = compile ps_2_0 SimplePixelShader();
    }
}