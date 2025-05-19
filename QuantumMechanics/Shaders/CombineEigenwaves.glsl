#version 430

layout(local_size_x = 128) in;

layout(std430, binding = 0) readonly buffer Eigenvalues{
	double[] eigenvals;
};

layout(std430, binding = 1) readonly buffer Eigenwavefunctions{
	double[] eigenfunctions;
};

layout(std430, binding = 2) readonly buffer EigenfunctionComponents{
	double[] components;
};

layout(std430, binding = 3) writeonly buffer WavefunctionResult{ // Complex valued
	double[] wavefunction;
};

uniform double hbar; // Physical constants

uniform float t; // Timestamp

uniform int datapointCount;
uniform int eigenfunctionCount;

void main(){
	
	uint datapointIndex = gl_GlobalInvocationID.x;

	if(datapointIndex >= datapointCount) return;

	double sum_real = 0;
	double sum_imag = 0;

	uint eigenfunctionIndex;
	double real_comp;
	double imag_comp;
	double eigenfunctionValue;
	float exponent;
	for(int e=0; e<eigenfunctionCount; e++){
		eigenfunctionIndex = e * datapointCount;
		
		real_comp = components[2*eigenfunctionIndex]; // Cause of funky memory layout
		imag_comp = components[2*eigenfunctionIndex + 1];

		eigenfunctionValue = eigenfunctions[eigenfunctionIndex + datapointIndex];

		exponent = -float(eigenvals[e] / hbar * t);

		sum_real += eigenfunctionValue * (real_comp * cos(exponent) - imag_comp * sin(exponent));
		sum_imag += eigenfunctionValue * (real_comp * sin(exponent) + imag_comp * cos(exponent));
	}

	wavefunction[2*datapointIndex] = sum_real;
	wavefunction[2*datapointIndex+1] = sum_imag;

}
