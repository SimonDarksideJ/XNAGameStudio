float4x4 World;
float4x4 View;
float4x4 Projection;
float ViewportHeight;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float Size : PSIZE0;
	float Opacity : COLOR0;
	float Rotation : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float Size : PSIZE0;
	float4 Color : COLOR0;
	float4 Rotation : COLOR1;
};

// Vertex shader helper for computing the rotation of a particle.
float4 GetParticleRotation(float rotation)
{    
    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    // Normally we would output this matrix using a texture coordinate interpolator,
    // but texture coordinates are generated directly by the hardware when drawing
    // point sprites. So we have to use a color interpolator instead. Only trouble
    // is, color interpolators are clamped to the range 0 to 1. Our rotation values
    // range from -1 to 1, so we have to scale them to avoid unwanted clamping.
    
    rotationMatrix *= 0.5;
    rotationMatrix += 0.5;
    
    return rotationMatrix;
}

float GetParticleSize(float4 projectedPosition, float inputSize)
{
    return inputSize * Projection._m11 / projectedPosition.w * ViewportHeight / 2;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Size = GetParticleSize(output.Position, input.Size);
	output.Color = float4(1,1,1,input.Opacity);
	output.Rotation = GetParticleRotation(input.Rotation);
    return output;
}

struct PixelShaderInput
{
	float4 Color : COLOR0;
	float4 Rotation : COLOR1;
#ifdef XBOX
	float2 TextureCoordinate : SPRITETEXCOORD;
#else
	float4 TextureCoordinate : TEXCOORD0;
#endif
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float2 textureCoordinate = input.TextureCoordinate;
    textureCoordinate -= 0.5;    
    float4 rotation = input.Rotation * 2 - 1;    
    textureCoordinate = mul(textureCoordinate, float2x2(rotation));

    textureCoordinate *= sqrt(2);
    
    // Undo the offset used to control the rotation origin.
    textureCoordinate += 0.5;

    return tex2D(Sampler, textureCoordinate) * input.Color;    
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
