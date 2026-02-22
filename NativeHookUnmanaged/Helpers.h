#pragma once
#include <string>
#include <winscard.h>
#include "Structs.h"

namespace Helpers {
	extern "C" __declspec(dllexport)
		void NH_SetErrorMessageCallback(LPCVOID ptr);
	void ShowErrorMessage(std::string msg);
	void CreateHook(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID* addressVariablePtr, std::string signature, std::string name, LPVOID detour, LPVOID* originalPtr);
}