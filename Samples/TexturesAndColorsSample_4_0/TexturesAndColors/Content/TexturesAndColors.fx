float4x4 world;
float4x4 view;
float4x4 projection;

float4 lightColor;
float3 lightDirection;
float4 ambientColor;

//////////////////////////////////////////////////////////////
// Example 1.1                                              //
//                                                          //
// Textures may be set as EffectParameters as any other     //
// uniform data.                                            //
//////////////////////////////////////////////////////////////
texture modelTexture;

//////////////////////////////////////////////////////////////
// Example 1.2                                              //
//                                                          //
// Samplers are defined in the Effect code, and referred    //
// to by name in sampling instrinsic functions (like tex2D) //
// elsewhere in the effect.                                 //
//////////////////////////////////////////////////////////////
sampler ModelTextureSampler = 
sampler_state
{
    // all samplers must specify which texture they are sampling from.
    Texture = <modelTexture>;
    
    //The MinFilter describes how the texture sampler will read for pixels
    //larger than one texel.
    MinFilter = Linear;
    //The MagFilter describes how the texture sampler will read for pixels
    //smaller than one texel.
    MagFilter = Linear;
    //The MipFilter describes how the texture sampler will combine different
    //mip levels of the texture.
    MipFilter = Linear;

    //The AddressU and AddressV values describe how the texture sampler will treat
    //a texture coordinate that is outside the range of [0-1].
    //In this case, it "clamps", where all values less than 0 are treated as 0, 
    //and all values greater than 1 are treated as 1.  
    AddressU = Clamp;
    AddressV = Clamp;
};


struct VertexShaderInput 
{
     float3 Position : POSITION;
     float4 Normal : NORMAL;
//////////////////////////////////////////////////////////////
// Example 1.3                                              //
//                                                          //
// The TEXCOORD0 and COLOR0 semantics now give us useful    //
// data, as the versions of the models for this sample      //
// include texture coordinate and vertex color data.        //
//////////////////////////////////////////////////////////////
     float2 TextureCoordinate : TEXCOORD0;
     float4 Color : COLOR0;
};

struct VertexShaderOutput 
{
     float4 Position : POSITION;
     float4 Color : COLOR0;
     //The texture coordinate is interpolated across the triangle just like the color.
     float2 TextureCoordinate : TEXCOORD0;
};

struct PixelShaderInput
{
     float4 Color: COLOR0;
     //The interpolated texture coordinate for this pixel, calculated from the 
     //texture coordinate values passed to the rasterizer for the three vertices that
     //make up this triangle.
     float2 TextureCoordinate : TEXCOORD0;
};


//////////////////////////////////////////////////////////////
// Example 1.4                                              //
//                                                          //
// Note the parameters passed to the functions, new to this //
// sample.  These fill the "uniform" bool arguments on the  //
// functions above.  The techniques are evaluated when the  //
// effect is compiled, and the code is recompiled with each //
// boolean "variable" treated as a constant.  This means    //
// that the effect can duplicate code duplication while     //
// simultaneously avoiding often-costly run-time branches.  //
//////////////////////////////////////////////////////////////
VertexShaderOutput VertexShaderFunction(
     VertexShaderInput input,
     uniform bool modulateWithLight,
     uniform bool addToLight,
     uniform bool lightOnly )
{
     VertexShaderOutput output;

     //generate the world-view-proj matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(input.Position, 1.0), wvp);

     // calculate the lighting color
     float3 worldNormal =  mul(input.Normal, world);
     float diffuseIntensity = saturate( dot(-lightDirection, worldNormal));
     float4 diffuseColor = lightColor * diffuseIntensity;
     float4 lightingColor = diffuseColor + ambientColor;
     diffuseColor.a = 1.0;
     
     // calculate the final color
//////////////////////////////////////////////////////////////
// Example 1.5                                              //
//                                                          //
// "Modulation" is a mathematical operation where one value //
// is scaled by another, via multiplication.                //
//////////////////////////////////////////////////////////////
     if (modulateWithLight)
     {
         output.Color = input.Color * lightingColor;
     }
     else if (addToLight)
     {
//////////////////////////////////////////////////////////////
// Example 1.6                                              //
//                                                          //
// The "saturate" intrinsic function, new to this sample,   //
// caps the value (or each value in a vector type) to the   //
// range [0.0, 1.0].                                        //
//                                                          //
// The decision of whether to saturate a particular value   //
// is an important one in many complex shaders, including   //
// high-dynamic range (HDR) effects.                        //
//////////////////////////////////////////////////////////////
         output.Color = saturate(input.Color + lightingColor);
     }
     else if (lightOnly)
     {
         output.Color = lightingColor;
     }
     else
     {
         output.Color = input.Color;
     }
     
     // pass through the texture coordinate
     output.TextureCoordinate = input.TextureCoordinate;

     //return the output structure
     return output;
}

//This function also now features "uniform" parameters.
//These parameters are specified in the technique definition, allowing different
//techniques to specify different "paths" through one function.  This minimizes
//code duplication, a common source of bugs.
//These booleans are evaluated at compile-time, avoiding costly run-time
//code-path evalution.
float4 PixelShaderFunction(PixelShaderInput input,
                           uniform bool modulateWithTexture,
                           uniform bool addToTexture,
                           uniform bool textureOnly) : COLOR
{
//////////////////////////////////////////////////////////////
// Example 1.7                                              //
//                                                          //
// The "tex2D" intrinsic function reads from a 2D texture.  //
// In the XNA framework, this means a Texture2D object.     //
// There are similar texCUBE and tex3D functions for        //
// TextureCube and Texture3D, respectively.                 //
//////////////////////////////////////////////////////////////
     float4 textureColor = tex2D(ModelTextureSampler, input.TextureCoordinate);

     //calculate the final color.  The input color represents the interpolated output
     //of the vertex shader, whatever it happened to be.
     float4 outputColor;
     if (modulateWithTexture)
     {
         outputColor = input.Color * textureColor;
     }
     else if (addToTexture)
     {
//////////////////////////////////////////////////////////////
// Example 1.6 (duplicated)                                 //
//                                                          //
// The "saturate" intrinsic function, new to this sample,   //
// caps the value (or each value in a vector type) to the   //
// range [0.0, 1.0].                                        //
//                                                          //
// The decision of whether to saturate a particular value   //
// is an important one in many complex shaders, including   //
// high-dynamic range (HDR) effects.                        //
//////////////////////////////////////////////////////////////
         outputColor = saturate(input.Color + textureColor);
     }
     else if (textureOnly)
     {
         outputColor = textureColor;
     }
     else
     {
         outputColor = input.Color;
     }

     return outputColor;
}



//////////////////////////////////////////////////////////////
// Example 1.8                                              //
//                                                          //
// The definitions for all of the techniques follow.  There //
// is one technique for each logical combination of         //
// lighting color, vertex color, and texture color, and     //
// basic math operations to combine them.  The first        //
// technique shows a typical combination, then each element //
// of the calculation, then simple two-way combinations,    //
// then alternative three-way combinations.                 //
//                                                          //
// "Mod" in the technique name stands for "modulate", an    //
// operation where one value is scaled by another, via      //
// multiplication.                                          //
//                                                          //
// Note the parameters passed to the functions, new to this //
// sample.  These fill the "uniform" bool arguments on the  //
// functions above.  The techniques are evaluated when the  //
// effect is compiled, and the code is recompiled with each //
// boolean "variable" treated as a constant.  This means    //
// that the effect can duplicate code duplication while     //
// simultaneously avoiding often-costly run-time branches.  //
//////////////////////////////////////////////////////////////

technique LightingModColorModTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(true, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(true, false, false);
    }
}

technique LightingOnly
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, true);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, false, false);
    }
}

technique ColorOnly
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, false, false);
    }
}

technique TextureOnly
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, false, true);
    }
}


technique LightingModColor
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(true, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, false, false);
    }
}

technique LightingAddColor
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, true, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, false, false);
    }
}

technique LightingModTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, true);
          PixelShader = compile ps_2_0 PixelShaderFunction(true, false, false);
    }
}

technique LightingAddTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, true);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, true, false);
    }
}

technique ColorModTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(true, false, false);
    }
}

technique ColorAddTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, true, false);
    }
}

technique LightingAddColorAddTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, true, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, true, false);
    }
}

technique LightingAddColorModTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(false, true, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(true, false, false);
    }
}

technique LightingModColorAddTexture
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShaderFunction(true, false, false);
          PixelShader = compile ps_2_0 PixelShaderFunction(false, true, false);
    }
}