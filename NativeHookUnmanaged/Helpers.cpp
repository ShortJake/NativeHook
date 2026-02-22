#include "pch.h"
#include <MinHook.h>
#include "Helpers.h"
#include "SignatureScanner.h"
using namespace std;
namespace Helpers {
	void(*ShowErrorMessageCallback)(const char* msg);

	void NH_SetErrorMessageCallback(LPCVOID ptr)
	{
		ShowErrorMessageCallback = (void(*)(const char*))ptr;
	}
	void ShowErrorMessage(string msg)
	{
		if (ShowErrorMessageCallback != 0) ShowErrorMessageCallback(msg.c_str());
	}
	void CreateHook(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID* addressVariablePtr, string signature, string name, LPVOID detour, LPVOID* originalPtr)
	{
		if (signature.empty())
		{
			Helpers::ShowErrorMessage("Signature for " + name + " is empty. Is this intended?");
			return;
		}
		*addressVariablePtr = ScanForFirstResult((LPVOID)nativeDLLAdress, buffer, signature, name);
		if (*addressVariablePtr == 0)
		{
			Helpers::ShowErrorMessage("Error hooking " + name);
			return;
		}
		if (MH_CreateHook((LPVOID)*addressVariablePtr, detour, originalPtr) != MH_OK)
		{
			Helpers::ShowErrorMessage("Error hooking " + name);
			return;
		}
	}
}