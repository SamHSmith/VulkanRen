#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texcoords;
layout(location = 3) in float textureID;

layout(location = 0) out vec2 frag_texcoords;

void main(){
    frag_texcoords=texcoords;
    gl_Position=vec4(position,1);
}