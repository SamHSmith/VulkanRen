#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2DArray texSampler;

void main()
{
    outColour=texture(texSampler,vec3(texcoords.x,-texcoords.y,textureID));
}