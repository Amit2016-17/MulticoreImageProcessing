#pragma once

struct BGRA {
	BYTE B, G, R, A;
};

// For the Gaussian Blur. Initializes an array of double which represents the Gaussian kernel.
// This kernel will be applied to the picture.
void InitGaussian(double** tab, double size);

// Converts a BGRA picture into a Grayscale picture. This is needed for some filtering techniques.
// It can be executed on multiple cores if the omp parameter is set to true.
void Grayscale(BYTE* in, BYTE* out, int stride, int width, int height, bool omp);