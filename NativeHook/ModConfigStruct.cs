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
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableAiTick;
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableAgentTick;
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableUpdateDynamicsFlags;
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableAnimTreeTick;
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableAnimGetEntitialQuat;
    }
}
