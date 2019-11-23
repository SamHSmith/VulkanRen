#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) flat in uint textureID;

layout(location=0) out vec4 outColour;

const uint textureCount=7;

layout(binding=0) uniform sampler2D texSampler[textureCount];

void main()
{
    outColour=texture(texSampler[uint(mod(textureID,textureCount))],vec2(texcoords.x,-texcoords.y));
}