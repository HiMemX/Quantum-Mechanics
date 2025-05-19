#version 430

layout(local_size_x = 32, local_size_y = 32) in;

struct Vertex{
	vec4 position;
	vec4 normal;
	vec4 color;
};

layout(std430, binding = 0) readonly buffer FinalWavefunction{
	double[] wavefunction;
};

layout(std430, binding = 1) buffer Mesh{
	Vertex[] vertices;
};

double probability(uint index){
	return wavefunction[2*index]*wavefunction[2*index] + wavefunction[2*index+1]*wavefunction[2*index+1];
}

vec3 hsv2rgb(vec3 hsv) {
    float h = hsv.x, s = hsv.y, v = hsv.z;

    float c = v * s;
    float hp = h * 6.0;            // sector 0‑6
    float x = c * (1.0 - abs(mod(hp, 2.0) - 1.0));

    vec3 rgb;
    if      (hp < 1.0) rgb = vec3(c, x, 0.0);
    else if (hp < 2.0) rgb = vec3(x, c, 0.0);
    else if (hp < 3.0) rgb = vec3(0.0, c, x);
    else if (hp < 4.0) rgb = vec3(0.0, x, c);
    else if (hp < 5.0) rgb = vec3(x, 0.0, c);
    else              rgb = vec3(c, 0.0, x);

    float m = v - c;
    return rgb + vec3(m);
}

vec3 phaseColor(vec2 z) {
    float hue = (atan(z.y, z.x) + 3.14159265359) / (2.0 * 3.14159265359);
    return hsv2rgb(vec3(hue, 1, 1));
}

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
	vertices[index].position.y = float(probability(index)); //float(wavefunction[2*index]);//
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
	
	vec3 normal1 = vec3(offsets[3].x * ds, float(probability(idx) - probability(index)), offsets[3].y * ds);
	vec3 normal2;
	for (int i=0; i<4; i++){
		if(x_index + offsets[i].x >= spacialResolution) continue;
		if(x_index + offsets[i].x < 0) continue;
		if(y_index + offsets[i].y >= spacialResolution) continue;
		if(y_index + offsets[i].y < 0) continue;

		idx = (x_index + offsets[i].x) + (y_index + offsets[i].y) * spacialResolution;
	
		normal2 = vec3(offsets[i].x * ds , float(probability(idx) - probability(index)), offsets[i].y * ds);

		vertices[index].normal += vec4(normalize(cross(normal1, normal2)), 0);
		
		normal1 = normal2;
	}

	if(vertices[index].normal.y < 0) vertices[index].normal *= -1;

	vertices[index].normal = normalize(vertices[index].normal);

	float weight = 0;

	double p = probability(index);

	if(p < 0.004) weight = float(p) * 250;
	else weight = 1;

	vertices[index].color = (1-weight) * vec4(1) + weight * vec4(phaseColor(vec2(wavefunction[2*index], wavefunction[2*index+1])), 1);
}
