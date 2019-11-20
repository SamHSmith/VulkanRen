#version 450

layout(location = 0) in vec2 texcoords;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2D texSampler;

void main()
{
    outColour=texture(texSampler,texcoords);
}