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
                var errorMsg = "NativeHook Error! Could not find TaleWorlds.Native.dll";
                InformationManager.DisplayMessage(new InformationMessage(errorMsg, ErrorColor));
                MBDebug.ShowWarning(errorMsg);
                MBDebug.Print(errorMsg);
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
            CreateHook(rglAnim_tree_TickAddr, new rglAnim_tree_TickDelegate(rglAnim_tree_Tick));
            call_Agent_SetAnimSystem = Marshal.GetDelegateForFunctionPointer<Agent_SetAnimSystemDelegate>(Agent_SetAnimSystemAddr);
#if DEBUG
            //CreateHook(DebugMethod_Addr, new DebugMethodDelegate(OnDebugMethod));
#endif
        }
        private void GetHookedMethodAddresses()
        {
            var buffer = GetMemoryBuffer(NativeDLLAddr, NativeDLLSize);
#if Editor
            Agent_AiTickAddr = ScanForFirstResult(buffer, "48 8b c4 f3 0f 11 48 10 55 41 54 41 55");
            Agent_TickAddr = ScanForFirstResult(buffer, "40 53 48 81 ec a0 00 00 00 80 b9 97");
            Agent_SetAnimSystemAddr = ScanForFirstResult(buffer, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 30 48 8b d9 33 f6 48 8b 89 98 05 00 00");
            AgentMovementAndDynamicsSystem_UpdateFlagsAddr = ScanForFirstResult(buffer, "40 55 56 48 8d 6c 24 b9 48 81 ec");
            rglAnim_tree_TickAddr = ScanForFirstResult(buffer, "40 55 57 48 8d ac 24 28 ec ff ff b8 d8");
#else
            Agent_AiTickAddr = ScanForFirstResult(buffer, "48 8b c4 f3 0f 11 48 10 55 41 54 41");
            Agent_TickAddr = ScanForFirstResult(buffer, "40 53 41 57 48 81 ec 88 00 00 00 8b");
            Agent_SetAnimSystemAddr = ScanForFirstResult(buffer, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 48 8b d9 33 f6 48 8b 89 90");
            AgentMovementAndDynamicsSystem_UpdateFlagsAddr = ScanForFirstResult(buffer, "40 55 57 48 8b ec 48 83 ec 48 48 89");
            rglAnim_tree_TickAddr = ScanForFirstResult(buffer, "40 55 57 48 8d ac 24 28 ed ff ff");
#endif
#if DEBUG
            var hits = ScanFor(buffer, DebugMethodSignature);
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
                var errorMsg = "NativeHook Error! Invalid adddress for '" + functionName + "'. Not hooking!";
                InformationManager.DisplayMessage(new InformationMessage(errorMsg, ErrorColor));
                MBDebug.ShowWarning(errorMsg);
                MBDebug.Print(errorMsg);
                return;
            }
            try
            {
                var callDelField = AccessTools.Field(this.GetType(), "call_" + functionName);
                if (callDelField == null)
                {
                    var errorMsg = "NativeHook Error!Function name doesnt match for " + functionName;
                    InformationManager.DisplayMessage(new InformationMessage(errorMsg, ErrorColor));
                    MBDebug.ShowWarning(errorMsg);
                    MBDebug.Print(errorMsg);
                    return;
                }
                callDelField.SetValue(this, Marshal.GetDelegateForFunctionPointer(address, functionDelegate.GetType()));
                var hook = LocalHook.Create(address, functionDelegate, null);
                hook.ThreadACL.SetExclusiveACL(new int[] { });
                NativeHooks.Add(hook);
            }
            catch (Exception ex)
            {
                var errorMsg = "NativeHook Error! Error hooking '" + functionName + "'";
                InformationManager.DisplayMessage(new InformationMessage(errorMsg, ErrorColor));
                MBDebug.ShowWarning(errorMsg);
                MBDebug.Print(errorMsg);
                throw ex;
            }
        }


        #region AI Tick
        public delegate void OnPostAiTickDelegate(Agent agent, float dt);
        public static event OnPostAiTickDelegate OnPostAiTick;
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
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAiTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
        }
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_AiTickDelegate(UIntPtr agentPtr, float dt);
        unsafe static private void Agent_AiTick(UIntPtr agentPtr, float dt)
        {
            call_Agent_AiTick(agentPtr, dt);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAiTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
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
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAgentTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
        }
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void Agent_TickDelegate(UIntPtr agentPtr, float dt);
        unsafe static private void Agent_Tick(UIntPtr agentPtr, float dt)
        {
            call_Agent_Tick(agentPtr, dt);
            var agentObjIndex = *(int*)(agentPtr + rglAgent.obj_id).ToPointer();
            var agentObj = GetManagedObjWithId.Invoke(null, new object[] { agentObjIndex }) as Agent;
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnPostAgentTick;
            if (Mission.Current == null || agentObj == null || ev == null) return;
            ev(agentObj, dt);
        }
#endif
        #endregion

        #region Agent Set Animation System
        private static IntPtr Agent_SetAnimSystemAddr;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void Agent_SetAnimSystemDelegate(UIntPtr agent, UIntPtr newAnimSystem);
        public static Agent_SetAnimSystemDelegate call_Agent_SetAnimSystem;
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
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = AfterUpdateDynamicsFlags;
            if (Mission.Current != null && ev != null)
            {
                var agentObjIndex = *(int*)(agentRecPtr + rglAgentRecord.owner_index).ToPointer();
                var agent = Mission.Current.FindAgentWithIndex(agentObjIndex);
                if (agent == null) return;
                ev(agent, dt, oldFlags, newFlags);
            }
        }
#endregion

#region Anim Tree Tick
        public delegate void OnPostAnimTreeTickDelegate(Agent agent, Skeleton skeleton);
        public static event OnPostAnimTreeTickDelegate OnPostAnimTreeTick;
        private static IntPtr rglAnim_tree_TickAddr;
        private static rglAnim_tree_TickDelegate call_rglAnim_tree_Tick;
#if Editor
        internal static int UnkownBoneMatrixFrameBuffer = 0x1725990;        
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void rglAnim_tree_TickDelegate(UIntPtr animTree, UIntPtr skeleton, UIntPtr param, UIntPtr debug_param);
        unsafe static private void rglAnim_tree_Tick(UIntPtr animTree, UIntPtr skeleton, UIntPtr param, UIntPtr debug_param)
        {
            call_rglAnim_tree_Tick(animTree, skeleton, param, debug_param);
        }
#else
        internal static int UnkownBoneMatrixFrameBuffer = 0xc86890;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate void rglAnim_tree_TickDelegate(UIntPtr animTree, UIntPtr skeleton);
        unsafe static private void rglAnim_tree_Tick(UIntPtr animTree, UIntPtr skeleton)
        {
            call_rglAnim_tree_Tick(animTree, skeleton);
        }
#endif
#endregion


#if DEBUG
        private static string DebugMethodSignature = "44 0f 10 46 04 0f 10 46 14 44 0f 11 85";
        private static DebugMethodDelegate call_DebugMethod;
        private static IntPtr DebugMethod_Addr = IntPtr.Zero;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
        private delegate void DebugMethodDelegate();
        unsafe static private void OnDebugMethod()
        {
            call_DebugMethod();
        }

        public struct BoneTransformation
        {
            public Quaternion q;
            public Vec3 o;
            public BoneTransformation(Quaternion quat, Vec3 pos)
            {
                q = quat;
                o = pos;
            }
        }
#endif

    }
}