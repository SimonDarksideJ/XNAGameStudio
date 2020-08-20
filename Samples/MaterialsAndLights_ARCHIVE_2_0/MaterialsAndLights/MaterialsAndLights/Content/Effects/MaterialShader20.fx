#include "Includes.inc"


shared Light lights[2];

//////////////////////////////////////////////////////////////
// Example 6.1                                              //
//                                                          //
// The 2.0 shader makes some tradeoffs to get within the    //
// 64 instruction limit imposed by the pixel shader model.  //
// The total number of possible lights is 2, and both       //
// lights are calculated as there are no dynamic branches   //
// in pixel shader model 2.0.  To account for the fact that //
// not all the lights will be on, the color is multiplied   //
// a boolian comparison which will resolve to 0 if the      //
// current light is greater than the number of lights       //
// currently enabled.                                       //
//////////////////////////////////////////////////////////////
float4 MultipleLightPS(PixelShaderInput input) : COLOR
{

    float4 diffuseColor = materialColor;
    float4 specularColor = materialColor;
    
    if(diffuseTexEnabled)
    {
        diffuseColor *= tex2D(diffuseSampler, input.TexCoords);
    }
     
     
     
    float4 color = ambientLightColor * diffuseColor;
     
    //all color components are summed in the pixel shader
    for(int i=0; i< 2; i++)
    {
        color += CalculateSingleLight(lights[i], 
                            input.WorldPosition, input.WorldNormal,
                            diffuseColor, specularColor)
                            * (i < numLights);
        
     }
     color.a = 1.0;
     return color;
}




technique MultipleLights
{
     
    pass P0
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
        
        //set render states
        AlphaBlendEnable = FALSE;
    
    
        //////////////////////////////////////////////////////////////
        // Example 6.2                                              //
        //                                                          //
        // By compiling to a lower shader model, this shader        //
        // supports a broader range of PC hardware.  This shader    //
        // won't be used on the XBox 360.                           //
        //////////////////////////////////////////////////////////////
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 MultipleLightPS();
    }
}