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
    public static class AgentExtensions
    {
        private static MethodBase GetPtr = AccessTools.Method(typeof(Agent), "GetPtr");
        public unsafe static void Set3DPosition(this Agent agent, Vec3 position)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            Vec3* pos = (Vec3*)(movementSystemPointer + 0xc).ToPointer();
            *pos = position;
        }
        /// <summary>
        /// Sets the agent's world velocity. Only affects free-falling agents NOT on land.
        /// </summary>
        /// <param name="velocity">New velocity in world co-ordinates</param>
        public unsafe static void SetGlobalVelocity(this Agent agent, Vec3 velocity)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            Vec3* v = (Vec3*)(movementSystemPointer + 0x24).ToPointer();
            *v = velocity;
        }
        /// <summary>
        /// Returns the agent's world velocity in world co-ordinates
        /// </summary>
        public unsafe static Vec3 GetGlobalVelocity(this Agent agent)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            Vec3* v = (Vec3*)(movementSystemPointer + 0x24).ToPointer();
            return *v;
        }
        /// <summary>
        /// Sets the agent's movement velocity. Only affects agents ON land.
        /// </summary>
        /// <param name="velocity">New velocity in local co-ordinates</param>
        public unsafe static void SetMovementVelocity(this Agent agent, Vec2 velocity)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            Vec2* v = (Vec2*)(movementSystemPointer + 0x1c).ToPointer();
            *v = velocity;
        }
        public unsafe static void SetOnLand(this Agent agent, bool isOnLand)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            bool* onLand = (bool*)(movementSystemPointer + 0x40).ToPointer();
            *onLand = isOnLand;
        }
        /// <summary>
        /// </summary>
        /// <returns>Flags that control agent dynamics. Not fully understood yet.</returns>
        public unsafe static AgentDynamicsFlags GetDynamicsFlags(this Agent agent)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            return *(AgentDynamicsFlags*)(movementSystemPointer + 0x8).ToPointer();
        }
        /// <summary>
        /// Sets flags that control agent dynamics. Not fully understood yet.
        /// </summary>
        public unsafe static void SetDynamicsFlags(this Agent agent, AgentDynamicsFlags newFlags)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            AgentDynamicsFlags* flags = (AgentDynamicsFlags*)(movementSystemPointer + 0x8).ToPointer();
            *flags = newFlags;
        }
        public unsafe static void SetMovementDirectionAsAngle(this Agent agent, float angle)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            UIntPtr movementSystemPointer = new UIntPtr(*(ulong*)(agentPointer + 0x20).ToPointer());
            *(float*)(movementSystemPointer + 0x34).ToPointer() = angle;
        }
        public unsafe static void SetRotationMatrixFrame(this Agent agent, MatrixFrame frame)
        {
            UIntPtr agentPointer = (UIntPtr)GetPtr.Invoke(agent, new object[] { });
            MatrixFrame* f = (MatrixFrame*)(agentPointer + 0x528).ToPointer();
            *f = frame;
        }

        [Flags]
        public enum AgentDynamicsFlags : uint
        {
            None = 0,
            TickedDynamicsThisFrame = 0b0000_0000_0000_0000_0000_0000_0000_0001, //Ticked this frame?
            Flag2 = 0b0000_0000_0000_0000_0000_0000_0000_0010,
            Walking1 = 0b0000_0000_0000_0000_0000_0000_0000_0100, //Related to walking, does the animation
            Walking2 = 0b0000_0000_0000_0000_0000_0000_0000_1000, //Related to walking, slows down speed
            Crouch = 0b0000_0000_0000_0000_0000_0000_0001_0000, //Crouch
            TorsoRot1 = 0b0000_0000_0000_0000_0000_0000_0010_0000, //Related to torso rotation when changing look direction
            TorsoRot2 = 0b0000_0000_0000_0000_0000_0000_0100_0000, //Related to torso rotation when changing look direction
            Flag8 = 0b0000_0000_0000_0000_0000_0000_1000_0000,
            Flag9 = 0b0000_0000_0000_0000_0000_0001_0000_0000, //Always On??
            Flag10 = 0b0000_0000_0000_0000_0000_0010_0000_0000,
            Flag11 = 0b0000_0000_0000_0000_0000_0100_0000_0000,
            Flag12 = 0b0000_0000_0000_0000_0000_1000_0000_0000,
            Flag13 = 0b0000_0000_0000_0000_0001_0000_0000_0000,
            Flag14 = 0b0000_0000_0000_0000_0010_0000_0000_0000,
            JumpStart = 0b0000_0000_0000_0000_0100_0000_0000_0000, //Jump start
            JumpReady = 0b0000_0000_0000_0000_1000_0000_0000_0000, //Spawns particles for jump?
            Falling = 0b0000_0000_0000_0001_0000_0000_0000_0000, //Falling
            LandedHeavy = 0b0000_0000_0000_0010_0000_0000_0000_0000, //Landed Heavy
            LandedLight = 0b0000_0000_0000_0100_0000_0000_0000_0000, //Landed Light
            Flag20 = 0b0000_0000_0000_1000_0000_0000_0000_0000,
            CollidingWithObject = 0b0000_0000_0001_0000_0000_0000_0000_0000, //Hit object slowing down speed
            Flag22 = 0b0000_0000_0010_0000_0000_0000_0000_0000,
            Flag23 = 0b0000_0000_0100_0000_0000_0000_0000_0000, 
            CollidedWithObjectBelow = 0b0000_0000_1000_0000_0000_0000_0000_0000, //Hit object below
            SwitchToRightStance = 0b0000_0001_0000_0000_0000_0000_0000_0000, //Switch to right stance
            SwitchToLeftStance = 0b0000_0010_0000_0000_0000_0000_0000_0000, //Switch to left stance
            TryStartJump = 0b0000_0100_0000_0000_0000_0000_0000_0000, //Try start jump? (On space pressed?)
            Flag28 = 0b0000_1000_0000_0000_0000_0000_0000_0000,
            Flag29 = 0b0001_0000_0000_0000_0000_0000_0000_0000,
            Flag30 = 0b0010_0000_0000_0000_0000_0000_0000_0000,
            Flag31 = 0b0100_0000_0000_0000_0000_0000_0000_0000,
        }
    }
}
