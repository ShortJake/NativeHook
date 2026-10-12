#include "pch.h"
#include "AnimSysGetBlendedAnimStructHook.h"

using namespace std;

LPCVOID AnimSysGetBlendedAnimStructHook::Address = 0;
const string AnimSysGetBlendedAnimStructHook::Name = "AnimSysGetBlendedAnimStruct";
const NativeHookConfiguration AnimSysGetBlendedAnimStructHook::ConfigValue = Config_AnimSysGetBlendedAnimStruct;
#if EDITOR
const string AnimSysGetBlendedAnimStructHook::Signature = "48 8b c4 55 53 56 57 41 54 41 55 41 56 41 57 48 8d a8 28 ff ff ff";
#else
const string AnimSysGetBlendedAnimStructHook::Signature = "48 8b c4 55 56 57 41 54 41 55 41 56 41 57 48 8d 6c 24 80 48 81 ec 80 01 00 00 0f 29 70 a8";
#endif
AnimSysGetBlendedAnimStructHook::ManagedCallBackSig AnimSysGetBlendedAnimStructHook::ManagedCallback = 0;
AnimSysGetBlendedAnimStructHook::OriginalFuncSig AnimSysGetBlendedAnimStructHook::Original = 0;

void AnimSysGetBlendedAnimStructHook::Detour(LPVOID agentAnimSysPtr, LPVOID skeletonPtr,
	LPVOID outResultAnimStruct, LPVOID InAnimStructChannel0, LPVOID InAnimStructChannel1,
	LPVOID slerpMat3, LPVOID ikHandTargetPos, unsigned long long perBoneSampleFlags,
	float* ch0BlendWeights, float* ch1BlendWeights)
{
	Original(agentAnimSysPtr, skeletonPtr, outResultAnimStruct, InAnimStructChannel0, InAnimStructChannel1,
		slerpMat3, ikHandTargetPos, perBoneSampleFlags, ch0BlendWeights, ch1BlendWeights);
	ManagedCallback(skeletonPtr, outResultAnimStruct);
}

void AnimSysGetBlendedAnimStructHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (AnimSysGetBlendedAnimStructHook::ManagedCallBackSig)managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}