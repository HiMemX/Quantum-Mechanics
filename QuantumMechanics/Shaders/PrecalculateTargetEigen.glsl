#version 430

layout(local_size_x = 128, local_size_y=8) in;

layout(std430, binding = 0) readonly buffer TargetWavefunction{ // Complex valued
	double targetwave[];
};

layout(std430, binding = 1) readonly buffer Eigenfunctions{ // Real valued
	double eigenwaves[];
};

layout(std430, binding = 2) writeonly buffer MultiplicationResult{ // Complex valued
	double result[];
};

uniform int datapointCount;
uniform int eigenfunctionCount;

uniform float ds;

void main(){
	uint datapointIndex = gl_GlobalInvocationID.x;
	uint eigenfunctionIndex = gl_GlobalInvocationID.y;

	if(datapointIndex >= datapointCount) return;
	if(eigenfunctionIndex >= eigenfunctionCount) return;

	uint eigendatapointIndex = datapointIndex + datapointCount * eigenfunctionIndex;

	result[2 * eigendatapointIndex] = ds * targetwave[2 * datapointIndex] * eigenwaves[eigendatapointIndex];
	result[2 * eigendatapointIndex + 1] = ds * targetwave[2 * datapointIndex + 1] * eigenwaves[eigendatapointIndex];

	
}