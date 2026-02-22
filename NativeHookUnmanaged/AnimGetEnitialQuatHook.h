#pragma once
class AnimGetEnitialQuatHook
{
private:
	static LPCVOID Address;
	const static NativeHookConfiguration ConfigValue;
	const static std::string Name;
	const static std::string Signature;
	static void(*ManagedCallback)(LPVOID animPtr, LPVOID skeletalModelPtr, BYTE boneIndex);
	static LPVOID(*Original)(LPVOID, BYTE, LPVOID);
	LPVOID static Detour(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr);
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
};

