#version 450 core
out vec4 FragColor;

in vec3 normal;
in vec3 color;

void main()
{
	float val = (dot(normal, normalize(vec3(1, 1, 0))) + 1) / 2;


	vec4 c = vec4(color, 1);
	FragColor = val * c + (1-val) * c / 2;
}