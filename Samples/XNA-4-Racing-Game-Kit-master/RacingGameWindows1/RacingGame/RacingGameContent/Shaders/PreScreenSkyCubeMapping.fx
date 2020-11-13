// Shows the sky bg with help of a cube map texture,
// should be called before anything else is rendered.";

// We need view, projection, ambientColor for the scene brightness and
// diffuseTexture, which is the background cube map texture for the sky.

const float SkyCubeScale = 100.0f;

float4x4 view : View;
float4x4 projection : Projection;

// The ambient color for the sky, should be 1 for normal brightness.
float4 ambientColor : Ambient = {1.0f, 1.0f, 1.0f, 1.0f};

// Texture and samplers
texture diffuseTexture : Environment;

samplerCUBE diffuseTextureSampler = sampler_state
{
    Texture = <diffuseTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

//-------------------------------------

struct VertexInput
{
    float3 pos : POSITION;
};

struct VB_OutputPos3DTexCoord
{
    float4 pos      : POSITION;
    float3 texCoord : TEXCOORD0;
};

VB_OutputPos3DTexCoord VS_SkyCubeMap(VertexInput In)
{
    VB_OutputPos3DTexCoord Out;
    
    // Scale the box up so that we don't hit the near clip plane
    float3 pos = In.pos * SkyCubeScale;
    
    // In.pos is a float 3 for this calculation so that translation is ignored
    pos = mul(pos, view);
    // However, we need the translation for the projection
    Out.pos = mul(float4(pos, 1.0f), projection);
    
    // Just use the positions to infer the texture coordinates
    // Swap y and z because we use +z as up
    Out.texCoord = float3(In.pos.xzy);
        
    return Out;
}

float4 PS_SkyCubeMap(VB_OutputPos3DTexCoord In) : COLOR
{
    float4 texCol = ambientColor *
        texCUBE(diffuseTextureSampler, In.texCoord);
    return texCol;
}

technique SkyCubeMap
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_SkyCubeMap();
        PixelShader  = compile ps_2_0 PS_SkyCubeMap();
    }
}
