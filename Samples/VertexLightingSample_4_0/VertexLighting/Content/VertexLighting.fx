float4x4 world;
float4x4 view;
float4x4 projection;

//////////////////////////////////////////////////////////////
// Example 3.1                                              //
//                                                          //
// These variables are necessary to define a single         //
// directional light.  The light has only a direction and a //
// color.                                                   //
//////////////////////////////////////////////////////////////
float4 lightColor;
float3 lightDirection;


//////////////////////////////////////////////////////////////
// Example 3.2                                              //
//                                                          //
// We are also including an "ambient" lighting color.  This //
// represents a color / intensity of scattered light in a   //
// scene.  It is not directional, so we only need one       //
// variable to represent it.                                //
//////////////////////////////////////////////////////////////
float4 ambientColor;


//////////////////////////////////////////////////////////////
// Example 3.3                                              //
//                                                          //
// For this shader, we need to return multiple fields from  //
// the vertex shader.  To facilitate this, we define a      //
// struct to use as the return type.  Notice the semantics  //
// are "baked in" to the structure fields.  This helps      //
// clean up the function defintions.                        //
//////////////////////////////////////////////////////////////
struct VertexShaderOutput 
{
     float4 Position : POSITION;
     float4 Color : COLOR0;
};

//////////////////////////////////////////////////////////////
// Example 3.4                                              //
//                                                          //
// You can also use stucts for inputs.  This structure      //
// represents the input to the pixel shader.                //
//////////////////////////////////////////////////////////////
struct PixelShaderInput
{
     float4 Color: COLOR0;
};


//////////////////////////////////////////////////////////////
// Example 3.5                                              //
//                                                          //
// This simple vertex shader sets a vertex color by         //
// calculating the sum of the directional and ambient light //
// at each vertex.                                          //
//////////////////////////////////////////////////////////////
VertexShaderOutput DiffuseLighting(
     float3 position : POSITION,
     float3 normal : NORMAL )
{

     VertexShaderOutput output;

     //generate the world-view-proj matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);
     
     
     //////////////////////////////////////////////////////////////
     // Example 3.6                                              //
     //                                                          //
     // In this shader, we expect the light to be defined in     //
     // terms of World Space.  We need to transform the model    //
     // normals to World Space as well to make them useful in    //
     // the lighting caculation.                                 //
     //////////////////////////////////////////////////////////////
     float3 worldNormal =  mul(normal, world);
     
     //////////////////////////////////////////////////////////////
     // Example 3.7                                              //
     //                                                          //
     // The actual lighting calculation for diffuse light is     //
     // normal-dot-lightDirection.  The intrinsic function       //
     // used is called "dot" and takes two vectors as parameters //
     // and returns a scalar value.                              //
     //                                                          //
     // The lambertian lighting calculation uses the direction   //
     // from the vertex to the light.  Therefore the light       //
     // direction vector is reversed (negated) as lightDirection //
     // indicates the direction of the light is pointing.        //
     //////////////////////////////////////////////////////////////
     float diffuseIntensity = saturate( dot(-lightDirection, worldNormal));
     
     //////////////////////////////////////////////////////////////
     // Example 3.8                                              //
     //                                                          //
     // The diffuse intensity is then mutiplied by the light     //
     // color to get the diffuse color at the vertex.            //
     //////////////////////////////////////////////////////////////
     float4 diffuseColor = lightColor * diffuseIntensity;
     
     
     
     //////////////////////////////////////////////////////////////
     // Example 3.9                                              //
     //                                                          //
     // The final vertex color is the sum of the diffuse         //
     // lighting at the vertex and the global ambient light.     //
     //                                                          //
     // This simplistic shader will unintentionally modify the   //
     // alpha (opacity) of the vertex color, so the alpha        //
     // component of the color is set fixed at 1.0 (totally      //
     // opaque) for the purposes of this sample.                 //
     //////////////////////////////////////////////////////////////
     output.Color = diffuseColor + ambientColor;
     diffuseColor.a = 1.0;


     //return the output structure
     return output;
}

//////////////////////////////////////////////////////////////
// Example 3.10                                             //
//                                                          // 
// This pixel shader returns the color provided to it from  //
// the interpolator.  This means that the color input is    //
// interpolated from the vertex colors output.  This        //
// causes the smooth shaded effect known as Gouraud Shading //
//////////////////////////////////////////////////////////////
float4 SimplePixelShader(PixelShaderInput input) : COLOR
{
     return input.Color;
}

//////////////////////////////////////////////////////////////
// Example 3.11                                             //
//                                                          //
// This simple technique is called "VertexLighting" and     //
// draws polygons lit by a single directional light and     //
// ambient light                                            //
//////////////////////////////////////////////////////////////
technique VertexLighting
{
     
    pass P0
    {
          //set the VertexShader state to the vertex shader function
          VertexShader = compile vs_2_0 DiffuseLighting();
          
          //set the PixelShader state to the pixel shader function          
          PixelShader = compile ps_2_0 SimplePixelShader();
    }
}