#version 430

layout(local_size_x = 32, local_size_y = 32) in;

struct Vertex{
	vec4 position;
	vec4 normal;
};

layout(std430, binding = 0) readonly buffer ProbabilityDistribution{
	double[] probability;
};

layout(std430, binding = 1) buffer Mesh{
	Vertex[] vertices;
};


uniform int spacialResolution;
uniform double domainLength;

void main(){
	uint x_index = gl_GlobalInvocationID.x;
	uint y_index = gl_GlobalInvocationID.y;

	if(x_index >= spacialResolution) return;
	if(y_index >= spacialResolution) return;

	uint index = x_index + y_index * spacialResolution;

	vertices[index].position.x = float(x_index * domainLength / double(spacialResolution));
	vertices[index].position.z = float(y_index * domainLength / double(spacialResolution));
	vertices[index].position.y = float(probability[index]);
	vertices[index].position.w = 1;

	//vertices[index].normal = vec4(1,2,3,4);

	vertices[index].normal = vec4(0);

	ivec2[4] offsets;
	offsets[0] = ivec2(-1, 0);
	offsets[1] = ivec2(0, -1);
	offsets[2] = ivec2(1, 0);
	offsets[3] = ivec2(0, 1);

	float ds = float(domainLength) / float(spacialResolution);
	
	uint idx;
	idx = (x_index + offsets[3].x) + (y_index + offsets[3].y) * spacialResolution;
	
	vec3 normal1 = vec3(offsets[3].x * ds, float(probability[idx] - probability[index]), offsets[3].y * ds);
	vec3 normal2;
	for (int i=0; i<4; i++){
		if(x_index + offsets[i].x >= spacialResolution) continue;
		if(x_index + offsets[i].x < 0) continue;
		if(y_index + offsets[i].y >= spacialResolution) continue;
		if(y_index + offsets[i].y < 0) continue;

		idx = (x_index + offsets[i].x) + (y_index + offsets[i].y) * spacialResolution;
	
		normal2 = vec3(offsets[i].x * ds , float(probability[idx] - probability[index]), offsets[i].y * ds);

		vertices[index].normal += vec4(normalize(cross(normal1, normal2)), 0);
		
		normal1 = normal2;
	}

	if(vertices[index].normal.y < 0) vertices[index].normal *= -1;

	vertices[index].normal = normalize(vertices[index].normal);
}
