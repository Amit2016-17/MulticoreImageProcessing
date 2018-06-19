#pragma once

// The following structure and function are setting up a link between the library and the client application
// It allows the DLL to receive input parameters chosen by the user, for example:
//	- Whether to run the process on a single core or on multiple cores
//  - The size of the convolution kernel to apply to the source picture


struct KVP {
	const char* key;	// represents the name of the parameter ("radius" for example)
	double value;		// contains the value of the parameter
};

double parameter(const char* name, double defValue, KVP* arr, int nArr);
