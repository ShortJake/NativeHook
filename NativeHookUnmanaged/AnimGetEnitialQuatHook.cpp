#include "pch.h"
#include "AnimGetEnitialQuatHook.h"
using namespace std;

LPCVOID AnimGetEnitialQuatHook::Address = 0;
const string AnimGetEnitialQuatHook::Name = "AnimGetEntitialQuats";
const NativeHookConfiguration AnimGetEnitialQuatHook::ConfigValue = Config_AnimGetEntitialQuat;
#if EDITOR
const string AnimGetEnitialQuatHook::Signature = "48 8b c4 88 [01......] ? 55 53 48 8d 6c [..100...]";
#else
const string AnimGetEnitialQuatHook::Signautre = "48 89 5c 24 18 55 48 81";
#endif
void(*AnimGetEnitialQuatHook::ManagedCallback)(LPVOID animPtr, LPVOID skeletalModelPtr, BYTE boneIndex) = 0;
LPVOID(*AnimGetEnitialQuatHook::Original)(LPVOID, BYTE, LPVOID) = 0;

LPVOID AnimGetEnitialQuatHook::Detour(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr)
{
	ManagedCallback(animPtr, skeletonModelPtr, boneIndex);
	LPVOID outQuat = Original(animPtr, boneIndex, skeletonModelPtr);
	return outQuat;
}

void AnimGetEnitialQuatHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (void(*)(LPVOID, LPVOID, BYTE))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}
