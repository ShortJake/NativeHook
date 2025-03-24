using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SampleMod
{
    public class AIKickComponent : AgentComponent
    {
        private MissionTime LastKickTime;
        private float KickClearTime;
        private float KickChance;
        public AIKickComponent(Agent agent) : base(agent)
        {
            LastKickTime = MissionTime.Now;
            KickClearTime = 3f;
            KickChance = 0.2f;
        }

        public void PostAiTick(float dt)
        {
            var target = Agent.GetTargetAgent();
            // Check if the target agent is blocking
            if (target == null || (target.MovementFlags & Agent.MovementControlFlag.DefendMask) == 0) return;
            // Don't spam kicks. Kick only every few seconds
            if (LastKickTime.ElapsedSeconds < KickClearTime) return;
            if (MBRandom.RandomFloat > KickChance) return;
            LastKickTime = MissionTime.Now;
            Agent.EventControlFlags |= Agent.EventControlFlag.Kick;
            Agent.MovementFlags &= ~Agent.MovementControlFlag.DefendMask;
        }
    }
}
