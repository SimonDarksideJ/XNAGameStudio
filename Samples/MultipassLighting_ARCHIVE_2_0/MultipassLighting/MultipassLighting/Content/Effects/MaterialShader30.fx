//The includes file contains definitions for the structures,
//inputs and outputs used by the material shader
#include "Includes.inc"

//////////////////////////////////////////////////////////////
// Example 3.1: Multiple Lights Per Draw                    //
//                                                          //
// Each light is summed into a final pixel color.  The 3.0  //
// shader supports dynamic control instructions, which      //
// allows for loops to  behave as they would on a general   //
// purpose CPU.                                             //
//////////////////////////////////////////////////////////////
Light lights[12];

float4 MultipleLightPS(PixelShaderInput input) : COLOR
{
    float4 diffuseColor = materialColor;
    float4 specularColor = materialColor;
     
    if(diffuseTexEnabled)
    {
        diffuseColor *= tex2D(diffuseSampler, input.TexCoords);
    }
     
    if(specularTexEnabled)
    {
        specularColor *= tex2D(specularSampler, input.TexCoords);
    }
     
    float4 color = float4(0,0,0,0);
     
    //////////////////////////////////////////////////////////////
    // Example 3.2: Light Contribution Loop                     //
    //                                                          //
    // Each light is summed into a final pixel color.  The 3.0  //
    // shader supports dynamic control instructions, which      //
    // allows for loops to  behave as they would on a general   //
    // purpose CPU.                                             //
    //////////////////////////////////////////////////////////////
    for(int i=0; i< numLightsPerPass; i++)
    {
        color += CalculateSingleLight(lights[i], 
                  input.WorldPosition, input.WorldNormal,
                  diffuseColor, specularColor );
    }
    color.a = 1.0;
    return color;
}


technique MultipleLights
{

     //////////////////////////////////////////////////////////////
     // Example 3.3: Ambient Pass                                //
     //                                                          //
     // This pass sets up the per batch shader states and and    //
     // draws a single opaque ambient lighting pass.             //
     //////////////////////////////////////////////////////////////
    pass Ambient
    {
        //set sampler states
        MagFilter[0] = LINEAR;
        MinFilter[0] = LINEAR;
        MipFilter[0] = LINEAR;
        AddressU[0] = WRAP;
        AddressV[0] = WRAP;
        MagFilter[1] = LINEAR;
        MinFilter[1] = LINEAR;
        MipFilter[1] = LINEAR;
        AddressU[1] = WRAP;
        AddressV[1] = WRAP;
        
        //set texture states (notice the '<' , '>' brackets)
        //as the texture state assigns a reference
        Texture[0] = <diffuseTexture>;
        Texture[1] = <specularTexture>;
        
        VertexShader = compile vs_3_0 BasicVS();
        PixelShader = compile ps_3_0 AmbientPS();
    }
    pass PointLight
    {
        VertexShader = compile vs_3_0 BasicVS();
        PixelShader = compile ps_3_0 MultipleLightPS();
    }
}