#include "stdafx.h"
#include <string>

// The following function is looking for the value of a parameter into an array of parameters (KVP).
// If the parameter is not found into the array, it returns a default value.

double parameter(const char* name, double defValue, KVP* arr, int nArr)
{
	for (int i = 0; i < nArr; ++i)
		if (!strcmp(name, arr[i].key))
			return arr[i].value;
	return defValue;
}