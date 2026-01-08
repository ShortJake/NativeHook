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
using System.Security.Policy;
using TaleWorlds.Engine.GauntletUI;

namespace NativeHook
{
    public class NativeHookSubModule : MBSubModuleBase
    {
        private bool _initialized;
        private Color ErrorColor;
        private static NativeHookConfiguration Config;
        public static IntPtr NativeDLLAddr;
        private static int NativeDLLSize;
        //Prevent GC'ing of delegates
        private static List<Delegate> CallbackDelegates;
        // Returns the managed object that corresponds to this ID. ID is at an offset of 0x18 for agents
        private static MethodBase GetManagedObjWithId;

        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_SetErrorMessageCallback(IntPtr delegatePointer);
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_Initialize(IntPtr nativeDllAddress, IntPtr nativeDllSize, NativeHookConfiguration configs);
        [DllImport("NativeHookUnmanaged.dll")]

        private static extern void NH_FillCallbacks(MethodCallbackAddressStruct sigHolder);
        //private static extern void NH_FillCallbacks(IntPtr postAiTick, IntPtr postAgentTick, IntPtr afterUpdateDynamicsFlags, IntPtr onAnimTreeTick);
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_Cleanup();
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern IntPtr NH_ManagedScanForFirst(IntPtr baseAddress, UIntPtr bufferSize, [MarshalAs(UnmanagedType.LPStr)] string signature, [MarshalAs(UnmanagedType.LPStr)] string errorMsgName);

#if DEBUG
        [DllImport("NativeHookUnmanaged.dll")]
        private static extern void NH_FillDebugMethodCallback(IntPtr debugMethod);
#endif

        public override void OnInitialState()
        {
            NH_Initialize(NativeDLLAddr, new IntPtr(NativeDLLSize), Config);
            var bufferSize = new UIntPtr(Convert.ToUInt64(NativeDLLSize));
#if Editor
            UnkownBoneMatrixFrameBuffer = NativeDLLAddr + 0x1725990;
            Agent_SetAnimSystemAddr = NH_ManagedScanForFirst(NativeDLLAddr, bufferSize, "48 89 5c [..100...] ? 48 89 74 [..100...] ? 57 48 83 ec ? 48 8b d9 33 [11......] 48 8b [10001...]", "Agent_SetAnimSystemAddr");
            rglSkeletonAnim_SetEntitialQuatAddr = NH_ManagedScanForFirst(NativeDLLAddr, bufferSize, "48 89 5c [..100...] ? 48 89 6c [..100...] ? 48 89 74 [..100...] ? 57 48 83 ec ? 49 8b e9 49 8b f0", "rglSkeletonAnim_SetEntitialQuat");
#else
            //TODO: Update to non-editor v1.3.13
            UnkownBoneMatrixFrameBuffer = NativeDLLAddr + 0xc86890;
            Agent_SetAnimSystemAddr = NH_ManagedScanFor(NativeDLLAddr, bufferSize, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 48 8b d9 33 f6 48 8b 89 90", "Agent_SetAnimSystemAddr");
            rglSkeletonAnim_SetEntitialQuatAddr = NH_ManagedScanFor(NativeDLLAddr, bufferSize, "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 48 8b d9 48 0f be f2", "rglSkeletonAnim_SetInEntitialQuat");
#endif
            if (Agent_SetAnimSystemAddr != IntPtr.Zero) call_Agent_SetAnimSystem = Marshal.GetDelegateForFunctionPointer<Agent_SetAnimSystemDelegate>(Agent_SetAnimSystemAddr);
            if (rglSkeletonAnim_SetEntitialQuatAddr != IntPtr.Zero) call_rglSkeletonAnim_SetEntitialQuat = Marshal.GetDelegateForFunctionPointer<rglSkeletonAnim_SetEntitialQuatDelegate>(rglSkeletonAnim_SetEntitialQuatAddr);
            FillNativeCallbacks();
            _initialized = true;
        }
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            ErrorColor = new Color(1f, 0.2f, 0.15f);
            CallbackDelegates = new List<Delegate>();
            var showErrorMsg = new ShowErrorMessageDelegate(ShowErrorMessage);
            NH_SetErrorMessageCallback(Marshal.GetFunctionPointerForDelegate(showErrorMsg));
            CallbackDelegates.Add(showErrorMsg);

            Config = 0;
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
                ShowErrorMessage("Could not find TaleWorlds.Native.dll");
                return;
            }

            GetManagedObjWithId = AccessTools.Method(typeof(DotNetObject), "GetManagedObjectWithId", new Type[] { typeof(int) });
        }

        public static void SetConfiguration(NativeHookConfiguration newConfig)
        {
            Config |= newConfig;
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            NH_Cleanup();
        }

#if DEBUG
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new DebugLogic());
        }
#endif

        private void FillNativeCallbacks()
        {
            var onPostAiTick = new Callback_OnPostAiTickDelegate(Callback_OnPostAiTick);
            CallbackDelegates.Add(onPostAiTick);
            var onPostAgentTick = new Callback_OnPostAgentTickDelegate(Callback_OnPostAgentTick);
            CallbackDelegates.Add(onPostAgentTick);
            var afterUpdateDynamicsFlags = new Callback_AfterUpdateDynamicsFlagsDelegate(Callback_AfterUpdateDynamicsFlags);
            CallbackDelegates.Add(afterUpdateDynamicsFlags);
            var onAnimTreeTick = new Callback_OnAnimTreeTickDelegate(Callback_OnAnimTreeTick);
            CallbackDelegates.Add(onAnimTreeTick);
            var animGetEntitialQuat = new Callback_AnimGetEntitialQuatDelegate(Callback_AnimGetEntitialQuat);
            CallbackDelegates.Add(animGetEntitialQuat);
            var sigHolder = new MethodCallbackAddressStruct
            {
                OnPostAiTick = Marshal.GetFunctionPointerForDelegate(onPostAiTick),
                OnPostAgentTick = Marshal.GetFunctionPointerForDelegate(onPostAgentTick),
                AfterUpdateDynamicsFlags = Marshal.GetFunctionPointerForDelegate(afterUpdateDynamicsFlags),
                OnAnimTreeTick = Marshal.GetFunctionPointerForDelegate(onAnimTreeTick),
                AnimGetEntitialQuat = Marshal.GetFunctionPointerForDelegate(animGetEntitialQuat),
            };
            NH_FillCallbacks(sigHolder);
#if DEBUG

            var debugMethod = new Callback_DebugMethodDelegate(Callback_DebugMethod);
            CallbackDelegates.Add(debugMethod);
            NH_FillDebugMethodCallback(Marshal.GetFunctionPointerForDelegate(debugMethod));
#endif
        }

        public static IntPtr UnkownBoneMatrixFrameBuffer;

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

        #region Agent Set Animation System
        private static IntPtr Agent_SetAnimSystemAddr;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void Agent_SetAnimSystemDelegate(UIntPtr agent, UIntPtr newAnimSystem);
        public static Agent_SetAnimSystemDelegate call_Agent_SetAnimSystem;
        #endregion

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

        #region Anim Tree Tick
        public delegate void OnAnimTreeTickDelegate(Skeleton skeleton, byte lastBoneIndex, ref MatrixFrame[] cachedMatrixFrame);
        public static event OnAnimTreeTickDelegate OnAnimTreeTick;
        private delegate void Callback_OnAnimTreeTickDelegate(IntPtr animTreePtr, IntPtr skeletonPtr, byte lastBoneIndex, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] IntPtr[] cachedMatrixFramePtrsArray);
        unsafe static private void Callback_OnAnimTreeTick(IntPtr animTreePtr, IntPtr skeletonPtr, byte lastBoneIndex, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] IntPtr[] cachedMatrixFramePtrsArray)
        {
            var ev = OnAnimTreeTick;
            if (ev != null)
            {
                //ev(null, lastBoneIndex, ref cachedMatrixFrameArray);
            }
            /*var unsginedSkeletonPtr = new UIntPtr(skeletonPtr.ToPointer());

            var IManaged = AccessTools.Field("TaleWorlds.DotNet.LibraryApplicationInterface:IManaged").GetValue(null);
            var DecreaseReferenceCount = AccessTools.Method("TaleWorlds.DotNet.IManaged:DecreaseReferenceCount");

            if (IManaged == null || DecreaseReferenceCount == null) return;

            var skeleton = (Skeleton)AccessTools.Constructor(typeof(Skeleton), new Type[] { typeof(UIntPtr) })?.Invoke(new object[] { unsginedSkeletonPtr });
            if (skeleton == null) return;

            DecreaseReferenceCount.Invoke(IManaged, new object[] { unsginedSkeletonPtr });
            // Copying event to a local variable prevents a race condition when another thread unsubscribes from event
            var ev = OnAnimTreeTick;
            if (ev != null)
            {
                var cachedMatrixFrame = *(MatrixFrame*)cachedMatrixFramePtr.ToPointer();
                ev(skeleton, boneIndex, ref cachedMatrixFrame);
            }
            */
        }
        #endregion

        #region Anim Get Entitial Quat 
        private delegate void Callback_AnimGetEntitialQuatDelegate(IntPtr animPtr, IntPtr skeletonModelPtr, sbyte boneIndex);
        unsafe static private void Callback_AnimGetEntitialQuat(IntPtr animPtr, IntPtr skeletonModelPtr, sbyte boneIndex)
        {
            var outQuat = rglSkeletonAnim.GetOutQuat(animPtr, boneIndex);
            if (outQuat.IsUnit) return;

            var parentIndex = *(sbyte*)(skeletonModelPtr + rglSkeletonModel.bone_parents + boneIndex).ToPointer();
            var parentQuat = Quaternion.Identity;
            var parentPastTrans = BoneTransformation.Identity;
            var modelBonesArray = (byte*)(*(ulong*)(skeletonModelPtr + rglSkeletonModel.bones_array).ToPointer());
            var skeleton = *(ulong*)(animPtr + rglSkeletonAnim.skeleton).ToPointer();
            if (skeleton == 0x0) return;
            var skeletonBonesArray = *(ulong*)(skeleton + rglSkeleton.bones);
            if (parentIndex > -1)
            {
                parentQuat = rglSkeletonAnim.GetOutEntitialQuat(animPtr, parentIndex);
                if (!parentQuat.IsUnit)
                {
                    Callback_AnimGetEntitialQuat(animPtr, skeletonModelPtr, parentIndex);
                    parentQuat = rglSkeletonAnim.GetOutEntitialQuat(animPtr, parentIndex);
                }
                parentPastTrans = *(BoneTransformation*)(skeletonBonesArray + (uint)parentIndex * rglBoneStruct.size + rglBoneStruct.transformation);
            }     
            var pastTrans = *(BoneTransformation*)(skeletonBonesArray + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.transformation);
            var pastLocalQuat = parentPastTrans.q.TransformToLocal(pastTrans.q);
            if (!pastLocalQuat.IsUnit)
            {
   
                var localRestFrame = *(MatrixFrame*)(modelBonesArray + boneIndex * rglBoneModelStruct.size + rglBoneModelStruct.local_rest_frame);
                var newInQuat = localRestFrame.rotation.ToQuaternion();
                newInQuat = parentQuat.TransformToParent(newInQuat);
                rglSkeletonAnim.SetOutQuat(animPtr, boneIndex, newInQuat, skeletonModelPtr);
            }
            else
            {
                pastLocalQuat = parentQuat.TransformToParent(pastLocalQuat);
                rglSkeletonAnim.SetOutQuat(animPtr, boneIndex, pastLocalQuat, skeletonModelPtr);
            }   
        }
        #endregion
        
        #region Anim Set Entitial Quaternion
        private static IntPtr rglSkeletonAnim_SetEntitialQuatAddr;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void rglSkeletonAnim_SetEntitialQuatDelegate(IntPtr rglSkeletonAnim, sbyte boneIndex, Quaternion newEntitialQuat, IntPtr skeletonModel);
        public static rglSkeletonAnim_SetEntitialQuatDelegate call_rglSkeletonAnim_SetEntitialQuat;
        #endregion

        #region DebugMethod
#if DEBUG
        private delegate void Callback_DebugMethodDelegate(IntPtr animPtr, IntPtr skeletonModelPtr, byte boneIndex, IntPtr outQuat);
        unsafe static private void Callback_DebugMethod(IntPtr animPtr, IntPtr skeletonModelPtr, byte boneIndex, IntPtr outQuat)
        {
            
        }
#endif
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void ShowErrorMessageDelegate([MarshalAs(UnmanagedType.LPStr)] string msg);
        public void ShowErrorMessage(string msg)
        {
            msg = $"NativeHook: ({msg})";
            InformationManager.DisplayMessage(new InformationMessage(msg, ErrorColor));
            MBDebug.Print(msg);
        }
    }
}