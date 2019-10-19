#version 440 core

in vec3 pos;
in vec2 uvCoords;

out vec2 pass_UV;

uniform mat4 persp;
uniform mat4 view;
uniform mat4 transform;

void main(void)
{
    gl_Position = persp * inverse(view) * transform * vec4(pos,  1.0);
    pass_UV=uvCoords;
}