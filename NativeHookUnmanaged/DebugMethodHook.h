#pragma once
#include <string>
#include "framework.h"
#include "Structs.h"
class DebugMethodHook
{
private:
	static LPCVOID Address;
	const static std::string Name;
	const static std::string Signature;
	static void(*ManagedCallback)(LPVOID animPtr, LPVOID skeletonModelPtr, BYTE boneIndex, LPVOID outQuat);
	static LPVOID(*Original)(LPVOID, BYTE, LPVOID);
	LPVOID static Detour(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr);
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
};

