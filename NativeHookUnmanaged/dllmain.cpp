// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <MinHook.h>
#include "SignatureScanner.h"
#include <typeinfo>
#include "Structs.h"
#include "HookHeaders.h"
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
MethodCallbackAddressStruct ManagedCallbacks;
LPCVOID ManagedCallback_DebugMethod;

void CreateAllHooks()
{
    vector<BYTE> buffer = GetMemoryBuffer((LPVOID)NativeDLLAddress, NativeDLLSize);
    AiTickHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallbacks.OnPostAiTick);
    AgentTickHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallbacks.OnPostAgentTick);
    UpdateMovDynSysHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallbacks.AfterUpdateDynamicsFlags);
    AnimGetEnitialQuatHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallbacks.AnimGetEntitialQuat);
    // Doesn't work. Don't hook it for now
    AnimTreeTickHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallbacks.OnAnimTreeTick);
#if _DEBUG
    DebugMethodHook::Create(Config, NativeDLLAddress, &buffer, ManagedCallback_DebugMethod);
#endif
    buffer.clear();
    MH_EnableHook(MH_ALL_HOOKS);
}

extern "C" __declspec(dllexport)
void NH_FillCallbacks(MethodCallbackAddressStruct sigStruct)
{
    ManagedCallbacks = sigStruct;
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
    CreateAllHooks();
}

extern "C" __declspec(dllexport)
void NH_Cleanup()
{
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}
