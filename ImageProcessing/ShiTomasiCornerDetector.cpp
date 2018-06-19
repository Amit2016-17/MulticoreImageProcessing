#include "stdafx.h"
#include <fstream>
#include "omp.h"

using namespace std;

extern "C" __declspec(dllexport) int __stdcall ShiTomasiCornerDetector(BYTE* inBGR, BYTE* outBGR, int stride, int width, int height, KVP* arr, int nArr)
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
	const int radius_kernel = parameter("radius", 3, arr, nArr);	// Radius(->size) of the window to detect the corner ( and of the gaussian matrix)

	int radius_x = width / 2;
	int radius_y = height / 2;

	// Creating Sobel Kernels
	double M[2][3][3] = { { { -1,0,1 },{ -2,0,2 },{ -1,0,1 } },{ { -1,-2,-1 },{ 0,0,0 },{ 1,2,1 } } };

	// Creating Gauss Kernel
	const int size_kernel = 2 * radius_kernel + 1;
	double ** MGauss = new double*[size_kernel];
	InitGaussian(MGauss, size_kernel);

	// Creating a temporary memory to keep the Grayscale picture
	BYTE* tmpBGR = new BYTE[stride*height * 4];
	if (tmpBGR) {
		// Creating the 3 matrices to store the Sobel results, for each thread
		int max_threads = omp_get_max_threads();
		double *** Ix = new double**[max_threads];
		double *** Iy = new double**[max_threads];
		double *** Ixy = new double**[max_threads];
		for (int i = 0; i < max_threads; i++) {
			Ix[i] = new double*[size_kernel];
			Iy[i] = new double*[size_kernel];
			Ixy[i] = new double*[size_kernel];
			for (int j = 0;j < size_kernel;j++) {
				Ix[i][j] = new double[size_kernel];
				Iy[i][j] = new double[size_kernel];
				Ixy[i][j] = new double[size_kernel];
			}
		}

		// Converting the picture into a grayscale picture
		Grayscale(inBGR, tmpBGR, stride, width, height, openMP);

		// If the boolean openMP is true, this directive is interpreted so that the following for loop
		// will be run on multiple cores.
#pragma omp parallel for if(openMP)
		for (int v = 0; v < height; ++v) {
			auto offset = v * stride;
			BGRA* p = reinterpret_cast<BGRA*>(tmpBGR + offset);
			BGRA* q = reinterpret_cast<BGRA*>(outBGR + offset);
			for (int u = 0; u < width; ++u) {
				int x = u - radius_x;
				bool skip = abs(x) > (radius_x - radius_kernel - 1) || abs(radius_y - v) > (radius_y - radius_kernel - 1);
				if (skip) {
					q[u] = BGRA{ 0,0,0,255 };	// if convolution not possible (near the edges)
				}
				else {
					int id_thread = omp_get_thread_num();
					// For each pixel of the kernel, apply the Sobel operator
					for (int y = 0, dy = -radius_kernel; y < size_kernel; y++, dy++) {
						for (int x = 0, dx = -radius_kernel; x < size_kernel; x++, dx++) {
							int indexKernel = u + dx + dy * width;
							// Application of the Sobel operator
							double T[2];
							T[0] = 0;
							T[1] = 0;
							for (int yS = 0, dy_S = -1; yS < 3; yS++, dy_S++) {
								for (int xS = 0, dx_S = -1; xS < 3; xS++, dx_S++) {
									int indexSobel = indexKernel + dx_S + dy_S * width;
									T[0] += p[indexSobel].G * M[0][yS][xS];
									T[1] += p[indexSobel].G * M[1][yS][xS];
								}
							}
							// Save the results Ix2 and Iy2 and IxIy into the 3 matrices	
							double Tx(T[0]), Ty(T[1]);
							Ix[id_thread][y][x] = Tx * Tx;
							Iy[id_thread][y][x] = Ty * Ty;
							Ixy[id_thread][y][x] = Tx * Ty;
						}
					}

					// After the Sobel operator is applied to neighbors
					// For each matrix, apply the Gaussian kernel
					double Tx(0), Ty(0), Txy(0);
					for (int j = 0; j < size_kernel; j++) {
						for (int i = 0; i < size_kernel; i++) {
							Tx += MGauss[i][j] * Ix[id_thread][i][j];
							Ty += MGauss[i][j] * Iy[id_thread][i][j];
							Txy += MGauss[i][j] * Ixy[id_thread][i][j];
							// Reset values for the next time the thread will run
							Ix[id_thread][i][j] = 0;
							Iy[id_thread][i][j] = 0;
							Ixy[id_thread][i][j] = 0;
						}
					}
					// These 3 results are parts of a matrix A such as
					// A = | Tx	 Txy|
					//	   | Txy Ty | 	

					double det = Tx * Ty - (Txy*Txy);
					double trace = (Tx + Ty);

					// Calculation of the eigen values of the matrice A
					double a(1), b(-trace), c(det), D, l1, l2;
					D = b * b - 4 * a*c;
					l1 = fabs(-b - sqrt(D)) / (2 * a);
					l2 = fabs(-b + sqrt(D)) / (2 * a);
					double k = min(l1, l2);
					// Both eigen values have to be greater than a certain value 
					// for the pixel to be considered as part of a corner
					if (k > 10000)			// condition for corner detection
						q[u] = BGRA{ 255,255,255,255 };
					else
						q[u] = BGRA{ 0,0,0,255 };
				}
			}
		}

		// Delete the allocated memory for the convolution kernel and the temporary grayscale image
		for (int i = 0;i < size_kernel;i++) {
			delete MGauss[i];
		}
		delete[] MGauss;
		delete tmpBGR;
		// And also the allocated matrices for the Sobel operators
		for (int i = 0; i < size_kernel; i++) {
			for (int j = 0; j < size_kernel; j++) {
				delete Ix[i][j];
				delete Iy[i][j];
				delete Ixy[i][j];
			}
			delete Ix[i];
			delete Iy[i];
			delete Ixy[i];
		}
		delete[] Ix;
		delete[] Iy;
		delete[] Ixy;
	}
	return 0;
}
