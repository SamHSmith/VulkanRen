#version 440 core

in vec2 pass_UV;

out vec4 color;

uniform sampler2D texSampler;

void main(void)
{
    color = texture(texSampler,pass_UV);
}