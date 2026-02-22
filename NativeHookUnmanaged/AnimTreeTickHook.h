#pragma once
#include <string>
#include "framework.h"
#include "Structs.h"

extern "C" {
	void DetourASM_rglAnimTree_Tick();
	void DetourWithParams_rglAnimTree_Tick(LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr);
}

class AnimTreeTickHook
{
private:
	static LPCVOID Address;
	const static NativeHookConfiguration ConfigValue;
	const static std::string Name;
	const static std::string Signature;
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
	static void(*ManagedCallback)(LPVOID animTreePtr, LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr);
};

