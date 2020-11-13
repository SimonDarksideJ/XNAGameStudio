string description = "Generate and use a shadow map with a directional light";

float4x4 worldViewProj         : WorldViewProjection;
float4x4 world                 : World;
float4x4 viewInverse           : ViewInverse;

// Extra values for this shader
// Transformation matrix for converting world pos
// to texture coordinates of the shadow map.
float4x4 shadowTexTransform;
// worldViewProj of the light projection
float4x4 worldViewProjLight : WorldViewProjection;

// Hand adjusted near and far plane for better percision.
float nearPlane = 2.0f;
float farPlane = 8.0f;
// Depth bias, controls how much we remove from the depth
// to fix depth checking artifacts. For ps_1_1 this should
// be a very high value (0.01f), for ps_2_0 it can be very low.
float depthBias = 0.0025f;
// Substract a very low value from shadow map depth to
// move everything a little closer to the camera.
// This is done when the shadow map is rendered before any
// of the depth checking happens, should be a very small value.
float shadowMapDepthBias = -0.0005f;

// Color for shadowed areas, should be black too, but need
// some alpha value (e.g. 0.5) for blending the color to black.
float4 ShadowColor = {0.25f, 0.26f, 0.27f, 1.0f};

float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f};

texture shadowDistanceFadeoutTexture : Diffuse
<
    string UIName = "Shadow distance fadeout texture";
    string ResourceName = "ShadowDistanceFadeoutMap.dds";
>;
sampler shadowDistanceFadeoutTextureSampler = sampler_state
{
    Texture = <shadowDistanceFadeoutTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    // We just use the position here, nothing else is required.
    float3 pos      : POSITION;
};

// Struct used for passing data from VS_GenerateShadowMap to ps
struct VB_GenerateShadowMap
{
    float4 pos      : POSITION;
    // Ps 1.1 will use color, ps 2.0 will use TexCoord.
    // This way we get the most percision in each ps model.
    float4 depth    : COLOR0;
};

// Helper functions
float4 TransformPosition(float3 pos)
{
    return mul(float4(pos.xyz, 1), worldViewProj);
}

float3 GetWorldPos(float3 pos)
{
    return mul(float4(pos, 1), world).xyz;
}

float3 GetCameraPos()
{
    return viewInverse[3].xyz;
}

float3 CalcNormalVector(float3 nor)
{
    return normalize(mul(nor, (float3x3)world));
}

// Vertex shader function
VB_GenerateShadowMap VS_GenerateShadowMap(VertexInput In)
{
    VB_GenerateShadowMap Out = (VB_GenerateShadowMap) 0;
    Out.pos = TransformPosition(In.pos);

    // Use farPlane/10 for the internal near plane, we don't have any
    // objects near the light, use this to get much better percision!
    float internalNearPlane = farPlane / 10;
    
    // Linear depth calculation instead of normal depth calculation.
    float depthValue =
        (Out.pos.z - internalNearPlane)/
        (farPlane - internalNearPlane);
    // Set depth value to all 4 rgba components (only first is used anyway).
    Out.depth = depthValue + shadowMapDepthBias;

    return Out;
}

// Pixel shader function
float4 PS_GenerateShadowMap(VB_GenerateShadowMap In) : COLOR
{
    // Just set the interpolated depth value.
    // Format should be R32F or R16F, if that is not possible
    // A8R8G8B8 is used, which is obviously not that precise.
    return In.depth;
}

technique GenerateShadowMap
{
    pass P0
    {
        // Disable culling to throw shadow even if virtual
        // shadow light is inside big buildings!
        CullMode = None;
        VertexShader = compile vs_1_1 VS_GenerateShadowMap();
        PixelShader  = compile ps_2_0 PS_GenerateShadowMap();
    }
}

//-------------------------------------------------------------------

// Struct used for passing data from VS_GenerateShadowMap to ps
struct VB_GenerateShadowMap20
{
    float4 pos      : POSITION;
    float2 depth    : TEXCOORD0;
};

// Vertex shader function
VB_GenerateShadowMap20 VS_GenerateShadowMap20(VertexInput In)
{
    VB_GenerateShadowMap20 Out = (VB_GenerateShadowMap20) 0;
    Out.pos = TransformPosition(In.pos);

    // Use farPlane/10 for the internal near plane, we don't have any
    // objects near the light, use this to get much better percision!
    float internalNearPlane = farPlane / 10;

    // Linear depth calculation instead of normal depth calculation.
    Out.depth = float2(
        (Out.pos.z - internalNearPlane),
        (farPlane - internalNearPlane));

    return Out;
}

// Pixel shader function
float4 PS_GenerateShadowMap20(VB_GenerateShadowMap20 In) : COLOR
{
    // Just set the interpolated depth value.
    return (In.depth.x/In.depth.y) + shadowMapDepthBias;
}

technique GenerateShadowMap20
{
    pass P0
    {
        // Disable culling to throw shadow even if virtual
        // shadow light is inside big buildings!
        CullMode = None;
        VertexShader = compile vs_2_0 VS_GenerateShadowMap20();
        PixelShader  = compile ps_2_0 PS_GenerateShadowMap20();
    }
}

//-------------------------------------------------------------------

// Texture and samplers
texture shadowMap : Diffuse;
// This sampler is only used for the ps_1_1 version
sampler ShadowMapSampler = sampler_state
{
    Texture = <shadowMap>;
#ifdef XBOX360
    // Border color to white to make everything unshadowed outside our visble area
    BorderColor = 0xFFFFFFFF;
    AddressU  = Border;
    AddressV  = Border;
#else
    // Border is not reliably supported on Windows
    AddressU  = Clamp;
    AddressV  = Clamp;
#endif
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

// Vertex shader output structure for using the shadow map, ps_1_1 version
struct VB_UseShadowMap
{
    float4 pos            : POSITION;
    float2 shadowTexCoord : TEXCOORD0;
    float4 depthCompareHelper : TEXCOORD1;
    float4 depth          : COLOR0;
};

VB_UseShadowMap VS_UseShadowMap(VertexInput In)
{
    VB_UseShadowMap Out = (VB_UseShadowMap)0;
    // Convert to float4 pos, used several times here.
    float4 pos = float4(In.pos, 1);
    Out.pos = mul(pos, worldViewProj);

    // Transform model-space vertex position to light-space:
    float4 shadowTexPos =
        mul(pos, shadowTexTransform);
    // Set first texture coordinates
    Out.shadowTexCoord = float2(
        shadowTexPos.x/shadowTexPos.w,
        shadowTexPos.y/shadowTexPos.w);

    // Get depth of this point relative to the light position
    float4 depthPos = mul(pos, worldViewProjLight);
    
    // Use farPlane/10 for the internal near plane, we don't have any
    // objects near the light, use this to get much better percision!
    float internalNearPlane = farPlane / 10;
    
    // Same linear depth calculation as above.
    // Also substract depthBias to fix shadow mapping artifacts.
    Out.depth =
        ((depthPos.z - internalNearPlane)/
        (farPlane - internalNearPlane)) - depthBias;
    Out.depthCompareHelper = 0.505f * // little bit above 0.5 to fix overlapping!
        ((depthPos.z - internalNearPlane)/
        (farPlane - internalNearPlane)) - depthBias;

    return Out;
}

technique UseShadowMap
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_UseShadowMap();
        // Using PS Instructions directly here, easier to tweak.
        
        // We just use the samplers for the shadow map here.
        sampler[0] = (ShadowMapSampler);
        // Set shadow color
        PixelShaderConstant1[2]   = <ShadowColor>;
        pixelshader = asm
        {
            // Optimized for ps_1_1, limited by max. 8 instructions.
            ps_1_1
            // Mask to copy red component to all components.
            def c0, 1.0, 0.0, 0.0, 0.0
            def c1, 1.0, 1.0, 1.0, 1.0 // dummy non shadow color
            def c3, 0.5, 0.5, 0.5, 0.5 // helper for comparing depth
            //-------------------------------------------
            // t0 contains the shadow map texture coordinates.
            // c0 is color mask, c1 is non shadow color (usually 0, 0, 0, 0)
            // and c2 is shadow color (0, 0, 0, 0.5). The alpha component
            // is the important part here because we use alpha blending to
            // darken areas where the shadow is.
            // v0 holds the depth of the current pixel from the light source.
            // t1 holds the half of the depth value for comparing above 0.5
            // for skipping everthing above our visible depth.
            //-------------------------------------------

            // Sample shadow map texture coordinates
            tex t0
            texcoord t1
            
            // Copy red color to all channels (need that for comparing)
            dp3 r0, t0, c0
            // Compare depth value (c0) with shadow map depth (v0)
            // Use -0.5 bias for cnd formula (comparing to +0.5)
            // This is the only way to compare values in ps_1_1.
            sub r0.a, r0.a, v0_bias
            cnd r1, r0.a, c1, c2
            
            // Compare depth, everything above the farplane should be unshadowed
            sub r0.a, c1.a, t1
            cnd r0, r0.a, r1, c1
        };
    }
}

//-------------------------------------------------------------------

// Sampler for ps_2_0, use point filtering to do bilinear filtering ourself!
sampler ShadowMapSampler20 = sampler_state
{
    Texture = <shadowMap>;
#ifdef XBOX360
    // Border color to white to make everything unshadowed outside our visble area
    BorderColor = 0xFFFFFFFF;
    AddressU  = Border;
    AddressV  = Border;
#else
    // Border is not reliably supported on Windows
    AddressU  = Clamp;
    AddressV  = Clamp;
#endif
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

// Vertex shader output structure for using the shadow map
struct VB_UseShadowMap20
{
    float4 pos            : POSITION;
    float4 shadowTexCoord : TEXCOORD0;
    float2 depth          : TEXCOORD1;
};

VB_UseShadowMap20 VS_UseShadowMap20(VertexInput In)
{
    VB_UseShadowMap20 Out = (VB_UseShadowMap20)0;
    // Convert to float4 pos, used several times here.
    float4 pos = float4(In.pos, 1);
    Out.pos = mul(pos, worldViewProj);

    // Transform model-space vertex position to light-space:
    float4 shadowTexPos =
        mul(pos, shadowTexTransform);
    // Set first texture coordinates
    Out.shadowTexCoord = float4(
        shadowTexPos.x,
        shadowTexPos.y,
        0.0f,
        shadowTexPos.w);

    // Get depth of this point relative to the light position
    float4 depthPos = mul(pos, worldViewProjLight);
    
    // Use farPlane/10 for the internal near plane, we don't have any
    // objects near the light, use this to get much better percision!
    float internalNearPlane = farPlane / 10;
    
    // Same linear depth calculation as above.
    // Also substract depthBias to fix shadow mapping artifacts.
    Out.depth = float2(
        (depthPos.z - internalNearPlane),
        (farPlane - internalNearPlane));

    return Out;
}

float2 shadowMapTexelSize = float2(1.0f/1024.0f, 1.0f/1024);

// Poison filter pseudo random filter positions for PCF with 10 samples
float2 FilterTaps[10] =
{
    // First test, still the best.
    {-0.84052f, -0.073954f},
    {-0.326235f, -0.40583f},
    {-0.698464f, 0.457259f},
    {-0.203356f, 0.6205847f},
    {0.96345f, -0.194353f},
    {0.473434f, -0.480026f},
    {0.519454f, 0.767034f},
    {0.185461f, -0.8945231f},
    {0.507351f, 0.064963f},
    {-0.321932f, 0.5954349f}
};
// Advanced pixel shader for shadow depth calculations in ps 2.0.
// However this shader looks blocky like PCF3x3 and should be smoothend
// out by a good post screen blur filter. This advanced shader does a good
// job faking the penumbra and can look very good when adjusted carefully.
float4 PS_UseShadowMap20(VB_UseShadowMap20 In) : COLOR
{
    float depth = (In.depth.x/In.depth.y) - depthBias;

    float2 shadowTex =
        (In.shadowTexCoord.xy / In.shadowTexCoord.w) -
        shadowMapTexelSize / 2.0f;

    float resultDepth = 0;
    for (int i=0; i<10; i++)
        resultDepth += depth > tex2D(ShadowMapSampler20,
            shadowTex+FilterTaps[i]*shadowMapTexelSize).r ? 1.0f/10.0f : 0.0f;
            
#ifndef XBOX360
    // Simulate texture border addressing mode on Windows
    if (shadowTex.x < 0 || shadowTex.y < 0 ||
        shadowTex.x > 1 || shadowTex.y > 1)
    {
        resultDepth = 0;
    }
#endif
            
    // Multiply the result by the shadowDistanceFadeoutTexture, which
    // fades shadows in and out at the max. shadow distances
    resultDepth *= tex2D(shadowDistanceFadeoutTextureSampler, shadowTex).r;

    // We can skip this if its too far away anway (else very far away landscape
    // parts will be darkenend)
    if (depth > 1)
        return 1;
    else
        // And apply
        return lerp(1, ShadowColor, resultDepth);
}

technique UseShadowMap20
{  
    pass P0
    {
        VertexShader = compile vs_2_0 VS_UseShadowMap20();
        PixelShader  = compile ps_2_0 PS_UseShadowMap20();
    }
}
