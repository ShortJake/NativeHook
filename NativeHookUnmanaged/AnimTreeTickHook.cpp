#include "pch.h"
#include "AnimTreeTickHook.h"
using namespace std;

extern "C" {
	void* Original_rglAnimTree_Tick;
}
LPCVOID AnimTreeTickHook::Address = 0;
const string AnimTreeTickHook::Name = "AnimTreeTick";
const NativeHookConfiguration AnimTreeTickHook::ConfigValue = Config_AnimTreeTick;
#if EDITOR
const string AnimTreeTickHook::Signature = "0f 8c ? ? ? ? 44 0f 28 bc [..100100] 20 14 00 00";
#else
const string AnimTreeTickHook::Signautre = "0f 8c e6 f8 ff ff 44 0f b6 64 24 20";
#endif
void(*AnimTreeTickHook::ManagedCallback)(LPVOID animTreePtr, LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr) = 0;

void DetourWithParams_rglAnimTree_Tick(LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr)
{
	AnimTreeTickHook::ManagedCallback(0x0, skeletonPtr, boneIndex, cachedMatrixFrameArrayPtr);
}

void AnimTreeTickHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	return;
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (void(*)(LPVOID, LPVOID, BYTE, LPVOID))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, DetourASM_rglAnimTree_Tick, &Original_rglAnimTree_Tick);
}