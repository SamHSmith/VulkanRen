#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec4 position;
layout(location = 3) in vec3 WorldPos;
layout(location = 4) in vec3 camPos;
layout(location = 5) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2DArray texSampler;

const float PI = 3.14159265359;

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}

void main()
{
    gl_FragDepth=(position.z/position.w);

    vec4 tex=texture(texSampler,vec3(texcoords.x,-texcoords.y,textureID));
    vec3 albedo=tex.xyz;
    float alpha=tex.w;
    float metallic=1;
    float roughness=0.4;
    float ao=1;

    vec3 lightColour=vec3(1,1,1);
    vec3 lightPosition=vec3(10,4,5);
    float brightness=15;
    lightColour*=brightness;

    vec3 N = normalize(Normal); 
    vec3 V = normalize(camPos - WorldPos);

    vec3 Lo = vec3(0.0);

    vec3 L = normalize(lightPosition - WorldPos);
    vec3 H = normalize(V + L);
  
    float distance    = length(lightPosition - WorldPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance     = lightColour * attenuation; 

    vec3 F0 = vec3(0.04); 
    F0      = mix(F0, albedo, metallic);
    vec3 F  = fresnelSchlick(max(dot(H, V), 0.0), F0);

    float NDF = DistributionGGX(N, H, roughness);       
    float G   = GeometrySmith(N, V, L, roughness);

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    vec3 specular     = numerator / max(denominator, 0.001);

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
  
    kD *= 1.0 - metallic;
  
    float NdotL = max(dot(N, L), 0.0);        
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;

    vec3 ambient = vec3(0.0001) * albedo * ao;
    vec3 color   = ambient + Lo;

    //color = color / (color + vec3(1.0));
    //color = pow(color, vec3(1.0/2.2)); 

    outColour = vec4(color, alpha);
    outColour=vec4(albedo,1);
}