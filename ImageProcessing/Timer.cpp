

#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include "stdafx.h"
#include <cassert>
#include <climits>

#include "Timer.h"


struct Timer::Data
{
    unsigned __int64 precision;
    unsigned __int64 start;
    static unsigned __int64 freq;
};

unsigned __int64 Timer::Data::freq;

Timer::Timer (Precision precision) : _(*new Data)
{
    _.precision = precision;
	if (!_.freq)	{
		BOOL rc = QueryPerformanceFrequency ((PLARGE_INTEGER)&_.freq); assert (rc);
	}

	start();
}

Timer::~Timer() { delete &_; }

void
Timer::start()
{
	BOOL rc = QueryPerformanceCounter ((PLARGE_INTEGER)&_.start); assert (rc);
}

unsigned __int64
Timer::frequency64 () const
{
    return _.freq;
}

unsigned __int64
Timer::elapsed64 () const
{
    unsigned __int64 now;
	BOOL rc = QueryPerformanceCounter ((PLARGE_INTEGER)&now); assert (rc);
    unsigned __int64 elapsed = now >= _.start ? now - _.start : _UI64_MAX - _.start + now;
    return _.precision == RAW ? elapsed : elapsed * _.precision / _.freq;
}

unsigned long
Timer::elapsed () const
{
    unsigned __int64 elapsed = elapsed64();  assert (elapsed < ULONG_MAX);
	return (unsigned long) elapsed;
}
