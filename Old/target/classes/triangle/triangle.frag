#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec2 texCoord;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(texCoord,0.5,1);
}
