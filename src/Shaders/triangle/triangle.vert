#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec3 inPos;

layout(location = 1) in vec2 intexCoord;

layout(location = 2) in mat4 transform;

layout(location = 6) in mat4 viewMatrix;

layout(location = 0) out vec2 texCoord;



void main() {
	
	
    gl_Position = viewMatrix * transform * vec4(inPos, 1.0);
    texCoord = intexCoord;
}

