using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
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
            /*NativeHookSubModule.OnPostAgentTick += OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags += AfterUpdateDynamicsFlags;*/
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            NativeHookSubModule.OnPostAiTick -= OnAiAgentTick;
            /*NativeHookSubModule.OnPostAgentTick -= OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags -= AfterUpdateDynamicsFlags;*/
        }
        public override void OnMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            if (_oldSkeleton == null) _oldSkeleton = Agent.Main.AgentVisuals.GetSkeleton();
            if (Input.IsKeyPressed(InputKey.SemiColon))
            {
                var mat = Agent.Main.AgentVisuals.GetSkeleton().GetBoneLocalRestFrame(0);
                //mat.Rotate(0.4f, Vec3.Up);
                Agent.Main.AgentVisuals.GetSkeleton().SetBoneRestFrame(0, mat);
                /*var animSysData = Agent.Main.Monster.FillAnimationSystemData(Agent.Main.ActionSet, 1f, false);
                if (Agent.Main.AgentVisuals.GetSkeleton() == _oldSkeleton)
                {
                    if (_newSkeleton == null)
                    {   
                        _newSkeleton = MBSkeletonExtensions.CreateWithActionSet(ref animSysData);
                        Agent.Main.SetSkeleton(_newSkeleton, animSysData);
                        var equipment = new Equipment(Agent.Main.SpawnEquipment);
                        equipment[2].Clear();
                        Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(equipment);
                    }
                    else Agent.Main.SetSkeleton(_newSkeleton, animSysData);
                }
                else if (Agent.Main.AgentVisuals.GetSkeleton() == _newSkeleton)
                {
                    Agent.Main.SetSkeleton(_oldSkeleton, animSysData);
                }*/
            }
        }
        public override void OnPreMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            
        }

        private void OnAiAgentTick(Agent agent, float dt)
        {
            if (!agent.IsHuman) return;
            if (Input.IsKeyDown(InputKey.M)) agent.SetMovementVelocity(Vec2.Forward * -4);
        }

        private void OnPostAgentTick(Agent agent, float dt)
        {
            if (agent != Agent.Main || !agent.IsActive()) return;    
        }

        private void AfterUpdateDynamicsFlags(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags)
        {
            if (agent != Agent.Main) return;
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
    }
#endif
}
