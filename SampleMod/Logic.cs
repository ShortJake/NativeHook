using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using NativeHook;
using TaleWorlds.Core;

namespace SampleMod
{
    public class Logic : MissionLogic
    {
        public Logic()
        {
            // Subscribe to NativeHook's PostAiTick event
            // PostAiTick is triggered every tick after the engine handles AI stuff
            // This allows changing EventControlFlags and MovementFlags of AI agents such that they won't be overwritten by the engine
            NativeHookSubModule.OnPostAiTick += OnPostAiTick;

            // PostAgentTick is similar but for all agents not just AI

            // AfterUpdateDynamicsFlags is triggered after the engine calculates the new AgentDynamicsFlags based on 
            // agent velocity, and based on EventControlFlags among other things
            // These flags control stuff related to agent motion including crouching, jumping, walking, etc...
            NativeHookSubModule.AfterUpdateDynamicsFlags += AfterUpdateDynamicsFlags;
        }

        private void AfterUpdateDynamicsFlags(Agent agent, float dt, AgentDynamicsFlags oldFlags, AgentDynamicsFlags newFlags)
        {
            agent.GetComponent<FlyingAgentComponent>()?.AfterDynamicsUpdateFlags();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            // Don't forget to unsubscribe from the events to avoid leaks
            NativeHookSubModule.OnPostAiTick -= OnPostAiTick;
        }

        public override void OnMissionTick(float dt)
        {
            if (Agent.Main != null)
            {
                Agent.Main.GetComponent<FlyingAgentComponent>()?.PlayerTick(dt);
            }
        }

        public void OnPostAiTick(Agent agent, float dt)
        {
            agent.GetComponent<FlyingAgentComponent>()?.PostAiTick(dt);
            agent.GetComponent<AIKickComponent>()?.PostAiTick(dt);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            if (!agent.IsHuman) return;
            agent.AddComponent(new FlyingAgentComponent(agent));
            if (agent.Controller == Agent.ControllerType.AI) agent.AddComponent(new AIKickComponent(agent));
        }
    }
}
