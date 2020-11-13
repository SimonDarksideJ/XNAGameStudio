string description = "Line rendering helper shader for XNA";

// Default variables, supported by the engine
float4x4 worldViewProj : WorldViewProjection;

struct VertexInput
{
    float3 pos   : POSITION;
    float4 color : COLOR;
};

struct VertexOutput 
{
   float4 pos   : POSITION;
   float4 color : COLOR;
};

VertexOutput LineRenderingVS(VertexInput In)
{
    VertexOutput Out;
    
    // Transform position
    Out.pos = mul(float4(In.pos, 1), worldViewProj);
    Out.color = In.color;

    // And pass everything to the pixel shader
    return Out;
}

float4 LineRenderingPS(VertexOutput In) : Color
{
    return In.color;
}

VertexOutput LineRendering2DVS(VertexInput In)
{
    VertexOutput Out;
    
    // Transform position (just pass over)
    Out.pos = float4(In.pos, 1);
    Out.color = In.color;

    // And pass everything to the pixel shader
    return Out;
}

float4 LineRendering2DPS(VertexOutput In) : Color
{
    return In.color;
}

// Techniques
technique LineRendering3D
{
    pass PassFor3D
    {
        VertexShader = compile vs_1_1 LineRenderingVS();
        PixelShader = compile ps_2_0 LineRenderingPS();
    }
}

technique LineRendering2D
{
    pass PassFor2D
    {
        VertexShader = compile vs_1_1 LineRendering2DVS();
        PixelShader = compile ps_2_0 LineRendering2DPS();
    }
}
