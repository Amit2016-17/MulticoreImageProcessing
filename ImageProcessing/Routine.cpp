#include "stdafx.h"
#include <math.h>
#include <fstream>

void InitGaussian(double** tab, double size)
{
	// Setting up the values to initialize the Gaussian kernel
	double radius = (size - 1) / 2;
	double sigma = radius / 3.0;
	double dsq_sigma = 2 * sigma*sigma;
	double pi = acos(-1.0);
	double k = 1 / (pi * dsq_sigma);
	double total = 0;

	// Allocating the memory
	for (int i = 0;i < size;i++) {
		tab[i] = new double[size];
	}

	// Calculating each element of the kernel
	for (int i = 0; i < size; i++) {
		int y = radius - i;
		for (int j = 0; j < size; j++) {
			int x = j - radius;
			double G = k * exp(-((x*x) + (y*y)) / dsq_sigma);
			total += G;
			tab[i][j] = G;
		}
	}
	// Normalizing so that the sum of every element is 1
	// So that the picture is not darken or lighten 
	for (int i = 0; i < size; i++) {
		for (int j = 0; j < size;j++) {
			tab[i][j] /= total;
		}
	}

}

void Grayscale(BYTE* in, BYTE* out, int stride, int width, int height, bool omp) {

	// If the boolean omp is true, this directive is interpreted so that the following for loop
	// will be run on multiple cores.
#pragma omp parallel for if(omp)
	// For each pixel of the picture applying a formula to convert a RGB image to a Grayscale one
	// Every pixel will get the same value for Blue, Green and Red
	for (int i = 0; i < height; i++) {
		auto offset = i * stride;
		BGRA* p = reinterpret_cast<BGRA*>(in + offset);
		BGRA* tmp = reinterpret_cast<BGRA*>(out + offset);
		for (int j = 0; j < width; j++) {
			BYTE same = (0.299 * p[j].R) + (0.587 * p[j].G) + (0.114 * p[j].B);
			tmp[j] = BGRA{ same,same,same,255 };
		}
	}
}