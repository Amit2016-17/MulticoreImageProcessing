#include "stdafx.h"
#include <fstream>

using namespace std;

extern "C" int __stdcall LaplacianEdgeDetector(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr);
extern "C" int __stdcall GaussianBlur(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr);

extern "C" __declspec(dllexport) int __stdcall LaplacianOfGaussian(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr)
{
	int rc = -1;

	// Creating a temporary memory to keep the results of the Gaussian Filter
	BYTE* gauBGR = new BYTE[stride*height * 4];
	if (gauBGR) {
		// Apply the Gaussian Blur
		rc = GaussianBlur(inBGR, gauBGR, stride, width, height, arr, nArr);

		if (!rc)
			rc = LaplacianEdgeDetector(gauBGR, outBGR, stride, width, height, arr, nArr);

		// Delete the allocated memory for the temporary Gaussian results
		delete gauBGR;
	}
	// Return the status
	return rc;
}
