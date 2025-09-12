#pragma once
struct MethodCallbackAddressStruct
{
    LPCVOID OnPostAiTick;
    LPCVOID OnPostAgentTick;
    LPCVOID AfterUpdateDynamicsFlags;
    LPCVOID OnAnimTreeTick;
    LPCVOID AnimGetEntitialQuat;
};

struct ModConfigStruct
{
    bool EnableAiTick;
    bool EnableAgentTick;
    bool EnableUpdateDynamicsFlags;
    bool EnableAnimTreeTick;
    bool EnableAnimGetEntitialQuat;
};