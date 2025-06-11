// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <MinHook.h>
#include "SignatureScanner.h"
using namespace std;

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

LPCVOID NativeDLLAddress;
SIZE_T NativeDLLSize;


#pragma region  AiTick
LPCVOID Agent_AiTick_Address;
void(*ManagedCallback_OnPostAiTick)(int, float);
#if EDITOR
const string Agent_AiTick_Signature = "48 8b c4 f3 0f 11 48 10 55 41 54 41 55";
void(*Original_Agent_AiTick)(LPVOID, float, LPVOID, LPVOID);
void Hooked_Agent_AiTick(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
    Original_Agent_AiTick(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
    int agentObjIndex = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAiTick(agentObjIndex, dt);
}
#else
const string Agent_AiTick_Signature = "48 8b c4 f3 0f 11 48 10 55 41 54 41";
void(*Original_Agent_AiTick)(LPVOID, float);
void Hooked_Agent_AiTick(LPBYTE agentPtr, float dt)
{
    Original_Agent_AiTick(agentPtr, dt);
    int agentObjIndex = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAiTick(agentObjIndex, dt);
}
#endif
#pragma endregion

#pragma region  AgentTick
LPCVOID Agent_Tick_Address;
void(*ManagedCallback_OnPostAgentTick)(int, float);
#if EDITOR
const string Agent_Tick_Signature = "40 53 48 81 ec a0 00 00 00 80 b9 97";
void(*Original_Agent_Tick)(LPVOID, float, LPVOID, LPVOID);
void Hooked_Agent_Tick(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
    Original_Agent_Tick(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
    int agentObjIndex = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAgentTick(agentObjIndex, dt);
}
#else
const string Agent_Tick_Signature = "40 53 41 57 48 81 ec 88 00 00 00 8b";
void(*Original_Agent_Tick)(LPVOID, float);
void Hooked_Agent_Tick(LPBYTE agentPtr, float dt)
{
    Original_Agent_Tick(agentPtr, dt);
    int agentObjIndex = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAgentTick(agentObjIndex, dt);
}
#endif
#pragma endregion

#pragma region  Update Mov And Dyn Sys Flags
LPCVOID AgentMovDynSys_UpdateFlags_Address;
void(*ManagedCallback_AfterUpdateDynamicsFlags)(int, float, unsigned int, unsigned int);
void(*Original_AgentMovDynSys_UpdateFlags)(LPBYTE, LPVOID, float, LPBYTE, BYTE);
void Hooked_AgentMovDynSys_UpdateFlags(LPBYTE dynamicsSystemPtr, LPVOID missionPtr, float dt, LPBYTE agentRecPtr, BYTE param)
{
    unsigned int oldFlags = *(unsigned int*)(dynamicsSystemPtr + RGL_AGENT_MOV_DYN_dynamics_flags);
    Original_AgentMovDynSys_UpdateFlags(dynamicsSystemPtr, missionPtr, dt, agentRecPtr, param);
    unsigned int newFlags = *(unsigned int*)(dynamicsSystemPtr + RGL_AGENT_MOV_DYN_dynamics_flags);
    int agentIndex = *(int*)(agentRecPtr + RGL_AGENT_RECORD_owner_index);
    ManagedCallback_AfterUpdateDynamicsFlags(agentIndex, dt, oldFlags, newFlags);
}
#if EDITOR
const string AgentMovDynSys_UpdateFlags_Signature = "40 55 56 48 8d 6c 24 b9 48 81 ec";
#else
const string AgentMovDynSys_UpdateFlags_Signature = "40 55 57 48 8b ec 48 83 ec 48 48 89";
#endif
#pragma endregion

void GetFunctionAddresses()
{
    vector<BYTE> buffer = GetMemoryBuffer((LPVOID)NativeDLLAddress, NativeDLLSize);
    Agent_AiTick_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, Agent_AiTick_Signature, "AiTick");
    Agent_Tick_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, Agent_Tick_Signature, "AgentTick");
    AgentMovDynSys_UpdateFlags_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, AgentMovDynSys_UpdateFlags_Signature, "UpdateDynamicsFlags");
    buffer.clear();
}

void CreateAllHooks()
{
    if (MH_CreateHook((LPVOID)Agent_AiTick_Address, &Hooked_Agent_AiTick, (LPVOID*)(&Original_Agent_AiTick)) != MH_OK)
    {
        cout << "Error hooking AiTick";
    }
    if (MH_CreateHook((LPVOID)Agent_Tick_Address, &Hooked_Agent_Tick, (LPVOID*)(&Original_Agent_Tick)) != MH_OK)
    {
        cout << "Error hooking AiTick";
    }
    if (MH_CreateHook((LPVOID)AgentMovDynSys_UpdateFlags_Address, &Hooked_AgentMovDynSys_UpdateFlags, (LPVOID*)(&Original_AgentMovDynSys_UpdateFlags)) != MH_OK)
    {
        cout << "Error hooking AiTick";
    }
    MH_EnableHook(MH_ALL_HOOKS);
}

extern "C" __declspec(dllexport)
void NH_FillCallbacks(LPVOID onPostAiTick, LPVOID onPostAgentTick, LPVOID afterUpdateDynamicsFlags)
{
    ManagedCallback_OnPostAiTick = (void(*)(int, float))onPostAiTick;
    ManagedCallback_OnPostAgentTick = (void(*)(int, float))onPostAgentTick;
    ManagedCallback_AfterUpdateDynamicsFlags = (void(*)(int, float, unsigned int, unsigned int))afterUpdateDynamicsFlags;
}


extern "C" __declspec(dllexport)
void NH_Initialize(LPVOID nativeDllAddress, SIZE_T nativeDllSize)
{
    MH_Initialize();
    NativeDLLAddress = nativeDllAddress;
    NativeDLLSize = nativeDllSize;
    GetFunctionAddresses();
    CreateAllHooks();
}

extern "C" __declspec(dllexport)
void NH_Cleanup()
{
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}
