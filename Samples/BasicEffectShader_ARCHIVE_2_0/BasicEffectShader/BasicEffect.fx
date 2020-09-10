//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// This is a simple shader that supports 1 ambient and 3 directional lights.
// All lighting computations happen in world space.
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const texture BasicTexture;

uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (BasicTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};


//-----------------------------------------------------------------------------
// Fog settings
//-----------------------------------------------------------------------------

uniform const float		FogEnabled		: register(c0);
uniform const float		FogStart		: register(c1);
uniform const float		FogEnd			: register(c2);
uniform const float3	FogColor		: register(c3);

uniform const float3	EyePosition		: register(c4);		// in world space


//-----------------------------------------------------------------------------
// Material settings
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;


//-----------------------------------------------------------------------------
// Lights
// All directions and positions are in world space and must be unit vectors
//-----------------------------------------------------------------------------

uniform const float3	AmbientLightColor		: register(c10);

uniform const float3	DirLight0Direction		: register(c11);
uniform const float3	DirLight0DiffuseColor	: register(c12);
uniform const float3	DirLight0SpecularColor	: register(c13);

uniform const float3	DirLight1Direction		: register(c14);
uniform const float3	DirLight1DiffuseColor	: register(c15);
uniform const float3	DirLight1SpecularColor	: register(c16);

uniform const float3	DirLight2Direction		: register(c17);
uniform const float3	DirLight2DiffuseColor	: register(c18);
uniform const float3	DirLight2SpecularColor	: register(c19);


//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31


//-----------------------------------------------------------------------------
// Structure definitions
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

struct CommonVSOutput
{
	float4	Pos_ws;
	float4	Pos_ps;
	float4	Diffuse;
	float3	Specular;
	float	FogFactor;
};


//-----------------------------------------------------------------------------
// Shader I/O structures
// Nm: Normal
// Tx: Texture
// Vc: Vertex color
//
// Nm Tx Vc
//  0  0  0	VSInput
//  0  0  1 VSInputVc
//  0  1  0 VSInputTx
//  0  1  1 VSInputTxVc
//  1  0  0 VSInputNm
//  1  0  1 VSInputNmVc
//  1  1  0 VSInputNmTx
//  1  1  1 VSInputNmTxVc


//-----------------------------------------------------------------------------
// Vertex shader inputs
//-----------------------------------------------------------------------------

struct VSInput
{
	float4	Position	: POSITION;
};

struct VSInputVc
{
	float4	Position	: POSITION;
	float4	Color		: COLOR;
};

struct VSInputNm
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
};

struct VSInputNmVc
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
	float4	Color		: COLOR;
};

struct VSInputTx
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
};

struct VSInputTxVc
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float4	Color		: COLOR;
};

struct VSInputNmTx
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
};

struct VSInputNmTxVc
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
	float4	Color		: COLOR;
};


//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct VertexLightingVSOutput
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;		// Specular.rgb and fog factor
};

struct VertexLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
};

struct PixelLightingVSOutput
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct VertexLightingPSInput
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
};

struct VertexLightingPSInputTx
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
};

struct PixelLightingPSInput
{
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct PixelLightingPSInputTx
{
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};


//-----------------------------------------------------------------------------
// Compute lighting
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputeLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;

	// Directional Light 0
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight0DiffuseColor * ret.x;
	result.Specular += DirLight0SpecularColor * ret.y;
	
	// Directional Light 1
	L = -DirLight1Direction;
	H = normalize(E + L);
	ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight1DiffuseColor * ret.x;
	result.Specular += DirLight1SpecularColor * ret.y;
	
	// Directional Light 2
	L = -DirLight2Direction;
	H = normalize(E + L);
	ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight2DiffuseColor * ret.x;
	result.Specular += DirLight2SpecularColor * ret.y;
		
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Specular	*= SpecularColor;
		
	return result;
}


//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;
	
	// Light0
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float dt = max(0,dot(L,N));
    result.Diffuse += DirLight0DiffuseColor * dt;
    if (dt != 0)
		result.Specular += DirLight0SpecularColor * pow(max(0,dot(H,N)), SpecularPower);

	// Light1
	L = -DirLight1Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight1DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight1SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
	// Light2
	L = -DirLight2Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight2DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight2SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}


//-----------------------------------------------------------------------------
// Compute fog factor
//-----------------------------------------------------------------------------
float ComputeFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}


CommonVSOutput ComputeCommonVSOutput(float4 position)
{
	CommonVSOutput vout;
	
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	vout.Pos_ws = pos_ws;
	vout.Pos_ps = pos_ps;
	
	vout.Diffuse	= float4(DiffuseColor.rgb + EmissiveColor, Alpha);
	vout.Specular	= 0;
	vout.FogFactor	= ComputeFogFactor(length(EyePosition - pos_ws ));
	
	return vout;
}


CommonVSOutput ComputeCommonVSOutputWithLighting(float4 position, float3 normal)
{
	CommonVSOutput vout;
	
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	vout.Pos_ws = pos_ws;
	vout.Pos_ps = pos_ps;
	
	float3 N = normalize(mul(normal, World));
	float3 posToEye = EyePosition - pos_ws;
	float3 E = normalize(posToEye);
	ColorPair lightResult = ComputeLights(E, N);
	
	vout.Diffuse	= float4(lightResult.Diffuse.rgb, Alpha);
	vout.Specular	= lightResult.Specular;
	vout.FogFactor	= ComputeFogFactor(length(posToEye));
	
	return vout;
}


//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------

VertexLightingVSOutput VSBasic(VSInput vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	
	return vout;
}


VertexLightingVSOutput VSBasicVc(VSInputVc vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	
	return vout;
}


VertexLightingVSOutput VSBasicNm(VSInputNm vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	
	return vout;
}


VertexLightingVSOutput VSBasicNmVc(VSInputNmVc vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	
	return vout;
}


VertexLightingVSOutputTx VSBasicTx(VSInputTx vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	vout.TexCoord	= vin.TexCoord;

	return vout;
}


VertexLightingVSOutputTx VSBasicTxVc(VSInputTxVc vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	vout.TexCoord	= vin.TexCoord;
	
	return vout;
}


VertexLightingVSOutputTx VSBasicNmTx(VSInputNmTx vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	vout.TexCoord	= vin.TexCoord;

	return vout;
}


VertexLightingVSOutputTx VSBasicNmTxVc(VSInputNmTxVc vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, cout.FogFactor);
	vout.TexCoord	= vin.TexCoord;
	
	return vout;
}


//-----------------------------------------------------------------------------
// Per-pixel lighting vertex shaders
//-----------------------------------------------------------------------------

PixelLightingVSOutput VSBasicPixelLightingNm(VSInputNm vin)
{
	PixelLightingVSOutput vout;
	
	float4 pos_ws = mul(vin.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse		= float4(1, 1, 1, Alpha);
	
	return vout;
}


PixelLightingVSOutput VSBasicPixelLightingNmVc(VSInputNmVc vin)
{
	PixelLightingVSOutput vout;
	
	float4 pos_ws = mul(vin.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse.rgb	= vin.Color.rgb;
	vout.Diffuse.a		= vin.Color.a * Alpha;
	
	return vout;
}


PixelLightingVSOutputTx VSBasicPixelLightingNmTx(VSInputNmTx vin)
{
	PixelLightingVSOutputTx vout;
	
	float4 pos_ws = mul(vin.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse		= float4(1, 1, 1, Alpha);
	vout.TexCoord		= vin.TexCoord;

	return vout;
}


PixelLightingVSOutputTx VSBasicPixelLightingNmTxVc(VSInputNmTxVc vin)
{
	PixelLightingVSOutputTx vout;
	
	float4 pos_ws = mul(vin.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse.rgb	= vin.Color.rgb;
	vout.Diffuse.a		= vin.Color.a * Alpha;
	vout.TexCoord		= vin.TexCoord;
	
	return vout;
}


//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------

float4 PSBasic(VertexLightingPSInput pin) : COLOR
{
	float4 color = pin.Diffuse + float4(pin.Specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.Specular.w);
	return color;
}


float4 PSBasicTx(VertexLightingPSInputTx pin) : COLOR
{
	float4 color = tex2D(TextureSampler, pin.TexCoord) * pin.Diffuse + float4(pin.Specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.Specular.w);
	return color;
}


float4 PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	float3 posToEye = EyePosition - pin.PositionWS.xyz;
	
	float3 N = normalize(pin.NormalWS);
	float3 E = normalize(posToEye);
	
	ColorPair lightResult = ComputePerPixelLights(E, N);

	float4 diffuse = float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);
	
	return color;
}


float4 PSBasicPixelLightingTx(PixelLightingPSInputTx pin) : COLOR
{
	float3 posToEye = EyePosition - pin.PositionWS.xyz;
	
	float3 N = normalize(pin.NormalWS);
	float3 E = normalize(posToEye);
	
	ColorPair lightResult = ComputePerPixelLights(E, N);
	
	float4 diffuse = tex2D(TextureSampler, pin.TexCoord) * float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);
	
	return color;
}


//-----------------------------------------------------------------------------
// Shader and technique definitions
//-----------------------------------------------------------------------------

int ShaderIndex = 0;


VertexShader VSArray[12] =
{
	compile vs_1_1 VSBasic(),
	compile vs_1_1 VSBasicVc(),
	compile vs_1_1 VSBasicTx(),
	compile vs_1_1 VSBasicTxVc(),

	compile vs_1_1 VSBasicNm(),
	compile vs_1_1 VSBasicNmVc(),
	compile vs_1_1 VSBasicNmTx(),
	compile vs_1_1 VSBasicNmTxVc(),
	
	compile vs_1_1 VSBasicPixelLightingNm(),
	compile vs_1_1 VSBasicPixelLightingNmVc(),
	compile vs_1_1 VSBasicPixelLightingNmTx(),
	compile vs_1_1 VSBasicPixelLightingNmTxVc(),
};


PixelShader PSArray[12] =
{
	compile ps_1_1 PSBasic(),
	compile ps_1_1 PSBasic(),
	compile ps_1_1 PSBasicTx(),
	compile ps_1_1 PSBasicTx(),
	compile ps_1_1 PSBasic(),
	compile ps_1_1 PSBasic(),
	compile ps_1_1 PSBasicTx(),
	compile ps_1_1 PSBasicTx(),
	
	compile ps_2_0 PSBasicPixelLighting(),
	compile ps_2_0 PSBasicPixelLighting(),
	compile ps_2_0 PSBasicPixelLightingTx(),
	compile ps_2_0 PSBasicPixelLightingTx(),
};


Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[ShaderIndex]);
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
