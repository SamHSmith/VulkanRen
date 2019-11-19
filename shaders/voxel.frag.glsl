#version 450

layout(location = 0) in vec2 texcoords;

layout(location=0) out vec4 outColour;

void main()
{
    outColour=vec4(texcoords,0.0,1.0);
}