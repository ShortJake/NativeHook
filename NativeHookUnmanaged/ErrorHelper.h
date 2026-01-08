#pragma once
#include <string>
#include <winscard.h>
using namespace std;

namespace ErrorHelper {
	extern "C" __declspec(dllexport)
		void NH_SetErrorMessageCallback(LPCVOID ptr);
	void ShowErrorMessage(string msg);
}