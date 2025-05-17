#version 430

layout(local_size_x = 32, local_size_y = 32) in;


layout(std430, binding = 0) readonly buffer ProbabilityDistribution{
	double[] probability;
};

layout(std430, binding = 1) buffer Mesh{
	vec4[] vertices;
};

uniform int spacialResolution;
uniform double domainLength;

void main(){
	uint x_index = gl_GlobalInvocationID.x;
	uint y_index = gl_GlobalInvocationID.y;

	if(x_index >= spacialResolution) return;
	if(y_index >= spacialResolution) return;

	uint index = x_index + y_index * spacialResolution;

	vertices[index].x = float(x_index * domainLength / double(spacialResolution));
	vertices[index].z = float(y_index * domainLength / double(spacialResolution));
	vertices[index].y = float(probability[index]);
	vertices[index].w = 1;
}
