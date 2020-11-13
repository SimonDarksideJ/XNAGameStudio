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

float4x4 viewProj         : ViewProjection;
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
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
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
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
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

bool UseAlpha = true;

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
            def c1, 0, 0, 0, -0.45
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

    Out.lightVec = mul(worldToTangentSpace, GetLightDir());
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
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // For ps_2_0 we don't need to unpack the vectors to -1 - 1

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    if (UseAlpha)
    {
        return diffuseTexture * ambDiffColor +
            bump * spec * specularColor * diffuseTexture.a;
    }
    else
    {
        return float4(diffuseTexture.rgb * ambDiffColor +
            bump * spec * specularColor, 1.0f);
    }
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
	float3 lightVector = normalize(In.lightVec);
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

// vertex shader output structure (optimized for ps_1_1)
struct VertexOutput_SpecularWithReflection
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
    float3 lightVec     : COLOR0;
    float3 lightVecDiv3 : COLOR1;
};

// Vertex shader function
VertexOutput_SpecularWithReflection VS_SpecularWithReflection(VertexInput In)
{
    VertexOutput_SpecularWithReflection Out = (VertexOutput_SpecularWithReflection) 0; 
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

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    float3 R = reflect(-viewVec, normal);
    R = float3(R.x, R.z, R.y);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

technique SpecularWithReflection
{
    pass P0
    {
        // Use the same as Specular
        VertexShader = compile vs_1_1 VS_SpecularWithReflection();
        sampler[0] = (diffuseTextureSampler);
        sampler[1] = (normalTextureSampler);
        sampler[2] = (NormalizeCubeTextureSampler);
        sampler[3] = (reflectionCubeTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[2] = <diffuseColor>;
        PixelShaderConstant1[3] = <specularColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c1, 0, 0, 0, -0.35
            //def c2, 0, 0, 0, 4
            def c4, 1, 0, 0, 1
            def c5, 0.5, 0.5, 0.5, 1
            // Sample diffuse and normal map
            tex t0
            tex t1
            // Normalize view vector (t2)
            tex t2
            // Light vector (t3)
            tex t3
            // v0 is lightVec
            // v1 is lightVecDiv3!
            // Convert agb to xyz (costs 1 instuction)
            lrp r1.xyz, c4, t1.w, t1
            // Now work with r1 instead of t1
            dp3_sat r0.xyz, r1_bx2, v0_bx2
            mad r1.xyz, r1_bx2, r0, -v1_bx2
            dp3_sat r1, r1, t2_bx2
            // Increase pow(spec) effect
            // Adding reflection here
            mad r0.rgb, t3, c5, r0
            +mul_x2_sat r1.w, r1.w, r1.w            
            //we have to skip 1 mul because we lost 1 instruction because of agb
            //mul_x2_sat r1.w, r1.w, r1.w
            mad r0.rgb, r0, c2, c0
            // Combine 2 instructions because we need 1 more to set alpha!
            +add_sat r1.w, r1.w, c1.w
            //mul r0.rgb, t0, r0
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
    Out.pos = mul(worldVertPos, viewProj);
    
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
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    // Darken down bump factor on back faces
    float4 reflection = texCUBE(reflectionCubeTextureSampler,
        In.cubeTexCoord);
    float3 ambDiffColor = ambientColor + bump * diffuseColor;
    float4 ret;
    ret.rgb = diffuseTexture * ambDiffColor +
        bump * spec * specularColor * diffuseTexture.a;
    ret.rgb *= (0.85f + reflection * 0.75f);
    // Apply color
    ret.a = 1.0f;
    return ret;
}

technique SpecularWithReflection20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_SpecularWithReflection20();
        PixelShader  = compile ps_2_0 PS_SpecularWithReflection20();
    }
}

//----------------------------------------------------

// For ps1.1 we can't do this advanced stuff,
// just render the material with the reflection and basic lighting
struct VertexOutput_Texture
{
    float4 pos          : POSITION;
    float3 cubeTexCoord : TEXCOORD0;
    float3 normal       : TEXCOORD1;
    float3 halfVec        : TEXCOORD2;
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
    Out.halfVec = 0.5 + 0.5 * normalize(eyeVec + lightDir);

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
    float4 ret = reflColor * reflectionAmount +
        diffAmbColor;
    ret.a = alphaFactor;
    return ret +
        spec * specularColor;
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
    Out.normal = mul(In.normal, (float3x3)world);
    Out.viewVec = GetCameraPos() - GetWorldPos(In.pos);
    Out.halfVec = Out.viewVec + lightDir;
    return Out;
}

float4 PS_ReflectionSpecular20(VertexOutput20 In) : COLOR
{
    half3 N = normalize(In.normal);
    float3 V = normalize(In.viewVec);
	float3 hV = normalize(In.halfVec);

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
    float spec = pow(saturate(dot(N, hV)), shininess);
    
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
        VertexShader = compile vs_1_1 VS_ReflectionSpecular20();
        PixelShader  = compile ps_2_0 PS_ReflectionSpecular20();
    }
}

//---------------------------------------------------

// vertex shader output structure
struct VertexOutput_SpecularWithReflectionForCar20
{
    float4 pos          : POSITION;
    float2 texCoord     : TEXCOORD0;
    float3 lightVec     : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
};

// Special shader for car rendering, which allows to change the car color!
float3 carHueColor = 0.0;

// Vertex shader function
VertexOutput_SpecularWithReflectionForCar20
    VS_SpecularWithReflectionForCar20(VertexInput In)
{
    VertexOutput_SpecularWithReflectionForCar20 Out =
        (VertexOutput_SpecularWithReflectionForCar20) 0;
    
    float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
    Out.pos = TransformPosition(In.pos);
    
    // Copy texture coordinates for diffuse and normal maps
    Out.texCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();

    Out.lightVec = mul(worldToTangentSpace, GetLightDir());
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(worldEyePos - worldVertPos);
    float3 R = reflect(-viewVec, normal);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_SpecularWithReflectionForCar20(
  VertexOutput_SpecularWithReflectionForCar20 In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
    diffuseTexture.rgb = lerp(diffuseTexture.rgb, carHueColor, diffuseTexture.a);
    
    float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.texCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    // Darken down bump factor on back faces
    float4 reflection = texCUBE(reflectionCubeTextureSampler,
        In.cubeTexCoord);
    float3 ambDiffColor = ambientColor + bump * diffuseColor;
    float4 ret;
    ret.rgb = diffuseTexture * ambDiffColor +
        bump * spec * specularColor * diffuseTexture.a;
    ret.rgb *= (0.85f + reflection * 0.75f);    
    
    // Apply color
    ret.a = 1.0f;
    return ret;
}

technique SpecularWithReflectionForCar20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_SpecularWithReflectionForCar20();
        PixelShader  = compile ps_2_0 PS_SpecularWithReflectionForCar20();
    }
}

//----------------------------------------------

sampler diffuseTextureRoadSampler = sampler_state
{
    Texture = <diffuseTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
};

sampler normalTextureRoadSampler = sampler_state
{
    Texture = <normalTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
};

// Techniques
technique SpecularRoad
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular();
        sampler[0] = (diffuseTextureRoadSampler);
        sampler[1] = (normalTextureRoadSampler);
        sampler[2] = (NormalizeCubeTextureSampler);
        PixelShaderConstant1[0] = <ambientColor>;
        PixelShaderConstant1[2] = <diffuseColor>;
        PixelShaderConstant1[3] = <specularColor>;
        PixelShader = asm
        {
            // Optimized for ps_1_1, uses all possible 8 instructions.
            ps_1_1
            // Helper to calculate fake specular power.
            def c1, 0, 0, 0, -0.45
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

// Pixel shader function
float4 PS_SpecularRoad20(VertexOutput_Specular20 In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureRoadSampler, In.diffTexCoord);
    float3 normalVector = (2.0*tex2D(normalTextureRoadSampler, In.normTexCoord).agb)-1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
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
technique SpecularRoad20
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_Specular20();
        PixelShader  = compile ps_2_0 PS_SpecularRoad20();
    }
}
