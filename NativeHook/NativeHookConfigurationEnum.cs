using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NativeHook
{
    public enum NativeHookConfiguration : ulong
    {
        AiTick = 1,
        AgentTick = 2,
        UpdateDynamicsFlags = 4,
        AnimTreeTick = 8,
        AnimGetEntitialQuat = 16,
        All = AiTick | AgentTick | UpdateDynamicsFlags | AnimTreeTick | AnimGetEntitialQuat
    }
}
