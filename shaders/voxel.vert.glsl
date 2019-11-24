#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texcoords;
layout(location = 3) in uint textureID;

layout(location = 0) out vec2 frag_texcoords;
layout(location = 1) out uint frag_textureID;

layout(binding=1) uniform UniformBufferObject{
    mat4 view;
    mat4 proj;
    mat4 model[];
} ubo;

void main(){
    frag_texcoords=texcoords;
    frag_textureID=textureID;
    gl_Position=ubo.proj*ubo.view*ubo.model[0]*vec4(position,1);
}