using System.Runtime.InteropServices;
using System;
using TaleWorlds.MountAndBlade;
using EasyHook;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using TaleWorlds.DotNet;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;

namespace NativeHook
{
    public class NativeHookSubModule : MBSubModuleBase
    {
        public static long NativeDLLAddr;
        //Hooks will be disposed of if kept as local variables
        private static List<LocalHook> NativeHooks;
        // Returns the managed object that corresponds to this ID. ID is at an offset of 0x18 for agents
        private static MethodBase GetManagedObjWithId;

        private static IntPtr Agent_MaybeAiTickAddr;
        private static IntPtr Agent_MaybeTickAddr;
        private static IntPtr rglSkeleton_MaybeUpdateAddr;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var proc = Process.GetCurrentProcess();
            NativeDLLAddr = GetModuleBaseAddress(new IntPtr(proc.Id), "TaleWorlds.Native.dll").ToInt64();
            NativeHooks = new List<LocalHook>();
            GetManagedObjWithId = AccessTools.Method(typeof(DotNetObject), "GetManagedObjectWithId", new Type[] { typeof(int) });
            if (Utilities.EditModeEnabled) GetMethodAddressesEditor();
            else GetMethodAddressesNoEditor();

            CreateHook(Agent_MaybeAiTickAddr, new Agent_MaybeAiTickDelegate(Agent_OnMaybeAiTick), "Agent_MaybeAiTick");
            CreateHook(Agent_MaybeTickAddr, new Agent_MaybeTickDelegate(Agent_OnMaybeTick), "Agent_MaybeTick");
            //CreateHook(rglSkeleton_MaybeUpdateAddr, new rglSkeleton_MaybeUpdateDelegate(rglSkeleton_MaybeUpdate), "rglSkeleton_MaybeUpdate");
        }
        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            foreach (var hook in NativeHooks)
            {
                hook.Dispose();
            }
            NativeHooks.Clear();
        }
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            #if DEBUG
            mission.AddMissionBehavior(new DebugLogic());
            #endif
        }

        private void GetMethodAddressesEditor()
        {
            //byte[] maybeAiTickSig = { 0x48, 0x8b, 0xc4, 0xf3, 0x0f, 0x11, 0x48, 0x10, 0x55, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57 };
            var gameVer = Utilities.GetApplicationVersionWithBuildNumber().ToString();
            switch (gameVer)
            {
                case "v1.2.12.54620":
                    Agent_MaybeAiTickAddr = new IntPtr(NativeDLLAddr + 0xaea3e0 + 0x50);
                    Agent_MaybeTickAddr = new IntPtr(NativeDLLAddr + 0xae9410);
                    //rglSkeleton_MaybeUpdateAddr = new IntPtr(NativeDLLAddr + 0x5a4a60);
                    rglSkeleton_MaybeUpdateAddr = new IntPtr(NativeDLLAddr + 0x86b730);
                    
                    break;
            }
        }
        private void GetMethodAddressesNoEditor()
        {
            var gameVer = Utilities.GetApplicationVersionWithBuildNumber().ToString();
            switch (gameVer)
            {
                case "v1.2.12.54620":
                    Agent_MaybeAiTickAddr = new IntPtr(NativeDLLAddr + 0x511e20);
                    Agent_MaybeTickAddr = new IntPtr(NativeDLLAddr + 0x510e00);
                    break;
            }
        }

        private void CreateHook(IntPtr address, Delegate functionDelegate, string functionName)
        {
            if (address == IntPtr.Zero)
            {
                InformationManager.DisplayMessage(new InformationMessage("Could not get adddress for '" + functionName + "'. Not hooking!"
                , Color.FromUint(0xcd2828)));
                return;
            }
            try
            {
                var hook = LocalHook.Create(address, functionDelegate, null);
                hook.ThreadACL.SetExclusiveACL(new int[] { });
                NativeHooks.Add(hook);
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage("Error hooking '" + functionName + "'"
                , Color.FromUint(0xcd2828)));
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_MaybeAiTickDelegate(ulong agentPtr, ulong param1, ulong param2, ulong param3);

        unsafe static private void Agent_OnMaybeAiTick(ulong agentPtr, ulong param1, ulong param2, ulong param3)
        {
            Marshal.GetDelegateForFunctionPointer<Agent_MaybeAiTickDelegate>(Agent_MaybeAiTickAddr)(agentPtr, param1, param2, param3);
            var agentObjIndex = *(int*)new UIntPtr(agentPtr + 0x18).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            foreach (var behavior in Mission.Current.MissionBehaviors.OfType<IMissionBehaviorNativeHook>())
            {
                behavior.OnAiAgentTick(agentObj);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_MaybeTickDelegate(ulong agentPtr, float dt, ulong param2, ulong param3);

        unsafe static private void Agent_OnMaybeTick(ulong agentPtr, float dt, ulong param2, ulong param3)
        {
            Marshal.GetDelegateForFunctionPointer<Agent_MaybeTickDelegate>(Agent_MaybeTickAddr)(agentPtr, dt, param2, param3);
            var agentObjIndex = *(int*)new UIntPtr(agentPtr + 0x18).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            foreach (var behavior in Mission.Current.MissionBehaviors.OfType<IMissionBehaviorNativeHook>())
            {
                behavior.OnPostAgentTick(agentObj, dt);
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void rglSkeleton_MaybeUpdateDelegate(ulong skeletonPtr, ulong param1, ulong param2, ulong param3);

        unsafe static private void rglSkeleton_MaybeUpdate(ulong animSysPtr, ulong skeletonPtr, ulong param2, ulong param3)
        {
            if (!(Input.IsKeyDown(InputKey.LeftAlt) && Input.IsKeyDown(InputKey.M)))
            {
                Marshal.GetDelegateForFunctionPointer<rglSkeleton_MaybeUpdateDelegate>(rglSkeleton_MaybeUpdateAddr)(animSysPtr, skeletonPtr, param2, param3);
            }
            if (Mission.Current != null)
            {
                var skeletonObj = AccessTools.Constructor(typeof(Skeleton), new Type[] { typeof(UIntPtr) })?.Invoke(new object[] { new UIntPtr(skeletonPtr) }) as Skeleton;
                foreach (var behavior in Mission.Current.MissionBehaviors.OfType<IMissionBehaviorNativeHook>())
                {
                    behavior.OnSkeletonUpdate(skeletonObj, animSysPtr, 0f);
                }
            }
        }

        #region GetBaseAddr
        const Int64 INVALID_HANDLE_VALUE = -1;
        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MODULEENTRY32
        {
            internal uint dwSize;
            internal uint th32ModuleID;
            internal uint th32ProcessID;
            internal uint GlblcntUsage;
            internal uint ProccntUsage;
            internal IntPtr modBaseAddr;
            internal uint modBaseSize;
            internal IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szExePath;
        }

        [DllImport("kernel32.dll")]
        static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, IntPtr th32ProcessID);

        public static IntPtr GetModuleBaseAddress(IntPtr procId, string modName)
        {
            IntPtr modBaseAddr = IntPtr.Zero;
            IntPtr hSnap = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, procId);

            if (hSnap.ToInt64() != INVALID_HANDLE_VALUE)
            {
                MODULEENTRY32 modEntry = new MODULEENTRY32();
                modEntry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                if (Module32First(hSnap, ref modEntry))
                {
                    do
                    {
                        if (modEntry.szModule.Equals(modName))
                        {
                            modBaseAddr = modEntry.modBaseAddr;
                            break;
                        }
                    } while (Module32Next(hSnap, ref modEntry));
                }
            }
            CloseHandle(hSnap);

            return modBaseAddr;
        }
        #endregion
    }
}