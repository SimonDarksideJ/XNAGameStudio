//-----------------------------------------------------------------------------
// SpriteBatch.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);
sampler  TextureSampler  : register(s0);


#ifdef XBOX360


// Vertex shader for rendering sprites on Xbox.
void SpriteVertexShader(int        index          : INDEX,
		  				out float4 outputPosition : POSITION0,
		  				out float4 outputColor    : COLOR0,
		  				out float2 outputTexCoord : TEXCOORD0)
{
	// Read input data from the vertex buffer.
    float4 source;
    float4 destination;
    float4 originRotationDepth;
	float4 effects;
	float4 color;

	int vertexIndex = index / 4;
	int cornerIndex = index % 4;

    asm
    {
        vfetch source,              vertexIndex, texcoord0
        vfetch destination,         vertexIndex, texcoord1
        vfetch originRotationDepth, vertexIndex, texcoord2
        vfetch effects,             vertexIndex, texcoord3
        vfetch color,               vertexIndex, texcoord4
    };

	// Unpack into local variables, to make the following code more readable.
	float2 texCoordPosition = source.xy;
	float2 texCoordSize = source.zw;

	float2 position = destination.xy;
	float2 size = destination.zw;

	float2 origin = originRotationDepth.xy;
	float rotation = originRotationDepth.z;
	float depth = originRotationDepth.w;

	// Which of the four sprite corners are we currently shading?
	float2 whichCorner;

	if      (cornerIndex == 0) whichCorner = float2(0, 0);
	else if (cornerIndex == 1) whichCorner = float2(1, 0);
	else if (cornerIndex == 2) whichCorner = float2(1, 1);
	else                       whichCorner = float2(0, 1);

	// Calculate the vertex position.
	float2 cornerOffset = (whichCorner - origin / texCoordSize) * size;

	// Rotation.
	float cosRotation = cos(rotation);
	float sinRotation = sin(rotation);

	position += mul(cornerOffset, float2x2(cosRotation, sinRotation, -sinRotation, cosRotation));

    // Apply the matrix transform.
    outputPosition = mul(float4(position, depth, 1), transpose(MatrixTransform));
    
	// Half pixel offset for correct texel centering.
	outputPosition.xy -= 0.5;

	// Viewport adjustment.
	outputPosition.xy /= ViewportSize;
	outputPosition.xy *= float2(2, -2);
	outputPosition.xy -= float2(1, -1);

	// Texture mirroring.
	whichCorner = lerp(whichCorner, 1 - whichCorner, effects);

	// Compute the texture coordinate.
    outputTexCoord = (texCoordPosition + whichCorner * texCoordSize) / TextureSize;

	// Simple color output.
	outputColor = color;
}


#else


// Vertex shader for rendering sprites on Windows.
void SpriteVertexShader(inout float4 position : POSITION0,
		  				inout float4 color    : COLOR0,
						inout float2 texCoord : TEXCOORD0)
{
    // Apply the matrix transform.
    position = mul(position, transpose(MatrixTransform));
    
	// Half pixel offset for correct texel centering.
	position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);

	// Compute the texture coordinate.
	texCoord /= TextureSize;
}


#endif


// Pixel shader for rendering sprites (shared between Windows and Xbox).
void SpritePixelShader(inout float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
    color *= tex2D(TextureSampler, texCoord);
}


technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_1_1 SpriteVertexShader();
		PixelShader  = compile ps_1_1 SpritePixelShader();
	}
}
