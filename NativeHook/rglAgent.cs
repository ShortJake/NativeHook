using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static NativeHook.NativeHookSubModule;

namespace NativeHook
{
    public static class rglAgent
    {
        #region Extension Methods
        private static MethodBase GetPtr = AccessTools.Method(typeof(Agent), "GetPtr");
        private static MethodBase InitializeAgentRecord = AccessTools.Method(typeof(Agent), "InitializeAgentRecord");
        internal static UIntPtr GetPointer(this Agent agent)
        {
            return (UIntPtr)GetPtr.Invoke(agent, new object[] { });
        }

        public unsafe static void SetSkeleton(this Agent agent, Skeleton newSkeleton, AnimationSystemData animData)
        {
            if (agent == null || newSkeleton == null) throw new ArgumentNullException();
            
            UIntPtr agentPointer = agent.GetPointer();
            agent.AgentVisuals.SetSkeleton(newSkeleton);
            // Set the agent's cached skeleton to the new one
            *(ulong*)(agentPointer + cached_skeleton).ToPointer() = newSkeleton.Pointer.ToUInt64();
            var newSkeleton_animTree = (ulong*)(newSkeleton.Pointer + rglSkeleton.anim_tree).ToPointer();
            
            //call_Agent_SetAnimSystem(agentPointer, new UIntPtr(*newSkeleton_animTree));

            agent.SetActionSet(ref animData);
            InitializeAgentRecord.Invoke(agent, new object[] { });
        }
        #endregion

        #region Offsets
        /// <summary>
        /// int;
        /// </summary>
        internal const int index = 0x10;
        /// <summary>
        /// int;
        /// </summary>
        internal const int obj_id = 0x18;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_movement_and_dynamics_system = 0x20;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_combat_system = 0x28;
        /// <summary>
        /// float[84] (agent_driven_properties Struct);
        /// </summary>
        internal const int agent_driven_properties = 0x2c8;
        /// <summary>
        /// Agent.EventControlFlag;
        /// </summary>
        internal const int event_control_flags = 0x4c4;
        /// <summary>
        /// Agent.MovementControlFlag;
        /// </summary>
        internal const int movement_control_flags = 0x4c8;
        /// <summary>
        /// MatrixFrame;
        /// </summary>
#if Editor
        internal const int rotation_frame = 0x528;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_anim_system = 0x598;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int cached_skeleton = 0x660;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_visuals = 0x6e0;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_ai = 0x6e8;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int humanoid_record = 0x730;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_record = 0x740;
#else
        internal const int rotation_frame = 0x520;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_anim_system = 0x590;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int cached_skeleton = 0x658;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_visuals = 0x6d8;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_ai = 0x6e0;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int humanoid_record = 0x728;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_record = 0x738;
#endif
        #endregion
    }
}
