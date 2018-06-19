#include "windows.h"
#include "stdafx.h"
#include <math.h>
#include <fstream>

using namespace std;

extern "C" __declspec(dllexport) int __stdcall SobelEdgeDetector(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr)
{
	// Pack the following structure on one-byte boundaries: smallest possible alignment
	// This allows to use the minimal memory space for this type: exact fit - no padding 
#pragma pack(push, 1)
	struct BGRA {
		BYTE B, G, R, A;
	};
#pragma  pack(pop)		// Back to the default packing mode 


	// Reading the input parameters
	bool openMP = parameter("openMP", 1, arr, nArr) == 1 ? true : false;	// If openMP should be used for multithreading

	// Creating Sobel Kernels
	const int radius = 1;
	const int size = 3;
	double M[2][size][size] = { { { 1,0,-1 },{ 2,0,-2 },{ 1,0,-1 } },{ { 1,2,1 },{ 0,0,0 },{ -1,-2,-1 } } };

	// Creating a temporary memory to keep the Grayscale picture
	BYTE* tmpBGR = new BYTE[stride*height * 4];
	if (tmpBGR) {

		// Converting the image to a grayscale picture.
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
					double _T[2];
					_T[0] = 0; _T[1] = 0;
					// Applying the two Sobel operators (dX dY) to every applicable pixel
					for (int jj = 0, dY = -radius; jj < size; jj++, dY++) {
						for (int ii = 0, dX = -radius; ii < size; ii++, dX++) {
							int index = j + dX + dY * width;
							// Multiplicating each pixel in the neighborhood by the two Sobel Operators
							// It calculates the vertical and horizontal derivatives of the image at a point.
							_T[1] += p[index].G * M[1][ii][jj];
							_T[0] += p[index].G * M[0][ii][jj];
						}
					}
					// Then is calculated the magnitude of the derivatives
					BYTE a = sqrt((_T[0] * _T[0]) + (_T[1] * _T[1]));
					// Condition for edge detection
					q[j] = a > 0.20 * 255 ? BGRA{ a,a,a,255 } : BGRA{ 0,0,0,255 };
				}
			}
		}

		//Delete the allocated memory for the temporary grayscale image
		delete tmpBGR;
	}
	return 0;
}
