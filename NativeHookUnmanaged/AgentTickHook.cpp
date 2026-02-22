#include "pch.h"
#include "AgentTickHook.h"
using namespace std;

LPCVOID AgentTickHook::Address = 0;
const string AgentTickHook::Name = "AgentTick";
const NativeHookConfiguration AgentTickHook::ConfigValue = Config_AiTick;
#if EDITOR
const string AgentTickHook::Signature = "48 8b c4 56 57 41 56 48 81 ec a0 00 00 00 48 c7 40 80 fe ff ff ff";
#else
const string AgentTickHook::Signautre = "40 53 41 57 48 81 ec 88 00 00 00 8b";
#endif
void(*AgentTickHook::ManagedCallback)(int agentObjId, float dt) = 0;
void(*AgentTickHook::Original)(LPVOID, float, LPVOID, LPVOID) = 0;

#if EDITOR
void AgentTickHook::Detour(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
	Original(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
	int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
	ManagedCallback(agentObjId, dt);
}
#else
void AgentTickHook::Detour(LPBYTE agentPtr, float dt)
{
	Original_Agent_AiTick(agentPtr, dt);
	int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
	ManagedCallback_OnPostAiTick(agentObjId, dt);
}
#endif

void AgentTickHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (void(*)(int, float))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}