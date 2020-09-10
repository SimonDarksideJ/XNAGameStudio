//-----------------------------------------------------------------------------
// SimpleScreen.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

struct VS_INPUT
{
    float4 ObjectPos: POSITION;
    float4 VertexColor: COLOR;
};

struct VS_OUTPUT 
{
   float4 ScreenPos:   POSITION;
   float4 VertexColor: COLOR;
};


VS_OUTPUT SimpleScreenVS(VS_INPUT In)
{
   VS_OUTPUT Out;

    //Move to screen space. For the starfield coordinates are already transformed.
    Out.ScreenPos.x = (In.ObjectPos.x - 640) / 640;
    Out.ScreenPos.y = (In.ObjectPos.y - 360) / 360;    
    Out.ScreenPos.z = 0;
    Out.ScreenPos.w = 1;	
    
    Out.VertexColor = In.VertexColor;

    return Out;
}

float4 SimpleScreenPS(float4 color : COLOR0) : COLOR0
{
    return color;
}

//--------------------------------------------------------------//
// Technique Section for Simple screen transform
//--------------------------------------------------------------//
technique SimpleScreen
{
   pass Single_Pass
   {
        VertexShader = compile vs_1_1 SimpleScreenVS();
        PixelShader = compile ps_2_0 SimpleScreenPS();
   }

}
