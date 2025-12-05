#pragma once
struct MethodCallbackAddressStruct
{
    LPCVOID OnPostAiTick;
    LPCVOID OnPostAgentTick;
    LPCVOID AfterUpdateDynamicsFlags;
    LPCVOID OnAnimTreeTick;
    LPCVOID AnimGetEntitialQuat;
};

enum NativeHookConfiguration : unsigned long
{
    Config_AiTick = 1,
    Config_AgentTick = 2,
    Config_UpdateDynamicsFlags = 4,
    Config_AnimTreeTick = 8,
    Config_AnimGetEntitialQuat = 16,
};