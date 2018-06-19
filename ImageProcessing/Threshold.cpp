#include "stdafx.h"
#include <fstream>

using namespace std;

extern "C" __declspec(dllexport) int __stdcall Threshold(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr)
{
	// Pack the following structure on one-byte boundaries: smallest possible alignment
	// This allows to use the minimal memory space for this type: exact fit - no padding 
#pragma pack(push, 1)
	struct BGRA {
		BYTE B, G, R, A;
	};
#pragma  pack(pop)		// Back to the default packing mode 

	// Reading the input parameters
	double threshold = parameter("threshold", 0.75, arr, nArr);				// Value for thresholding (%)
	bool openMP = parameter("openMP", 1, arr, nArr) == 1 ? true : false;	// If openMP should be used for multithreading

	// Apply the following algorithm to every pixel of the picture

	// If the boolean openMP is true, this directive is interpreted so that the following for loop
	// will be run on multiple cores.
#pragma omp parallel for if(openMP)
	for (int i = 0; i < height; ++i) {
		auto offset = i * stride;
		BGRA* p = reinterpret_cast<BGRA*>(inBGR + offset);
		BGRA* q = reinterpret_cast<BGRA*>(outBGR + offset);
		for (int j = 0; j < width; ++j) {
			// Calculation of the relative luminance of each pixel
			auto Y = (0.299 * p[j].R) + (0.587 * p[j].G) + (0.114 * p[j].B);
			// Condition for thresholding
			q[j] = Y > threshold * 255 ? p[j] : BGRA{ 0,0,0,255 };
		}
	}
	return 0;
}
