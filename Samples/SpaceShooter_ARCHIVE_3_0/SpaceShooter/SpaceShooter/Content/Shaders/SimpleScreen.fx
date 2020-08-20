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

float2 screenSize;

VS_OUTPUT SimpleScreenVS(VS_INPUT In)
{
   VS_OUTPUT Out;

	// Move to screen space. Vertices passed in are already transformed...
	Out.ScreenPos.x = (In.ObjectPos.x - (screenSize.x / 2)) / (screenSize.x / 2);
	Out.ScreenPos.y = (In.ObjectPos.y - (screenSize.y / 2)) / (screenSize.y / 2);
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
        CULLMODE = NONE;
        ALPHABLENDENABLE = TRUE;
        SRCBLEND = SRCALPHA;
        DESTBLEND = INVSRCALPHA;
        ZENABLE = FALSE; //Always want this on top
        POINTSIZE = 1;
      
		VertexShader = compile vs_1_1 SimpleScreenVS();
		PixelShader = compile ps_1_1 SimpleScreenPS();
   }
}
