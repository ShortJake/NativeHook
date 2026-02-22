#include "pch.h"
#include "AiTickHook.h"
using namespace std;

LPCVOID AiTickHook::Address = 0;
const string AiTickHook::Name = "AiTick";
const NativeHookConfiguration AiTickHook::ConfigValue = Config_AiTick;
#if EDITOR
const string AiTickHook::Signature = "48 8b c4 f3 0f 11 [01001...] ? 55 41 54 41 55 ";
#else
const string AiTickHook::Signautre = "48 8b c4 f3 0f 11 48 10 55 41 54 41";
#endif
void(*AiTickHook::ManagedCallback)(int agentObjId, float dt) = 0;
void(*AiTickHook::Original)(LPVOID, float, LPVOID, LPVOID) = 0;

#if EDITOR
void AiTickHook::Detour(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
	Original(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
	int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
	ManagedCallback(agentObjId, dt);
}
#else
void AiTickHook::Detour(LPBYTE agentPtr, float dt)
{
	Original_Agent_AiTick(agentPtr, dt);
	int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
	ManagedCallback_OnPostAiTick(agentObjId, dt);
}
#endif

void AiTickHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (void(*)(int, float))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}

