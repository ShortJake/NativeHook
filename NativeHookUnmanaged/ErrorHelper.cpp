#include "pch.h"
#include "ErrorHelper.h"
using namespace std;
namespace ErrorHelper {
	void(*ShowErrorMessageCallback)(const char* msg);

	void NH_SetErrorMessageCallback(LPCVOID ptr)
	{
		ShowErrorMessageCallback = (void(*)(const char*))ptr;
	}
	void ShowErrorMessage(string msg)
	{
		if (ShowErrorMessageCallback != 0) ShowErrorMessageCallback(msg.c_str());
	}
}