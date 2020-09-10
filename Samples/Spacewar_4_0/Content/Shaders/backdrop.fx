//-----------------------------------------------------------------------------
// Backdrop.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//Input variables
float4x4 world;
float4x4 worldViewProjection;
float layerFactor;
float4 layer1Offset;
float4 layer2Offset;

texture layer1;
sampler nebula1 = sampler_state
{
   Texture = (layer1);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture layer2;
sampler nebula2 = sampler_state
{
   Texture = (layer2);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture layer3;
sampler stars = sampler_state
{
   Texture = (layer3);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

struct VS_OUTPUT
{
	float4 pos : POSITION;
	float2 tex0 : TEXCOORD0;
	float2 tex1 : TEXCOORD1;
	float2 tex2 : TEXCOORD2;
};

//vertex shader
VS_OUTPUT backdropVS(float4 Pos : POSITION)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;
	
	//Calculate texture coordinates for the 3 layers
	//The background coordinates are in the range 0.0-1.0 for easy tex lookup
	Out.tex0.x = Pos.x / 1480 + layer1Offset.x; //textures are 1480 need to only take 1280
	Out.tex0.y = Pos.y / 960 + layer1Offset.y; //textures are 920 need to only take 720
	Out.tex1.x = Pos.x / 1480 + layer2Offset.x; //textures are 1480 need to only take 1280
	Out.tex1.y = Pos.y / 960 + layer2Offset.y; //textures are 920 need to only take 720
	Out.tex2.x = Pos.x / 1280; //Star texture is the right size
	Out.tex2.y = Pos.y / 720;

	
	//And move to screen space
	Out.pos.x = (Pos.x - 640) / 640;
	Out.pos.y = (Pos.y - 360) / 360;
	Out.pos.z = 0;
	Out.pos.w = 1;	
	
	return Out;
}

//pixel shader
void backdropPS(in float2 tex0 : TEXCOORD0, in float2 tex1 : TEXCOORD1, in float2 tex2 : TEXCOORD2,  out float4 outCol: COLOR0)
{
	outCol = (tex2D(nebula1, tex0) * (.2 + layerFactor * .8));
	outCol += (tex2D(nebula2, tex1) * (1 - saturate(layerFactor * .8)));
	outCol += tex2D(stars, tex2);
}

//technique
technique RenderBackdrop
{
	pass P0
	{
		CULLMODE = NONE;
		ZWRITEENABLE = FALSE;
		
		VertexShader = compile vs_1_1 backdropVS();
		PixelShader = compile ps_2_0 backdropPS();
	}
}

	

