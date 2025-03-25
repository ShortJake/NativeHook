using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using NativeHook;
using TaleWorlds.Core;
using TaleWorlds.Library;

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
            // We check if the agent was crouching before the crouching flag was removed because of attacking
            if ((oldFlags & AgentDynamicsFlags.Crouch) == 0) return;
            // Don't re-enable crouching if standing, jumping, mounting, dismounting, or kicking
            if (((agent.EventControlFlags &
                (Agent.EventControlFlag.Jump | Agent.EventControlFlag.Mount | Agent.EventControlFlag.Stand
                | Agent.EventControlFlag.Dismount | Agent.EventControlFlag.Kick)) != 0)) return;
            // Don't re-enable crouching if action channel 0 or 1 has enforce_all or enforce_lowerbody
            var animFlags = agent.GetCurrentAnimationFlag(0) | agent.GetCurrentAnimationFlag(1);
            if ((animFlags & (AnimFlags.anf_enforce_all | AnimFlags.anf_enforce_lowerbody)) != 0) return;
            // Don't re-enable crouching if speed is greater than 0.06f;
            //if (agent.Velocity.LengthSquared > 0.0036f) return;
            // These are the conditions checked by the native method which disable crouching.
            // There are a couple other conditions which I have not reverse engineered yet
            agent.SetDynamicsFlags(newFlags | AgentDynamicsFlags.Crouch);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            // Don't forget to unsubscribe from the events to avoid leaks
            NativeHookSubModule.OnPostAiTick -= OnPostAiTick;
            NativeHookSubModule.AfterUpdateDynamicsFlags -= AfterUpdateDynamicsFlags;
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
