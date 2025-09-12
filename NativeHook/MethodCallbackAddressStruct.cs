using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NativeHook
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodCallbackAddressStruct
    {
        public IntPtr OnPostAiTick;
        public IntPtr OnPostAgentTick;
        public IntPtr AfterUpdateDynamicsFlags;
        public IntPtr OnAnimTreeTick;
        public IntPtr AnimGetEntitialQuat;
    }
}
