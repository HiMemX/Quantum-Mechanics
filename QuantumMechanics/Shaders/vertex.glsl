#version 450 core

layout(location=0) in vec4 inPos;
layout(location=1) in vec4 inNormal;
layout(location=2) in vec4 inColor;

uniform mat4 mat;


out vec3 normal;
out vec3 color;

void main(){
    uint vid = gl_VertexID;
    //gl_Position = vec4(pos[vid].xy, 0, 1.0);
    gl_Position = mat * vec4(inPos.xyz, 1);
    normal = inNormal.xyz;
    color = inColor.xyz;
}