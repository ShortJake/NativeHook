#include "pch.h"
#include "UpdateMovDynSysHook.h"
using namespace std;

LPCVOID UpdateMovDynSysHook::Address = 0;
const string UpdateMovDynSysHook::Name = "UpdateDynamicsFlags";
const NativeHookConfiguration UpdateMovDynSysHook::ConfigValue = Config_UpdateDynamicsFlags;
#if EDITOR
const string UpdateMovDynSysHook::Signature = "40 55 53 48 8d 6c [..100...] ? 48 81 ec ? ? ? ? 4c 89 74 [..100...]";
#else
const string UpdateMovDynSysHook::Signautre = "40 55 57 48 8b ec 48 83 ec 48 48 89";
#endif
void(*UpdateMovDynSysHook::ManagedCallback)(int agentIndex, float dt, unsigned int oldFlags, unsigned int newFlags) = 0;
void(*UpdateMovDynSysHook::Original)(LPBYTE, LPVOID, float, LPBYTE, BYTE) = 0;

void UpdateMovDynSysHook::Detour(LPBYTE dynamicsSystemPtr, LPVOID missionPtr, float dt, LPBYTE agentRecPtr, BYTE param)
{
	unsigned int oldFlags = *(unsigned int*)(dynamicsSystemPtr + RGL_AGENT_MOV_DYN_dynamics_flags);
	Original(dynamicsSystemPtr, missionPtr, dt, agentRecPtr, param);
	unsigned int newFlags = *(unsigned int*)(dynamicsSystemPtr + RGL_AGENT_MOV_DYN_dynamics_flags);
	int agentIndex = *(int*)(agentRecPtr + RGL_AGENT_RECORD_owner_index);
	ManagedCallback(agentIndex, dt, oldFlags, newFlags);
}

void UpdateMovDynSysHook::Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback)
{
	if ((currentConfig & ConfigValue) == 0) return;
	ManagedCallback = (void(*)(int agentIndex, float dt, unsigned int oldFlags, unsigned int newFlags))managedCallback;
	Helpers::CreateHook(currentConfig, nativeDLLAdress, buffer, &Address, Signature, Name, Detour, (LPVOID*)&Original);
}