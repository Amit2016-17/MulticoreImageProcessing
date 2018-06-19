#include "stdafx.h"
#include <iostream>
#include <math.h>
#include <omp.h>
#include <fstream>
#include "Routine.h"

using namespace std;

extern "C" __declspec(dllexport) int __stdcall GaussianBlur(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr)
{
	// Pack the following structure on one-byte boundaries: smallest possible alignment
	// This allows to use the minimal memory space for this type: exact fit - no padding 
#pragma pack(push, 1)
	struct BGRA
	{
		BYTE B, G, R, A;
	};
#pragma  pack(pop)		// Back to the default packing mode

	// Reading the input parameters
	bool openMP = parameter("openMP", 1, arr, nArr) == 1 ? true : false;	// If openMP should be used for multithreading
	const int radius_kernel = parameter("radius", 2, arr, nArr);			// Radius of the convolution kernel

	// Creating Gauss matrix
	const int size = radius_kernel * 2 + 1;
	double sigma = (size - 1) / 6.0;
	double ** matrix = new double*[size];
	InitGaussian(matrix, size);

	int radius_x = width / 2;
	int radius_y = height / 2;

	// If the boolean openMP is true, this directive is interpreted so that the following for loop
	// will be run on multiple cores.

#pragma omp parallel for if(openMP)
	for (int i = 0; i < height; ++i) {
		int offset = i * stride;
		BGRA* p = reinterpret_cast<BGRA*>(inBGR + offset);
		BGRA* q = reinterpret_cast<BGRA*>(outBGR + offset);
		int y = radius_y - i;
		for (int j = 0; j < width; ++j) {
			int x = j - radius_x;
			bool skip = abs(x) > (radius_x - radius_kernel) || abs(y) > (radius_y - radius_kernel);
			if (skip)
				q[j] = p[j];	// if convolution not possible (near the edges)
			else {
				BYTE R, G, B;
				double red(0), blue(0), green(0);
				// Apply the convolution kernel to every applicable pixel of the image
				for (int jj = 0, dY = -radius_kernel; jj < size; jj++, dY++) {
					for (int ii = 0, dX = -radius_kernel; ii < size; ii++, dX++) {
						int index = j + dX + dY * width;
						// Multiply each element in the local neighboorhood of the center pixel
						//  by the corresponding element in the convolution kernel
						// For the three colors
						blue += p[index].B * matrix[ii][jj];
						red += p[index].R * matrix[ii][jj];
						green += p[index].G * matrix[ii][jj];
					}
				}
				// Writing the results to the output image
				B = blue;
				R = red;
				G = green;
				q[j] = BGRA{ B,G,R,255 };
			}
		}
	}

	// Delete the allocated memory for the convolution kernel 
	for (int i = 0;i < size;i++) {
		delete matrix[i];
	}
	delete[] matrix;
	return 0;
}
