//////////////////////////////////////////////////////////////
// Example 2.1                                              //
//                                                          //
// Each of these shader constant variables                  //
// corresponds to an EffectParameter in the C# source.      //
//////////////////////////////////////////////////////////////
float4x4 world;
float4x4 view;
float4x4 projection;

//////////////////////////////////////////////////////////////
// Example 2.2                                              //
//                                                          //
// This is about as simple a vertex shader as will          //
// ever be used.  It takes a vertex position as an input    //
// and simply outputs the position after transforming it    //
// using the world-view-projection matricies.               //
//                                                          //
// Notice the semantics (all-caps following the colon.)     //
// These tell the shader compiler what data is being        //
// input and returned from the shader function              //
//////////////////////////////////////////////////////////////
float4 SimpleVertexShader(float3 position : POSITION) : POSITION
{
     //////////////////////////////////////////////////////////////
     // Example 2.3                                              //
     //                                                          //
     // Calculate the world-view-position matrix                 //
     // by multiplying them in order.  It uses the "mul"         //
     // intrinsic function to first multiply the world matrix    //
     // by the view matrix, then the result is multiplied by     //
     // the projection matrix.                                   //
     //////////////////////////////////////////////////////////////
     
     float4x4 wvp = mul(mul(world, view), projection);
     
     //////////////////////////////////////////////////////////////
     // Example 2.4                                              //
     //                                                          //
     // Calculate the transformed position by multiplying the    //
     // input vertex by the world-view-position matrix.  Again   //
     // the "mul" intrinsic is used, only this time we're        //
     // mutiplying a poisiton vector (float4) by the wvp         //
     // matrix (float4x4).                                       //
     //                                                          //
     // The result of this is a float4 that represents the       //
     // transformed vertex position.                             //
     //////////////////////////////////////////////////////////////
     
     float4 transformedPosition = mul(float4(position, 1.0), wvp);

     //return the trasnformed vertex position
     return transformedPosition;
}

//////////////////////////////////////////////////////////////
// Example 2.5                                              //
//                                                          // 
// This very simple pixel shader has no inputs.             //
// When this shader is run (once for each pixel), it        //
// simply returns a white pixel.  This will cause any       //
// geometry drawn to show up as a flat, white image.        //
//////////////////////////////////////////////////////////////
float4 SimplePixelShader() : COLOR
{
     //return the the white color
     return float4(1,1,1,1);
}

//////////////////////////////////////////////////////////////
// Example 2.6                                              //
//                                                          //
// This simple technique is called "FlatShaded" and simply  //
// draws an unlit white representation of the mesh          //
//////////////////////////////////////////////////////////////
technique FlatShaded
{
     
     //////////////////////////////////////////////////////////////
     // Example 2.7                                              //
     //                                                          //
     // This very simple effect has only one pass, called 'P0'   //
     // Typically, a Draw() call is made for each pass in the    //
     // technique                                                //
     //////////////////////////////////////////////////////////////
    pass P0
    {
          //set the VertexShader state to the vertex shader function
          VertexShader = compile vs_2_0 SimpleVertexShader();
          
          //set the pixel shader state to the pixel shader function          
          PixelShader = compile ps_2_0 SimplePixelShader();
    }
}