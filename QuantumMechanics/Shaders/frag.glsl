#version 450 core
out vec4 FragColor;

in float height;

void main()
{
	FragColor = vec4(height+0.2,height+0.2,height+0.2,1);
}