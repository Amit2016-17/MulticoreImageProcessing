#include "stdafx.h"
#include <math.h>
#include <fstream>

using namespace std;

extern "C" __declspec(dllexport) int __stdcall LaplacianEdgeDetector(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int narr)
{
	// Pack the following structure on one-byte boundaries: smallest possible alignment
	// This allows to use the minimal memory space for this type: exact fit - no padding 
#pragma pack(push, 1)
	struct BGRA {
		BYTE B, G, R, A;
	};
#pragma  pack(pop)		// Back to the default packing mode 

	// Reading the input parameters
	bool openMP = parameter("openMP", 1, arr, narr) == 1 ? true : false;	// If openMP should be used for multithreading

	// Setting up the Laplacian Kernel 
	const int radius = 1;
	const int size = radius * 2 + 1;
	double M[size][size] = { {-1,-1,-1},{-1,8,-1},{ -1,-1,-1} };

	// Creating a temporary memory to keep the Grayscale picture
	BYTE* tmpBGR = new BYTE[stride*height * 4];
	if (tmpBGR) {

		// Converting the picture into a grayscale picture
		Grayscale(inBGR, tmpBGR, stride, width, height, openMP);

		// If the boolean openMP is true, this directive is interpreted so that the following for loop
		// will be run on multiple cores.
#pragma omp parallel for if(openMP)
		for (int i = 0; i < height; ++i) {
			auto offset = i * stride;
			BGRA* p = reinterpret_cast<BGRA*>(tmpBGR + offset);
			BGRA* q = reinterpret_cast<BGRA*>(outBGR + offset);
			for (int j = 0; j < width; ++j) {
				if (i == 0 || j == 0 || i == height - 1 || j == width - 1)
					q[j] = p[j];	// if convolution not possible (near the edges)
				else {
					double t = 0;
					// Apply the Laplacian Kernel to every applicable pixel of the image
					// This calculates the second order derivatives
					// to detect every edges with only one kernel
					// But very sensitive to noise, a Gaussian Blur previously applied
					// can provide better results
					for (int jj = 0, dY = -radius; jj < size; jj++, dY++) {
						for (int ii = 0, dX = -radius; ii < size; ii++, dX++) {
							int index = j + dX + dY * width;
							t += p[index].G * M[ii][jj];
						}
					}
					// Condition for edge detection
					BYTE tmp = t;
					q[j] = t > 0.20 * 255 ? BGRA{ tmp,tmp,tmp,255 } : BGRA{ 0,0,0,255 };
				}
			}
		}

		// Delete the allocated memory for the temporary grayscale image
		delete tmpBGR;
	}
	return 0;
}

