// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <MinHook.h>
#include "SignatureScanner.h"
#include <typeinfo>
#include "Structs.h"
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
NativeHookConfiguration Config;

#pragma region  AiTick
LPCVOID Agent_AiTick_Address;
void(*ManagedCallback_OnPostAiTick)(int agentObjId, float dt);
#if EDITOR
const string Agent_AiTick_Signature = "48 8b c4 f3 0f 11 [01001...] ? 55 41 54 41 55 ";
void(*Original_Agent_AiTick)(LPVOID, float, LPVOID, LPVOID);
void Hooked_Agent_AiTick(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
    Original_Agent_AiTick(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
    int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAiTick(agentObjId, dt);
}
#else
const string Agent_AiTick_Signature = "48 8b c4 f3 0f 11 48 10 55 41 54 41";
void(*Original_Agent_AiTick)(LPVOID, float);
void Hooked_Agent_AiTick(LPBYTE agentPtr, float dt)
{
    Original_Agent_AiTick(agentPtr, dt);
    int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAiTick(agentObjId, dt);
}
#endif
#pragma endregion

#pragma region  AgentTick
LPCVOID Agent_Tick_Address;
void(*ManagedCallback_OnPostAgentTick)(int agentObjectId, float dt);
#if EDITOR
const string Agent_Tick_Signature = "48 8b c4 56 57 41 56 48 81 ec a0 00 00 00 48 c7 40 80 fe ff ff ff";
void(*Original_Agent_Tick)(LPVOID, float, LPVOID, LPVOID);
void Hooked_Agent_Tick(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
    Original_Agent_Tick(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
    int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAgentTick(agentObjId, dt);
}
#else
const string Agent_Tick_Signature = "40 53 41 57 48 81 ec 88 00 00 00 8b";
void(*Original_Agent_Tick)(LPVOID, float);
void Hooked_Agent_Tick(LPBYTE agentPtr, float dt)
{
    Original_Agent_Tick(agentPtr, dt);
    int agentObjId = *(int*)(agentPtr + RGL_AGENT_obj_id);
    ManagedCallback_OnPostAgentTick(agentObjId, dt);
}
#endif
#pragma endregion

#pragma region  Update Mov And Dyn Sys Flags
LPCVOID AgentMovDynSys_UpdateFlags_Address;
void(*ManagedCallback_AfterUpdateDynamicsFlags)(int agentIndex, float dt, unsigned int oldFlags, unsigned int newFlags);
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
const string AgentMovDynSys_UpdateFlags_Signature = "40 55 53 48 8d 6c [..100...] ? 48 81 ec ? ? ? ? 4c 89 74 [..100...]";
#else
const string AgentMovDynSys_UpdateFlags_Signature = "40 55 57 48 8b ec 48 83 ec 48 48 89";
#endif
#pragma endregion

#pragma region  Anim Tree Tick
LPCVOID rglAnimTree_Tick_Address;
void(*ManagedCallback_OnAnimTreeTick)(LPVOID animTreePtr, LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr);
extern "C" {
    void* Original_rglAnimTree_Tick;
    void HookedASM_rglAnimTree_Tick();
}
#if EDITOR
const string rglAnimTree_Tick_Signature = "0f 8c ? ? ? ? 44 0f 28 bc [..100100] 20 14 00 00 ";
extern "C" void HookedWithParams_rglAnimTree_Tick(LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr)
{
    ManagedCallback_OnAnimTreeTick(0x0, skeletonPtr, boneIndex, cachedMatrixFrameArrayPtr);
}
#else
const string rglAnimTree_Tick_Signature = "0f 8c e6 f8 ff ff 44 0f b6 64 24 20";
extern "C" void HookedWithParams_rglAnimTree_Tick(LPVOID skeletonPtr, BYTE boneIndex, LPVOID cachedMatrixFrameArrayPtr)
{
    ManagedCallback_OnAnimTreeTick(0x0, skeletonPtr, boneIndex, cachedMatrixFrameArrayPtr);
}
#endif
#pragma endregion

#pragma region  Anim Get Entitial Quat
LPCVOID rglSkeleton_Anim_GetEntitialQuat_Address;
void(*ManagedCallback_AnimGetEntitialQuat)(LPVOID animPtr, LPVOID skeletalModelPtr, BYTE boneIndex);
#if EDITOR
const string rglSkeleton_Anim_GetEntitialQuat_Signature = "48 8b c4 88 [01......] ? 55 53 48 8d 6c [..100...]";
#else 
const string rglSkeleton_Anim_GetEntitialQuat_Signature = "48 89 5c 24 18 55 48 81";
#endif
LPVOID (*Original_rglSkeleton_Anim_GetEntitialQuat)(LPVOID, BYTE, LPVOID);
LPVOID Hooked_rglSkeleton_Anim_GetEntitialQuat(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr)
{
    ManagedCallback_AnimGetEntitialQuat(animPtr, skeletonModelPtr, boneIndex);
    LPVOID outQuat = Original_rglSkeleton_Anim_GetEntitialQuat(animPtr, boneIndex, skeletonModelPtr);
    return outQuat;
}
#pragma endregion

#pragma region  Debug Method
#if _DEBUG
LPCVOID DebugMethod_Address;
void(*ManagedCallback_DebugMethod)(LPVOID animPtr, LPVOID skeletonModelPtr, BYTE boneIndex, LPVOID outQuat);
const string DebugMethod_Signature = "";
LPVOID(*Original_DebugMethod)(LPVOID, BYTE, LPVOID);
LPVOID Hooked_DebugMethod(LPVOID animPtr, BYTE boneIndex, LPVOID skeletonModelPtr)
{
    LPVOID outQuat = Original_DebugMethod(animPtr, boneIndex, skeletonModelPtr);
    ManagedCallback_DebugMethod(animPtr, skeletonModelPtr, boneIndex, outQuat);
    return outQuat;
}
#endif
#pragma endregion

void GetFunctionAddresses()
{
    vector<BYTE> buffer = GetMemoryBuffer((LPVOID)NativeDLLAddress, NativeDLLSize);
    Agent_AiTick_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, Agent_AiTick_Signature, "AiTick");
    Agent_Tick_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, Agent_Tick_Signature, "AgentTick");
    AgentMovDynSys_UpdateFlags_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, AgentMovDynSys_UpdateFlags_Signature, "UpdateDynamicsFlags");
    rglAnimTree_Tick_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, rglAnimTree_Tick_Signature, "AnimTreeTick");
    rglSkeleton_Anim_GetEntitialQuat_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, rglSkeleton_Anim_GetEntitialQuat_Signature, "GetEntitialQuat");
#if _DEBUG
    if (!DebugMethod_Signature.empty()) DebugMethod_Address = ScanForFirstResult((LPVOID)NativeDLLAddress, &buffer, DebugMethod_Signature, "DebugMethod");
#endif
    buffer.clear();
}

void CreateAllHooks()
{
    if ((Config & Config_AiTick) != 0)
    { 
        if (MH_CreateHook((LPVOID)Agent_AiTick_Address, &Hooked_Agent_AiTick, (LPVOID*)(&Original_Agent_AiTick)) != MH_OK)
        {
            cout << "Error hooking AiTick";
        }
        else cout << "Hooked AiTick";
    }
    if ((Config & Config_AgentTick) != 0)
    {
        if (MH_CreateHook((LPVOID)Agent_Tick_Address, &Hooked_Agent_Tick, (LPVOID*)(&Original_Agent_Tick)) != MH_OK)
        {
            cout << "Error hooking AgentTick";
        }
        else cout << "Hooked AgentTick";
    }
    if ((Config & Config_UpdateDynamicsFlags) != 0)
    {
        if (MH_CreateHook((LPVOID)AgentMovDynSys_UpdateFlags_Address, &Hooked_AgentMovDynSys_UpdateFlags, (LPVOID*)(&Original_AgentMovDynSys_UpdateFlags)) != MH_OK)
        {
            cout << "Error hooking AgentMovementAndDynamicsSystemUpdateFlags";
        }
        else cout << "Hooked AgentMovementAndDynamicsSystemUpdateFlags";
    }
    if ((Config & Config_AnimTreeTick) != 0)
    {
        if (MH_CreateHook((LPVOID)rglAnimTree_Tick_Address, &HookedASM_rglAnimTree_Tick, (LPVOID*)(&Original_rglAnimTree_Tick)) != MH_OK)
        {
            cout << "Error hooking AnimTreeTick";
        }
        else cout << "Hooked AnimTreeTick";
    }
    if ((Config & Config_AnimGetEntitialQuat) != 0)
    {
        if (MH_CreateHook((LPVOID)rglSkeleton_Anim_GetEntitialQuat_Address, &Hooked_rglSkeleton_Anim_GetEntitialQuat, (LPVOID*)(&Original_rglSkeleton_Anim_GetEntitialQuat)) != MH_OK)
        {
            cout << "Error hooking AnimGetEntitialQuars";
        }
        else cout << "Hooked AnimGetEntitialQuars";
    }
#if _DEBUG
    if (DebugMethod_Address != 0)
    {
        if (MH_CreateHook((LPVOID)DebugMethod_Address, &Hooked_DebugMethod, (LPVOID*)(&Original_DebugMethod)) != MH_OK)
        {
            cout << "Disabled/Unable to hook DebugMethod";
        }
        else cout << "Hooked DebugMethod";
    }
#endif
    MH_EnableHook(MH_ALL_HOOKS);
}

extern "C" __declspec(dllexport)
void NH_FillCallbacks(MethodCallbackAddressStruct sigStruct)
{
    ManagedCallback_OnPostAiTick = (void(*)(int, float))sigStruct.OnPostAiTick;
    ManagedCallback_OnPostAgentTick = (void(*)(int, float))sigStruct.OnPostAgentTick;
    ManagedCallback_AfterUpdateDynamicsFlags = (void(*)(int, float, unsigned int, unsigned int))sigStruct.AfterUpdateDynamicsFlags;
    ManagedCallback_OnAnimTreeTick = (void(*)(LPVOID, LPVOID, BYTE, LPVOID))sigStruct.OnAnimTreeTick;
    ManagedCallback_AnimGetEntitialQuat = (void(*)(LPVOID, LPVOID, BYTE))sigStruct.AnimGetEntitialQuat;
}

#if _DEBUG
extern "C" __declspec(dllexport)
void NH_FillDebugMethodCallback(LPVOID debugMethodCallback)
{
    ManagedCallback_DebugMethod = (void(*)(LPVOID, LPVOID, BYTE, LPVOID))debugMethodCallback;
}
#endif

extern "C" __declspec(dllexport)
void NH_Initialize(LPVOID nativeDllAddress, SIZE_T nativeDllSize, NativeHookConfiguration configs)
{
    MH_Initialize();
    NativeDLLAddress = nativeDllAddress;
    NativeDLLSize = nativeDllSize;
    Config = configs;
    GetFunctionAddresses();
    CreateAllHooks();
}

extern "C" __declspec(dllexport)
void NH_Cleanup()
{
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}
