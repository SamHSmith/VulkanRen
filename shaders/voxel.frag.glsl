#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 position;
layout(location = 3) in vec3 worldposition;
layout(location = 4) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2DArray texSampler;

const float practicallyNoLightAfter1MConst=0.1;
const float ambientLighting=0.1;

void main()
{
    float range=50;

    vec3 fromLight=worldposition-vec3(4,3.5,4);
    float dist=length(fromLight)/practicallyNoLightAfter1MConst;
    dist/=range;

    float lightStrength=1/(dist*dist);

    float light=0;
    light+=lightStrength*ambientLighting;//ambient

    light+=clamp(dot(normalize(-normal),normalize(fromLight)),0,1)*lightStrength;//a default light should have no light beyond 1m

    outColour=texture(texSampler,vec3(texcoords.x,-texcoords.y,textureID))*vec4(light,light,light,1);

    gl_FragDepth=(position.z/position.w);
}