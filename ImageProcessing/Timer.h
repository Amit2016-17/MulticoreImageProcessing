#ifndef _OrionLib_Timer_H_
#define _OrionLib_Timer_H_


struct Timer
{
	enum Precision { NS = 1000000000, US = 1000000, MS = 1000, SEC = 1, RAW = 0 };

	Timer (Precision precision);
    virtual ~Timer();

	void start();

	unsigned long    elapsed  () const;
    unsigned __int64 elapsed64() const;
    unsigned __int64 frequency64() const;

    struct Data; Data& _;

    Timer& operator=(Timer&) = delete;
};



#endif
