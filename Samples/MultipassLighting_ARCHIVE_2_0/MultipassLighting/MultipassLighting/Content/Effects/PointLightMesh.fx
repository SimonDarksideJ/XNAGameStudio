#include "Includes.inc"

float4 lightColor;

//Vertex shader for drawing the flat-lit point-light representations
float4 PointLightVS( float3 position : POSITION) : POSITION0
{
     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     return mul(float4(position, 1.0), wvp);
}

//Pixel shader for drawing the flat-lit point-light representations
float4 PointLightPS( ) : COLOR
{
     return lightColor;
}

technique PointLightMesh
{
     pass
     {
          //Since we're now dealing with blending, this technique
          //should be explicit about disabling alpha blending
          AlphaBlendEnable = FALSE;
          VertexShader = compile vs_1_1 PointLightVS();
          PixelShader = compile ps_1_1 PointLightPS();
     }
     
}