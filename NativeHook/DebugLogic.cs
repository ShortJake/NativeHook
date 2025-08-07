using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Security.Policy;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace NativeHook
{
#if DEBUG
    internal class DebugLogic : MissionLogic
    {
        bool _debugBool;
        int _debugInt;
        float _debugFloat;
        string _debugString;
        Vec3 _debugVec;
        MatrixFrame _debugFrame;
        ulong _debugAddr;
        Skeleton _oldSkeleton;
        Skeleton _newSkeleton;
        public DebugLogic()
        {
            NativeHookSubModule.OnPostAiTick += OnAiAgentTick;
            NativeHookSubModule.OnPostAgentTick += OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags += AfterUpdateDynamicsFlags;
            NativeHookSubModule.OnAnimTreeTick += OnAnimTreeTick;
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            NativeHookSubModule.OnPostAiTick -= OnAiAgentTick;
            NativeHookSubModule.OnPostAgentTick -= OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags -= AfterUpdateDynamicsFlags;
            NativeHookSubModule.OnAnimTreeTick -= OnAnimTreeTick;
        }
        public override void OnMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            if (_oldSkeleton == null) _oldSkeleton = Agent.Main.AgentVisuals.GetSkeleton();
            if (Input.IsKeyPressed(InputKey.M))
            {
                /*for (byte j = 0; j < _oldSkeleton.GetBoneCount(); j++)
                {
                    MBDebug.RenderDebugFrame(Agent.Main.AgentVisuals.GetGlobalFrame().TransformToParent(_oldSkeleton.GetBoneRestFrame(j)), 0.2f, 10f);
                }*/
                var localFrame = _oldSkeleton.GetBoneLocalRestFrame(13);
                localFrame.Advance(0.5f);
                SetPropertyUnsafe(localFrame, _oldSkeleton.Pointer, rglSkeleton.skeleton_model, 0x140, 13 * 0x1b0 + 0x50UL);
            }
            if (Input.IsKeyPressed(InputKey.Comma))
            {
                for (sbyte j = 0; j < _oldSkeleton.GetBoneCount(); j++)
                {
                    MBDebug.RenderDebugFrame(Agent.Main.AgentVisuals.GetGlobalFrame().TransformToParent(_oldSkeleton.GetBoneEntitialRestFrame(j)), 0.2f, 10f);
                }
            }
            if (Input.IsKeyPressed(InputKey.K))
            {
                var currentSkeleton = Agent.Main.AgentVisuals.GetSkeleton();
                if (currentSkeleton == _oldSkeleton && _newSkeleton == null)
                {
                    var animData = Agent.Main.Monster.FillAnimationSystemData(Agent.Main.ActionSet, 1f, false);
                    var skin = new SkinGenerationParams((int)SkinMask.AllVisible, Equipment.UnderwearTypes.NoUnderwear, 0, 0, 0, 0, true, 0f, Agent.Main.IsFemale ? 1 : 0, 0, false, false);
                    _newSkeleton = MBSkeletonExtensions.CreateWithActionSet(ref animData);
                    Agent.Main.SetSkeleton(_newSkeleton, animData);
                    Agent.Main.AgentVisuals.ClearVisualComponents(false);
                    Agent.Main.AgentVisuals.AddSkinMeshes(skin, Agent.Main.BodyPropertiesValue, true, false);
                }
                else if (currentSkeleton == _oldSkeleton)
                {
                    var animData = Agent.Main.Monster.FillAnimationSystemData(Agent.Main.ActionSet, 1f, false);
                    Agent.Main.SetSkeleton(_newSkeleton, animData);
                }
                else if (currentSkeleton == _newSkeleton)
                {
                    var animData = Agent.Main.Monster.FillAnimationSystemData(Agent.Main.ActionSet, 1f, false);
                    Agent.Main.SetSkeleton(_oldSkeleton, animData);
                }
            }
        }
        public override void OnPreMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            
        }

        private void OnAiAgentTick(Agent agent, float dt)
        {
            if (!agent.IsHuman) return;
        }

        private void OnPostAgentTick(Agent agent, float dt)
        {
            if (!agent.IsActive()) return;
        }

        private void AfterUpdateDynamicsFlags(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags)
        {
            if (agent != Agent.Main || agent.MountAgent == null) return;
        }
        private void OnAnimTreeTick(Skeleton skeleton, byte firstBoneIndex, ref MatrixFrame[] cachedMatrixFrames)
        {
            
        }

        internal unsafe static void SetPropertyUnsafe<T>(T value, ulong baseAdr,  params ulong[] offsets) where T : unmanaged
        {
            if (offsets.Length == 0) return;
            var finalAddr = baseAdr;
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                finalAddr = *(ulong*)(finalAddr + offsets[i]);
            }
            *(T*)(finalAddr + offsets[offsets.Length - 1]) = value;
        }
        internal unsafe static T GetPropertyUnsafe<T>(ulong baseAdr, params ulong[] offsets) where T : unmanaged
        {
            if (offsets.Length == 0) return default(T);
            var finalAddr = baseAdr;
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                finalAddr = *(ulong*)(finalAddr + offsets[i]);
            }
            return *(T*)(finalAddr + offsets[offsets.Length - 1]);
        }
        internal unsafe static void SetPropertyUnsafe<T>(T value, UIntPtr baseAdr, params ulong[] offsets) where T : unmanaged
        {
            SetPropertyUnsafe<T>(value, baseAdr.ToUInt64(), offsets);
        }
        internal unsafe static T GetPropertyUnsafe<T>(UIntPtr baseAdr, params ulong[] offsets) where T : unmanaged
        {
            return GetPropertyUnsafe<T>(baseAdr.ToUInt64(), offsets);
        }

        internal unsafe static ulong FindInMemory<T>(ulong baseAdr, ulong range, T value) where T : unmanaged
        {
            for (ulong i = 0; i < range; i++)
            {
                var currentValue = *(T*)(baseAdr + i);
                if (currentValue.Equals(value)) return baseAdr + i;
            }
            return 0x0;
        }
    }
#endif
}
