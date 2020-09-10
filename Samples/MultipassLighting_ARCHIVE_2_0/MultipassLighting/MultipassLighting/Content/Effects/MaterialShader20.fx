//The includes file contains definitions for the structures,
//inputs and outputs used by the material shader
#include "Includes.inc"


Light lights[1];

//////////////////////////////////////////////////////////////
// Example 4.1:  One Light Per Draw                         //
//                                                          //
// Each light gets its own pass, but there are only enough  //
// instructions in PixelShader 2.0 to to display a single   //
// light using the CalculateSingleLight equation.           //
//////////////////////////////////////////////////////////////
float4 SingleLightPS(PixelShaderInput input) : COLOR
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
     
    float4 color = CalculateSingleLight(lights[0], 
                            input.WorldPosition, input.WorldNormal,
                            diffuseColor, specularColor);
        
     
     color.a = 1.0;
     return color;
}




technique MultipleLights
{
     //////////////////////////////////////////////////////////////
     // Example 4.2: Ambient Pass                                //
     //                                                          //
     // This pass sets up the necessary shader states and and    //
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
       
        
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 AmbientPS();
    }
    pass PointLight
    {
        
        //////////////////////////////////////////////////////////////
        // Example 4.3: Shader Model 2.0                            //
        //                                                          //
        // By compiling to a lower shader model, this shader        //
        // supports a broader range of PC hardware.  This shader    //
        // won't be used on the XBox 360.                           //
        //////////////////////////////////////////////////////////////
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 SingleLightPS();
    
    }
    
}