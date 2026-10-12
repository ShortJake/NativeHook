#pragma once
class AnimSysGetBlendedAnimStructHook
{
public:
	typedef void(*OriginalFuncSig)(LPVOID, LPVOID,
		LPVOID, LPVOID, LPVOID,
		LPVOID, LPVOID, unsigned long long,
		float*, float*);
	typedef void(*ManagedCallBackSig)(LPVOID skeletonPtr, LPVOID outResultAnimStruct);
private:
	static LPCVOID Address;
	const static NativeHookConfiguration ConfigValue;
	const static std::string Name;
	const static std::string Signature;
	static ManagedCallBackSig ManagedCallback;
	static OriginalFuncSig Original;
	void static Detour(LPVOID agentAnimSysPtr, LPVOID skeletonPtr,
		LPVOID outResultAnimStruct, LPVOID InAnimStructChannel0, LPVOID InAnimStructChannel1, 
		LPVOID slerpMat3, LPVOID ikHandTargetPos, unsigned long long perBoneSampleFlags, 
		float* ch0BlendWeights, float* ch1BlendWeights);
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
};

