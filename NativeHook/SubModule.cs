using System.Runtime.InteropServices;
using System;
using TaleWorlds.MountAndBlade;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using TaleWorlds.DotNet;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net;

namespace NativeHook
{
    public class NativeHookSubModule : MBSubModuleBase
    {
        private Color ErrorColor;
        public static IntPtr NativeDLLAddr;
        private static int NativeDLLSize;
        //Prevent GC'ing of delegates
        private static List<Delegate> CallbackDelegates;
        // Returns the managed object that corresponds to this ID. ID is at an offset of 0x18 for agents
        private static MethodBase GetManagedObjWithId;

        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_Initialize(IntPtr nativeDllAddress, IntPtr nativeDllSize);
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_FillCallbacks(IntPtr postAiTick, IntPtr postAgentTick, IntPtr afterUpdateDynamicsFlags);
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_Cleanup();

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            ErrorColor = new Color(1f, 0.2f, 0.15f);
            var proc = Process.GetCurrentProcess();
            foreach (ProcessModule module in proc.Modules)
            {
                if (module.ModuleName != "TaleWorlds.Native.dll") continue;
                NativeDLLAddr = module.BaseAddress;
                NativeDLLSize = module.ModuleMemorySize;
                break;
            }
            if (NativeDLLAddr == IntPtr.Zero)
            {
                var errorMsg = "NativeHook Error! Could not find TaleWorlds.Native.dll";
                InformationManager.DisplayMessage(new InformationMessage(errorMsg, ErrorColor));
                MBDebug.ShowWarning(errorMsg);
                MBDebug.Print(errorMsg);
                return;
            }
            
            GetManagedObjWithId = AccessTools.Method(typeof(DotNetObject), "GetManagedObjectWithId", new Type[] { typeof(int) });
            NH_Initialize(NativeDLLAddr, new IntPtr(NativeDLLSize));

            FillNativeCallbacks();
            
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            NH_Cleanup();
        }
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
#if DEBUG
            mission.AddMissionBehavior(new DebugLogic());
#endif
        }

        private void FillNativeCallbacks()
        {
            CallbackDelegates = new List<Delegate>();
            var onPostAiTick = new Callback_OnPostAiTickDelegate(Callback_OnPostAiTick);
            CallbackDelegates.Add(onPostAiTick);
            var onPostAgentTick = new Callback_OnPostAgentTickDelegate(Callback_OnPostAgentTick);
            CallbackDelegates.Add(onPostAgentTick);
            var afterUpdateDynamicsFlags = new Callback_AfterUpdateDynamicsFlagsDelegate(Callback_AfterUpdateDynamicsFlags);
            CallbackDelegates.Add(afterUpdateDynamicsFlags);
            NH_FillCallbacks(Marshal.GetFunctionPointerForDelegate(onPostAiTick),
                Marshal.GetFunctionPointerForDelegate(onPostAgentTick),
                Marshal.GetFunctionPointerForDelegate(afterUpdateDynamicsFlags));
        }

        /*
            Agent_SetAnimSystemAddr = ScanForFirstResult(buffer, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 30 48 8b d9 33 f6 48 8b 89 98 05 00 00");
        NON-EDITOR:
            Agent_SetAnimSystemAddr = ScanForFirstResult(buffer, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 48 8b d9 33 f6 48 8b 89 90");
        */

        #region AI Tick
        public delegate void OnPostAiTickDelegate(Agent agent, float dt);
        public static event OnPostAiTickDelegate OnPostAiTick;

        private delegate void Callback_OnPostAiTickDelegate(int agentObjIndex, float dt);
        static private void Callback_OnPostAiTick(int agentObjIndex, float dt)
        {
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAiTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
        }

#endregion

        #region Agent Tick
        public delegate void OnPostAgentTickDelegate(Agent agent, float dt);
        public static event OnPostAgentTickDelegate OnPostAgentTick;
        private delegate void Callback_OnPostAgentTickDelegate(int agentObjIndex, float dt);
        static private void Callback_OnPostAgentTick(int agentObjIndex, float dt)
        {
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAgentTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
        }
        #endregion

        /*#region Agent Set Animation System
        private static IntPtr Agent_SetAnimSystemAddr;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void Agent_SetAnimSystemDelegate(UIntPtr agent, UIntPtr newAnimSystem);
        public static Agent_SetAnimSystemDelegate call_Agent_SetAnimSystem;
        #endregion*/

        #region Agent Movement And Dynamics Update Flags
        public delegate void AfterUpdateDynamicsFlagsDelegate(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags);
        public static event AfterUpdateDynamicsFlagsDelegate AfterUpdateDynamicsFlags;
        private delegate void Callback_AfterUpdateDynamicsFlagsDelegate(int agentIndex, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags);
        static private void Callback_AfterUpdateDynamicsFlags(int agentIndex, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags)
        {
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = AfterUpdateDynamicsFlags;
            if (Mission.Current != null && ev != null)
            {
                var agent = Mission.Current.FindAgentWithIndex(agentIndex);
                if (agent == null) return;
                ev(agent, dt, oldFlags, newFlags);
            }
        }
#endregion
    }
}