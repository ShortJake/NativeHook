using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NativeHook
{
    public static class rglAgentMovementAndDynamicsSystem
    {
        #region Extension Methods
        public unsafe static void SetPosition(this Agent agent, Vec3 newPosition)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(Vec3*)(movementSystemPointer + position) = newPosition;
        }
        /// <summary>
        /// Sets the rotation of a bipedal agent's torso.
        /// </summary>
        public unsafe static void SetTorsoRotation(this Agent agent, float newAngle)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + torso_rot) = newAngle;
        }
        /// <summary>
        /// Returns the rotation of a bipedal agent's torso.
        /// </summary>
        public unsafe static float GetTorsoRotation(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + torso_rot);
        }
        /// <summary>
        /// Sets the rotation speed of mount agents
        /// </summary>
        public unsafe static void SetRotationVelocity(this Agent agent, float angularVelocity)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + turn_veclocity) = angularVelocity;
        }
        /// <summary>
        /// Returns the rotation speed of mount agents
        /// </summary>
        public unsafe static float GetRotationVelocity(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + turn_veclocity);
        }
        /// <summary>
        /// Sets the force acting on the agent. Only affects free-falling agents NOT on land.
        /// </summary>
        /// <param name="newForce">New force in world co-ordinates</param>
        public unsafe static void SetExternalForce(this Agent agent, Vec3 newForce)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(Vec3*)(movementSystemPointer + force) = newForce;
        }
        /// <summary>
        /// Returns the force acting on the agent in world co-ordinates
        /// </summary>
        public unsafe static Vec3 GetExternalForce(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(Vec3*)(movementSystemPointer + force);
        }
        /// <summary>
        /// Sets the agent's world velocity. Only affects free-falling agents NOT on land.
        /// </summary>
        /// <param name="newVelocity">New velocity in world co-ordinates</param>
        public unsafe static void SetGlobalVelocity(this Agent agent, Vec3 newVelocity)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(Vec3*)(movementSystemPointer + velocity) = newVelocity;
        }
        /// <summary>
        /// Returns the agent's world velocity in world co-ordinates
        /// </summary>
        public unsafe static Vec3 GetGlobalVelocity(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(Vec3*)(movementSystemPointer + velocity);
        }
        /// <summary>
        /// Sets the agent's movement velocity. Only affects agents ON land.
        /// </summary>
        /// <param name="newVelocity">New velocity in local co-ordinates</param>
        public unsafe static void SetMovementVelocity(this Agent agent, Vec2 newVelocity)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(Vec2*)(movementSystemPointer + movement_velocity) = newVelocity;
        }
        public unsafe static void SetOnLandState(this Agent agent, AgentOnLandFlags onLandState)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(AgentOnLandFlags*)(movementSystemPointer + on_land) = onLandState;
        }
        public unsafe static AgentOnLandFlags GetOnLandState(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(AgentOnLandFlags*)(movementSystemPointer + on_land);
        }
        /// <summary>
        /// </summary>
        /// <returns>Flags that control agent dynamics. Not fully understood yet.</returns>
        public unsafe static AgentDynamicsFlags GetDynamicsFlags(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(AgentDynamicsFlags*)(movementSystemPointer + dynamics_flags);
        }
        /// <summary>
        /// Sets flags that control agent dynamics. Not fully understood yet.
        /// </summary>
        public unsafe static void SetDynamicsFlags(this Agent agent, AgentDynamicsFlags newFlags)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(AgentDynamicsFlags*)(movementSystemPointer + dynamics_flags) = newFlags;
        }
        /// <param name="angle">Movement angle in radians</param>
        public unsafe static void SetMovementDirectionAsAngle(this Agent agent, float angle)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + movement_direction_as_angle) = angle;
        }
        /// <summary>
        /// Sets the timer that allows agent to jump. The agent can jump if the timer is +ve
        /// </summary>
        public unsafe static void SetJumpClearTimer(this Agent agent, float jumpClearTimer)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + jump_clear_time) = jumpClearTimer;
        }
        /// <summary>
        /// Gets the timer that allows agent to jump. The agent can jump if the timer is +ve
        /// </summary>
        public unsafe static float GetJumpClearTimer(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + jump_clear_time);
        }
        /// <summary>
        /// Sets a timer that makes the agent ignore land collision with terrain. Collision is ignored while the timer is -ve
        /// </summary>
        public unsafe static void SetIgnoreOnLandTimer(this Agent agent, float ignoreOnLandTimer)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + ignore_on_land_timer) = ignoreOnLandTimer;
        }
        /// <summary>
        /// Gets the timer that makes the agent ignore land collision with terrain. Collision is ignored while the timer is -ve
        /// </summary>
        public unsafe static float GetIgnoreOnLandTimer(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + ignore_on_land_timer);
        }
        /// <summary>
        /// Sets the agent rotation about x axis in radians.
        /// </summary>
        public unsafe static void SetRotationAboutX(this Agent agent, float rotation)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + local_frame_rot_about_x) = rotation;
        }
        /// <summary>
        /// Gets the agent rotation about x axis in radians.
        /// </summary>
        public unsafe static float GetRotationAboutX(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + local_frame_rot_about_x);
        }
        /// <summary>
        /// Sets the agent rotation about x axis in radians.
        /// </summary>
        public unsafe static void SetRotationAboutY(this Agent agent, float rotation)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            *(float*)(movementSystemPointer + local_frame_rot_about_y) = rotation;
        }
        /// <summary>
        /// Gets the agent rotation about x axis in radians.
        /// </summary>
        public unsafe static float GetRotationAboutY(this Agent agent)
        {
            UIntPtr agentPointer = agent.GetPointer();
            ulong movementSystemPointer = *(ulong*)(agentPointer + rglAgent.agent_movement_and_dynamics_system).ToPointer();
            return *(float*)(movementSystemPointer + local_frame_rot_about_y);
        }
        #endregion
        #region Offsets
        /// <summary>
        /// int;
        /// </summary>
        internal const int index = 0x4;
        /// <summary>
        /// AgentDynamicsFlags;
        /// </summary>
        internal const int dynamics_flags = 0x8;
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int position = 0xc;
        /// <summary>
        /// Vec2;
        /// </summary>
        internal const int movement_velocity = 0x1c;
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int velocity = 0x24;
        /// <summary>
        /// float;
        /// </summary>
        internal const int movement_direction_as_angle = 0x34;
        /// <summary>
        /// Vec2;
        /// </summary>
        internal const int movement_input_vector = 0x38;
        /// <summary>
        /// AgentOnLandState;
        /// </summary>
        internal const int on_land = 0x40;
        /// <summary>
        /// float;
        /// </summary>
        internal const int lock_movement_timer = 0x44;
        /// <summary>
        /// float;
        /// </summary>
        internal const int local_frame_rot_about_y = 0x58;
        /// <summary>
        /// float;
        /// </summary>
        internal const int local_frame_rot_about_x = 0x5c;
#if Editor
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int force = 0x138;
        /// <summary>
        /// float;
        /// </summary>
        internal const int torso_rot = 0x148;
        /// <summary>
        /// float;
        /// </summary>
        internal const int turn_input = 0x15c;
        /// <summary>
        /// float;
        /// </summary>
        internal const int turn_veclocity = 0x160;
        /// <summary>
        /// float;
        /// </summary>
        internal const int jump_clear_time = 0x16c;
        /// <summary>
        /// float;
        /// </summary>
        internal const int landing_anim_timer = 0x170;
        /// <summary>
        /// float;
        /// </summary>
        internal const int ignore_on_land_timer = 0x17c;
#else
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int force = 0x110;
        /// <summary>
        /// float;
        /// </summary>
        internal const int torso_rot = 0x120;
        /// <summary>
        /// float;
        /// </summary>
        internal const int turn_input = 0x15c - 0x28;
        /// <summary>
        /// float;
        /// </summary>
        internal const int turn_veclocity = 0x160 - 0x28;
        /// <summary>
        /// float;
        /// </summary>
        internal const int jump_clear_time = 0x144;
        /// <summary>
        /// float;
        /// </summary>
        internal const int landing_anim_timer = 0x148;
        /// <summary>
        /// float;
        /// </summary>
        internal const int ignore_on_land_timer = 0x154;
#endif

        #endregion
    }

    [Flags]
    public enum AgentOnLandFlags : byte
    {
        NotOnLand = 0,
        OnLand = 1,
        IgnoreTerrainCollision = 2,
        Falling = 4,
    }

    [Flags]
    public enum AgentDynamicsFlags : uint
    {
        None = 0,
        TickedDynamicsThisFrame = 1,
        Flag2 = 2,
        Walking1 = 4, //Related to walking
        Walking2 = 8, //Related to walking
        Crouch = 16,
        TorsoRot1 = 32, //Related to torso rotation when changing look direction
        TorsoRot2 = 64, //Related to torso rotation when changing look direction
        /// <summary>
        /// Use a smaller collision capsule for crouching
        /// </summary>
        UseSmallerCapsuleRadius = 128,
        Flag9 = 256, //Always On??
        Flag10 = 512,
        Flag11 = 1024,
        MountBrakeStop = 2048,
        MountBrakeStart = 4096,
        MountDashForward = 8192,
        /// <summary>
        /// Start the jump taking-off animation
        /// </summary>
        JumpAnimStart = 16_384,
        /// <summary>
        /// Start the main jump looping animation
        /// </summary>
        JumpLoopAnimStart = 32_768,
        Falling = 65_536,
        LandedHeavy = 131_072,
        LandedLight = 262_144,
        Flag20 = 524_288,
        /// <summary>
        /// True when colliding with an entity
        /// </summary>
        CollidingWithObject = 1_048_576,
        Flag22 = 2_097_152,
        /// <summary>
        /// True when the mount collides with object ahead forcefully
        /// </summary>
        MountHitObject = 4_194_304,
        /// <summary>
        ///  True when colliding with entity/terrain below
        /// </summary>
        CollidedWithObjectBelow = 8_388_608,
        SwitchToRightStance = 16_777_216,
        SwitchToLeftStance = 33_554_432,
        /// <summary>
        /// True when pressing jump button, and tries to start jump
        /// </summary>
        StartJump = 67_108_864,
        Flag28 = 134_217_728,
        /// <summary>
        /// Unkown, but true when double-pressing back with a mount while stopped
        /// </summary>
        MountDashBack = 268_435_456,
        Flag30 = 536_870_912,
        Flag31 = 1_073_741_824,
        Flag32 = 2_147_483_648,
    }
}
