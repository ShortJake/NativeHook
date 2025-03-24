using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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
        public DebugLogic()
        {
            NativeHookSubModule.OnPostAiTick += OnAiAgentTick;
            NativeHookSubModule.OnPostAgentTick += OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags += AfterUpdateDynamicsFlags;
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            NativeHookSubModule.OnPostAiTick -= OnAiAgentTick;
            NativeHookSubModule.OnPostAgentTick -= OnPostAgentTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags -= AfterUpdateDynamicsFlags;
        }

        public override void OnMissionTick(float dt)
        {
            if (Agent.Main == null) return;
            var agent = Agent.Main.MountAgent ?? Agent.Main;
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
            if (agent != Agent.Main || !agent.IsActive()) return;
            
        }

        private void AfterUpdateDynamicsFlags(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags)
        {
            if (agent != Agent.Main) return;
            Agent.Main.AgentVisuals.GetSkeleton()?.GetName();
            if (Input.IsKeyDown(InputKey.CloseBraces))
            {
                var flags = Agent.Main.GetDynamicsFlags();
                Agent.Main.SetDynamicsFlags(flags |= ~AgentDynamicsFlags.Walking2);
            }
            if (Agent.Main.MountAgent != null)
            {
                var flags = Agent.Main.MountAgent.GetDynamicsFlags();
                    //InformationManager.DisplayMessage(new InformationMessage(flags.ToString()));
            }
            else
            {
                var flags = Agent.Main.GetDynamicsFlags();
                //InformationManager.DisplayMessage(new InformationMessage(flags.ToString()));
            }
            
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
