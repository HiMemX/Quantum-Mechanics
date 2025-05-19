#version 450 core
out vec4 FragColor;

in vec3 normal;

void main()
{
	float val = (dot(normal, normalize(vec3(1, -1, 0))) + 1) / 2;



	FragColor = val * vec4(1) + (1-val) * vec4(0.3);
}