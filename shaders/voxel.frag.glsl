#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 position;
layout(location = 3) in vec3 worldposition;
layout(location = 4) in vec3 toCamera;
layout(location = 5) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2DArray texSampler;


void main()
{
    float brightness=20;
    float metalicness=1;
    float roughness=1;

    vec3 lightColour=vec3(1,0,1);
    vec3 lightPosition=vec3(5,1.5,5);

    vec3 fromLight=worldposition-lightPosition;
    float dist=length(fromLight);
    float totalLight=brightness/(dist*dist);
    fromLight=normalize(fromLight);

    float fresnel=pow(1-dot(normalize(toCamera),normal),10);
    roughness*=fresnel;

    vec4 albedo=texture(texSampler,vec3(texcoords.x,-texcoords.y,textureID));

    float specularAmount=totalLight*roughness;
    float diffuseAmount=totalLight-specularAmount;

    vec3 specularColour=lightColour*clamp(specularAmount*pow(dot(reflect(fromLight,normal),normalize(toCamera)),roughness*1000),0,1);
    

    vec3 diffuseColour=(vec3(1,1,1)*lightColour*clamp((dot(-fromLight,normal)*diffuseAmount),0,1));


    outColour=vec4(specularColour,1)+(albedo*vec4(diffuseColour,1));

    gl_FragDepth=(position.z/position.w);
}