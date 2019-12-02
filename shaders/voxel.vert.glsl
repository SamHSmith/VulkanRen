#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texcoords;
layout(location = 3) in uint textureID;

layout(location = 0) out vec2 frag_texcoords;
layout(location = 1) out vec3 frag_normals;
layout(location = 2) out vec4 frag_position;
layout(location = 3) out vec3 frag_worldposition;
layout(location = 4) out vec3 frag_toCamera;
layout(location = 5) out uint frag_textureID;

layout(binding=1) uniform UniformBufferObject{
    mat4 view;
    mat4 proj;
    mat4 model[];
} ubo;

void main(){
    frag_texcoords=texcoords;
    frag_textureID=textureID;
    frag_normals=normal;

    vec4 worldpos=ubo.model[0]*vec4(position,1);
    frag_worldposition=worldpos.xyz;

    vec4 cameraPos=inverse(ubo.view)*vec4(0,0,0,1);
    frag_toCamera=(cameraPos-worldpos).xyz;

    gl_Position=ubo.proj*ubo.view*worldpos;

    frag_position=gl_Position;
}