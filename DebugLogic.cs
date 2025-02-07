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
            }
        }
    }
}
