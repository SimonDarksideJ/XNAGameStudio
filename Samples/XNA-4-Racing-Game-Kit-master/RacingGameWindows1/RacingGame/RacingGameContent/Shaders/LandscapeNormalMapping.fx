string description = "Normal mapping shaders for RacingGame";

// Shader techniques in this file, all shaders work with vs/ps 1.1, shaders not
// working with 1.1 have names with 20 at the end:
// Diffuse           : Full vertex ambient+diffuse+specular lighting
// Diffuse20         : Same for ps20, only required for 3DS max to show shader!
//
// Specular           : Full vertex ambient+diffuse+specular lighting
// Specular20         : Nicer effect for ps20, also required for 3DS max to show shader!
//
// DiffuseSpecular    : Same as specular, but adding the specular component
//                        to diffuse (per vertex)
// DiffuseSpecular20  : Nicer effect for ps20, also required for 3DS max to show shader!

float4x4 worldViewProj    : WorldViewProjection;
float4x4 world            : World;
float4x4 viewInverse      : ViewInverse;

float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {-0.65f, 0.65f, -0.39f}; // Normalized by app. FxComposer still uses inverted stuff

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
    string UIName = "Ambient Color";
    string Space = "material";
> = {0.1f, 0.1f, 0.1f, 1.0f};

float4 diffuseColor : Diffuse
<
    string UIName = "Diffuse Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

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
> = 16.0;

// Texture and samplers
texture diffuseTexture : Diffuse
<
    string UIName = "Diffuse Texture";
    string ResourceName = "Landscape.dds";
>;
sampler diffuseTextureSampler = sampler_state
{
    Texture = <diffuseTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

texture normalTexture : Diffuse
<
    string UIName = "Normal Texture";
    string ResourceName = "LandscapeNormal.dds";
>;
sampler normalTextureSampler = sampler_state
{
    Texture = <normalTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

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

texture detailTexture : Diffuse
<
    string UIName = "Detail Texture";
    string ResourceName = "LandscapeDetail.dds";
>;
sampler detailTextureSampler = sampler_state
{
    Texture = <detailTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

texture NormalizeCubeTexture : Environment
<
    string UIName = "Normalize Cube Map Texture";
    string ResourceType = "CUBE";
    string ResourceName = "NormalizeCubeMap.dds";
>;

samplerCUBE NormalizeCubeTextureSampler = sampler_state
{
    Texture = <NormalizeCubeTexture>;
    // Clamp isn't good for negative values we need to normalize!
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    float3 pos      : POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal   : NORMAL;
    float3 tangent    : TANGENT;
};

// vertex shader output structure
struct VertexOutput
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 lightVec     : COLOR0;
};

//----------------------------------------------------

// Common functions
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

// Get light direction
float3 GetLightDir()
{
    return lightDir;
}
    
float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] =
        //left handed: mul(cross(tangent, normal), world);
        mul(cross(normal, tangent), world);
    worldToTangentSpace[1] = mul(tangent, world);
    worldToTangentSpace[2] = mul(normal, world);
    return worldToTangentSpace;
}

//----------------------------------------------------

// Vertex shader function
VertexOutput VS_Diffuse(VertexInput In)
{
    VertexOutput Out = (VertexOutput) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    Out.lightVec = 0.5 + 0.5 *
        normalize(mul(worldToTangentSpace, GetLightDir()));

    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse(VertexOutput In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    float3 normalTexture = tex2D(normalTextureSampler, In.normTexCoord).agb;
    float3 normalVector =
        (2.0 * normalTexture) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Unpack the light vector to -1 - 1
    float3 lightVector =
        (2.0 * In.lightVec) - 1.0;

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    return diffuseTexture * ambDiffColor;
}

// Techniques
technique Diffuse
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Diffuse();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[1] = <diffuseColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c2, 1, 0, 0, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // v0 is lightVector
            // Convert agb to xyz (costs 1 instuction)
            lrp r1, c2, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0, r1_bx2, v0_bx2
            mad r0, r0, c1, c0
            mul r0, r0, t0
        };
    }
}

// Same for ps20 to show up in 3DS Max.
technique Diffuse20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Diffuse();
        PixelShader  = compile ps_2_0 PS_Diffuse();
    }
}

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse_Transparent(VertexOutput In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    //return diffuseTexture;
    float3 normalTexture = tex2D(normalTextureSampler, In.normTexCoord).agb;
    float3 normalVector =
        (2.0 * normalTexture) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Unpack the light vector to -1 - 1
    float3 lightVector =
        (2.0 * In.lightVec) - 1.0;

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    ambDiffColor.a = 0.33f;
    return diffuseTexture * ambDiffColor;
}

// Helper technique to display stuff with transparency in max.
technique Diffuse20Transparent
{
    pass P0
    {
        // Enable alpha for max
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        
        VertexShader = compile vs_1_1 VS_Diffuse();
        PixelShader  = compile ps_2_0 PS_Diffuse_Transparent();
    }
}

//------------------------------------------------

// vertex shader output structure (optimized for ps_1_1)
struct VertexOutput_Specular
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 lightVec     : TEXCOORD3;
    float3 lightVecDiv3 : COLOR0;
};

// Vertex shader function
VertexOutput_Specular VS_Specular(VertexInput In)
{
    VertexOutput_Specular Out = (VertexOutput_Specular) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    float3 lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
    Out.lightVec = 0.5 + 0.5 * lightVec;
    Out.lightVecDiv3 = 0.5 + 0.5 * lightVec / 3;
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    // And pass everything to the pixel shader
    return Out;
}

// Techniques
technique Specular
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        sampler[2] = (NormalizeCubeTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[2] = <diffuseColor>;
        PixelShaderConstant1[3] = <specularColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c1, 0, 0, 0, -0.25
            //def c2, 0, 0, 0, 4
            def c4, 1, 0, 0, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // Normalize view vector (t2)
            tex t2
            // Light vector (t3)
            texcoord t3
            // v0 is lightVecDiv3!
            // Convert agb to xyz (costs 1 instuction)
            lrp r1.xyz, c4, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0.xyz, r1_bx2, t3_bx2
            mad r1.xyz, r1_bx2, r0, -v0_bx2
            dp3_sat r1, r1, t2_bx2
            // Increase pow(spec) effect
            mul_x2_sat r1.w, r1.w, r1.w
            //we have to skip 1 mul because we lost 1 instruction because of agb
            //mul_x2_sat r1.w, r1.w, r1.w
            mad r0.rgb, r0, c2, c0
            // Combine 2 instructions because we need 1 more to set alpha!
            +add_sat r1.w, r1.w, c1.w
            mul r0.rgb, t0, r0
            +mul_x2_sat r1.w, r1.w, r1.w
            mad r0.rgb, r1.w, c3, r0
            // Set alpha from texture to result color!
            // Can be combined too :)
            +mov r0.w, t0.w
        };
    }
}

//----------------------------------------

// vertex shader output structure
struct VertexOutput_Specular20
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 lightVec     : TEXCOORD2;
    float3 viewVec      : TEXCOORD3;
};

// Vertex shader function
VertexOutput_Specular20 VS_Specular20(VertexInput In)
{
    VertexOutput_Specular20 Out = (VertexOutput_Specular20) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    Out.lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_Specular20(VertexOutput_Specular20 In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.normTexCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = In.lightVec;
    float3 viewVector = normalize(In.viewVec);
    // For ps_2_0 we don't need to unpack the vectors to -1 - 1

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    return diffuseTexture * ambDiffColor +
        bump * spec * specularColor * diffuseTexture.a;
}

// Techniques
technique Specular20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular20();
        PixelShader  = compile ps_2_0 PS_Specular20();
    }
}

//----------------------------------------

// Techniques
technique DiffuseSpecular
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        sampler[2] = (NormalizeCubeTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[1] = <diffuseColor>;
        PixelShaderConstant1[2] = <specularColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c3, 0, 0, 0, -0.25
            //def c2, 0, 0, 0, 4
            def c4, 1, 0, 0, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // Normalize view vector (t2)
            tex t2
            // Light vector (t3)
            texcoord t3

            // v0 is lightVecDiv3!
            // Convert agb to xyz (costs 1 instuction)
            lrp r1.xyz, c4, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0, r1_bx2, t3_bx2
            mad r1.xyz, r1_bx2, r0, -v0_bx2
            dp3_sat r1, r1, t2_bx2
            //mul_x2_sat r1.w, r1.w, r1.w
            //no more instructions left:
            // mul_x2_sat r1.w, r1.w, r1.w
            //add_sat r1.w, r1.w, c3.w
            mad_x2_sat r1.w, r1.w, r1.w, c3.w
            // r1 = r1 (spec) * specularColor + diffuseColor
            mad r1, r1.w, c2, c1
            // r0 = r0 (bump) * r1 (diff+spec color) + ambientColor
            mad r0, r0, r1, c0
            // r0 = r0 * diffuseTexture
            mul r0.rgb, t0, r0
            +mov r0.w, t0.w
        };
    }
}

//----------------------------------------

// Pixel shader function
float4 PS_DiffuseSpecular20(VertexOutput_Specular20 In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.normTexCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = In.lightVec;
    float3 viewVector = normalize(In.viewVec);
    // For ps_2_0 we don't need to unpack the vectors to -1 - 1

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    return diffuseTexture * (ambientColor +
        bump * (diffuseColor + spec * specularColor));
}

// Techniques
technique DiffuseSpecular20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular20();
        PixelShader  = compile ps_2_0 PS_DiffuseSpecular20();
    }
}

// ------------------------------

technique SpecularWithReflection
{
    pass P0
    {
        // Use the same as Specular
        VertexShader = compile vs_1_1 VS_Specular();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        sampler[2] = (NormalizeCubeTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[2] = <diffuseColor>;
        PixelShaderConstant1[3] = <specularColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c1, 0, 0, 0, -0.25
            //def c2, 0, 0, 0, 4
            def c4, 1, 0, 0, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // Normalize view vector (t2)
            tex t2
            // Light vector (t3)
            texcoord t3
            // v0 is lightVecDiv3!
            // Convert agb to xyz (costs 1 instuction)
            lrp r1.xyz, c4, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0.xyz, r1_bx2, t3_bx2
            mad r1.xyz, r1_bx2, r0, -v0_bx2
            dp3_sat r1, r1, t2_bx2
            // Increase pow(spec) effect
            mul_x2_sat r1.w, r1.w, r1.w
            //we have to skip 1 mul because we lost 1 instruction because of agb
            //mul_x2_sat r1.w, r1.w, r1.w
            mad r0.rgb, r0, c2, c0
            // Combine 2 instructions because we need 1 more to set alpha!
            +add_sat r1.w, r1.w, c1.w
            mul r0.rgb, t0, r0
            +mul_x2_sat r1.w, r1.w, r1.w
            mad r0.rgb, r1.w, c3, r0
            // Set alpha from texture to result color!
            // Can be combined too :)
            +mov r0.w, t0.w
        };
    }
}

// ------------------------------

// vertex shader output structure
struct VertexOutput_SpecularWithReflection20
{
    float4 pos          : POSITION;
    float2 texCoord     : TEXCOORD0;
    float3 lightVec     : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
};

// Vertex shader function
VertexOutput_SpecularWithReflection20
    VS_SpecularWithReflection20(VertexInput In)
{
    VertexOutput_SpecularWithReflection20 Out =
        (VertexOutput_SpecularWithReflection20) 0;
    
    float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
    Out.pos = mul(float4(In.pos.xyz, 1), worldViewProj);
    
    // Copy texture coordinates for diffuse and normal maps
    Out.texCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    Out.lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(worldEyePos - worldVertPos);
    float3 R = reflect(-viewVec, normal);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_SpecularWithReflection20(VertexOutput_SpecularWithReflection20 In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
    float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.texCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // Compute the angle to the light
    float bump = dot(normalVector, lightVector);
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    // Darken down bump factor on back faces
    float4 reflection = texCUBE(reflectionCubeTextureSampler, In.cubeTexCoord);
    float3 ambDiffColor = ambientColor + bump * diffuseColor;
    float4 ret;
    ret.rgb = diffuseTexture * ambDiffColor +
        bump * spec * specularColor * diffuseTexture.a;
    // Apply color
    ret.a = diffuseTexture.a * diffuseColor.a;
    return ret * (0.85f + reflection * 0.75f);
}

technique SpecularWithReflection20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_SpecularWithReflection20();
        PixelShader  = compile ps_2_0 PS_SpecularWithReflection20();
    }
}

// -----------------------------------

// vertex shader output structure
struct VertexOutput_Detail
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float2 detailTexCoord : TEXCOORD2;
    float3 lightVec     : COLOR0;
};

float DetailFactor
<
    string UIName = "Detail Factor";
    string UIWidget = "slider";
    float UIMin = 0.5;
    float UIMax = 128.0;
    float UIStep = 0.5;
> = 24;

// Vertex shader function
VertexOutput_Detail VS_DiffuseWithDetail(VertexInput In)
{
    VertexOutput_Detail Out = (VertexOutput_Detail) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;
    Out.detailTexCoord = In.texCoord * DetailFactor;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    Out.lightVec = 0.5 + 0.5 *
        normalize(mul(worldToTangentSpace, GetLightDir()));

    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_DiffuseWithDetail(VertexOutput_Detail In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    //return diffuseTexture;
    float3 normalTexture = tex2D(normalTextureSampler, In.normTexCoord).agb;
    float3 normalVector = (2.0 * normalTexture) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);
    
    // Detail texture
    float4 detailTexture = tex2D(detailTextureSampler, In.detailTexCoord);
    detailTexture = (2.0 * detailTexture);

    // Unpack the light vector to -1 - 1
    float3 lightVector = (2.0 * In.lightVec) - 1.0;

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    return diffuseTexture * ambDiffColor * detailTexture;
}

// Techniques
technique DiffuseWithDetail
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_DiffuseWithDetail();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        sampler[2] = (detailTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[1] = <diffuseColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c2, 1, 0, 0, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // And detail map
            tex t2
            // v0 is lightVector
            // Convert agb to xyz (costs 1 instuction)
            lrp r1, c2, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0, r1_bx2, v0_bx2
            mad r0, r0, c1, c0
            mul r0, r0, t0
            mul_x2 r0, r0, t2            
        };
    }
}

// Same for ps20 to show up in 3DS Max.
technique DiffuseWithDetail20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_DiffuseWithDetail();
        PixelShader  = compile ps_2_0 PS_DiffuseWithDetail();
    }
}
