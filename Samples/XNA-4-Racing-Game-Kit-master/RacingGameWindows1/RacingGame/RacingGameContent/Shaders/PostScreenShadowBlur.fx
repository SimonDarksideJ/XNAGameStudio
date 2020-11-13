string description = "Post screen shader for shadow blurring";

// Blur post processing effect.
// ScreenAdvancedBlur : 2 pass blur filter (horizontal and vertical) for ps11
// ScreenAdvancedBlur20 : 2 pass blur filter (horizontal and vertical) for ps20

// This script is only used for FX Composer, most values here
// are treated as constants by the application anyway.
// Values starting with an upper letter are constants.
float Script : STANDARDSGLOBAL
<
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";

    // We just call a script in the main technique.
    string Script = "Technique=ScreenAdvancedBlur20;";
> = 1.0;

const float4 ClearColor : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
const float ClearDepth = 1.0f;

// Render-to-Texture stuff
float2 windowSize : VIEWPORTPIXELSIZE;

const float BlurWidth = 1.25f;

// For ps_2_0 use only half the blur width
// because we cover a range twice as big.
// Update: For shadows 2.0 looks much better and smoother :)
// ps_1_1 can't archive that effect with just 4 samples.
const float BlurWidth20 = 1.5f;

texture sceneMap : RENDERCOLORTARGET
<
    float2 ViewportRatio = { 1.0, 1.0 };
    int MIPLEVELS = 1;
>;
sampler sceneMapSampler = sampler_state 
{
    texture = <sceneMap>;
    AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = None;
    MINFILTER = Linear;
    MAGFILTER = Linear;
};

// Only for 2 passes (horz/vertical blur)
texture blurMap : RENDERCOLORTARGET
<
    float2 ViewportRatio = { 1.0, 1.0 };
    int MIPLEVELS = 1;
>;
sampler blurMapSampler = sampler_state 
{
    texture = <blurMap>;
    AddressU  = Clamp;
    AddressV  = Clamp;
    AddressW  = Clamp;
    MIPFILTER = None;
    MINFILTER = Linear;
    MAGFILTER = Linear;
};

struct VB_OutputPosTexCoord
{
       float4 pos      : POSITION;
    float2 texCoord : TEXCOORD0;
};

struct VB_OutputPos2TexCoords
{
       float4 pos         : POSITION;
    float2 texCoord[2] : TEXCOORD0;
};

struct VB_OutputPos4TexCoords
{
       float4 pos         : POSITION;
    float2 texCoord[4] : TEXCOORD0;
};

float4 PS_Display(
    VB_OutputPosTexCoord In,
    uniform sampler2D tex) : COLOR
{   
    float4 outputColor = tex2D(tex, In.texCoord);
    // Display color
    return outputColor;
}

float4 PS_DisplayAlpha(
    VB_OutputPosTexCoord In,
    uniform sampler2D tex) : COLOR
{
    float4 outputColor = tex2D(tex, In.texCoord);
    // Just display alpha
    return float4(outputColor.a, outputColor.a, outputColor.a, 0.0f);
}

//-----------------------------------------------------------

// generate texcoords for avanced blur
VB_OutputPos4TexCoords VS_AdvancedBlur(
    float4 pos      : POSITION, 
    float2 texCoord : TEXCOORD0,
    uniform float2 dir)
{
    VB_OutputPos4TexCoords Out = (VB_OutputPos4TexCoords)0;
    Out.pos = pos;
    float2 texelSize = 1.0f / windowSize;
    float2 s = texCoord - texelSize*(4-1)*0.5*dir*BlurWidth + texelSize*0.5;
    for(int i=0; i<4; i++)
        Out.texCoord[i] = s + texelSize*i*dir*BlurWidth;
    return Out;
}

// Advanced blur technique for ps_1_1 with 2 passes (horizontal and vertical)
technique ScreenAdvancedBlur
<
    // Script stuff is just for FX Composer
    string Script =
        "ClearSetDepth=ClearDepth;"
        "RenderColorTarget=sceneMap;"
        "ClearSetColor=ClearColor;"
        "ClearSetDepth=ClearDepth;"
        "Clear=Color;"
        "Clear=Depth;"
        "ScriptSignature=color;"
        "ScriptExternal=;"
        "Pass=AdvancedBlurHorizontal;"
        "Pass=AdvancedBlurVertical;";
>
{
    // Advanced blur shader
    pass AdvancedBlurHorizontal
    <
        string Script =
            "RenderColorTarget=blurMap;"
            "RenderDepthStencilTarget=;"
            "Draw=Buffer;";
    >
    {
        VertexShader = compile vs_1_1 VS_AdvancedBlur(float2(1, 0));
        sampler[0] = (sceneMapSampler);
        sampler[1] = (sceneMapSampler);
        sampler[2] = (sceneMapSampler);
        sampler[3] = (sceneMapSampler);
        PixelShader  = asm
        {
            // Optimized for ps_1_1, needs only 3 instructions!
            ps_1_1
            // Sample all texture coordinates
            tex t0
            tex t1
            tex t2
            tex t3

            // Mix all equally (0.2, 0.3, 0.3, 0.2 does not look much
            // different, but doesn't require all 8 ps instructions).
            add_d2 r0, t0, t1
            add_d2 r1, t2, t3
            add_d2 r0, r0, r1
        };
    }
    
    pass AdvancedBlurVertical
    <
        string Script =
            "RenderColorTarget=;"
            "RenderDepthStencilTarget=;"
            "Draw=Buffer;";
    >
    {
        VertexShader = compile vs_1_1 VS_AdvancedBlur(float2(0, 1));
        sampler[0] = (blurMapSampler);
        sampler[1] = (blurMapSampler);
        sampler[2] = (blurMapSampler);
        sampler[3] = (blurMapSampler);
        PixelShader  = asm
        {
            // Optimized for ps_1_1, needs only 3 instructions!
            ps_1_1
            def c0, 0.0, 0.0, 0.0, 1.0
            def c1, 0.0, 0.0, 0.0, 0.0
            // Sample all texture coordinates
            tex t0
            tex t1
            tex t2
            tex t3

            // Mix all equally (0.2, 0.3, 0.3, 0.2 does not look much
            // different, but doesn't require all 8 ps instructions).
            add_d2 r0, t0, t1
            add_d2 r1, t2, t3
            add_d2 r0, r0, r1
        };
    }
}

//-----------------------------------------------------------

// 8 Weights for ps_2_0
const float Weights8[8] =
{
    // more strength to middle to reduce effect of lighten up
    // shadowed areas due mixing and bluring!
    0.035,
    0.09,
    0.125,
    0.25,
    0.25,
    0.125,
    0.09,
    0.035,
};

struct VB_OutputPos8TexCoords
{
       float4 pos         : POSITION;
    float2 texCoord[8] : TEXCOORD0;
};

// generate texcoords for avanced blur
VB_OutputPos8TexCoords VS_AdvancedBlur20(
    float4 pos      : POSITION, 
    float2 texCoord : TEXCOORD0,
    uniform float2 dir)
{
    VB_OutputPos8TexCoords Out = (VB_OutputPos8TexCoords)0;
    Out.pos = pos;
    float2 texelSize = 1.0 / windowSize;
    float2 s = texCoord - texelSize*(8-1)*0.5*dir*BlurWidth20 + texelSize*0.5;
    for(int i=0; i<8; i++)
    {
        Out.texCoord[i] = s + texelSize*i*dir*BlurWidth20;
    }
    return Out;
}

float4 PS_AdvancedBlur20(
    VB_OutputPos8TexCoords In,
    uniform sampler2D tex) : COLOR
{
    float4 ret = 0;
    // This loop will be unrolled by the compiler
    for (int i=0; i<8; i++)
    {
        float4 col = tex2D(tex, In.texCoord[i]);
        ret += col * Weights8[i];
    }
    return ret;
}

// Advanced blur technique for ps_2_0 with 2 passes (horizontal and vertical)
// This one uses not only 4, but 8 texture samples!
technique ScreenAdvancedBlur20
<
    // Script stuff is just for FX Composer
    string Script =
        "ClearSetDepth=ClearDepth;"
        "RenderColorTarget=sceneMap;"
        "ClearSetColor=ClearColor;"
        "ClearSetDepth=ClearDepth;"
        "Clear=Color;"
        "Clear=Depth;"
        "ScriptSignature=color;"
        "ScriptExternal=;"
        "Pass=AdvancedBlurHorizontal;"
        "Pass=AdvancedBlurVertical;";
>
{
    // Advanced blur shader
    pass AdvancedBlurHorizontal
    <
        string Script =
            "RenderColorTarget=blurMap;"
            "RenderDepthStencilTarget=;"
            "ClearSetColor=ClearColor;"
            "ClearSetDepth=ClearDepth;"
            "Clear=Color;"
            "Clear=Depth;"
            "Draw=Buffer;";
    >
    {
        VertexShader = compile vs_1_1 VS_AdvancedBlur20(float2(1, 0));
        PixelShader  = compile ps_2_0 PS_AdvancedBlur20(sceneMapSampler);
    }

    pass AdvancedBlurVertical
    <
        string Script =
            "RenderColorTarget=;"
            "RenderDepthStencilTarget=;"
            "ClearSetColor=ClearColor;"
            "ClearSetDepth=ClearDepth;"
            "Clear=Color;"
            "Clear=Depth;"
            "Draw=Buffer;";
    >
    {
        VertexShader = compile vs_1_1 VS_AdvancedBlur20(float2(0, 1));
        PixelShader  = compile ps_2_0 PS_AdvancedBlur20(blurMapSampler);
    }
}
