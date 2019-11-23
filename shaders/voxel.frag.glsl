#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2D texSampler[7];

void main()
{
    outColour=texture(texSampler[uint(mod(textureID/10000,7))],vec2(texcoords.x,-texcoords.y));
}