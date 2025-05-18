#version 450 core

layout(location=0) in vec4 inPos;

uniform mat4 mat;

out float height;

void main(){
    uint vid = gl_VertexID;
    //gl_Position = vec4(pos[vid].xy, 0, 1.0);
    gl_Position = mat * vec4(inPos.xyz, 1);
    height = 2*inPos.y;
}