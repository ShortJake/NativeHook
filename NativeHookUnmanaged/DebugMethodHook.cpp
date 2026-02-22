#include "pch.h"
#include "DebugMethodHook.h"
using namespace std;

LPCVOID DebugMethodHook::Address = 0;
const string DebugMethodHook::Name = "DebugMethod";
const string DebugMethodHook::Signature = "48 8b c4 56 57 41 56 48 81 ec a0 00 00 00 48 c7 40 80 fe ff ff ff";
void(*DebugMethodHook::ManagedCallback)(LPVOID animPtr, LPVOID skeletonModelPtr, BYTE boneIndex, LPVOID outQuat) = 0;
LPVOID (*DebugMethodHook::Original)(LPVOID, BYTE, LPVOID) = 0;

LPVOID DebugMethodHook::Detour(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr)
{
	LPVOID outQuat = Original(animPtr, boneIndex, skeletonModelPtr);
	ManagedCallback(animPtr, skeletonModelPtr, boneIndex, outQuat);
	return outQuat;
}

void DebugMethodHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	ManagedCallback = (void(*)(LPVOID, LPVOID, BYTE, LPVOID))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}