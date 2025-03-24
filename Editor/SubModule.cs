using System.Runtime.InteropServices;
using System;
using TaleWorlds.MountAndBlade;
using EasyHook;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using TaleWorlds.DotNet;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;
using TaleWorlds.Core;

namespace NativeHook
{
    public class NativeHookSubModule : MBSubModuleBase
    {
        private Color ErrorColor;
        public static IntPtr NativeDLLAddr;
        private static int NativeDLLSize;
        //Hooks will be disposed of if kept as local variables
        private static List<LocalHook> NativeHooks;
        // Returns the managed object that corresponds to this ID. ID is at an offset of 0x18 for agents
        private static MethodBase GetManagedObjWithId;

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
                InformationManager.DisplayMessage(new InformationMessage("NativeHook Error! Could not find TaleWorlds.Native.dll", ErrorColor));
                return;
            }
            NativeHooks = new List<LocalHook>();
            GetManagedObjWithId = AccessTools.Method(typeof(DotNetObject), "GetManagedObjectWithId", new Type[] { typeof(int) });
            GetHookedMethodAddresses();
            CreateHooks();
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
        private void CreateHooks()
        {
            CreateHook(Agent_AiTickAddr, new Agent_AiTickDelegate(Agent_AiTick));
            CreateHook(Agent_TickAddr, new Agent_TickDelegate(Agent_Tick));
            CreateHook(AgentMovementAndDynamicsSystem_UpdateFlagsAddr, new AgentMovementAndDynamicsSystem_UpdateFlagsDelegate(AgentMovementAndDynamicsSystem_UpdateFlags));
#if DEBUG
            CreateHook(DebugMethod_Addr, new DebugMethodDelegate(OnDebugMethod));
#endif
        }
        private void GetHookedMethodAddresses()
        {
            var buffer = GetMemoryBuffer(NativeDLLAddr, NativeDLLSize);
#if Editor
            Agent_AiTickAddr = ScanForFirstResult(buffer, "48 8b c4 f3 0f 11 48 10 55 41 54 41 55");
            Agent_TickAddr = ScanForFirstResult(buffer, "40 53 48 81 ec a0 00 00 00 80 b9 97");
            AgentMovementAndDynamicsSystem_UpdateFlagsAddr = ScanForFirstResult(buffer, "40 55 56 48 8d 6c 24 b9 48 81 ec");
#else
            Agent_AiTickAddr = ScanForFirstResult(buffer, "48 8b c4 f3 0f 11 48 10 55 41 54 41");
            Agent_TickAddr = ScanForFirstResult(buffer, "40 53 41 57 48 81 ec 88 00 00 00 8b");
            AgentMovementAndDynamicsSystem_UpdateFlagsAddr = ScanForFirstResult(buffer, "40 55 57 48 8b ec 48 83 ec 48 48 89");
#endif
#if DEBUG
            var hits = ScanFor(buffer, "48 8b c4 48 89 58 20 57 48 83 ec 70 0f 29 70 e8 49 8b f8 44 0f 29 40 c8 48 8b d9 48 89 70 18 44");
            if (hits.Count > 0) DebugMethod_Addr = hits[0];
#endif
        }
        /// <summary>
        /// Reads memory as a byte array
        /// </summary>
        /// <param name="startAddress">Address to start reading from</param>
        /// <param name="size">How many bytes to read</param>
        /// <returns></returns>
        private unsafe byte[] GetMemoryBuffer(IntPtr startAddress, int size)
        {
            // This isn't a good implementation, but I'm using it temporarily for now until I can implement a proper method
            if (startAddress == IntPtr.Zero || size == 0) return new byte[1];
            byte[] buffer = new byte[size];
            for (int i = 0; i < buffer.LongLength; i++)
            {
                var b = *(byte*)(startAddress + i).ToPointer();
                buffer[i] = b;
            }
            return buffer;
        }
        private int[] ParseSignatureString(string signature)
        {
            var splitSig = signature.Split(' ');
            var bytes = new int[splitSig.Length];
            for (int i = 0; i < splitSig.Length; i++)
            {
                // -1 represents wild card. Set any string that cannot be parsed to -1
                if (int.TryParse(splitSig[i], System.Globalization.NumberStyles.HexNumber, null, out var result)) bytes[i] = result;
                else bytes[i] = -1;
            }
            return bytes;
        }
        /// <summary>
        /// Scans the memory buffer for the signature
        /// </summary>
        /// <param name="signature">Signature as a string of hexadecimals separated by spaces. Use ?? for wildcard</param>
        /// <returns>First match for the signature</returns>
        private IntPtr ScanForFirstResult(byte[] buffer, string signatureString)
        {
            int[] signature = ParseSignatureString(signatureString);
            for (int i = 0; i < buffer.Length; i++)
            {

                for (int j = 0; j < signature.Length; j++)
                {
                    if (signature[j] != -1 && signature[j] != buffer[i + j]) break;
                    if (j + 1 == signature.Length)
                    {
                        return NativeDLLAddr + i;
                    }
                }
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// Scans the memory buffer for the signature
        /// </summary>
        /// <param name="signature">Signature as a string of hexadecimals separated by spaces. Use ?? for wildcard</param>
        /// <returns>List of all matches for the signature</returns>
        private List<IntPtr> ScanFor(byte[] buffer, string signatureString)
        {
            int[] signature = ParseSignatureString(signatureString);
            var hits = new List<IntPtr>();
            for (int i = 0; i < buffer.Length; i++)
            {

                for (int j = 0; j < signature.Length; j++)
                {
                    if (signature[j] != -1 && signature[j] != buffer[i + j]) break;
                    if (j + 1 == signature.Length)
                    {
                        hits.Add(NativeDLLAddr + i);
                    }
                }
            }
            return hits;
        }
        private void CreateHook(IntPtr address, Delegate functionDelegate)
        {
            var functionName = functionDelegate.GetType().Name;
            functionName = functionName.Replace("Delegate", String.Empty);
            if (address == IntPtr.Zero)
            {
                InformationManager.DisplayMessage(new InformationMessage("NativeHook Error! Invalid adddress for '" + functionName + "'. Not hooking!"
                , ErrorColor));
                return;
            }
            try
            {
                var callDelField = AccessTools.Field(this.GetType(), "call_" + functionName);
                if (callDelField == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("NativeHook Error! Function name doesnt match for " + functionName
                , ErrorColor));
                    return;
                }
                callDelField.SetValue(this, Marshal.GetDelegateForFunctionPointer(address, functionDelegate.GetType()));
                var hook = LocalHook.Create(address, functionDelegate, null);
                hook.ThreadACL.SetExclusiveACL(new int[] { });
                NativeHooks.Add(hook);
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage("NativeHook Error! Error hooking '" + functionName + "'"
                , ErrorColor));
            }
        }

        #region AI Tick
        public delegate void OnPostAiTickDelegate(Agent agent, float dt);
        public static event  OnPostAiTickDelegate OnPostAiTick;
        private static IntPtr Agent_AiTickAddr;
        private static Agent_AiTickDelegate call_Agent_AiTick;
#if Editor
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_AiTickDelegate(UIntPtr agentPtr, float dt, UIntPtr param1, UIntPtr param2);
        unsafe static private void Agent_AiTick(UIntPtr agentPtr, float dt, UIntPtr param1, UIntPtr param2)
        { 
            call_Agent_AiTick(agentPtr, dt, param1, param2);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            OnPostAiTick(agentObj, dt);
        }
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_AiTickDelegate(UIntPtr agentPtr, float dt);
        unsafe static private void Agent_AiTick(UIntPtr agentPtr, float dt)
        {
            call_Agent_AiTick(agentPtr, dt);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            OnPostAiTick(agentObj, dt);
        }
#endif

        #endregion

        #region Agent Tick
        public delegate void OnPostAgentTickDelegate(Agent agent, float dt);
        public static event OnPostAgentTickDelegate OnPostAgentTick;
        private static IntPtr Agent_TickAddr;
        private static Agent_TickDelegate call_Agent_Tick;
#if Editor
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_TickDelegate(UIntPtr agentPtr, float dt, UIntPtr param1, UIntPtr param2);
        unsafe static private void Agent_Tick(UIntPtr agentPtr, float dt, UIntPtr param1, UIntPtr param2)
        {
            call_Agent_Tick(agentPtr, dt, param1, param2);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            OnPostAgentTick(agentObj, dt);
        }
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_TickDelegate(UIntPtr agentPtr, float dt);
        unsafe static private void Agent_Tick(UIntPtr agentPtr, float dt)
        {
            call_Agent_Tick(agentPtr, dt);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            if (Mission.Current == null || agentObj == null) return;
            OnPostAgentTick(agentObj, dt);
        }
#endif
        #endregion

        #region Agent Movement And Dynamics Update Flags
        public delegate void AfterUpdateDynamicsFlagsDelegate(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags);
        public static event AfterUpdateDynamicsFlagsDelegate AfterUpdateDynamicsFlags;
        private static IntPtr AgentMovementAndDynamicsSystem_UpdateFlagsAddr;
        private static AgentMovementAndDynamicsSystem_UpdateFlagsDelegate call_AgentMovementAndDynamicsSystem_UpdateFlags;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void AgentMovementAndDynamicsSystem_UpdateFlagsDelegate(UIntPtr dynamicsSystemPtr, UIntPtr missionPtr, float dt, UIntPtr agentRecPtr, byte param);
        unsafe static private void AgentMovementAndDynamicsSystem_UpdateFlags(UIntPtr dynamicsSystemPtr, UIntPtr missionPtr, float dt, UIntPtr agentRecPtr, byte param)
        {
            var oldFlags = *(AgentDynamicsFlags*)(dynamicsSystemPtr + rglAgentMovementAndDynamicsSystem.dynamics_flags).ToPointer();
            call_AgentMovementAndDynamicsSystem_UpdateFlags(dynamicsSystemPtr, missionPtr, dt, agentRecPtr, param);
            var newFlags = *(AgentDynamicsFlags*)(dynamicsSystemPtr + rglAgentMovementAndDynamicsSystem.dynamics_flags).ToPointer();
            if (Mission.Current != null)
            {
                var agentObjIndex = *(int*)(agentRecPtr + rglAgentRecord.owner_index).ToPointer();
                var agent = Mission.Current.FindAgentWithIndex(agentObjIndex);
                if (agent == null) return;
                AfterUpdateDynamicsFlags(agent, dt, oldFlags, newFlags);
            }
        }
#endregion

#if DEBUG
        private static DebugMethodDelegate call_DebugMethod;
        private static IntPtr DebugMethod_Addr = IntPtr.Zero;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void DebugMethodDelegate(ulong dynamicsSystemPtr, float dt, ulong agentRecPtr, ulong debug);
        unsafe static private void OnDebugMethod(ulong dynamicsSystemPtr, float dt, ulong agentRecPtr, ulong debug)
        {
            call_DebugMethod(dynamicsSystemPtr, dt, agentRecPtr, debug);
            if (Input.IsKeyDown(InputKey.N))
                DebugLogic.SetPropertyUnsafe<Vec2>(Vec2.Forward, Agent.Main.GetPointer(), 0x20, 0x48);
        }
#endif

    }
}