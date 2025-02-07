using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NativeHook
{
    internal class DebugLogic : MissionLogic, IMissionBehaviorNativeHook
    {
        public override void OnMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            if (Input.IsKeyPressed(InputKey.B))
            {
                /*unsafe
                {
                    var agentPtr = (UIntPtr)AccessTools.Method(typeof(Agent), "GetPtr").Invoke(Agent.Main, new object[] {});
                    delegate*<ulong, MatrixFrame, ulong, ulong, void> setFrameDel = (delegate*<ulong, MatrixFrame, ulong, ulong, void>)(new IntPtr(NativeHookSubModule.NativeDLLAddr + 0xb1e180).ToPointer());
                    //delegate*<ulong, MatrixFrame, ulong, ulong, void> setFrameDel = (delegate*<ulong, MatrixFrame, ulong, ulong, void>)(new IntPtr(NativeHookSubModule.NativeDLLAddr + 0xaf75d0).ToPointer());
                    ulong param3 = 0x0U;
                    ulong param4 = 0x0U;
                    var frame = Agent.Main.Frame;
                    frame.Rotate(1f, Vec3.Up);
                    Agent.Main.SetRotationMatrixFrame(frame);
                    setFrameDel(Agent.Main.AgentVisuals.Pointer.ToUInt64(), frame, param3, param4);                
                    //setFrameDel(agentPtr.ToUInt64(), frame, param3, param4);
                }*/
            }
        }
        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);
        }

        public void OnAiAgentTick(Agent agent)
        {
        }

        public void OnSkeletonUpdate(Skeleton skeleton, ulong param1, float param2)
        {
            if (Input.IsKeyDown(InputKey.B))
            {
                var skelPtr = Agent.Main.AgentVisuals.GetSkeleton().Pointer;
                if (skeleton.Pointer != skelPtr) return;
                unsafe
                {
                    sbyte bone = 2;
                    var entititial_frame = skeleton.GetBoneEntitialFrame(bone);
                    entititial_frame = entititial_frame.TransformToParent(Agent.Main.AgentVisuals.GetFrame());
                    UIntPtr bones_array = new UIntPtr(*(ulong*)(skelPtr + 0x18).ToPointer());

                    MatrixFrame* tested_frame = (MatrixFrame*)(bones_array + 0x100 * bone + 0x4).ToPointer();
                    tested_frame->rotation.RotateAboutAnArbitraryVector(tested_frame->rotation.s.NormalizedCopy(), -1.6f);
                    //tested_frame->rotation.RotateAboutAnArbitraryVector(tested_frame->rotation.f.NormalizedCopy(), 1.57f);
                    skeleton.ForceUpdateBoneFrames();
                }
            }
        }
        public void OnPostAgentTick(Agent agent, float dt)
        {
            if (!agent.IsActive()) return;
            if (Input.IsKeyDown(InputKey.B))
            {
                /*var skelPtr = Agent.Main.AgentVisuals.GetSkeleton().Pointer;
                unsafe
                {
                    var i = 1;
                    UIntPtr bones_array = new UIntPtr(*(ulong*)(skelPtr + 0x18).ToPointer());
                    MatrixFrame* bone_frame = (MatrixFrame*)(bones_array + 0x100 * i + 0x4).ToPointer();
                    var newFrame = *bone_frame;
                    newFrame.Advance(0.5f);
                    *bone_frame = newFrame;
                    var t = true;
                }*/
                //agent.MovementFlags |= Agent.MovementControlFlag.TurnRight;
                //Agent.Main.SetDynamicsFlags(AgentExtensions.AgentDynamicsFlags.Flag22);
                /*unsafe
                {
                    var agentPtr = (UIntPtr)AccessTools.Method(typeof(Agent), "GetPtr").Invoke(Agent.Main, new object[] { });
                    delegate*<ulong, MatrixFrame, ulong, ulong, void> setFrameDel = (delegate*<ulong, MatrixFrame, ulong, ulong, void>)(new IntPtr(NativeHookSubModule.NativeDLLAddr + 0xaf75d0).ToPointer());
                    ulong param3 = 0x0U;
                    ulong param4 = 0x0U;
                    var frame = Agent.Main.Frame;
                    frame.Rotate(1f, Vec3.Up);
                    Agent.Main.SetRotationMatrixFrame(frame);
                    setFrameDel(agentPtr.ToUInt64(), frame, param3, param4);
                }*/
            }
        }
    }
}
