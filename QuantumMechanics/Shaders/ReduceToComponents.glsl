#version 430

layout(local_size_x = 128, local_size_y = 8) in;


layout(std430, binding = 0) buffer MultiplicationResult{ // Complex valued
	double result[];
};

uniform int datapointCount;
uniform int eigenfunctionCount;

uniform int currentBlockSize;

void main(){
	
	uint datapointIndex = gl_GlobalInvocationID.x;
	uint eigenfunctionIndex = gl_GlobalInvocationID.y;

	if(datapointIndex >= currentBlockSize) return;
	if((datapointIndex + currentBlockSize) >= datapointCount) return;
	if(eigenfunctionIndex >= eigenfunctionCount) return;

	
	uint eigendatapointIndex = datapointIndex + datapointCount * eigenfunctionIndex;

	result[2 * eigendatapointIndex] += result[2 * (eigendatapointIndex + currentBlockSize)];
	result[2 * eigendatapointIndex + 1] += result[2 * (eigendatapointIndex + currentBlockSize) + 1];
}