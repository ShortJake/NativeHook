using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NativeHook
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ModConfigStruct
    {
        public bool EnableAiTick;
        public bool EnableAgentTick;
        public bool EnableUpdateDynamicsFlags;
        public bool EnableAnimTreeTick;
        public bool EnableAnimGetEntitialQuat;
    }
}
