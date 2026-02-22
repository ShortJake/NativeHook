#pragma once
#include <string>
#include "framework.h"
#include "Structs.h"
class AgentTickHook
{
private:
	static LPCVOID Address;
	const static NativeHookConfiguration ConfigValue;
	const static std::string Name;
	const static std::string Signature;
	static void(*ManagedCallback)(int agentObjId, float dt);
	static void(*Original)(LPVOID, float, LPVOID, LPVOID);
#if EDITOR
	void static Detour(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr);
#else 
	void static Detour(LPBYTE agentPtr, float dt);
#endif
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
};

