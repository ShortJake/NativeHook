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
            
            call_Agent_SetAnimSystem(agentPointer, new UIntPtr(*newSkeleton_animTree));

            agent.SetActionSet(ref animData);
            InitializeAgentRecord.Invoke(agent, new object[] { });
        }

        public unsafe static void SetMovementLockedState(this Agent agent, AgentMovementLockedState newState)
        {
            if (agent == null) throw new ArgumentNullException();
            UIntPtr agentPointer = agent.GetPointer();
            *(AgentMovementLockedState*)(agentPointer + movement_locked_state) = newState;
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
        internal const int event_control_flags = 0x498;
        /// <summary>
        /// Agent.MovementControlFlag;
        /// </summary>
        internal const int movement_control_flags = 0x49c;
#if Editor
        internal const int rotation_frame = 0x510 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_anim_system = 0x580 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// Agent.MovementLockedState;
        /// </summary>
        internal const int movement_locked_state = 0x5e8 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int cached_skeleton = 0x808 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_visuals = 0x878 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_ai = 0x880 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int humanoid_record = 0x8f8 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_record = 0x900 + 0x10; //There's 0x10 between versions 1.3.9 and 1.3.15
#else
        internal const int rotation_frame = 0x518;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_anim_system = 0x588;
        /// <summary>
        /// Agent.MovementLockedState;
        /// </summary>
        internal const int movement_locked_state = 0x5f0;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int cached_skeleton = 0x810;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_visuals = 0x880;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_ai = 0x888;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int humanoid_record = 0x900;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int agent_record = 0x908;
#endif
        #endregion
    }
}
