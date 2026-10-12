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
        [Obsolete]
        AnimTreeTick = 8,
        [Obsolete]
        AnimGetEntitialQuat = 16,
        AnimSysGetBlendedAnimStruct = 32,
        All = ulong.MaxValue
    }
}
