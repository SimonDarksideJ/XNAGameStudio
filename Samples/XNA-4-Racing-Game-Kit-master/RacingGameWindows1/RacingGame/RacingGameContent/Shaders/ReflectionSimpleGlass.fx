string description = "Reflection shader for glass materials in RacingGame";

// Variables that are provided by the application.
// Support for UIWidget is also added for FXComposer and 3DS Max :)

float4x4 viewProj              : ViewProjection;
float4x4 world                 : World;
float4x4 viewInverse           : ViewInverse;

float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f};

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
    string UIName = "Ambient Color";
    string Space = "material";
> = {0.15f, 0.15f, 0.15f, 1.0f};

float4 diffuseColor : Diffuse
<
    string UIName = "Diffuse Color";
    string Space = "material";
> = {0.25f, 0.25f, 0.25f, 1.0f};

float4 specularColor : Specular
<
    string UIName = "Specular Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float shininess : SpecularPower
<
    string UIName = "Specular Power";
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
> = 24.0;

float alphaFactor
<
    string UIName = "Alpha factor";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.66f;

float fresnelBias = 0.5f;
float fresnelPower = 1.5f;
float reflectionAmount = 1.0f;

texture reflectionCubeTexture : Environment
<
    string UIName = "Reflection cube map";
    string ResourceType = "CUBE";
    string ResourceName = "SkyCubeMap.dds";
>;
samplerCUBE reflectionCubeTextureSampler = sampler_state
{
    Texture = <reflectionCubeTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    float3 pos      : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal   : NORMAL;
    float3 tangent  : TANGENT;
};

//----------------------------------------------------

// Common functions
float4 TransformPosition(float3 pos)
{
    return mul(mul(float4(pos.xyz, 1), world), viewProj);
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

//----------------------------------------------------

// For ps1.1 we can't do this advanced stuff,
// just render the material with the reflection and basic lighting
struct VertexOutput_Texture
{
    float4 pos          : POSITION;
    float3 cubeTexCoord : TEXCOORD1;
    float3 normal       : TEXCOORD2;
    float3 halfVec        : TEXCOORD3;
};

// vertex shader
VertexOutput_Texture VS_ReflectionSpecular(VertexInput In)
{
    VertexOutput_Texture Out;
    Out.pos = TransformPosition(In.pos);
    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    float3 R = reflect(-viewVec, normal);
    R = float3(R.x, R.z, R.y);
    Out.cubeTexCoord = R;
    
    // Determine the eye vector
    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);
    
    // Calc normal vector
    Out.normal = 0.5 + 0.5 * CalcNormalVector(In.normal);
    // Eye vector
    float3 eyeVec = normalize(worldEyePos - worldVertPos);
    // Half angle vector
    Out.halfVec = 0.5 + 0.5 *
        normalize(eyeVec + lightDir);

    return Out;
}

float4 PS_ReflectionSpecular(VertexOutput_Texture In) : COLOR
{
    // Convert colors back to vectors. Without normalization it is
    // a bit faster (2 instructions less), but not as correct!
    float3 normal = 2.0 * (saturate(In.normal)-0.5);
    float3 halfVec = 2.0 * (saturate(In.halfVec)-0.5);

    // Diffuse factor
    float diff = saturate(dot(normal, lightDir));
    // Specular factor
    float spec = saturate(dot(normal, halfVec));
    //max. possible pow fake with mults here: spec = pow(spec, 8);
    //same as: spec = spec*spec*spec*spec*spec*spec*spec*spec;

    // (saturate(4*(dot(N,H)^2-0.75))^2*2 is a close approximation
    // to pow(dot(N,H), 16). I use something like
    // (saturate(4*(dot(N,H)^4-0.75))^2*2 for approx. pow(dot(N,H), 32)
    spec = pow(saturate(4*(pow(spec, 2)-0.795)), 2);

    // Output the color
    float4 diffAmbColor = ambientColor + diff * diffuseColor;

    float3 reflect = In.cubeTexCoord;
    half4 reflColor = texCUBE(reflectionCubeTextureSampler, reflect);
    float4 ret = reflColor * reflectionAmount + diffAmbColor;
    ret.a = alphaFactor;
    return ret + spec * specularColor;
}

technique ReflectionSpecular
{
    pass P0
    {
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;    
        VertexShader = compile vs_1_1 VS_ReflectionSpecular();
        PixelShader  = compile ps_2_0 PS_ReflectionSpecular();
    }
}

//----------------------------------------------------

struct VertexOutput20
{
    float4 pos      : POSITION;
    float3 normal   : TEXCOORD0;
    float3 viewVec  : TEXCOORD1;
    float3 halfVec  : TEXCOORD2;
};

// vertex shader
VertexOutput20 VS_ReflectionSpecular20(VertexInput In)
{
    VertexOutput20 Out;
    Out.pos = TransformPosition(In.pos);
    Out.normal = CalcNormalVector(In.normal);
    Out.viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    Out.halfVec = normalize(Out.viewVec + lightDir);
    return Out;
}

float4 PS_ReflectionSpecular20(VertexOutput20 In) : COLOR
{
    half3 N = normalize(In.normal);
    float3 V = normalize(In.viewVec);

    // Reflection
    half3 R = reflect(-V, N);
    R = float3(R.x, R.z, R.y);
    half4 reflColor = texCUBE(reflectionCubeTextureSampler, R);
    
    // Fresnel
    float3 E = -V;
    float facing = 1.0 - max(dot(E, -N), 0);
    float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);

    // Diffuse factor
    float diff = saturate(dot(N, lightDir));

    // Specular factor
    float spec = pow(saturate(dot(N, In.halfVec)), shininess);
    
    // Output the colors
    float4 diffAmbColor = ambientColor + diff * diffuseColor;
    float4 ret;
    ret.rgb = reflColor * reflectionAmount * fresnel * 1.5f +
        diffAmbColor;
    ret.a = alphaFactor;
    ret += spec * specularColor;
    return ret;
}

technique ReflectionSpecular20
{
    pass P0
    {
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        
        VertexShader = compile vs_1_1 VS_ReflectionSpecular20();
        PixelShader  = compile ps_2_0 PS_ReflectionSpecular20();
    }
}
