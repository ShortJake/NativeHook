// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>
#include <MinHook.h>
#include <vector>;
#include <iostream>
using namespace std;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
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

LPCVOID OnPi;
LPCVOID AiTickAddress;
void(*Original_AiTick)(LPVOID, float, LPVOID, LPVOID);
void(*ManagedCallback_OnPostAiTick)(int, float);
void Hooked_AiTick(LPBYTE agentPtr, float dt, LPVOID debugParam1Ptr, LPVOID debugParam2Ptr)
{
    Original_AiTick(agentPtr, dt, debugParam1Ptr, debugParam2Ptr);
    int agentObjIndex = *(int*)(agentPtr + 0x18);
    ManagedCallback_OnPostAiTick(agentObjIndex, dt);
}

vector<BYTE> GetMemoryBuffer()
{
    vector<BYTE> buffer(NativeDLLSize);
    SIZE_T numOfBytesRead;
    HANDLE curProc = OpenProcess(PROCESS_VM_READ, false, GetCurrentProcessId());
    if (ReadProcessMemory(curProc, NativeDLLAddress, buffer.data(), NativeDLLSize, &numOfBytesRead) == 0)
    {
        cout << "Failed to get memory buffer";
        return buffer;
    }
    CloseHandle(curProc);
    return buffer;
}
LPCVOID ScanForFirstResult(vector<BYTE>* buffer, vector<int>* signature, string functionName)
{
    if (buffer == NULL || signature == NULL)
    {
        cout << "Failed to find address for function " + functionName + ". Null buffer or signature";
        return 0;
    }
    for (int i = 0; i < buffer->size(); i++)
    {
        for (int j = 0; j < signature->size(); j++)
        {
            int element = (*signature)[j];
            if (element != -1 && element != (*buffer)[i + j]) break;
            if (j + 1 == signature->size())
            {
                return (char*)NativeDLLAddress + i;
            }
        }
    }
    cout << "Failed to find address for function " + functionName;
    return 0;
}
void GetFunctionAddresses()
{
    vector<BYTE> buffer = GetMemoryBuffer();
    vector<int> aiTickSig = { 0x48, 0x8b, 0xc4, 0xf3, 0x0f, 0x11, 0x48, 0x10, 0x55, 0x41, 0x54, 0x41, 0x55 };
    AiTickAddress = ScanForFirstResult(&buffer, &aiTickSig, "AiTick");
    buffer.clear();
}
void CreateHooks()
{
    if (MH_CreateHook((LPVOID)AiTickAddress, &Hooked_AiTick, (LPVOID*)(&Original_AiTick)) != MH_OK)
    {
        cout << "Error hooking AiTick";
    }
    MH_EnableHook(MH_ALL_HOOKS);
}

extern "C" __declspec(dllexport)
void NH_FillCallbacks(LPVOID onPostAiTick)
{
    ManagedCallback_OnPostAiTick = (void(*)(int, float))onPostAiTick;
}


extern "C" __declspec(dllexport)
void NH_Initialize(LPVOID nativeDllAddress, SIZE_T nativeDllSize)
{
    MH_Initialize();
    NativeDLLAddress = nativeDllAddress;
    NativeDLLSize = nativeDllSize;
    GetFunctionAddresses();
    CreateHooks();
}

extern "C" __declspec(dllexport)
void NH_Cleanup()
{
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}
