float4x4 WorldViewProj;

float4 StartColor = float4(1,0,0,1);    // start color and opacity
float4 EndColor = float4(1,1,0,0);      // end color and opacity

float2 PointSize;        // min and max point sizes
float VelocityScale;     // velocity multiplier

float3 Times;            // elapsed time and particle time

#define ElapsedTime      Times.x
#define ParticleTime     Times.y
#define TotalTime        Times.z
#define ParticleSize     InTexCoord.x
#define ParticleOffset   InTexCoord.y

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
};

void ParticleVS( 
     in float4 InPosition    : POSITION,
     in float3 InVelocity    : NORMAL,
     in float2 InTexCoord    : TEXCOORD0,
    out float4 OutPosition   : POSITION,
    out float4 OutColor      : COLOR0,
    out float  OutSize       : PSIZE,
    out float4 OutRotation   : COLOR1)
{
    // particle time
    float time = ElapsedTime + ParticleOffset * ParticleTime;

    // particle position
    float4 Pos = InPosition;
    if (time < 0) // if not yet alive move far away
        Pos.xyz = 1e10;
    
    // particle time (may loop)
    time = fmod(time, ParticleTime);
    
    // normalized particle time
    float norm_time = time / ParticleTime;
    
    // length of velocity
    float vel_len = length(InVelocity);
    
    // itegrate movement
    float integral = vel_len * (norm_time - 0.5 * norm_time * norm_time);
    
    // normalized velocity
    float3 norm_vel = normalize(InVelocity);
    
    // compute final particle position                     
    Pos.xyz += VelocityScale * norm_vel * integral * ParticleTime;

    // output position
    OutPosition = mul(Pos, WorldViewProj);
    
    // compute color inerpolation position
    float color_factor = 1.0 - (1.0 - norm_time)*VelocityScale;
    
    // output color
    OutColor = lerp(StartColor, EndColor, color_factor);
    
    // project velocity in view space and peoject in XY plane
    float2 screen_vel = mul(norm_vel, (float3x3)WorldViewProj).xy;
    
    // scaling factor for projected angle
    float angle_scale = length(screen_vel);
    
    // normalize screen velocity
    screen_vel = normalize(screen_vel);
    
    // compute 2x2 rotation matrix
    float4 rot = float4(screen_vel.y, -screen_vel.x, screen_vel.x, screen_vel.y);
    
    // output rotation
    OutRotation = rot * 0.5 + 0.5;

    // compute size
    float size = lerp(PointSize.x, PointSize.y, ParticleSize);
    
    // if in a burst mode (not loop mode) scale particles with angle
    if (TotalTime == ParticleTime)
        size *= angle_scale;
    
    // output size
    OutSize = size / OutPosition.w * 360;
}

float4 ParticlePS( 
    in float4 Color     : COLOR0,
    in float4 Rotation  : COLOR1,
#ifdef XBOX
    in float2 TexCoord  : SPRITETEXCOORD
#else
    in float2 TexCoord  : TEXCOORD0
#endif
    ) : COLOR0
{
    // unpack rotation matrix
    Rotation = Rotation * 2 - 1;

    // rotate point sprite texcoord
    float2 tc = 0.5 + mul(TexCoord - 0.5, float2x2(Rotation));

    // return final color
    return Color * tex2D(TextureSampler, tc);
}

Technique Particle
{
    Pass
    {
        VertexShader = compile vs_1_1 ParticleVS();
        PixelShader = compile ps_2_0 ParticlePS();
    }
}

